using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Utils;
using VRageMath;
using System.Linq;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using PEPCO.LogError;

namespace PEPCO.iSurvival.CustomEntitySpawner
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class CustomEntitySpawner : MySessionComponentBase
    {
        public static Networking NetworkingInstance { get; private set; }
        public CustomEntitySpawnerSettings CESsettings = new CustomEntitySpawnerSettings();
        public CustomEntitySpawnerChat chat = new CustomEntitySpawnerChat();
        public PEPCO_LogError log = new PEPCO_LogError();

        public HashSet<string> validBotIds = new HashSet<string>();

        private int updateTickCounter = 0;
        private long totalUpdateTicks = 0;
        private bool isLoading = true;
        private int loadingTickCount = 100;
        private static readonly Random randomGenerator = new Random();
        private int cleanupTickCounter = 0;


        private const string ModDataFile = "CES.ini";
        private const string WorldStorageFolder = "CustomEntitySpawner";
        private const string WorldStorageFile = "CES.ini";


        private static List<IMyPlayer> players = new List<IMyPlayer>();
        private static List<long> justSpawned = new List<long>();

        private static int skippedTicks = 0;
        private bool isServer;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            isServer = MyAPIGateway.Multiplayer.IsServer;
            if (!isServer) return;
            MyVisualScriptLogicProvider.PlayerSpawned += (playerId) =>
            {
                justSpawned.Add(playerId);
            };

            NetworkingInstance = new Networking(58430); // last 5 of Workshop 
            NetworkingInstance.Register();

            MyAPIGateway.Utilities.ShowMessage("start", "Init");
            EnsureDefaultIniFilesExist();
            CopyAllCESFilesToWorldStorage();
            LoadAllFilesFromWorldStorage();
            CESsettings.Load();
            InitializeBotSpawnerConfig();
            LoadValidBotIds();
            MyAPIGateway.Utilities.MessageEntered += chat.OnMessageEntered;
        }
       

        public void LoadValidBotIds()
        {
            MyAPIGateway.Utilities.ShowMessage("BotSpawner", "LoadValidBotIds");
            var botDefinitions = MyDefinitionManager.Static.GetBotDefinitions();
            foreach (var botDefinition in botDefinitions)
            {
                MyAPIGateway.Utilities.ShowMessage("BotSpawner", $"{botDefinition.Id.SubtypeName}");
                validBotIds.Add(botDefinition.Id.SubtypeName);
            }
        }

        public void ListValidBotIds()
        {
            MyAPIGateway.Utilities.ShowMessage("BotSpawner", "ListValidBotIds");
            foreach (var botId in validBotIds)
            {
                MyAPIGateway.Utilities.ShowMessage("BotSpawner", botId);
            }
        }
        public void EnsureDefaultIniFilesExist()
        {
            CreateIniFileIfNotExists(CESsettings.GlobalFileName, CESsettings.DefaultGlobalIniContent);
            CreateIniFileIfNotExists(CESsettings.EntitySpawnerFileName, CESsettings.DefaultEntitySpawnerIniContent);
        }

        private void CreateIniFileIfNotExists(string fileName, string content)
        {
            if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, typeof(CustomEntitySpawnerSettings)))
            {
                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(fileName, typeof(CustomEntitySpawnerSettings)))
                {
                    writer.Write(content);
                }
            }
        }

        protected override void UnloadData()
        {
            if (MyAPIGateway.Multiplayer.IsServer)
            {
                MyAPIGateway.Utilities.MessageEntered -= chat.OnMessageEntered;
                validBotIds.Clear();
            }
            base.UnloadData();
        }

        public override void UpdateBeforeSimulation()
        {
            
            if (!MyAPIGateway.Multiplayer.IsServer) return;

            if (isLoading && loadingTickCount-- > 0) return;
            isLoading = false;
            totalUpdateTicks++;

            if (++updateTickCounter >= CESsettings.BaseUpdateInterval)
            {
                updateTickCounter = 0;
                try
                {
                    int entitiesSpawned = 0;
                    SpawnEntitiesNearBlocks(ref entitiesSpawned);
                }
                catch (Exception ex)
                {
                    log.LogError($"Update error: {ex.Message}");
                }
            }

            if (CESsettings.CleanupInterval != 0 && ++cleanupTickCounter >= CESsettings.CleanupInterval)
            {
                cleanupTickCounter = 0;
                try
                {
                    chat.CleanupDeadEntities();
                }
                catch (Exception ex)
                {
                    log.LogError($"Cleanup error: {ex.Message}");
                }
            }
        }

        public void PauseScript()
        {
            CESsettings.scriptPaused = !CESsettings.scriptPaused;
            MyAPIGateway.Utilities.ShowMessage("CES", $"Script is now {(CESsettings.scriptPaused ? "paused" : "resumed")}.");
        }

        private void SpawnEntitiesNearBlocks(ref int entitiesSpawned)
        {
            var paused = CESsettings.scriptPaused;
            MyAPIGateway.Utilities.ShowMessage("CES", $"NearBlocks?{paused}");
            if (paused) return;
            long baseUpdateCycles = totalUpdateTicks / CESsettings.BaseUpdateInterval;
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            var entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities);

            foreach (var entity in entities)
            {
                var grid = entity as IMyCubeGrid;
                if (grid != null)
                {
                    var blocks = new List<IMySlimBlock>();
                    grid.GetBlocks(blocks, b => b.FatBlock != null);

                    foreach (var block in blocks)
                    {
                        foreach (var blockSettings in CESsettings.BlockSpawnSettings)
                        {
                            if (block.FatBlock.BlockDefinition.TypeIdString == blockSettings.BlockType &&
                                block.FatBlock.BlockDefinition.SubtypeId == blockSettings.BlockId &&
                                IsValidBlockForSpawning(block, blockSettings, baseUpdateCycles, players))
                            {
                                ProcessBlockSpawning(block, blockSettings, ref entitiesSpawned);
                            }
                        }
                    }
                }
            }
        }


        private bool IsValidBlockForSpawning(IMySlimBlock block, BotSpawnerConfig blockSettings, long baseUpdateCycles, List<IMyPlayer> players)
        {
            if (!blockSettings.Enabled) return false;
            if (baseUpdateCycles % blockSettings.SpawnTriggerInterval != 0) return false;
            if (!IsEnvironmentSuitable(block, blockSettings)) return false;
            if (!IsPlayerInRange(block, players, blockSettings.PlayerDistanceCheck)) return false;
            if (!AreRequiredEntitiesInVicinity(block, blockSettings)) return false;

            float blockHealthPercentage = block.Integrity / block.MaxIntegrity;
            if (blockHealthPercentage < blockSettings.MinHealthPercentage || blockHealthPercentage > blockSettings.MaxHealthPercentage)
                return false;

            return CheckInventoryForRequiredItems(block, blockSettings);
        }

        private bool IsEnvironmentSuitable(IMySlimBlock block, BotSpawnerConfig blockSettings)
        {
            var grid = block.CubeGrid;

            if (blockSettings.RequiredEntityNumber > 0)
            {
                int entityCount = GetEntityCountInRadius(block.FatBlock.GetPosition(), blockSettings.MaxEntitiesRadius, "All", true);
                if (entityCount >= blockSettings.MaxEntitiesInArea)
                {
                    log.LogError($"Entity spawn limit reached: {entityCount} entities within radius {blockSettings.MaxEntitiesRadius}");
                    return false;
                }
            }

            if (blockSettings.EnableAirtightAndOxygen)
            {
                bool isAirtight = grid.IsRoomAtPositionAirtight(block.FatBlock.Position);
                double oxygenLevel = MyAPIGateway.Session.OxygenProviderSystem.GetOxygenInPoint(block.FatBlock.GetPosition());
                if (!isAirtight && oxygenLevel <= 0.5)
                {
                    log.LogError($"Airtight and oxygen conditions not met for {block.FatBlock.BlockDefinition.SubtypeId}");
                    return false;
                }
            }

            return true;
        }

        private void ProcessBlockSpawning(IMySlimBlock block, BotSpawnerConfig blockSettings, ref int entitiesSpawned)
        {
            bool entitiesSpawnedThisCycle = false;
            RemoveItemsFromInventory(block, blockSettings);
            if (blockSettings.EnableEntitySpawning)
            {
                int currentGlobalEntityCount = GetTotalEntityCount();
                if (currentGlobalEntityCount >= CESsettings.GlobalMaxEntities)
                {
                    log.LogError($"Entity global limit reached: {currentGlobalEntityCount} entities");
                    return;
                }

                int currentEntityCount = GetEntityCountInRadius(block.FatBlock.GetPosition(), blockSettings.MaxEntitiesRadius, "All", true);
                if (currentEntityCount >= blockSettings.MaxEntitiesInArea)
                {
                    log.LogError($"Entity spawn limit reached: {currentEntityCount} entities within radius {blockSettings.MaxEntitiesRadius}");
                    return;
                }

                SpawnEntitiesAndApplyDamage(block, blockSettings, ref entitiesSpawned, ref entitiesSpawnedThisCycle);
            }

            if (blockSettings.EnableItemSpawning)
            {
                SpawnItemsAndApplyDamage(block, blockSettings);
            }
        }

        private void SpawnEntitiesAndApplyDamage(IMySlimBlock block, BotSpawnerConfig blockSettings, ref int entitiesSpawned, ref bool entitiesSpawnedThisCycle)
        {
            for (int i = 0; i < 1; i++) // Default spawn iterations
            {
                int entitySpawnAmount = randomGenerator.Next(blockSettings.MinEntityAmount, blockSettings.MaxEntityAmount + 1);
                if (entitySpawnAmount > 0)
                {
                    if (blockSettings.RequireEntityCenterOn)
                    {
                        CenterSpawnAroundEntities(block, blockSettings, entitySpawnAmount);
                    }
                    else
                    {
                        SpawnEntities(block, blockSettings, entitySpawnAmount);
                    }

                    ApplyDamageToBlock(block, blockSettings, entitySpawnAmount);
                    entitiesSpawned++;
                    entitiesSpawnedThisCycle = true;
                }
            }
        }

        private void SpawnItemsAndApplyDamage(IMySlimBlock block, BotSpawnerConfig blockSettings)
        {
            for (int i = 0; i < 1; i++) // Default spawn iterations
            {
                int itemSpawnAmount = blockSettings.UseWeightedDrops ?
                    GetWeightedRandomNumber(blockSettings.MinItemAmount, GenerateProbabilities(blockSettings.MinItemAmount, blockSettings.MaxItemAmount)) :
                    randomGenerator.Next(blockSettings.MinItemAmount, blockSettings.MaxItemAmount + 1);

                if (itemSpawnAmount > 0)
                {
                    if (blockSettings.RequireEntityCenterOn)
                    {
                        CenterSpawnItemsAroundEntities(block, blockSettings, itemSpawnAmount);
                    }
                    else
                    {
                        SpawnItems(block, blockSettings);
                    }

                    ApplyDamageToBlock(block, blockSettings, itemSpawnAmount);
                }
            }
        }

        private void ApplyDamageToBlock(IMySlimBlock block, BotSpawnerConfig blockSettings, int amount)
        {
            float maxHealth = block.MaxIntegrity;
            float damageAmount = maxHealth * (blockSettings.DamageAmount / 100.0f) * amount;
            if (!blockSettings.Repair)
            {
                block.DoDamage(damageAmount, MyDamageType.Destruction, true);
            }
            else
            {
                for (int i = 0; i <= blockSettings.DamageAmount * amount; i++)
                {
                    block.SpawnFirstItemInConstructionStockpile();
                }
                block.IncreaseMountLevel(blockSettings.DamageAmount * amount, block.OwnerId, null, MyAPIGateway.Session.WelderSpeedMultiplier, true);
            }
        }

        private bool AreRequiredEntitiesInVicinity(IMySlimBlock block, BotSpawnerConfig blockSettings)
        {
            if (blockSettings.RequiredEntityNumber > 0)
            {
                int entityCount = GetEntityCountInRadius(block.FatBlock.GetPosition(), blockSettings.RequiredEntityRadius, blockSettings.RequiredEntity, true);
                return entityCount >= blockSettings.RequiredEntityNumber;
            }
            return true;
        }

        private int GetEntityCountInRadius(Vector3D position, double radius, string requiredEntitySubtypeId, bool excludeDeadEntities = false)
        {
            var entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities, e => e is IMyCharacter);

            int entityCount = 0;
            foreach (var entity in entities)
            {
                if (requiredEntitySubtypeId.Equals("All") && excludeDeadEntities)
                {
                    entityCount++;
                }
                else
                {
                    string entityCurrentSubtypeId = (entity as MyEntity)?.DefinitionId?.SubtypeId.ToString();
                    if (entityCurrentSubtypeId != null && entityCurrentSubtypeId.Equals(requiredEntitySubtypeId, StringComparison.OrdinalIgnoreCase) &&
                        Vector3D.Distance(entity.GetPosition(), MyAPIGateway.Session.Player.GetPosition()) <= radius)
                    {
                        entityCount++;
                    }
                }
            }
            return entityCount;
        }

        private int GetTotalEntityCount()
        {
            var entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities, entity =>
            {
                IMyCharacter playerCharacter = MyAPIGateway.Session.Player.Character;
                var character = entity as IMyCharacter;
                return entity is IMyCharacter && character != playerCharacter && !character.IsDead;
            });
            return entities.Count;
        }

        private bool IsPlayerInRange(IMySlimBlock block, List<IMyPlayer> players, int playerDistanceCheck)
        {
            if (playerDistanceCheck == -1) return true;
            if (playerDistanceCheck == 0) return players.Count > 0;

            Vector3D blockPosition = block.FatBlock.GetPosition();
            foreach (var player in players)
            {
                var controlledEntity = player.Controller?.ControlledEntity?.Entity;
                if (controlledEntity != null && controlledEntity is IMyCharacter && player == MyAPIGateway.Session.LocalHumanPlayer)
                {
                    double distance = Vector3D.Distance(controlledEntity.GetPosition(), blockPosition);
                    if (distance <= playerDistanceCheck) return true;
                }
            }
            return false;
        }

        private bool CheckInventoryForRequiredItems(IMySlimBlock block, BotSpawnerConfig blockSettings)
        {
            var inventory = block.FatBlock.GetInventory() as IMyInventory;
            if (inventory == null) return true;

            for (int i = 0; i < blockSettings.RequiredItemTypes.Count; i++)
            {
                var requiredItemType = new MyDefinitionId(CESsettings.itemTypeMappings[blockSettings.RequiredItemTypes[i]], blockSettings.RequiredItemIds[i]);
                var itemAmount = (VRage.MyFixedPoint)blockSettings.RequiredItemAmounts[i];

                if (itemAmount > 0 && !inventory.ContainItems(itemAmount, requiredItemType))
                {
                    log.LogError($"Required item missing: {requiredItemType} with amount {itemAmount} in block {block.FatBlock.DisplayName}");
                    return false;
                }
            }

            for (int i = 0; i < blockSettings.PermanentRequiredItemTypes.Count; i++)
            {
                var permanentRequiredItemType = new MyDefinitionId(CESsettings.itemTypeMappings[blockSettings.PermanentRequiredItemTypes[i]], blockSettings.PermanentRequiredItemIds[i]);
                var permanentItemAmount = (VRage.MyFixedPoint)blockSettings.PermanentRequiredItemAmounts[i];

                if (permanentItemAmount > 0 && !inventory.ContainItems(permanentItemAmount, permanentRequiredItemType))
                {
                    log.LogError($"Permanent required item missing: {permanentRequiredItemType} with amount {permanentItemAmount} in block {block.FatBlock.DisplayName}");
                    return false;
                }
            }

            return true;
        }

        private void RemoveItemsFromInventory(IMySlimBlock block, BotSpawnerConfig blockSettings)
        {
            var inventory = block.FatBlock.GetInventory() as IMyInventory;
            if (inventory == null)
            {
                log.LogError("Inventory not found for block.");
                return;
            }

            for (int i = 0; i < blockSettings.RequiredItemTypes.Count; i++)
            {
                var requiredItemType = new MyDefinitionId(CESsettings.itemTypeMappings[blockSettings.RequiredItemTypes[i]], blockSettings.RequiredItemIds[i]);
                var totalAmountToRemove = (VRage.MyFixedPoint)(blockSettings.RequiredItemAmounts[i]);
                if (totalAmountToRemove > 0)
                {
                    if (inventory.ContainItems(totalAmountToRemove, requiredItemType))
                    {
                        inventory.RemoveItemsOfType(totalAmountToRemove, requiredItemType);
                        log.LogError($"Removed {totalAmountToRemove} of {requiredItemType} from inventory.");
                    }
                    else
                    {
                        log.LogError($"Insufficient items: {totalAmountToRemove} of {requiredItemType} in inventory.");
                    }
                }
            }
        }

        private void SpawnEntities(IMySlimBlock block, BotSpawnerConfig settings, int spawnAmount)
        {
            if (settings.EnableEntitySpawning)
            {
                foreach (var entityID in settings.EntityID)
                {
                    for (int j = 0; j < spawnAmount; j++)
                    {
                        int entityAmount = randomGenerator.Next(settings.MinEntityAmount, settings.MaxEntityAmount + 1);
                        Vector3D spawnPosition = CalculateDropPosition(block, settings);
                        MyVisualScriptLogicProvider.SpawnBot(entityID, spawnPosition);
                        if (settings.SpawnItemsWithEntities)
                        {
                            SpawnItems(block, settings);
                        }
                    }
                }
            }
        }

        private void SpawnItems(IMySlimBlock block, BotSpawnerConfig settings, Vector3D? position = null)
        {
            foreach (var itemType in settings.ItemTypes)
            {
                var itemId = settings.ItemIds[settings.ItemTypes.IndexOf(itemType)];
                int amount = settings.UseWeightedDrops
                    ? GetWeightedRandomNumber(settings.MinItemAmount, GenerateProbabilities(settings.MinItemAmount, settings.MaxItemAmount))
                    : randomGenerator.Next(settings.MinItemAmount, settings.MaxItemAmount + 1);

                if (settings.SpawnInsideInventory)
                {
                    PlaceItemInCargo(block, itemType, itemId, amount, settings.StackItems);
                }
                else
                {
                    if (position.HasValue)
                    {
                        SpawnItemNearPosition(position.Value, itemType, itemId, amount, settings.StackItems, settings);
                    }
                    else
                    {
                        SpawnItemNearBlock(block, itemType, itemId, amount, settings.StackItems, settings);
                    }
                }
            }
        }

        private void PlaceItemInCargo(IMySlimBlock block, string itemType, string itemId, int amount, bool stackItems)
        {
            var inventory = block.FatBlock.GetInventory() as IMyInventory;
            if (inventory != null)
            {
                Type itemTypeId;
                if (CESsettings.itemTypeMappings.TryGetValue(itemType, out itemTypeId))
                {
                    var itemDefinition = new MyDefinitionId(itemTypeId, itemId);
                    var itemObjectBuilder = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(itemDefinition);

                    if (itemObjectBuilder != null)
                    {
                        if (stackItems)
                        {
                            inventory.AddItems(amount, itemObjectBuilder);
                        }
                        else
                        {
                            for (int i = 0; i < amount; i++)
                            {
                                inventory.AddItems(1, itemObjectBuilder);
                            }
                        }
                    }
                    else
                    {
                        MyAPIGateway.Utilities.ShowMessage("Error", $"Invalid item ID: {itemId}");
                    }
                }
                else
                {
                    MyAPIGateway.Utilities.ShowMessage("Error", $"Invalid item type: {itemType}");
                }
            }
            else
            {
                MyAPIGateway.Utilities.ShowMessage("Error", "No inventory found.");
            }
        }


        private void SpawnItemNearBlock(IMySlimBlock block, string itemType, string itemId, int amount, bool stackItems, BotSpawnerConfig settings)
        {
            var itemDefinition = new MyDefinitionId(CESsettings.itemTypeMappings[itemType], itemId);
            var itemObjectBuilder = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(itemDefinition);

            List<Vector3D> dropPositions = new List<Vector3D>();
            List<Quaternion> dropRotations = new List<Quaternion>();

            if (stackItems)
            {
                dropPositions.Add(CalculateDropPosition(block, settings));
                dropRotations.Add(GenerateRandomRotation());
            }
            else
            {
                for (int j = 0; j < amount; j++)
                {
                    dropPositions.Add(CalculateDropPosition(block, settings));
                    dropRotations.Add(GenerateRandomRotation());
                }
            }

            for (int i = 0; i < dropPositions.Count; i++)
            {
                MyFloatingObjects.Spawn(
                    new MyPhysicalInventoryItem((VRage.MyFixedPoint)(stackItems ? amount : 1), itemObjectBuilder),
                    dropPositions[i], dropRotations[i].Up, block.FatBlock.WorldMatrix.Up
                );
            }
        }

        private Vector3D CalculateDropPosition(IMySlimBlock block, BotSpawnerConfig settings)
        {
            Vector3D basePosition = block.FatBlock.WorldAABB.Center;
            double height = settings.MinHeight + (randomGenerator.NextDouble() * (settings.MaxHeight - settings.MinHeight));
            double radius = settings.MinRadius + (randomGenerator.NextDouble() * (settings.MaxRadius - settings.MinRadius));
            double angle = randomGenerator.NextDouble() * Math.PI * 2;

            Vector3D offset = new Vector3D(
                Math.Cos(angle) * radius,
                height,
                Math.Sin(angle) * radius
            );

            return basePosition + offset;
        }

        private void CenterSpawnItemsAroundEntities(IMySlimBlock block, BotSpawnerConfig settings, int itemSpawnAmount)
        {
            foreach (var itemType in settings.ItemTypes)
            {
                var itemId = settings.ItemIds[settings.ItemTypes.IndexOf(itemType)];
                var entities = new HashSet<IMyEntity>();
                MyAPIGateway.Entities.GetEntities(entities, e => e is IMyCharacter);

                foreach (var entity in entities)
                {
                    var character = entity as IMyCharacter;
                    if (character != null && !character.IsDead && character.Definition.Id.SubtypeId.String.Equals(settings.RequiredEntity, StringComparison.OrdinalIgnoreCase))
                    {
                        for (int i = 0; i < itemSpawnAmount; i++)
                        {
                            SpawnItemNearPosition(entity.GetPosition(), itemType, itemId, itemSpawnAmount, settings.StackItems, settings);
                        }
                    }
                }
            }
        }


        private void SpawnItemNearPosition(Vector3D position, string itemType, string itemId, int amount, bool stackItems, BotSpawnerConfig settings)
        {
            var itemDefinition = new MyDefinitionId(CESsettings.itemTypeMappings[itemType], itemId);
            var itemObjectBuilder = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(itemDefinition);

            List<Vector3D> dropPositions = new List<Vector3D>();
            List<Quaternion> dropRotations = new List<Quaternion>();

            if (stackItems)
            {
                dropPositions.Add(CalculateDropPosition(position, settings));
                dropRotations.Add(GenerateRandomRotation());
            }
            else
            {
                for (int j = 0; j < amount; j++)
                {
                    dropPositions.Add(CalculateDropPosition(position, settings));
                    dropRotations.Add(GenerateRandomRotation());
                }
            }

            for (int i = 0; i < dropPositions.Count; i++)
            {
                MyFloatingObjects.Spawn(
                    new MyPhysicalInventoryItem((VRage.MyFixedPoint)(stackItems ? amount : 1), itemObjectBuilder),
                    dropPositions[i], dropRotations[i].Up, Vector3D.Up
                );
            }
        }
        
        private Vector3D CalculateDropPosition(Vector3D basePosition, BotSpawnerConfig settings)
        {
            double height = settings.MinHeight + (randomGenerator.NextDouble() * (settings.MaxHeight - settings.MinHeight));
            double radius = settings.MinRadius + (randomGenerator.NextDouble() * (settings.MaxRadius - settings.MinRadius));
            double angle = randomGenerator.NextDouble() * Math.PI * 2;

            Vector3D offset = new Vector3D(
                Math.Cos(angle) * radius,
                height,
                Math.Sin(angle) * radius
            );

            return basePosition + offset;
        }

        private Quaternion GenerateRandomRotation()
        {
            return Quaternion.CreateFromYawPitchRoll(
                (float)(randomGenerator.NextDouble() * Math.PI * 2),
                (float)(randomGenerator.NextDouble() * Math.PI * 2),
                (float)(randomGenerator.NextDouble() * Math.PI * 2)
            );
        }

        private void CenterSpawnAroundEntities(IMySlimBlock block, BotSpawnerConfig settings, int spawnAmount)
        {
            var entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities, e => e is IMyCharacter);

            foreach (var entity in entities)
            {
                var myEntity = entity as MyEntity;
                if (myEntity != null && myEntity.DefinitionId.HasValue && myEntity.DefinitionId.Value.SubtypeId == MyStringHash.GetOrCompute(settings.RequiredEntity))
                {
                    if (Vector3D.Distance(entity.GetPosition(), block.FatBlock.GetPosition()) <= settings.RequiredEntityRadius)
                    {
                        for (int i = 0; i < spawnAmount; i++)
                        {
                            MyVisualScriptLogicProvider.SpawnBot(settings.EntityID.FirstOrDefault(), entity.PositionComp.GetPosition());
                        }
                    }
                }
            }
        }


        private List<int> GenerateProbabilities(int minAmount, int maxAmount)
        {
            int range = maxAmount - minAmount + 1;
            int middle = (range + 1) / 2;
            var probabilities = new List<int>();

            for (int i = 1; i <= range; i++)
            {
                int weight = i <= middle ? i : range - i + 1;
                probabilities.Add(weight);
            }
            return probabilities;
        }

        private int GetWeightedRandomNumber(int minAmount, List<int> probabilities)
        {
            int totalWeight = probabilities.Sum();
            if (totalWeight == 0)
            {
                return minAmount;
            }

            int randomValue = randomGenerator.Next(1, totalWeight + 1);
            int cumulativeWeight = 0;

            for (int i = 0; i < probabilities.Count; i++)
            {
                cumulativeWeight += probabilities[i];
                if (randomValue <= cumulativeWeight)
                    return minAmount + i;
            }
            return minAmount;
        }

        public void InitializeBotSpawnerConfig()
        {
            CESsettings.BlockSpawnSettings.Clear();
            CESsettings.Load();

            foreach (var botSpawnerConfig in CESsettings.BlockSpawnSettings)
            {
                log.LogError($"Initialized settings for {botSpawnerConfig.BlockId}: " +
                             $"BlockType={botSpawnerConfig.BlockType}, " +
                             $"Enabled={botSpawnerConfig.Enabled}, " +
                             $"PlayerDistanceCheck={botSpawnerConfig.PlayerDistanceCheck}, " +
                             $"EnableEntitySpawning={botSpawnerConfig.EnableEntitySpawning}, " +
                             $"EntityID={string.Join(",", botSpawnerConfig.EntityID)}, " +
                             $"MinEntityAmount={botSpawnerConfig.MinEntityAmount}, MaxEntityAmount={botSpawnerConfig.MaxEntityAmount}, " +
                             $"MaxEntitiesInArea={botSpawnerConfig.MaxEntitiesInArea}, " +
                             $"EnableItemSpawning={botSpawnerConfig.EnableItemSpawning}, " +
                             $"ItemTypes={string.Join(",", botSpawnerConfig.ItemTypes)}, " +
                             $"ItemIds={string.Join(",", botSpawnerConfig.ItemIds)}, " +
                             $"MinItemAmount={botSpawnerConfig.MinItemAmount}, MaxItemAmount={botSpawnerConfig.MaxItemAmount}, " +
                             $"StackItems={botSpawnerConfig.StackItems}, SpawnInsideInventory={botSpawnerConfig.SpawnInsideInventory}, " +
                             $"DamageAmount={botSpawnerConfig.DamageAmount}, " +
                             $"MinHealthPercentage={botSpawnerConfig.MinHealthPercentage}, MaxHealthPercentage={botSpawnerConfig.MaxHealthPercentage}, " +
                             $"MinHeight={botSpawnerConfig.MinHeight}, MaxHeight={botSpawnerConfig.MaxHeight}, " +
                             $"MinRadius={botSpawnerConfig.MinRadius}, MaxRadius={botSpawnerConfig.MaxRadius}, " +
                             $"SpawnTriggerInterval={botSpawnerConfig.SpawnTriggerInterval}, EnableAirtightAndOxygen={botSpawnerConfig.EnableAirtightAndOxygen}, " +
                             $"RequiredItemTypes={string.Join(",", botSpawnerConfig.RequiredItemTypes)}, " +
                             $"RequiredItemIds={string.Join(",", botSpawnerConfig.RequiredItemIds)}, " +
                             $"RequiredItemAmounts={string.Join(",", botSpawnerConfig.RequiredItemAmounts)}, " +
                             $"PermanentRequiredItemTypes={string.Join(",", botSpawnerConfig.PermanentRequiredItemTypes)}, " +
                             $"PermanentRequiredItemIds={string.Join(",", botSpawnerConfig.PermanentRequiredItemIds)}, " +
                             $"PermanentRequiredItemAmounts={string.Join(",", botSpawnerConfig.PermanentRequiredItemAmounts)}, " +
                             $"RequiredEntity={botSpawnerConfig.RequiredEntity}, " +
                             $"RequiredEntityRadius={botSpawnerConfig.RequiredEntityRadius}, " +
                             $"RequiredEntityNumber={botSpawnerConfig.RequiredEntityNumber}, " +
                             $"RequireEntityNumberForTotalEntities={botSpawnerConfig.RequireEntityNumberForTotalEntities}, " +
                             $"RequireEntityCenterOn={botSpawnerConfig.RequireEntityCenterOn}");
            }

            log.LogError("Drop settings initialized.");
        }

        public void CopyAllCESFilesToWorldStorage()
        {
            log.LogError("Starting CopyAllCESFilesToWorldStorage");
            try
            {
                foreach (var modItem in MyAPIGateway.Session.Mods)
                {
                    string modFilePath = "Data/CES.ini";
                    if (MyAPIGateway.Utilities.FileExistsInModLocation(modFilePath, modItem))
                    {
                        string worldStorageFilePath = $"{modItem.PublishedFileId}_CES.ini";

                        if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(worldStorageFilePath, typeof(CustomEntitySpawner)))
                        {
                            string fileContent;
                            using (var reader = MyAPIGateway.Utilities.ReadFileInModLocation(modFilePath, modItem))
                            {
                                fileContent = reader.ReadToEnd();
                            }

                            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(worldStorageFilePath, typeof(CustomEntitySpawner)))
                            {
                                writer.Write(fileContent);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Error copying files to World Storage: {ex.Message}");
            }
        }

        public void LoadAllFilesFromWorldStorage()
        {
            try
            {
                HashSet<string> loadedFiles = new HashSet<string>();
                List<string> knownConfigFiles = new List<string> { "GlobalConfig.ini", "CustomEntitySpawner.ini" };
                List<string> additionalFilePatterns = new List<string> { "_CES.ini" };

                CESsettings.BlockSpawnSettings.Clear();

                LoadKnownConfigFiles(knownConfigFiles, loadedFiles);
                LoadAdditionalModConfigFiles(additionalFilePatterns, loadedFiles);
            }
            catch (Exception ex)
            {
                log.LogError($"Error loading files from World Storage: {ex.Message}");
            }
        }

        private void LoadKnownConfigFiles(List<string> knownConfigFiles, HashSet<string> loadedFiles)
        {
            foreach (string configFileName in knownConfigFiles)
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(configFileName, typeof(CustomEntitySpawner)) && !loadedFiles.Contains(configFileName))
                {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(configFileName, typeof(CustomEntitySpawner)))
                    {
                        string fileContent = reader.ReadToEnd();
                        if (configFileName.Equals("GlobalConfig.ini", StringComparison.OrdinalIgnoreCase))
                        {
                            LoadGlobalConfig(fileContent);
                        }
                        else if (configFileName.Equals("CustomEntitySpawner.ini", StringComparison.OrdinalIgnoreCase))
                        {
                            LoadCustomEntitySpawnerConfig(fileContent);
                        }
                        loadedFiles.Add(configFileName);
                    }
                }
            }
        }

        private void LoadAdditionalModConfigFiles(List<string> additionalFilePatterns, HashSet<string> loadedFiles)
        {
            foreach (var modItem in MyAPIGateway.Session.Mods)
            {
                foreach (string pattern in additionalFilePatterns)
                {
                    string cesFileName = $"{modItem.PublishedFileId}{pattern}";
                    if (MyAPIGateway.Utilities.FileExistsInWorldStorage(cesFileName, typeof(CustomEntitySpawner)) && !loadedFiles.Contains(cesFileName))
                    {
                        using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(cesFileName, typeof(CustomEntitySpawner)))
                        {
                            string fileContent = reader.ReadToEnd();
                            LoadCustomEntitySpawnerConfig(fileContent);
                            loadedFiles.Add(cesFileName);
                        }
                    }
                }
            }
        }

        private void LoadGlobalConfig(string fileContent)
        {
            MyIni iniParser = new MyIni();
            MyIniParseResult result;
            if (!iniParser.TryParse(fileContent, out result))
            {
                throw new Exception($"Config error: {result.ToString()}");
            }

            CESsettings.BaseUpdateInterval = iniParser.Get("Config", "BaseUpdateInterval").ToInt32(60);
            CESsettings.EnableLogging = iniParser.Get("Config", "EnableLogging").ToBoolean(false);
            CESsettings.CleanupInterval = iniParser.Get("Config", "CleanupInterval").ToInt32(180);
            CESsettings.GlobalMaxEntities = iniParser.Get("Config", "GlobalMaxEntities").ToInt32(100);
        }
        public void ApplyReceivedSettings(BotSpawnerConfig config)
        {
            CESsettings.UpdateBlockSettings(0, config); // Example entityId, replace with actual if necessary
        }
        private void LoadCustomEntitySpawnerConfig(string fileContent)
        {
            MyIni iniParser = new MyIni();
            MyIniParseResult result;
            if (!iniParser.TryParse(fileContent, out result))
            {
                throw new Exception($"Config error: {result.ToString()}");
            }

            List<string> sections = new List<string>();
            iniParser.GetSections(sections);

            foreach (var section in sections)
            {
                if (section == "Config") continue;

                var botSpawnerConfig = new BotSpawnerConfig
                {
                    BlockId = iniParser.Get(section, nameof(BotSpawnerConfig.BlockId)).ToString(section),
                    BlockType = iniParser.Get(section, nameof(BotSpawnerConfig.BlockType)).ToString(),
                    Enabled = iniParser.Get(section, nameof(BotSpawnerConfig.Enabled)).ToBoolean(),
                    PlayerDistanceCheck = iniParser.Get(section, nameof(BotSpawnerConfig.PlayerDistanceCheck)).ToInt32(),
                    EnableEntitySpawning = iniParser.Get(section, nameof(BotSpawnerConfig.EnableEntitySpawning)).ToBoolean(true),
                    EnableItemSpawning = iniParser.Get(section, nameof(BotSpawnerConfig.EnableItemSpawning)).ToBoolean(true),
                    SpawnItemsWithEntities = iniParser.Get(section, nameof(BotSpawnerConfig.SpawnItemsWithEntities)).ToBoolean(false),
                    MinEntityAmount = iniParser.Get(section, nameof(BotSpawnerConfig.MinEntityAmount)).ToInt32(),
                    MaxEntityAmount = iniParser.Get(section, nameof(BotSpawnerConfig.MaxEntityAmount)).ToInt32(),
                    MinItemAmount = iniParser.Get(section, nameof(BotSpawnerConfig.MinItemAmount)).ToInt32(),
                    MaxItemAmount = iniParser.Get(section, nameof(BotSpawnerConfig.MaxItemAmount)).ToInt32(),
                    UseWeightedDrops = iniParser.Get(section, nameof(BotSpawnerConfig.UseWeightedDrops)).ToBoolean(),
                    MaxEntitiesInArea = iniParser.Get(section, nameof(BotSpawnerConfig.MaxEntitiesInArea)).ToInt32(),
                    MaxEntitiesRadius = iniParser.Get(section, nameof(BotSpawnerConfig.MaxEntitiesRadius)).ToDouble(100),
                    StackItems = iniParser.Get(section, nameof(BotSpawnerConfig.StackItems)).ToBoolean(),
                    SpawnInsideInventory = iniParser.Get(section, nameof(BotSpawnerConfig.SpawnInsideInventory)).ToBoolean(),
                    DamageAmount = (float)iniParser.Get(section, nameof(BotSpawnerConfig.DamageAmount)).ToDouble(),
                    Repair = iniParser.Get(section, nameof(BotSpawnerConfig.Repair)).ToBoolean(),
                    MinHealthPercentage = (float)iniParser.Get(section, nameof(BotSpawnerConfig.MinHealthPercentage)).ToDouble(),
                    MaxHealthPercentage = (float)iniParser.Get(section, nameof(BotSpawnerConfig.MaxHealthPercentage)).ToDouble(),
                    MinHeight = iniParser.Get(section, nameof(BotSpawnerConfig.MinHeight)).ToDouble(),
                    MaxHeight = iniParser.Get(section, nameof(BotSpawnerConfig.MaxHeight)).ToDouble(),
                    MinRadius = iniParser.Get(section, nameof(BotSpawnerConfig.MinRadius)).ToDouble(),
                    MaxRadius = iniParser.Get(section, nameof(BotSpawnerConfig.MaxRadius)).ToDouble(),
                    SpawnTriggerInterval = iniParser.Get(section, nameof(BotSpawnerConfig.SpawnTriggerInterval)).ToInt32(),
                    EnableAirtightAndOxygen = iniParser.Get(section, nameof(BotSpawnerConfig.EnableAirtightAndOxygen)).ToBoolean(),
                    RequiredEntity = iniParser.Get(section, nameof(BotSpawnerConfig.RequiredEntity)).ToString(),
                    RequiredEntityRadius = iniParser.Get(section, nameof(BotSpawnerConfig.RequiredEntityRadius)).ToDouble(),
                    RequiredEntityNumber = iniParser.Get(section, nameof(BotSpawnerConfig.RequiredEntityNumber)).ToInt32(),
                    RequireEntityNumberForTotalEntities = iniParser.Get(section, nameof(BotSpawnerConfig.RequireEntityNumberForTotalEntities)).ToBoolean(),
                    RequireEntityCenterOn = iniParser.Get(section, nameof(BotSpawnerConfig.RequireEntityCenterOn)).ToBoolean()
                };

                botSpawnerConfig.EntityID.AddRange(iniParser.Get(section, nameof(BotSpawnerConfig.EntityID)).ToString().Split(','));
                botSpawnerConfig.ItemTypes.AddRange(iniParser.Get(section, nameof(BotSpawnerConfig.ItemTypes)).ToString().Split(','));
                botSpawnerConfig.ItemIds.AddRange(iniParser.Get(section, nameof(BotSpawnerConfig.ItemIds)).ToString().Split(','));
                botSpawnerConfig.RequiredItemTypes.AddRange(iniParser.Get(section, nameof(BotSpawnerConfig.RequiredItemTypes)).ToString().Split(','));
                botSpawnerConfig.RequiredItemIds.AddRange(iniParser.Get(section, nameof(BotSpawnerConfig.RequiredItemIds)).ToString().Split(','));
                botSpawnerConfig.RequiredItemAmounts.AddRange(iniParser.Get(section, nameof(BotSpawnerConfig.RequiredItemAmounts)).ToString().Split(',').Select(int.Parse));
                botSpawnerConfig.PermanentRequiredItemTypes.AddRange(iniParser.Get(section, nameof(BotSpawnerConfig.PermanentRequiredItemTypes)).ToString().Split(','));
                botSpawnerConfig.PermanentRequiredItemIds.AddRange(iniParser.Get(section, nameof(BotSpawnerConfig.PermanentRequiredItemIds)).ToString().Split(','));
                botSpawnerConfig.PermanentRequiredItemAmounts.AddRange(iniParser.Get(section, nameof(BotSpawnerConfig.PermanentRequiredItemAmounts)).ToString().Split(',').Select(int.Parse));
                CESsettings.BlockSpawnSettings.Add(botSpawnerConfig);
            }
        }
    }
}

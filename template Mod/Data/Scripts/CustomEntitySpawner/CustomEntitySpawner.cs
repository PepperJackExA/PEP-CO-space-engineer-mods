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

namespace PEPCO.iSurvival.CustomEntitySpawner
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class CustomEntitySpawner : MySessionComponentBase
    {
        private int updateTickCounter = 0;
        private long totalUpdateTicks = 0;
        private bool isLoading = true;
        private int loadingTickCount = 100;
        public bool scriptPaused = false;
        private static readonly Random randomGenerator = new Random();
        private int cleanupTickCounter = 0;

        private const string ModDataFile = "CES.ini";
        private const string WorldStorageFolder = "CustomEntitySpawner";
        private const string WorldStorageFile = "CES.ini";

        private static readonly Dictionary<string, Type> itemTypeMappings = new Dictionary<string, Type>
        {
            { "MyObjectBuilder_ConsumableItem", typeof(MyObjectBuilder_ConsumableItem) },
            { "MyObjectBuilder_Ore", typeof(MyObjectBuilder_Ore) },
            { "MyObjectBuilder_Ingot", typeof(MyObjectBuilder_Ingot) },
            { "MyObjectBuilder_Component", typeof(MyObjectBuilder_Component) }
        };

        public static CustomEntitySpawnerSettings settings = new CustomEntitySpawnerSettings();
        public static CustomEntitySpawnerChat chat = new CustomEntitySpawnerChat();
        public static PEPCO_LogError log = new PEPCO_LogError();
        public static HashSet<string> validBotIds = new HashSet<string>();

        private const string DefaultGlobalIniContent = @"
; ==============================================
; HOW TO USE GlobalConfig.ini
; ==============================================C
;BaseUpdateInterval 60 = 1 second
;EnableLogging will enable the storage log file and some ingame messages
;CleanupInterval 60 = 1 second
;GlobalMaxEntities Required Items will be lost if this is not set lower then server Max entities setting.
[Config]
BaseUpdateInterval=60    
EnableLogging=false
CleanupInterval=0
GlobalMaxEntities=30
";

        private const string DefaultEntitySpawnerIniContent = @"
; ==============================================
; HOW TO USE CustomEntitySpawner.ini
; ==============================================
; This file configures the spawning behavior of entities around specific block types.
; Each section must be unique.
;[SomethingUniqueHere]
;
; Block settings
; BlockId specifies the unique identifier for the block.
; Example: SmallBlockSmallContainer for a small cargo container.
; List of options: Any valid block ID.
;BlockId=SmallBlockSmallContainer
;
; BlockType specifies the type of block for which this configuration applies.
; Example: MyObjectBuilder_CargoContainer for cargo containers.
; List of options: MyObjectBuilder_CargoContainer, MyObjectBuilder_Refinery, MyObjectBuilder_Assembler, etc.
;BlockType=MyObjectBuilder_CargoContainer
;
; Enabled specifies whether this configuration is active.
; Values: true or false
; Example: true
;Enabled=true
;
; PlayerDistanceCheck is the maximum distance from a player for spawning entities.
; Example: 100 (100 meters)
; List of options: Any positive integer, -1 to disable the check.
;PlayerDistanceCheck=100
;
; Entity spawning settings
; EnableEntitySpawning specifies whether entities should be spawned.
; Values: true or false
;EnableEntitySpawning=true
;
; EntityID specifies the ID of the entities to spawn.
; Example: Wolf
; List of options: Any valid entity ID such as Wolf, Spider, etc.
;EntityID=Wolf
;
; MinEntityAmount is the minimum number of entities to spawn when conditions are met.
; Example: 1
; List of options: Any positive integer.
;MinEntityAmount=1
;
; MaxEntityAmount is the maximum number of entities to spawn when conditions are met.
; Example: 1
; List of options: Any positive integer.
;MaxEntityAmount=1
;
; MaxEntitiesInArea is the maximum number of entities allowed in the area for spawning.
; Example: 30
; List of options: Any positive integer.
;MaxEntitiesInArea=30
;
; MaxEntitiesRadius is the radius (in meters) within which the MaxEntitiesInArea limit is checked.
; This radius is spherical, meaning it is measured in 3D space.
; Example: 100 (100 meters)
; List of options: Any positive float value.
;MaxEntitiesRadius=100
;
; Item spawning settings
; EnableItemSpawning specifies whether items should be spawned.
; Values: true or false
;EnableItemSpawning=true
;
; ItemTypes specifies the types of items to spawn.
; Example: MyObjectBuilder_Component
; List of options: MyObjectBuilder_Component, MyObjectBuilder_Ore, MyObjectBuilder_Ingot, MyObjectBuilder_ConsumableItem, etc.
;ItemTypes=MyObjectBuilder_Component
;
; ItemIds specifies the IDs of the items to spawn.
; Example: SteelPlate
; List of options: Any valid item ID such as SteelPlate, Iron, etc.
;ItemIds=SteelPlate
;
; MinItemAmount is the minimum number of items to spawn when conditions are met.
; Example: 1
; List of options: Any positive integer.
;MinItemAmount=1
;
; MaxItemAmount is the maximum number of items to spawn when conditions are met.
; Example: 1
; List of options: Any positive integer.
;MaxItemAmount=1
;
; UseWeightedDrops determines if the number of items spawned should use weighted probabilities.
; Values: true or false
; Example: false
;UseWeightedDrops=false
;
; StackItems specifies whether items should be stacked when spawned.
; Values: true or false
;StackItems=false
;
; SpawnInsideInventory specifies whether items should be spawned inside the inventory of the block.
; Values: true or false
;SpawnInsideInventory=false
;
; SpawnItemsWithEntities specifies whether items should be spawned only when an entity is spawned.
; Values: true or false
;SpawnItemsWithEntities=true
;
; Environmental conditions
; DamageAmount is the amount of damage to apply to the block each time entities are spawned.
; Example: 0
; List of options: Any non-negative float value.
;DamageAmount=0
;
; MinHealthPercentage is the minimum health percentage the block must have to allow spawning.
; Example: 0.2 (20%)
; List of options: Any float value between 0 and 1.
;MinHealthPercentage=0.2
;
; MaxHealthPercentage is the maximum health percentage the block can have to allow spawning.
; Example: 1 (100%)
; List of options: Any float value between 0 and 1.
;MaxHealthPercentage=1
;
; MinHeight is the minimum height offset for spawning entities.
; Example: 0.5
; List of options: Any non-negative float value.
;MinHeight=0.5
;
; MaxHeight is the maximum height offset for spawning entities.
; Example: 2.0
; List of options: Any non-negative float value.
;MaxHeight=2.0
;
; MinRadius is the minimum radius for spawning entities around the block.
; Example: 0.5
; List of options: Any non-negative float value.
;MinRadius=0.5
;
; MaxRadius is the maximum radius for spawning entities around the block.
; Example: 2.0
; List of options: Any non-negative float value.
;MaxRadius=2.0
;
; SpawnTriggerInterval is the interval in update ticks for triggering entity spawn.
; Example: 3 (every 3 updates)
; List of options: Any positive integer.
;SpawnTriggerInterval=3
;
; EnableAirtightAndOxygen determines if airtight and oxygen levels are considered for spawning.
; Values: true or false
; Example: false
;EnableAirtightAndOxygen=false
;
; Required items for spawning (to be removed)
; RequiredItemTypes specifies the types of items required in the inventory for spawning (to be removed).
; Example: MyObjectBuilder_Component
; List of options: MyObjectBuilder_Component, MyObjectBuilder_Ore, MyObjectBuilder_Ingot, MyObjectBuilder_ConsumableItem, etc.
;RequiredItemTypes=MyObjectBuilder_Component,MyObjectBuilder_Component
;
; RequiredItemIds specifies the IDs of the required items (to be removed).
; Example: SteelPlate
; List of options: Any valid item ID such as SteelPlate, Iron, etc.
;RequiredItemIds=SteelPlate,InteriorPlate
;
; RequiredItemAmounts specifies the amounts of the required items (to be removed).
; Example: 5
; List of options: Any positive integer.
;RequiredItemAmounts=1,1
;
; Permanent required items for spawning (not removed)
; PermanentRequiredItemTypes specifies the types of items required in the inventory for spawning (not removed).
; Example: MyObjectBuilder_Ore
; List of options: MyObjectBuilder_Component, MyObjectBuilder_Ore, MyObjectBuilder_Ingot, MyObjectBuilder_ConsumableItem, etc.
;PermanentRequiredItemTypes=MyObjectBuilder_Ore,MyObjectBuilder_Ore
;
; PermanentRequiredItemIds specifies the IDs of the permanent required items (not removed).
; Example: Iron
; List of options: Any valid item ID such as Iron, SteelPlate, etc.
;PermanentRequiredItemIds=Iron,Gold
;
; PermanentRequiredItemAmounts specifies the amounts of the permanent required items (not removed).
; Example: 10
; List of options: Any positive integer.
;PermanentRequiredItemAmounts=1,1
;
; Required entities in the vicinity for spawning
; RequiredEntity specifies the entity type required in the vicinity for spawning.
; Example: Wolf
; List of options: Any valid entity ID such as Wolf, Spider, etc.
;RequiredEntity=Wolf
;
; RequiredEntityRadius is the radius within which the required entity must be present.
; Example: 10 (10 meters)
; List of options: Any positive float value.
;RequiredEntityRadius=10
;
; RequiredEntityNumber is the number of required entities needed for spawning.
; Example: 0 (no specific number required)
; List of options: Any positive integer.
;RequiredEntityNumber=0
;
; RequireEntityNumberForTotalEntities determines if the required entity number is for the total entities.
; Values: true or false
; Example: false
;RequireEntityNumberForTotalEntities=false
;
; MaxEntitiesInArea is the maximum number of entities allowed in the area for spawning.
; Example: 30
; List of options: Any positive integer.
;MaxEntitiesInArea=30
";

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            if (!MyAPIGateway.Session.IsServer) return;

            MyAPIGateway.Utilities.ShowMessage("start", "Init");
            EnsureDefaultIniFilesExist();
            CopyAllCESFilesToWorldStorage();
            LoadAllFilesFromWorldStorage();
            settings.Load();
            InitializeBotSpawnerConfig();
            MyAPIGateway.Utilities.MessageEntered += chat.OnMessageEntered;
        }

        public void EnsureDefaultIniFilesExist()
        {
            CreateIniFileIfNotExists(CustomEntitySpawnerSettings.GlobalFileName, DefaultGlobalIniContent);
            CreateIniFileIfNotExists(CustomEntitySpawnerSettings.EntitySpawnerFileName, DefaultEntitySpawnerIniContent);
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
            if (MyAPIGateway.Session.IsServer)
            {
                MyAPIGateway.Utilities.MessageEntered -= chat.OnMessageEntered;
                validBotIds.Clear();
            }
            base.UnloadData();
        }

        public override void UpdateBeforeSimulation()
        {
            if (!MyAPIGateway.Session.IsServer) return;

            if (isLoading && loadingTickCount-- > 0) return;
            isLoading = false;
            totalUpdateTicks++;

            if (++updateTickCounter >= settings.BaseUpdateInterval)
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

            if (settings.CleanupInterval != 0 && ++cleanupTickCounter >= settings.CleanupInterval)
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
            scriptPaused = !scriptPaused;
            MyAPIGateway.Utilities.ShowMessage("CES", $"Script is now {(scriptPaused ? "paused" : "resumed")}.");
        }

        private void SpawnEntitiesNearBlocks(ref int entitiesSpawned)
        {
            if (scriptPaused) return;

            long baseUpdateCycles = totalUpdateTicks / settings.BaseUpdateInterval;
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
                        foreach (var blockSettings in settings.BlockSpawnSettings)
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
                if (currentGlobalEntityCount >= settings.GlobalMaxEntities)
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
                var requiredItemType = new MyDefinitionId(itemTypeMappings[blockSettings.RequiredItemTypes[i]], blockSettings.RequiredItemIds[i]);
                var itemAmount = (VRage.MyFixedPoint)blockSettings.RequiredItemAmounts[i];

                if (itemAmount > 0 && !inventory.ContainItems(itemAmount, requiredItemType))
                {
                    log.LogError($"Required item missing: {requiredItemType} with amount {itemAmount} in block {block.FatBlock.DisplayName}");
                    return false;
                }
            }

            for (int i = 0; i < blockSettings.PermanentRequiredItemTypes.Count; i++)
            {
                var permanentRequiredItemType = new MyDefinitionId(itemTypeMappings[blockSettings.PermanentRequiredItemTypes[i]], blockSettings.PermanentRequiredItemIds[i]);
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
                var requiredItemType = new MyDefinitionId(itemTypeMappings[blockSettings.RequiredItemTypes[i]], blockSettings.RequiredItemIds[i]);
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
                if (itemTypeMappings.TryGetValue(itemType, out itemTypeId))
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
            var itemDefinition = new MyDefinitionId(itemTypeMappings[itemType], itemId);
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
            var itemDefinition = new MyDefinitionId(itemTypeMappings[itemType], itemId);
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
            settings.BlockSpawnSettings.Clear();
            settings.Load();

            foreach (var botSpawnerConfig in settings.BlockSpawnSettings)
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

                settings.BlockSpawnSettings.Clear();

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

            settings.BaseUpdateInterval = iniParser.Get("Config", "BaseUpdateInterval").ToInt32(60);
            settings.EnableLogging = iniParser.Get("Config", "EnableLogging").ToBoolean(false);
            settings.CleanupInterval = iniParser.Get("Config", "CleanupInterval").ToInt32(180);
            settings.GlobalMaxEntities = iniParser.Get("Config", "GlobalMaxEntities").ToInt32(100);
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
                settings.BlockSpawnSettings.Add(botSpawnerConfig);
            }
        }
    }
}

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
        private const string MOD_NAME = "Custom Entity Spawner";

        public HashSet<string> validBotIds = new HashSet<string>();

        private int updateTickCounter = 0;
        private long totalUpdateTicks = 0;
        private bool isLoading = true;
        private int loadingTickCount = 100;
        private static readonly Random randomGenerator = new Random();

        public Dictionary<string, Type> itemTypeMappings = new Dictionary<string, Type>
        {
            { "MyObjectBuilder_ConsumableItem", typeof(MyObjectBuilder_ConsumableItem) },
            { "MyObjectBuilder_Ore", typeof(MyObjectBuilder_Ore) },
            { "MyObjectBuilder_Ingot", typeof(MyObjectBuilder_Ingot) },
            { "MyObjectBuilder_Component", typeof(MyObjectBuilder_Component) }
        };

        // Global Settings

        public int BaseUpdateInterval { get; set; } = 60;
        public bool EnableLogging { get; set; } = true;
        public int CleanupInterval { get; set; } = 18000;
        public int GlobalMaxEntities { get; set; } = 32;
        
        

        public string DefaultGlobalIniContent = @"
; ==============================================
; HOW TO USE GlobalConfig.ini
; ==============================================
;BaseUpdateInterval 60 = 1 second
;EnableLogging will enable the storage file and some ingame messages
;CleanupInterval 60 = 1 second
;GlobalMaxEntities Required Items will be lost if this is not set lower than server Max entities setting.
[Config]
BaseUpdateInterval=60    
EnableLogging=true
CleanupInterval=0
GlobalMaxEntities=30
";

        //END Global Settings

        // block Settings
        public string BlockId { get; set; }
        public string BlockType { get; set; }
        public List<string> EntityID { get; set; } = new List<string>();
        public List<string> ItemTypes { get; set; } = new List<string>();
        public List<string> ItemIds { get; set; } = new List<string>();
        public bool StackItems { get; set; } = false;
        public bool SpawnInsideInventory { get; set; } = false;
        public bool EnableEntitySpawning { get; set; } = true;
        public bool EnableItemSpawning { get; set; } = true;
        public bool SpawnItemsWithEntities { get; set; } = false;
        public int MinEntityAmount { get; set; } = 1;
        public int MaxEntityAmount { get; set; } = 1;
        public int MinItemAmount { get; set; } = 1;
        public int MaxItemAmount { get; set; } = 1;
        public bool UseWeightedDrops { get; set; } = false;
        public float DamageAmount { get; set; } = 0;
        public bool Repair { get; set; } = false;
        public float MinHealthPercentage { get; set; } = 0.1f;
        public float MaxHealthPercentage { get; set; } = 1.0f;
        public double MinHeight { get; set; } = 1.0;
        public double MaxHeight { get; set; } = 2.0;
        public double MinRadius { get; set; } = 0.5;
        public double MaxRadius { get; set; } = 2.0;
        public int SpawnTriggerInterval { get; set; } = 10;
        public bool EnableAirtightAndOxygen { get; set; } = false;
        public bool Enabled { get; set; } = true;
        public List<string> RequiredItemTypes { get; set; } = new List<string>();
        public List<string> RequiredItemIds { get; set; } = new List<string>();
        public List<int> RequiredItemAmounts { get; set; } = new List<int>();
        public List<string> PermanentRequiredItemTypes { get; set; } = new List<string>();
        public List<string> PermanentRequiredItemIds { get; set; } = new List<string>();
        public List<int> PermanentRequiredItemAmounts { get; set; } = new List<int>();
        public int PlayerDistanceCheck { get; set; } = 1000;
        public string RequiredEntity { get; set; } = "";
        public double RequiredEntityRadius { get; set; } = 10;
        public int RequiredEntityNumber { get; set; } = 0;
        public bool RequireEntityNumberForTotalEntities { get; set; } = false;
        public int MaxEntitiesInArea { get; set; } = 10;
        public double MaxEntitiesRadius { get; set; } = 100;
        public bool RequireEntityCenterOn { get; set; } = false;

        public List<CustomEntitySpawner> BlockSpawnSettings { get; set; } = new List<CustomEntitySpawner>();

        public string DefaultEntitySpawnerIniContent = @"
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

        

        

        // End block Settings

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            LogError($"Starting Init IsDedicated: {MyAPIGateway.Utilities.IsDedicated} Server: {MyAPIGateway.Session.IsServer}");
            if (MyAPIGateway.Utilities.IsDedicated)
            {
                // Only run on dedicated server
                LogError("Only run on dedicated server");
                LoadOnHost();
            }
            else if (MyAPIGateway.Session.IsServer)
            {
                // Run in solo game
                LogError("Run in solo game");
                LoadOnHost();
                LoadOnClient();
            }
            else
            {
                // Load on client in multiplayer
                
            }


        }

        void LoadOnHost()
        {
            LogError("Starting LoadOnHost");
            var iniParser = new MyIni();
            EnsureDefaultIniFilesExist();            
            //CopyAllCESFilesToWorldStorage();
            LoadAllFilesFromWorldStorage();
            Load();
            

        }

        void LoadOnClient()
        {
            LogError("Starting LoadOnClient");
            try
            {
                InitializeBotSpawnerConfig();
                LoadValidBotIds();
                MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
            }
            catch
            {

            }
        }

        public void LoadValidBotIds()
        {
            LogError("Starting LoadValidBotIds");
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
            LogError("Starting ListValidBotIds");
            MyAPIGateway.Utilities.ShowMessage("BotSpawner", "ListValidBotIds");
            foreach (var botId in validBotIds)
            {
                MyAPIGateway.Utilities.ShowMessage("BotSpawner", botId);
            }
        }

        public void EnsureDefaultIniFilesExist()
        {
            LogError("Starting EnsureDefaultIniFilesExist");
            CreateIniFileIfNotExists(GlobalFileName, DefaultGlobalIniContent);
            CreateIniFileIfNotExists(EntitySpawnerFileName, DefaultEntitySpawnerIniContent);
        }

        private void CreateIniFileIfNotExists(string FileName, string content)
        {
            LogError("Starting CreateIniFileIfNotExists");
            if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(FileName, typeof(CustomEntitySpawner)))
            {
                using (var file = MyAPIGateway.Utilities.WriteFileInWorldStorage(FileName, typeof(CustomEntitySpawner)))
                {
                    file.Write(content);
                }
            }
        }
        protected override void UnloadData()
        {
            LogError("Starting UnloadData");
            if (MyAPIGateway.Multiplayer.IsServer)
            {
                MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
                validBotIds.Clear();
            }
            base.UnloadData();
        }

        public override void UpdateBeforeSimulation()
        {            

            if (isLoading && loadingTickCount-- > 0) return;
            isLoading = false;
            totalUpdateTicks++;

            if (++updateTickCounter >= BaseUpdateInterval)
            {
                updateTickCounter = 0;
                try
                {
                    if (MyAPIGateway.Utilities.IsDedicated)
                    {
                        // Only run on dedicated server
                        int entitiesSpawned = 0;
                        SpawnEntitiesNearBlocks(ref entitiesSpawned);
                    }
                    else if (MyAPIGateway.Session.IsServer)
                    {
                        // Run in solo game
                        int entitiesSpawned = 0;
                        SpawnEntitiesNearBlocks(ref entitiesSpawned);
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Update error: {ex.Message}");
                }
            }
          
        }

        public void PauseScript()
        {
            LogError("Starting PauseScript");
            scriptPaused = !scriptPaused;
            MyAPIGateway.Utilities.ShowMessage("CES", $"Script is now {(scriptPaused ? "paused" : "resumed")}.");
        }

        private void SpawnEntitiesNearBlocks(ref int entitiesSpawned)
        {
            LogError("Starting SpawnEntitiesNearBlocks");
            if (scriptPaused) return;

            long baseUpdateCycles = totalUpdateTicks / BaseUpdateInterval;
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            var entities = MyEntities.GetEntities();

            foreach (var entity in entities)
            {
                var grid = entity as IMyCubeGrid;
                if (grid != null)
                {
                    var blocks = new List<IMySlimBlock>();
                    grid.GetBlocks(blocks, b => b.FatBlock != null);

                    foreach (var block in blocks)
                    {
                        foreach (var blockSettings in BlockSpawnSettings)
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

        private bool IsValidBlockForSpawning(IMySlimBlock block, CustomEntitySpawner blockSettings, long baseUpdateCycles, List<IMyPlayer> players)
        {
            LogError("Starting IsValidBlockForSpawning");
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

        private bool IsEnvironmentSuitable(IMySlimBlock block, CustomEntitySpawner blockSettings)
        {
            LogError("Starting IsEnvironmentSuitable");
            var grid = block.CubeGrid;

            if (blockSettings.RequiredEntityNumber > 0)
            {
                int entityCount = GetEntityCountInRadius(block.FatBlock.GetPosition(), blockSettings.MaxEntitiesRadius, "All", true);
                if (entityCount >= blockSettings.MaxEntitiesInArea)
                {
                    LogError($"Entity spawn limit reached: {entityCount} entities within radius {blockSettings.MaxEntitiesRadius}");
                    return false;
                }
            }

            if (blockSettings.EnableAirtightAndOxygen)
            {
                bool isAirtight = grid.IsRoomAtPositionAirtight(block.FatBlock.Position);
                double oxygenLevel = MyAPIGateway.Session.OxygenProviderSystem.GetOxygenInPoint(block.FatBlock.GetPosition());
                if (!isAirtight && oxygenLevel <= 0.5)
                {
                    LogError($"Airtight and oxygen conditions not met for {block.FatBlock.BlockDefinition.SubtypeId}");
                    return false;
                }
            }

            return true;
        }

        private void ProcessBlockSpawning(IMySlimBlock block, CustomEntitySpawner blockSettings, ref int entitiesSpawned)
        {
            LogError("Starting ProcessBlockSpawning");
            bool entitiesSpawnedThisCycle = false;
            
            if (blockSettings.EnableEntitySpawning)
            {
                int currentGlobalEntityCount = GetTotalEntityCount();
                if (currentGlobalEntityCount >= GlobalMaxEntities)
                {
                    LogError($"Entity global limit reached: {currentGlobalEntityCount} entities");
                    return;
                }

                int currentEntityCount = GetEntityCountInRadius(block.FatBlock.GetPosition(), blockSettings.MaxEntitiesRadius, "All", true);
                if (currentEntityCount >= blockSettings.MaxEntitiesInArea)
                {
                    LogError($"Entity spawn limit reached: {currentEntityCount} entities within radius {blockSettings.MaxEntitiesRadius}");
                    return;
                }
                RemoveItemsFromInventory(block, blockSettings);
                SpawnEntitiesAndApplyDamage(block, blockSettings, ref entitiesSpawned, ref entitiesSpawnedThisCycle);
            }

            if (blockSettings.EnableItemSpawning)
            {
                RemoveItemsFromInventory(block, blockSettings);
                SpawnItemsAndApplyDamage(block, blockSettings);
            }
        }

        private void SpawnEntitiesAndApplyDamage(IMySlimBlock block, CustomEntitySpawner blockSettings, ref int entitiesSpawned, ref bool entitiesSpawnedThisCycle)
        {
            LogError("Starting SpawnEntitiesAndApplyDamage");
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

        private void SpawnItemsAndApplyDamage(IMySlimBlock block, CustomEntitySpawner blockSettings)
        {
            LogError("Starting SpawnItemsAndApplyDamage");
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

        private void ApplyDamageToBlock(IMySlimBlock block, CustomEntitySpawner blockSettings, int amount)
        {
            LogError("Starting ApplyDamageToBlock");
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

        private bool AreRequiredEntitiesInVicinity(IMySlimBlock block, CustomEntitySpawner blockSettings)
        {
            LogError("Starting AreRequiredEntitiesInVicinity");
            if (blockSettings.RequiredEntityNumber > 0)
            {
                int entityCount = GetEntityCountInRadius(block.FatBlock.GetPosition(), blockSettings.RequiredEntityRadius, blockSettings.RequiredEntity, true);
                return entityCount >= blockSettings.RequiredEntityNumber;
            }
            return true;
        }

        private int GetEntityCountInRadius(Vector3D position, double radius, string requiredEntitySubtypeId, bool excludeDeadEntities = false)
        {
            LogError("Starting GetEntityCountInRadius");
            var entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities, e => e is IMyCharacter);

            int entityCount = 0;
            foreach (var entity in entities)
            {
                var character = entity as IMyCharacter;
                if (character != null)
                {
                    if (requiredEntitySubtypeId.Equals("All"))
                    {
                        if (!excludeDeadEntities || !character.IsDead)
                        {
                            if (Vector3D.Distance(entity.GetPosition(), position) <= radius)
                            {
                                entityCount++;
                            }
                        }
                    }
                    else
                    {
                        string entityCurrentSubtypeId = (character as MyEntity)?.DefinitionId?.SubtypeId.ToString();
                        if (entityCurrentSubtypeId != null && entityCurrentSubtypeId.Equals(requiredEntitySubtypeId, StringComparison.OrdinalIgnoreCase) &&
                            Vector3D.Distance(entity.GetPosition(), position) <= radius)
                        {
                            if (!excludeDeadEntities || !character.IsDead)
                            {
                                entityCount++;
                            }
                        }
                    }
                }
            }
            return entityCount;
        }



        private int GetTotalEntityCount()
        {
            LogError("Starting GetTotalEntityCount");
            var entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities, entity =>
            {
                var character = entity as IMyCharacter;
                return character != null && !character.IsDead;
            });
            return entities.Count;
        }


        private bool IsPlayerInRange(IMySlimBlock block, List<IMyPlayer> players, int playerDistanceCheck)
        {
            LogError("Starting IsPlayerInRange");
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

        private bool CheckInventoryForRequiredItems(IMySlimBlock block, CustomEntitySpawner blockSettings)
        {
            LogError("Starting CheckInventoryForRequiredItems");
            var inventory = block.FatBlock.GetInventory() as IMyInventory;
            if (inventory == null) return true;

            for (int i = 0; i < blockSettings.RequiredItemTypes.Count; i++)
            {
                var requiredItemType = new MyDefinitionId(itemTypeMappings[blockSettings.RequiredItemTypes[i]], blockSettings.RequiredItemIds[i]);
                var itemAmount = (VRage.MyFixedPoint)blockSettings.RequiredItemAmounts[i];

                if (itemAmount > 0 && !inventory.ContainItems(itemAmount, requiredItemType))
                {
                    LogError($"Required item missing: {requiredItemType} with amount {itemAmount} in block {block.FatBlock.DisplayName}");
                    return false;
                }
            }

            for (int i = 0; i < blockSettings.PermanentRequiredItemTypes.Count; i++)
            {
                var permanentRequiredItemType = new MyDefinitionId(itemTypeMappings[blockSettings.PermanentRequiredItemTypes[i]], blockSettings.PermanentRequiredItemIds[i]);
                var permanentItemAmount = (VRage.MyFixedPoint)blockSettings.PermanentRequiredItemAmounts[i];

                if (permanentItemAmount > 0 && !inventory.ContainItems(permanentItemAmount, permanentRequiredItemType))
                {
                    LogError($"Permanent required item missing: {permanentRequiredItemType} with amount {permanentItemAmount} in block {block.FatBlock.DisplayName}");
                    return false;
                }
            }

            return true;
        }

        private void RemoveItemsFromInventory(IMySlimBlock block, CustomEntitySpawner blockSettings)
        {
            LogError("Starting RemoveItemsFromInventory");
            var inventory = block.FatBlock.GetInventory() as IMyInventory;
            if (inventory == null)
            {
                LogError("Inventory not found for block.");
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
                        LogError($"Removed {totalAmountToRemove} of {requiredItemType} from inventory.");
                    }
                    else
                    {
                        LogError($"Insufficient items: {totalAmountToRemove} of {requiredItemType} in inventory.");
                    }
                }
            }
        }

        private void SpawnEntities(IMySlimBlock block, CustomEntitySpawner Entitysettings, int spawnAmount)
        {
            LogError("Starting SpawnEntities");
            if (Entitysettings.EnableEntitySpawning)
            {
                foreach (var entityID in Entitysettings.EntityID)
                {
                    for (int j = 0; j < spawnAmount; j++)
                    {
                        int entityAmount = randomGenerator.Next(Entitysettings.MinEntityAmount, Entitysettings.MaxEntityAmount + 1);
                        Vector3D spawnPosition = CalculateDropPosition(block, Entitysettings);
                        MyVisualScriptLogicProvider.SpawnBot(entityID, spawnPosition);
                        if (Entitysettings.SpawnItemsWithEntities)
                        {
                            SpawnItems(block, Entitysettings);
                        }
                    }
                }
            }
        }

        private void SpawnItems(IMySlimBlock block, CustomEntitySpawner Itemsettings, Vector3D? position = null)
        {
            LogError("Starting SpawnItems");
            foreach (var itemType in Itemsettings.ItemTypes)
            {
                var itemId = Itemsettings.ItemIds[Itemsettings.ItemTypes.IndexOf(itemType)];
                int amount = Itemsettings.UseWeightedDrops
                    ? GetWeightedRandomNumber(Itemsettings.MinItemAmount, GenerateProbabilities(Itemsettings.MinItemAmount, Itemsettings.MaxItemAmount))
                    : randomGenerator.Next(Itemsettings.MinItemAmount, Itemsettings.MaxItemAmount + 1);

                if (Itemsettings.SpawnInsideInventory)
                {
                    PlaceItemInCargo(block, itemType, itemId, amount, Itemsettings.StackItems);
                }
                else
                {
                    if (position.HasValue)
                    {
                        SpawnItemNearPosition(position.Value, itemType, itemId, amount, Itemsettings.StackItems, Itemsettings);
                    }
                    else
                    {
                        SpawnItemNearBlock(block, itemType, itemId, amount, Itemsettings.StackItems, Itemsettings);
                    }
                }
            }
        }

        private void PlaceItemInCargo(IMySlimBlock block, string itemType, string itemId, int amount, bool stackItems)
        {
            LogError("Starting PlaceItemInCargo");
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

        private void SpawnItemNearBlock(IMySlimBlock block, string itemType, string itemId, int amount, bool stackItems, CustomEntitySpawner settings)
        {
            LogError("Starting SpawnItemNearBlock");
            var itemDefinition = new MyDefinitionId(settings.itemTypeMappings[itemType], itemId);
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

        private Vector3D CalculateDropPosition(IMySlimBlock block, CustomEntitySpawner settings)
        {
            LogError("Starting CalculateDropPosition");
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
        public string GlobalFileName = "GlobalConfig.ini";
        public string EntitySpawnerFileName = "CustomEntitySpawner.ini";
        private string IniSection = "Config";

        public bool scriptPaused = false;


        public void UpdateBlockSettings(long entityId, CustomEntitySpawner blockSettings)
        {
            LogError("Starting UpdateBlockSettings");
            var existingSetting = BlockSpawnSettings.FirstOrDefault(s => s.BlockId == blockSettings.BlockId);
            if (existingSetting != null)
            {
                existingSetting = blockSettings;
            }
            else
            {
                BlockSpawnSettings.Add(blockSettings);
            }
        }

        public void Load()
        {
            if (scriptPaused) return;
            LogError($"Starting Load()");
            try
            {
                MyIni iniParser = new MyIni();

                LoadIniFile(GlobalFileName, iniParser);
                BaseUpdateInterval = iniParser.Get(IniSection, nameof(BaseUpdateInterval)).ToInt32(BaseUpdateInterval);
                EnableLogging = iniParser.Get(IniSection, nameof(EnableLogging)).ToBoolean(EnableLogging);
                CleanupInterval = iniParser.Get(IniSection, nameof(CleanupInterval)).ToInt32(CleanupInterval);
                GlobalMaxEntities = iniParser.Get(IniSection, nameof(GlobalMaxEntities)).ToInt32(GlobalMaxEntities);

                LoadIniFile(EntitySpawnerFileName, iniParser);
                LoadBlockSpawnSettings(iniParser);

                foreach (var modItem in MyAPIGateway.Session.Mods)
                {
                    string cesFileName = $"{modItem.PublishedFileId}_CES.ini";
                    if (MyAPIGateway.Utilities.FileExistsInWorldStorage(cesFileName, typeof(CustomEntitySpawner)))
                    {
                        LoadIniFile(cesFileName, iniParser);
                        LoadBlockSpawnSettings(iniParser);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Load Error: {ex.Message}");

            }
        }

        private void LoadIniFile(string fileName, MyIni iniParser)
        {
            LogError("Starting LoadIniFile");
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, typeof(CustomEntitySpawner)))
            {
                using (TextReader file = MyAPIGateway.Utilities.ReadFileInWorldStorage(fileName, typeof(CustomEntitySpawner)))
                {
                    string text = file.ReadToEnd();
                    MyIniParseResult result;
                    if (!iniParser.TryParse(text, out result))
                        throw new Exception($"Config error in {fileName}: {result}");
                }
            }
            else
            {
                throw new Exception($"{fileName} not found.");
            }
        }

        private void LoadBlockSpawnSettings(MyIni iniParser)
        {
            LogError("Starting LoadBlockSpawnSettings");
            List<string> sections = new List<string>();
            iniParser.GetSections(sections);

            foreach (var section in sections)
            {
                if (section == IniSection)
                    continue;

                var blockSettings = new CustomEntitySpawner
                {
                    BlockId = iniParser.Get(section, nameof(CustomEntitySpawner.BlockId)).ToString(section),
                    BlockType = iniParser.Get(section, nameof(CustomEntitySpawner.BlockType)).ToString(),
                    Enabled = iniParser.Get(section, nameof(CustomEntitySpawner.Enabled)).ToBoolean(),
                    PlayerDistanceCheck = iniParser.Get(section, nameof(CustomEntitySpawner.PlayerDistanceCheck)).ToInt32(1000),
                    EnableEntitySpawning = iniParser.Get(section, nameof(CustomEntitySpawner.EnableEntitySpawning)).ToBoolean(false),
                    EnableItemSpawning = iniParser.Get(section, nameof(CustomEntitySpawner.EnableItemSpawning)).ToBoolean(false),
                    SpawnItemsWithEntities = iniParser.Get(section, nameof(CustomEntitySpawner.SpawnItemsWithEntities)).ToBoolean(false),
                    MinEntityAmount = iniParser.Get(section, nameof(CustomEntitySpawner.MinEntityAmount)).ToInt32(1),
                    MaxEntityAmount = iniParser.Get(section, nameof(CustomEntitySpawner.MaxEntityAmount)).ToInt32(1),
                    MinItemAmount = iniParser.Get(section, nameof(CustomEntitySpawner.MinItemAmount)).ToInt32(1),
                    MaxItemAmount = iniParser.Get(section, nameof(CustomEntitySpawner.MaxItemAmount)).ToInt32(1),
                    UseWeightedDrops = iniParser.Get(section, nameof(CustomEntitySpawner.UseWeightedDrops)).ToBoolean(false),
                    MaxEntitiesInArea = iniParser.Get(section, nameof(CustomEntitySpawner.MaxEntitiesInArea)).ToInt32(30),
                    MaxEntitiesRadius = iniParser.Get(section, nameof(CustomEntitySpawner.MaxEntitiesRadius)).ToDouble(100),
                    StackItems = iniParser.Get(section, nameof(CustomEntitySpawner.StackItems)).ToBoolean(false),
                    SpawnInsideInventory = iniParser.Get(section, nameof(CustomEntitySpawner.SpawnInsideInventory)).ToBoolean(false),
                    DamageAmount = (float)iniParser.Get(section, nameof(CustomEntitySpawner.DamageAmount)).ToDouble(0),
                    Repair = iniParser.Get(section, nameof(CustomEntitySpawner.Repair)).ToBoolean(false),
                    MinHealthPercentage = (float)iniParser.Get(section, nameof(CustomEntitySpawner.MinHealthPercentage)).ToDouble(0),
                    MaxHealthPercentage = (float)iniParser.Get(section, nameof(CustomEntitySpawner.MaxHealthPercentage)).ToDouble(1),
                    MinHeight = iniParser.Get(section, nameof(CustomEntitySpawner.MinHeight)).ToDouble(0),
                    MaxHeight = iniParser.Get(section, nameof(CustomEntitySpawner.MaxHeight)).ToDouble(2),
                    MinRadius = iniParser.Get(section, nameof(CustomEntitySpawner.MinRadius)).ToDouble(0),
                    MaxRadius = iniParser.Get(section, nameof(CustomEntitySpawner.MaxRadius)).ToDouble(2),
                    SpawnTriggerInterval = iniParser.Get(section, nameof(CustomEntitySpawner.SpawnTriggerInterval)).ToInt32(5),
                    EnableAirtightAndOxygen = iniParser.Get(section, nameof(CustomEntitySpawner.EnableAirtightAndOxygen)).ToBoolean(false),
                    RequiredEntity = iniParser.Get(section, nameof(CustomEntitySpawner.RequiredEntity)).ToString("NA"),
                    RequiredEntityRadius = iniParser.Get(section, nameof(CustomEntitySpawner.RequiredEntityRadius)).ToDouble(100),
                    RequiredEntityNumber = iniParser.Get(section, nameof(CustomEntitySpawner.RequiredEntityNumber)).ToInt32(0),
                    RequireEntityNumberForTotalEntities = iniParser.Get(section, nameof(CustomEntitySpawner.RequireEntityNumberForTotalEntities)).ToBoolean(false),
                    RequireEntityCenterOn = iniParser.Get(section, nameof(CustomEntitySpawner.RequireEntityCenterOn)).ToBoolean(false)
                };

                blockSettings.EntityID.AddRange(iniParser.Get(section, nameof(CustomEntitySpawner.EntityID)).ToString().Split(','));
                blockSettings.ItemTypes.AddRange(iniParser.Get(section, nameof(CustomEntitySpawner.ItemTypes)).ToString().Split(','));
                blockSettings.ItemIds.AddRange(iniParser.Get(section, nameof(CustomEntitySpawner.ItemIds)).ToString().Split(','));
                blockSettings.RequiredItemTypes.AddRange(iniParser.Get(section, nameof(CustomEntitySpawner.RequiredItemTypes)).ToString().Split(','));
                blockSettings.RequiredItemIds.AddRange(iniParser.Get(section, nameof(CustomEntitySpawner.RequiredItemIds)).ToString().Split(','));
                blockSettings.RequiredItemAmounts.AddRange(iniParser.Get(section, nameof(CustomEntitySpawner.RequiredItemAmounts)).ToString().Split(',').Select(int.Parse));
                blockSettings.PermanentRequiredItemTypes.AddRange(iniParser.Get(section, nameof(CustomEntitySpawner.PermanentRequiredItemTypes)).ToString().Split(','));
                blockSettings.PermanentRequiredItemIds.AddRange(iniParser.Get(section, nameof(CustomEntitySpawner.PermanentRequiredItemIds)).ToString().Split(','));
                blockSettings.PermanentRequiredItemAmounts.AddRange(iniParser.Get(section, nameof(CustomEntitySpawner.PermanentRequiredItemAmounts)).ToString().Split(',').Select(int.Parse));
                BlockSpawnSettings.Add(blockSettings);
            }
        }
        private void CenterSpawnItemsAroundEntities(IMySlimBlock block, CustomEntitySpawner settings, int itemSpawnAmount)
        {
            LogError("Starting CenterSpawnItemsAroundEntities");
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

        private void SpawnItemNearPosition(Vector3D position, string itemType, string itemId, int amount, bool stackItems, CustomEntitySpawner settings)
        {
            LogError("Starting SpawnItemNearPosition");
            var itemDefinition = new MyDefinitionId(settings.itemTypeMappings[itemType], itemId);
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

        private Vector3D CalculateDropPosition(Vector3D basePosition, CustomEntitySpawner settings)
        {
            LogError("Starting CalculateDropPosition");
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
            LogError("Starting GenerateRandomRotation");
            return Quaternion.CreateFromYawPitchRoll(
                (float)(randomGenerator.NextDouble() * Math.PI * 2),
                (float)(randomGenerator.NextDouble() * Math.PI * 2),
                (float)(randomGenerator.NextDouble() * Math.PI * 2)
            );
        }

        private void CenterSpawnAroundEntities(IMySlimBlock block, CustomEntitySpawner settings, int spawnAmount)
        {
            LogError("Starting CenterSpawnAroundEntities");
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
            LogError("Starting GenerateProbabilities");
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
            LogError("Starting GetWeightedRandomNumber");
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
            //BlockSpawnSettings.Clear();

            LogError("Starting InitializeBotSpawnerConfig");
            foreach (var blockSettings in BlockSpawnSettings)
            {
                LogError($"Initialized settings for {blockSettings.BlockId}: " +
                         $"BlockType={blockSettings.BlockType}, " +
                         $"Enabled={blockSettings.Enabled}, " +
                         $"PlayerDistanceCheck={blockSettings.PlayerDistanceCheck}, " +
                         $"EnableEntitySpawning={blockSettings.EnableEntitySpawning}, " +
                         $"EntityID={string.Join(",", blockSettings.EntityID)}, " +
                         $"MinEntityAmount={blockSettings.MinEntityAmount}, MaxEntityAmount={blockSettings.MaxEntityAmount}, " +
                         $"MaxEntitiesInArea={blockSettings.MaxEntitiesInArea}, " +
                         $"EnableItemSpawning={blockSettings.EnableItemSpawning}, " +
                         $"ItemTypes={string.Join(",", blockSettings.ItemTypes)}, " +
                         $"ItemIds={string.Join(",", blockSettings.ItemIds)}, " +
                         $"MinItemAmount={blockSettings.MinItemAmount}, MaxItemAmount={blockSettings.MaxItemAmount}, " +
                         $"StackItems={blockSettings.StackItems}, SpawnInsideInventory={blockSettings.SpawnInsideInventory}, " +
                         $"DamageAmount={blockSettings.DamageAmount}, " +
                         $"MinHealthPercentage={blockSettings.MinHealthPercentage}, MaxHealthPercentage={blockSettings.MaxHealthPercentage}, " +
                         $"MinHeight={blockSettings.MinHeight}, MaxHeight={blockSettings.MaxHeight}, " +
                         $"MinRadius={blockSettings.MinRadius}, MaxRadius={blockSettings.MaxRadius}, " +
                         $"SpawnTriggerInterval={blockSettings.SpawnTriggerInterval}, EnableAirtightAndOxygen={blockSettings.EnableAirtightAndOxygen}, " +
                         $"RequiredItemTypes={string.Join(",", blockSettings.RequiredItemTypes)}, " +
                         $"RequiredItemIds={string.Join(",", blockSettings.RequiredItemIds)}, " +
                         $"RequiredItemAmounts={string.Join(",", blockSettings.RequiredItemAmounts)}, " +
                         $"PermanentRequiredItemTypes={string.Join(",", blockSettings.PermanentRequiredItemTypes)}, " +
                         $"PermanentRequiredItemIds={string.Join(",", blockSettings.PermanentRequiredItemIds)}, " +
                         $"PermanentRequiredItemAmounts={string.Join(",", blockSettings.PermanentRequiredItemAmounts)}, " +
                         $"RequiredEntity={blockSettings.RequiredEntity}, " +
                         $"RequiredEntityRadius={blockSettings.RequiredEntityRadius}, " +
                         $"RequiredEntityNumber={blockSettings.RequiredEntityNumber}, " +
                         $"RequireEntityNumberForTotalEntities={blockSettings.RequireEntityNumberForTotalEntities}, " +
                         $"RequireEntityCenterOn={blockSettings.RequireEntityCenterOn}");
            }

            LogError("Drop settings initialized.");
        }

        public void CopyAllCESFilesToWorldStorage()
        {
            LogError("Starting CopyAllCESFilesToWorldStorage");
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
                LogError($"Error copying files to World Storage: {ex.Message}");
            }
        }

        public void LoadAllFilesFromWorldStorage()
        {
            LogError("Starting LoadAllFilesFromWorldStorage");
            try
            {
                HashSet<string> loadedFiles = new HashSet<string>();
                List<string> knownConfigFiles = new List<string> { "GlobalConfig.ini", "CustomEntitySpawner.ini" };
                List<string> additionalFilePatterns = new List<string> { "_CES.ini" };

                BlockSpawnSettings.Clear();

                LoadKnownConfigFiles(knownConfigFiles, loadedFiles);
                try
                {
                    LoadAdditionalModConfigFiles(additionalFilePatterns, loadedFiles);
                }
                catch (Exception ex)
                {
                    LogError($"Failed: LoadAdditionalModConfigFiles: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error loading files from World Storage: {ex.Message}");
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
            LogError("Starting: LoadGlobalConfig");
            MyIni iniParser = new MyIni();
            MyIniParseResult result;
            if (!iniParser.TryParse(fileContent, out result))
            {
                throw new Exception($"Config error: {result.ToString()}");
            }

            BaseUpdateInterval = iniParser.Get("Config", "BaseUpdateInterval").ToInt32(60);
            EnableLogging = iniParser.Get("Config", "EnableLogging").ToBoolean(false);
            CleanupInterval = iniParser.Get("Config", "CleanupInterval").ToInt32(180);
            GlobalMaxEntities = iniParser.Get("Config", "GlobalMaxEntities").ToInt32(100);
        }

        public void ApplyReceivedSettings(CustomEntitySpawner config)
        {
            // UpdateBlockSettings(0, config); // Example entityId, replace with actual if necessary
        }

        private const string CommandPrefix = "/pepco";

        public void OnMessageEntered(string messageText, ref bool sendToOthers)
        {
            LogError("Starting: OnMessageEntered");
            var player = MyAPIGateway.Session.Player;
            if (player == null || !MyAPIGateway.Session.IsUserAdmin(player.SteamUserId))
            {
                MyAPIGateway.Utilities.ShowMessage("BotSpawner", "Only admins can use this command.");
                return;
            }

            var command = messageText.Split(' ');
            if (command[0].Equals(CommandPrefix, StringComparison.OrdinalIgnoreCase))
            {
                HandlePepcoCommand(command, ref sendToOthers);
            }
        }

        public void HandlePepcoCommand(string[] command, ref bool sendToOthers)
        {
            LogError("Starting: HandlePepcoCommand");
            if (command.Length < 2) return;

            if (command[1].Equals("pause", StringComparison.OrdinalIgnoreCase))
            {
                PauseScript();
                MyAPIGateway.Utilities.ShowMessage("PEPCO", $"PauseScript:{scriptPaused}");
                MyAPIGateway.Utilities.ShowMessage("PEPCO", $"Load:{scriptPaused}");

                sendToOthers = false;
                return;
            }

            if (scriptPaused)
            {
                MyAPIGateway.Utilities.ShowMessage("PEPCO", "Script is currently paused.");
                return;
            }

            switch (command[1].ToLower())
            {
                case "spawn":
                    HandleSpawnCommand(command);
                    break;
                case "listbots":
                    ListValidBotIds();
                    break;
                case "ces":
                    HandleCESCommand(command, ref sendToOthers);
                    break;
                case "kill":
                    HandleKillCommand(command, ref sendToOthers);
                    break;
                case "cleanup":
                    if (command.Length > 2 && command[2].Equals("dead", StringComparison.OrdinalIgnoreCase))
                    {
                        CleanupDeadEntities();
                        MyAPIGateway.Utilities.ShowMessage("Custom Entity Spawner", $"Cleanup Dead Entities!");
                        sendToOthers = false;
                    }
                    break;
                case "show":
                    HandleShowCommand(command, ref sendToOthers);
                    break;
                case "logging":
                    EnableLogging = !EnableLogging;
                    MyAPIGateway.Utilities.ShowMessage("Custom Entity Spawner", $"Logging is now {(EnableLogging ? "enabled" : "disabled")}.");
                    sendToOthers = false;
                    break;
                case "help":
                    ShowHelp();
                    sendToOthers = false;
                    break;
                default:
                    MyAPIGateway.Utilities.ShowMessage("PEPCO", "Unknown command. Use /pepco help for a list of commands.");
                    break;
            }
        }

        private void HandleSpawnCommand(string[] command)
        {
            LogError("Starting: HandleSpawnCommand");
            //LoadValidBotIds();  //Removed for testing if its needed.
            if (command.Length == 3)
            {
                MyAPIGateway.Utilities.ShowMessage("BotSpawner", $"command.Length");
                string botId = command[2];
                if (validBotIds.Contains(botId))
                {
                    Vector3D playerPosition = MyAPIGateway.Session.Player.GetPosition();
                    Vector3D forwardDirection = MyAPIGateway.Session.Player.Character.WorldMatrix.Forward;
                    Vector3D offset = forwardDirection * 5 * 2.5;
                    Vector3D spawnPosition = playerPosition + offset;
                    
                    MyVisualScriptLogicProvider.SpawnBot(botId, spawnPosition);
                    MyAPIGateway.Utilities.ShowMessage("BotSpawner", $"Spawned bot: {botId} at {spawnPosition}");

                    int entitiesCount = CountEntitiesInRadius(playerPosition, 10);
                    MyAPIGateway.Utilities.ShowMessage("BotSpawner", $"{entitiesCount} entities are within a 10-meter radius.");
                }
                else
                {
                    MyAPIGateway.Utilities.ShowMessage("BotSpawner", $"Invalid bot ID: {botId}");
                }
            }
        }

        private void HandleCESCommand(string[] command, ref bool sendToOthers)
        {
            LogError("Starting: HandleCESCommand");
            if (command.Length == 3)
            {
                switch (command[2].ToLower())
                {
                    case "reload":
                        ReloadSettings();
                        sendToOthers = false;
                        break;
                    case "list":
                        ListAllBlocksAndSpawns();
                        sendToOthers = false;
                        break;
                    case "update":
                        var newSettings = new CustomEntitySpawner();
                        //var packet = new PacketBlockSettings(0, newSettings); // Example entityId, replace if necessary
                        //packet.Send();
                        sendToOthers = false;
                        break;
                    default:
                        MyAPIGateway.Utilities.ShowMessage("PEPCO", "Unknown CES command. Use /pepco help for a list of commands.");
                        break;
                }
            }
        }

        private void HandleKillCommand(string[] command, ref bool sendToOthers)
        {
            LogError("Starting: HandleKillCommand");
            if (command.Length >= 3)
            {
                string entityName = command[2];
                double radius = 10;
                if (command.Length == 4)
                {
                    double parsedRadius;
                    if (double.TryParse(command[3], out parsedRadius))
                    {
                        radius = parsedRadius;
                    }
                }

                if (entityName.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    KillAllEntities(radius);
                }
                else
                {
                    KillEntitiesByName(entityName, radius);
                }
                sendToOthers = false;
            }
            else
            {
                MyAPIGateway.Utilities.ShowMessage("BotSpawner", "Usage: /pepco kill <EntityName|all> [Radius]");
            }
        }

        private void HandleShowCommand(string[] command, ref bool sendToOthers)
        {
            LogError("Starting: HandleShowCommand");
            if (command.Length > 2 && command[2].Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                ShowAllEntities();
                sendToOthers = false;
            }
            else if (command.Length == 4)
            {
                string entityId = command[2];
                double radius;
                if (double.TryParse(command[3], out radius))
                {
                    ShowEntities(entityId, radius);
                    sendToOthers = false;
                }
                else
                {
                    MyAPIGateway.Utilities.ShowMessage("PEPCO", "Invalid radius. Usage: /pepco show <entityID> <radius>");
                }
            }
            else
            {
                MyAPIGateway.Utilities.ShowMessage("PEPCO", "Usage: /pepco show <entityID> <radius>");
            }
        }

        private void ShowHelp()
        {
            LogError("Starting: ShowHelp");
            var commands = new List<string>
            {
                "/pepco help - Displays this help message.",
                "/pepco CES reload - Reloads the Custom Entity Spawner",
                "/pepco CES list - Lists all blocks and their spawn",
                "/pepco kill <EntityName|all> [Radius] - Kills entities by name or all entities within a radius.",
                "/pepco cleanup dead - Cleans up dead entities.",
                "/pepco show all - Shows all entities.",
                "/pepco show <entityID> <radius> - Shows entities of a specific ID within a radius.",
                "/pepco spawn <BotID> - Spawns a bot with the specified ID.",
                "/pepco listBots - Lists all valid bot IDs.",
                "/pepco logging - Enable or disable debug logging.",
                "/pepco pause - Pause script execution."
            };

            foreach (var command in commands)
            {
                MyAPIGateway.Utilities.ShowMessage("PEPCO Commands", command);
            }
        }

        private void ReloadSettings()
        {
            LogError("Starting: ReloadSettings");
            try
            {
                MyAPIGateway.Utilities.ShowMessage("PEPCO", "Reloading ..");
                LogError("Reloading ..");

                EnsureDefaultIniFilesExist();
                CopyAllCESFilesToWorldStorage();
                LoadAllFilesFromWorldStorage();
                InitializeBotSpawnerConfig();
                Load();

                MyAPIGateway.Utilities.ShowMessage("PEPCO", "Settings reloaded successfully.");
                LogError("Settings reloaded successfully.");
            }
            catch (Exception ex)
            {
                MyAPIGateway.Utilities.ShowMessage("PEPCO", $"Error reloading settings: {ex.Message}");
                LogError($"Error reloading settings: {ex.Message}");
            }
        }

        private void ListAllBlocksAndSpawns()
        {
            LogError("Starting: ListAllBlocksAndSpawns");
            if (BlockSpawnSettings.Count == 0)
            {
                MyAPIGateway.Utilities.ShowMessage("CustomEntitySpawner", "No block spawn settings found.");
                return;
            }

            foreach (var blockSettings in BlockSpawnSettings)
            {
                string message = $"BlockId: {blockSettings.BlockId}, BlockType: {blockSettings.BlockType}, Entities: {string.Join(", ", blockSettings.EntityID)}";
                MyAPIGateway.Utilities.ShowMessage("CustomEntitySpawner", message);
            }
        }

        private void KillAllEntities(double radius)
        {
            LogError("Starting: KillAllEntities");
            var entities = new HashSet<IMyEntity>();
            Vector3D playerPosition = MyAPIGateway.Session.Player.GetPosition();
            IMyCharacter playerCharacter = MyAPIGateway.Session.Player.Character;

            MyAPIGateway.Entities.GetEntities(entities, e => e is IMyCharacter);

            int entitiesKilled = 0;
            foreach (var entity in entities)
            {
                IMyCharacter character = entity as IMyCharacter;
                if (character != null && Vector3D.Distance(character.GetPosition(), playerPosition) <= radius && character != playerCharacter)
                {
                    entity.Close();
                    entitiesKilled++;
                }
            }

            MyAPIGateway.Utilities.ShowMessage("BotSpawner", $"Killed {entitiesKilled} entities within {radius} meters.");
        }

        private void KillEntitiesByName(string entitySubtypeId, double radius)
        {
            LogError("Starting: KillEntitiesByName");
            var entities = new HashSet<IMyEntity>();
            Vector3D playerPosition = MyAPIGateway.Session.Player.GetPosition();
            IMyCharacter playerCharacter = MyAPIGateway.Session.Player.Character;

            MyAPIGateway.Entities.GetEntities(entities, e => e is IMyCharacter);

            int entitiesKilled = 0;
            foreach (var entity in entities)
            {
                IMyCharacter character = entity as IMyCharacter;
                string entitySubtypeIdCurrent = (entity as MyEntity)?.DefinitionId?.SubtypeId.ToString();
                if (character != null && Vector3D.Distance(character.GetPosition(), playerPosition) <= radius && character != playerCharacter && entitySubtypeIdCurrent != null && entitySubtypeIdCurrent.IndexOf(entitySubtypeId, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    entity.Close();
                    entitiesKilled++;
                }
            }

            MyAPIGateway.Utilities.ShowMessage("BotSpawner", $"Killed {entitiesKilled} entities with SubtypeId: {entitySubtypeId} within {radius} meters.");
        }

        private void ShowEntities(string entitySubtypeId, double radius)
        {
            LogError("Starting: ShowEntities");
            var entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities, e => e is IMyCharacter);

            int entityCount = 0;
            foreach (var entity in entities)
            {
                string entityCurrentSubtypeId = (entity as MyEntity)?.DefinitionId?.SubtypeId.ToString();

                if (entityCurrentSubtypeId != null && entityCurrentSubtypeId.Equals(entitySubtypeId, StringComparison.OrdinalIgnoreCase) &&
                    Vector3D.Distance(entity.GetPosition(), MyAPIGateway.Session.Player.GetPosition()) <= radius)
                {
                    entityCount++;
                    string entityType = entity.GetType().Name;
                    Vector3D entityPosition = entity.GetPosition();

                    LogError($"Type: {entityType}, SubtypeId: {entityCurrentSubtypeId}, Location: {entityPosition}");
                }
            }

            LogError($"Total entities of subtype '{entitySubtypeId}' within {radius} meters: {entityCount}");
        }

        private void ShowAllEntities()
        {
            LogError("Starting: ShowAllEntities");
            var entities = new HashSet<IMyEntity>();
            IMyCharacter playerCharacter = MyAPIGateway.Session.Player.Character;
            MyAPIGateway.Entities.GetEntities(entities);
            foreach (var entity in entities)
            {
                var character = entity as IMyCharacter;
                if (entity is IMyCharacter && character != playerCharacter && !character.IsDead)
                {
                    string entityType = entity.GetType().Name;
                    string entitySubtypeId = (entity as MyEntity)?.DefinitionId?.SubtypeId.ToString() ?? "Unnamed";
                    Vector3D entityPosition = entity.GetPosition();

                    MyAPIGateway.Utilities.ShowMessage("Entity", $"Type: {entityType}, SubtypeId: {entitySubtypeId}, Location: {entityPosition}");
                }
            }
        }

        public void CleanupDeadEntities()
        {
            LogError("Starting: ShowAllEntities");
            var entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities);

            foreach (var entity in entities)
            {
                if (IsEntityDead(entity))
                {
                    entity.Close();
                }
            }
        }

        private bool IsEntityDead(IMyEntity entity)
        {
            LogError("Starting: IsEntityDead");
            var character = entity as IMyCharacter;
            if (character != null && character.IsDead)
            {
                return true;
            }
            return false;
        }

        private int CountEntitiesInRadius(Vector3D position, double radius)
        {
            LogError("Starting: CountEntitiesInRadius");
            var entities = new HashSet<IMyEntity>();
            Vector3D playerPosition = MyAPIGateway.Session.Player.GetPosition();
            IMyCharacter playerCharacter = MyAPIGateway.Session.Player.Character;

            MyAPIGateway.Entities.GetEntities(entities, e => e is IMyCharacter);
            int entitiesCount = 0;
            foreach (var entity in entities)
            {
                IMyCharacter character = entity as IMyCharacter;
                if (character != null && Vector3D.Distance(character.GetPosition(), playerPosition) <= radius && character != playerCharacter)
                {
                    entitiesCount++;
                }
            }
            return entitiesCount;
        }

        public void LogError(string message)
        {
            string LogfilePath = "CustomEntitySpawner.log";
            string existingContent = ReadExistingLogContent(LogfilePath);

            WriteLogContent(LogfilePath, existingContent, message);

            NotifyAdminPlayer(message);
        }

        private string ReadExistingLogContent(string LogfilePath)
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(LogfilePath, typeof(CustomEntitySpawner)))
            {
                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(LogfilePath, typeof(CustomEntitySpawner)))
                {
                    return reader.ReadToEnd();
                }
            }
            return string.Empty;
        }

        private void WriteLogContent(string ilePath, string existingContent, string message)
        {
            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(ilePath, typeof(CustomEntitySpawner)))
            {
                if (!string.IsNullOrEmpty(existingContent))
                {
                    writer.Write(existingContent);
                }
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }

        private void NotifyAdminPlayer(string message)
        {
            var player = MyAPIGateway.Session.Player;
            if (player != null && MyAPIGateway.Session.IsUserAdmin(player.SteamUserId))
            {
                MyAPIGateway.Utilities.ShowMessage("CES", message);
            }
        }

        private void LoadCustomEntitySpawnerConfig(string fileContent)
        {
            LogError("Starting: LoadCustomEntitySpawnerConfig");
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

                var blockSettings = new CustomEntitySpawner
                {
                    BlockId = iniParser.Get(section, nameof(BlockId)).ToString(),
                    BlockType = iniParser.Get(section, nameof(BlockType)).ToString(),
                    Enabled = iniParser.Get(section, nameof(Enabled)).ToBoolean(),
                    PlayerDistanceCheck = iniParser.Get(section, nameof(PlayerDistanceCheck)).ToInt32(),
                    EnableEntitySpawning = iniParser.Get(section, nameof(EnableEntitySpawning)).ToBoolean(true),
                    EnableItemSpawning = iniParser.Get(section, nameof(EnableItemSpawning)).ToBoolean(true),
                    SpawnItemsWithEntities = iniParser.Get(section, nameof(SpawnItemsWithEntities)).ToBoolean(false),
                    MinEntityAmount = iniParser.Get(section, nameof(MinEntityAmount)).ToInt32(),
                    MaxEntityAmount = iniParser.Get(section, nameof(MaxEntityAmount)).ToInt32(),
                    MinItemAmount = iniParser.Get(section, nameof(MinItemAmount)).ToInt32(),
                    MaxItemAmount = iniParser.Get(section, nameof(MaxItemAmount)).ToInt32(),
                    UseWeightedDrops = iniParser.Get(section, nameof(UseWeightedDrops)).ToBoolean(),
                    MaxEntitiesInArea = iniParser.Get(section, nameof(MaxEntitiesInArea)).ToInt32(),
                    MaxEntitiesRadius = iniParser.Get(section, nameof(MaxEntitiesRadius)).ToDouble(100),
                    StackItems = iniParser.Get(section, nameof(StackItems)).ToBoolean(),
                    SpawnInsideInventory = iniParser.Get(section, nameof(SpawnInsideInventory)).ToBoolean(),
                    DamageAmount = (float)iniParser.Get(section, nameof(DamageAmount)).ToDouble(),
                    Repair = iniParser.Get(section, nameof(Repair)).ToBoolean(),
                    MinHealthPercentage = (float)iniParser.Get(section, nameof(MinHealthPercentage)).ToDouble(),
                    MaxHealthPercentage = (float)iniParser.Get(section, nameof(MaxHealthPercentage)).ToDouble(),
                    MinHeight = iniParser.Get(section, nameof(MinHeight)).ToDouble(),
                    MaxHeight = iniParser.Get(section, nameof(MaxHeight)).ToDouble(),
                    MinRadius = iniParser.Get(section, nameof(MinRadius)).ToDouble(),
                    MaxRadius = iniParser.Get(section, nameof(MaxRadius)).ToDouble(),
                    SpawnTriggerInterval = iniParser.Get(section, nameof(SpawnTriggerInterval)).ToInt32(),
                    EnableAirtightAndOxygen = iniParser.Get(section, nameof(EnableAirtightAndOxygen)).ToBoolean(),
                    RequiredEntity = iniParser.Get(section, nameof(RequiredEntity)).ToString(),
                    RequiredEntityRadius = iniParser.Get(section, nameof(RequiredEntityRadius)).ToDouble(),
                    RequiredEntityNumber = iniParser.Get(section, nameof(RequiredEntityNumber)).ToInt32(),
                    RequireEntityNumberForTotalEntities = iniParser.Get(section, nameof(RequireEntityNumberForTotalEntities)).ToBoolean(),
                    RequireEntityCenterOn = iniParser.Get(section, nameof(RequireEntityCenterOn)).ToBoolean()
                };

                blockSettings.EntityID.AddRange(iniParser.Get(section, nameof(blockSettings.EntityID)).ToString().Split(','));
                blockSettings.ItemTypes.AddRange(iniParser.Get(section, nameof(blockSettings.ItemTypes)).ToString().Split(','));
                blockSettings.ItemIds.AddRange(iniParser.Get(section, nameof(blockSettings.ItemIds)).ToString().Split(','));
                blockSettings.RequiredItemTypes.AddRange(iniParser.Get(section, nameof(blockSettings.RequiredItemTypes)).ToString().Split(','));
                blockSettings.RequiredItemIds.AddRange(iniParser.Get(section, nameof(blockSettings.RequiredItemIds)).ToString().Split(','));
                blockSettings.RequiredItemAmounts.AddRange(iniParser.Get(section, nameof(blockSettings.RequiredItemAmounts)).ToString().Split(',').Select(int.Parse));
                blockSettings.PermanentRequiredItemTypes.AddRange(iniParser.Get(section, nameof(blockSettings.PermanentRequiredItemTypes)).ToString().Split(','));
                blockSettings.PermanentRequiredItemIds.AddRange(iniParser.Get(section, nameof(blockSettings.PermanentRequiredItemIds)).ToString().Split(','));
                blockSettings.PermanentRequiredItemAmounts.AddRange(iniParser.Get(section, nameof(blockSettings.PermanentRequiredItemAmounts)).ToString().Split(',').Select(int.Parse));
                BlockSpawnSettings.Add(blockSettings);
            }
        }
        
    }
}


 


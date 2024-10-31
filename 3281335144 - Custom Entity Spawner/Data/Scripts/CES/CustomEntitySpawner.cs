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
using Sandbox.Game.Entities.Character;
using Jakaria.API;
using Sandbox.Game.EntityComponents;

namespace PEPCO.iSurvival.CustomEntitySpawner
{

    public enum WaterSpawnOption
    {
        Above,
        Below,
        Both
    }
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class CustomEntitySpawner : MySessionComponentBase
    {
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
        public bool EnableLogging { get; set; } = false;
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
EnableLogging=false
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
        public bool EnableEntitySpawning { get; set; } = false;
        public bool EnableItemSpawning { get; set; } = false;
        public bool SpawnItemsWithEntities { get; set; } = false;
        public int MinEntityAmount { get; set; } = 1;
        public int MaxEntityAmount { get; set; } = 1;
        public List<double> MinItemAmount { get; set; } = new List<double>();
        public List<double> MaxItemAmount { get; set; } = new List<double>();
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
        public List<double> RequiredItemAmounts { get; set; } = new List<double>();
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
        //Water mod support
        private bool EnableWaterAPI { get; set; } = false;
        public double MinWaterDepth { get; set; } = 0.0;
        public double MaxWaterDepth { get; set; } = 100.0;
        public List<CustomEntitySpawner> BlockSpawnSettings { get; set; } = new List<CustomEntitySpawner>();

        //NPC Death Spawning
        // Settings for the entity that triggers the spawn on death
        public string NPCDeathEntity { get; set; } = "";
        public double NPCDeathEntityRadius { get; set; } = 20.0;
        public int NPCDeathEntityMinAmount { get; set; } = 1;
        public int NPCDeathEntityMaxAmount { get; set; } = 1;

        // Settings for the entity to spawn upon death of NPCDeathEntity
        public bool EnableNPCDeathSpawn { get; set; } = false;
        public string NPCDeathSpawnEntity { get; set; } = "";
        public int NPCDeathSpawnMinAmount { get; set; } = 1;
        public int NPCDeathSpawnMaxAmount { get; set; } = 1;
        public double NPCDeathSpawnRadius { get; set; } = 10.0;

        // Settings for items to spawn upon NPC death
        public bool EnableNPCDeathItemSpawn { get; set; } = false;
        public List<string> NPCDeathItemTypes { get; set; } = new List<string>();
        public List<string> NPCDeathItemIds { get; set; } = new List<string>();
        public int NPCDeathItemMinAmount { get; set; } = 1;
        public int NPCDeathItemMaxAmount { get; set; } = 1;
        public double NPCDeathItemRadius { get; set; } = 5.0;

        public string DefaultEntitySpawnerIniContent = @"
[SmallBlockSmallContainerSpawn]
BlockId=SmallBlockSmallContainer
BlockType=MyObjectBuilder_CargoContainer
Enabled=false

; PlayerDistanceCheck is the maximum distance from a player for spawning entities.
PlayerDistanceCheck=10

; Entity spawning settings
EnableEntitySpawning=true
EntityID=Wolf
MinEntityAmount=1
MaxEntityAmount=4

; MaxEntitiesInArea is the maximum number of entities allowed in the area for spawning.
MaxEntitiesInArea=10
MaxEntitiesRadius=50

; Item spawning settings
EnableItemSpawning=true
ItemTypes=MyObjectBuilder_Component
ItemIds=SteelPlate
MinItemAmount=0
MaxItemAmount=10
StackItems=true
SpawnInsideInventory=true

; SpawnItemsWithEntities specifies whether items should be spawned only when an entity is spawned.
SpawnItemsWithEntities=false

; Environmental conditions
DamageAmount=0
MinHealthPercentage=0
MaxHealthPercentage=1
MinHeight=1.0
MaxHeight=2.5
MinRadius=0.5
MaxRadius=1.5
SpawnTriggerInterval=3
EnableAirtightAndOxygen=false

; Permanent required items for spawning (not removed)
PermanentRequiredItemTypes=MyObjectBuilder_Ore,MyObjectBuilder_Ore
PermanentRequiredItemIds=Iron,Gold
PermanentRequiredItemAmounts=0,0

; Required entities in the vicinity for spawning
RequiredEntity=Spider
RequiredEntityRadius=20
RequiredEntityNumber=0
RequireEntityNumberForTotalEntities=false
RequireEntityCenterOn=false

; Water settings
EnableWaterAPI=true
MinWaterDepth=5.0
MaxWaterDepth=20.0

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
            CopyAllCESFilesToWorldStorage();
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
        public override void LoadData()
        {
            MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
            MyAPIGateway.Entities.OnEntityRemove += OnEntityRemoved;
        }
        protected override void UnloadData()
        {
            LogError("Starting UnloadData");
            if (MyAPIGateway.Multiplayer.IsServer)
            {
                MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
                MyAPIGateway.Entities.OnEntityRemove -= OnEntityRemoved;
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

        //Start NPC Death Spawn

        private void OnEntityRemoved(IMyEntity entity)
        {
            MyAPIGateway.Utilities.ShowNotification($"Something was killed", 2000, MyFontEnum.Red);

            // Check if entity is a dead NPC character
            var character = entity as IMyCharacter;
            
            if (character != null && character.IsDead)
            {
                string entityName = character.DisplayName;
                Vector3D position = character.GetPosition();
                MyAPIGateway.Utilities.ShowNotification($"{entityName} was killed at {position}", 2000, MyFontEnum.Red);

                // Invoke drop method if the character is close to a designated block
                HandleNPCDeathDrop(character);
            }
        }

        private void HandleNPCDeathDrop(IMyCharacter character)
        {
            Vector3D characterPosition = character.GetPosition();

            // Iterate over blocks in `BlockSpawnSettings` to check for proximity
            foreach (var blockSetting in BlockSpawnSettings)
            {
                if (blockSetting.Enabled && blockSetting.EnableNPCDeathSpawn)
                {
                    BoundingSphereD searchSphere = new BoundingSphereD(characterPosition, blockSetting.NPCDeathEntityRadius);
                    var blocksInRadius = MyAPIGateway.Entities.GetEntitiesInSphere(ref searchSphere)
                        .OfType<IMyCubeBlock>()
                        .Where(b => b.BlockDefinition.SubtypeId == blockSetting.BlockId &&
                                    Vector3D.Distance(characterPosition, b.GetPosition()) <= blockSetting.NPCDeathEntityRadius)
                        .ToList();

                    if (blocksInRadius.Any() && IsMatchingNPCDeathEntity(character, blockSetting.NPCDeathEntity))
                    {
                        SpawnEntitiesNearPosition(characterPosition, blockSetting);

                        if (blockSetting.EnableNPCDeathItemSpawn)
                        {
                            SpawnItemsNearPosition(characterPosition, blockSetting);
                        }
                    }
                }
            }
        }
        private void SpawnItemsNearPosition(Vector3D position, CustomEntitySpawner blockSetting)
        {
            for (int i = 0; i < blockSetting.NPCDeathItemTypes.Count; i++)
            {
                string itemType = blockSetting.NPCDeathItemTypes[i];
                string itemId = blockSetting.NPCDeathItemIds[i];

                int spawnAmount = randomGenerator.Next(blockSetting.NPCDeathItemMinAmount, blockSetting.NPCDeathItemMaxAmount + 1);

                for (int j = 0; j < spawnAmount; j++)
                {
                    Vector3D dropPosition = position + GetRandomOffsetWithinRadius(blockSetting.NPCDeathItemRadius);
                    MyFloatingObjects.Spawn(new MyPhysicalInventoryItem((VRage.MyFixedPoint)1, CreatePhysicalObject(itemType, itemId)), dropPosition, Vector3D.Up, Vector3D.Forward);
                }
            }
        }

        private MyObjectBuilder_PhysicalObject CreatePhysicalObject(string itemType, string itemId)
        {
            var definitionId = new MyDefinitionId(itemTypeMappings[itemType], itemId);
            return (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(definitionId);
        }

        private Vector3D GetRandomOffsetWithinRadius(double radius)
        {
            Random random = new Random();
            double offsetX = (random.NextDouble() * 2 - 1) * radius;
            double offsetY = (random.NextDouble() * 2 - 1) * radius;
            double offsetZ = (random.NextDouble() * 2 - 1) * radius;
            return new Vector3D(offsetX, offsetY, offsetZ);
        }
        private bool IsMatchingNPCDeathEntity(IMyCharacter character, string requiredEntitySubtype)
        {
            // Check if character matches the required NPCDeathEntity type
            string entitySubtypeId = (character as MyEntity)?.DefinitionId?.SubtypeId.ToString();
            return entitySubtypeId != null && entitySubtypeId.Equals(requiredEntitySubtype, StringComparison.OrdinalIgnoreCase);
        }
        private void SpawnEntitiesNearPosition(Vector3D position, CustomEntitySpawner blockSetting)
        {
            if (string.IsNullOrEmpty(blockSetting.NPCDeathSpawnEntity))
                return;

            int spawnAmount = randomGenerator.Next(blockSetting.NPCDeathSpawnMinAmount, blockSetting.NPCDeathSpawnMaxAmount + 1);
            for (int i = 0; i < spawnAmount; i++)
            {
                Vector3D spawnPosition = position + GetRandomOffsetWithinRadius(blockSetting.NPCDeathSpawnRadius);
                MyVisualScriptLogicProvider.SpawnBot(blockSetting.NPCDeathSpawnEntity, spawnPosition);
            }
        }

        //END NPC Death Spawn 

        private void SpawnEntitiesNearBlocks(ref int entitiesSpawned)
        {
            if (scriptPaused) return;
            //LogError("Starting SpawnEntitiesNearBlocks");


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
            bool isEnabled = blockSettings.Enabled;
            bool isBlockEnabled = IsBlockFunctional(block, blockSettings);
            bool isBlockPowered = IsBlockPowered(block, blockSettings);
            bool isSpawnTrigger = baseUpdateCycles % blockSettings.SpawnTriggerInterval == 0;
            bool isWaterSuitable = IsWaterLevelSuitable(block, blockSettings);
            bool isEnvironmentSuitable = IsEnvironmentSuitable(block, blockSettings);
            bool isPlayerInRange = IsPlayerInRange(block, players, blockSettings.PlayerDistanceCheck);
            bool areRequiredEntitiesPresent = AreRequiredEntitiesInVicinity(block, blockSettings);

            // Check if the block is loaded on the server
            var grid = block.CubeGrid as IMyCubeGrid;
            bool isGridValid = grid != null && MyAPIGateway.Entities.EntityExists(grid.EntityId);

            float blockHealthPercentage = block.Integrity / block.MaxIntegrity;
            bool isHealthInRange = blockHealthPercentage >= blockSettings.MinHealthPercentage &&
                                   blockHealthPercentage <= blockSettings.MaxHealthPercentage;

            bool hasRequiredItems = CheckInventoryForRequiredItems(block, blockSettings);

            // Log errors for any condition that fails
            if (!isEnabled) LogError("Block is disabled for spawning.");
            if (!isBlockEnabled) LogError("Block is Turned Off in game.");
            if (!isBlockPowered) LogError("Block is not powered");
            if (!isSpawnTrigger) LogError("Spawn trigger interval not met.");
            if (!isWaterSuitable) LogError("Water level is not suitable.");
            if (!isEnvironmentSuitable) LogError("Environment is not suitable.");
            if (!isPlayerInRange) LogError("No player in range.");
            if (!areRequiredEntitiesPresent) LogError("Required entities are not in vicinity.");
            if (!isGridValid) LogError("Block is not loaded on the server.");
            if (!isHealthInRange) LogError("Block health is not within the required range.");
            if (!hasRequiredItems) LogError("Required items are not in the block's inventory.");

            // Only return true if all conditions are met
            return isEnabled &&
                   isBlockEnabled &&
                   isBlockPowered &&
                   isSpawnTrigger &&
                   isWaterSuitable &&
                   isEnvironmentSuitable &&
                   isPlayerInRange &&
                   areRequiredEntitiesPresent &&
                   isGridValid &&
                   isHealthInRange &&
                   hasRequiredItems;
        }

        private bool IsBlockFunctional(IMySlimBlock block, CustomEntitySpawner blockSettings)
        {
            // Check if the block has a FatBlock (meaning it has a physical representation in the world)
            if (block.FatBlock == null)
            {
                LogError($"Block {block.BlockDefinition.Id.SubtypeName} does not have a FatBlock.");
                return false;
            }

            // Check if the block is functional
            if (!block.FatBlock.IsFunctional)
            {
                LogError($"Block {block.BlockDefinition.Id.SubtypeName} is not functional.");
                return false;
            }

            // Check if the block is enabled (for functional blocks)
            var functionalBlock = block.FatBlock as IMyFunctionalBlock;
            if (functionalBlock != null && !functionalBlock.Enabled)
            {
                LogError($"Block {block.BlockDefinition.Id.SubtypeName} is not enabled.");
                return false;
            }

            // If the block passes all checks, return true
            return true;
        }

        private bool IsBlockPowered(IMySlimBlock block, CustomEntitySpawner blockSettings)
        {
            // Check if the block is a cube block and part of a valid grid
            var cubeBlock = block.FatBlock as IMyCubeBlock;
            if (cubeBlock == null || cubeBlock.CubeGrid == null)
            {
                LogError($"Block {block.BlockDefinition.Id.SubtypeName} is not a valid cube block or is not part of a valid grid.");
                return false;
            }
            // Check if the block is a cube block and has a power requirement
            var blockresourceSink = cubeBlock.Components.Get<MyResourceSinkComponent>();
            if (blockresourceSink != null)
            {

                float requiredPower = blockresourceSink.RequiredInputByType(MyResourceDistributorComponent.ElectricityId);
                float currentPower = blockresourceSink.CurrentInputByType(MyResourceDistributorComponent.ElectricityId);


                // Check if the block requires power and whether it is receiving enough power to function
                if ((currentPower - requiredPower) < 0)
                {
                    LogError($"Block {block.BlockDefinition.Id.SubtypeName} is not receiving enough power (Required: {requiredPower}, Current: {currentPower}).");
                    return false;
                }
            }            
            return true;
        }        

        private bool IsWaterLevelSuitable(IMySlimBlock block, CustomEntitySpawner blockSettings)
        {

            if (blockSettings.EnableWaterAPI == false) return true;
            // Ensure the WaterModAPI is registered
            if (!Jakaria.API.WaterModAPI.Registered)
            {
                LogError("WaterModAPI is not registered, allowing spawning.");
                return true; // If the API isn't available, allow spawning
            }

            Vector3D blockPosition = block.FatBlock.GetPosition();
            MyPlanet closestPlanet = Jakaria.API.WaterModAPI.GetClosestWater(blockPosition);
            var depth = (Jakaria.API.WaterModAPI.GetDepth(blockPosition) * -1);

            if (closestPlanet == null)
            {
                LogError("No water found near the block, allowing spawning.");
                return true; // If there's no water nearby, allow spawning
            }

            if (depth < blockSettings.MinWaterDepth)
            {
                LogError($"Block is too shallow (depth: {depth}m, minimum required: {blockSettings.MinWaterDepth}m), disallowing spawning.");
                return false;
            }

            if (depth > blockSettings.MaxWaterDepth)
            {
                LogError($"Block is too deep underwater (depth: {depth}m, maximum allowed: {blockSettings.MaxWaterDepth}m), disallowing spawning.");
                return false;
            }

            LogError($"End of IsWaterLevelSuitable: Depth: {depth}");
            return true;
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

            if (blockSettings.EnableEntitySpawning || blockSettings.EnableItemSpawning)
            {
                // Check if required items are present before removing them
                if (CheckInventoryForRequiredItems(block, blockSettings))
                {
                    RemoveItemsFromInventory(block, blockSettings);

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
                        SpawnEntitiesAndApplyDamage(block, blockSettings, ref entitiesSpawned, ref entitiesSpawnedThisCycle);
                    }

                    if (blockSettings.EnableItemSpawning)
                    {
                        SpawnItemsAndApplyDamage(block, blockSettings);
                    }
                }
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
            for (int i = 0; i < blockSettings.ItemTypes.Count; i++)
            {
                double itemSpawnAmount = blockSettings.UseWeightedDrops ?
                    GetWeightedRandomNumber(blockSettings.MinItemAmount[i], GenerateProbabilities(blockSettings.MinItemAmount[i], blockSettings.MaxItemAmount[i])) :
                    blockSettings.MinItemAmount[i] + randomGenerator.NextDouble() * (blockSettings.MaxItemAmount[i] - blockSettings.MinItemAmount[i]);

                int roundedItemSpawnAmount = (int)Math.Round(itemSpawnAmount);

                if (roundedItemSpawnAmount > 0)
                {
                    if (blockSettings.RequireEntityCenterOn)
                    {
                        CenterSpawnItemsAroundEntities(block, blockSettings, roundedItemSpawnAmount);
                    }
                    else
                    {
                        SpawnItems(block, blockSettings);
                    }

                    ApplyDamageToBlock(block, blockSettings, roundedItemSpawnAmount);
                }
            }
        }




        private void ApplyDamageToBlock(IMySlimBlock block, CustomEntitySpawner blockSettings, double amount)
        {
            LogError("Starting ApplyDamageToBlock");
            float maxHealth = block.MaxIntegrity;
            float damageAmount = maxHealth * (blockSettings.DamageAmount / 100.0f) * (float)amount;
            if (!blockSettings.Repair)
            {
                block.DoDamage(damageAmount, MyDamageType.Destruction, true);
            }
            else
            {
                int damageSteps = (int)Math.Ceiling(blockSettings.DamageAmount * amount);
                for (int i = 0; i < damageSteps; i++)
                {
                    block.SpawnFirstItemInConstructionStockpile();
                }
                block.IncreaseMountLevel(blockSettings.DamageAmount * (float)amount, block.OwnerId, null, MyAPIGateway.Session.WelderSpeedMultiplier, true);
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
                    string entityCurrentSubtypeId = (character as MyEntity)?.DefinitionId?.SubtypeId.ToString();
                    LogError($"Entity: {entity.DisplayName}, SubtypeId: {entityCurrentSubtypeId}, Position: {entity.GetPosition()}, Distance: {Vector3D.Distance(entity.GetPosition(), position)}");

                    if (requiredEntitySubtypeId.Equals("All", StringComparison.OrdinalIgnoreCase) ||
                        (entityCurrentSubtypeId != null && entityCurrentSubtypeId.Equals(requiredEntitySubtypeId, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (Vector3D.Distance(entity.GetPosition(), position) <= radius)
                        {
                            if (!excludeDeadEntities || !character.IsDead)
                            {
                                entityCount++;
                            }
                        }
                    }
                }
            }

            LogError($"Entities found: {entityCount}");
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
            if (block.FatBlock == null)
            {
                LogError("Block does not have a FatBlock");
                return false;
            }

            Vector3D blockPosition = block.FatBlock.GetPosition();
            LogError($"Block Position: {blockPosition}");

            foreach (var player in players)
            {
                // Check if the player is controlled by a human player
                var controlledEntity = player.Controller?.ControlledEntity?.Entity;
                if (controlledEntity != null && controlledEntity is IMyCharacter)
                {
                    // Ensure that the character's controller is the same as the player
                    var character = controlledEntity as IMyCharacter;
                    if (!player.IsBot && !character.IsDead)
                    {
                        double distance = Vector3D.Distance(character.GetPosition(), blockPosition);
                        LogError($"Player Position:{player.DisplayName} {character.GetPosition()}, Distance: {distance}");
                        if (distance <= playerDistanceCheck) return true;
                    }
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
        private void CenterSpawnAroundEntities(IMySlimBlock block, CustomEntitySpawner settings, int spawnAmount)
        {
            LogError("Starting CenterSpawnAroundEntities");
            if (block.FatBlock == null)
            {
                LogError("Block does not have a FatBlock");
                return;
            }

            var entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities, e => e is IMyCharacter);

            Vector3D blockPosition = block.FatBlock.GetPosition();
            LogError($"Block Position: {blockPosition}");

            foreach (var entity in entities)
            {
                var character = entity as IMyCharacter;
                if (character != null && !character.IsDead && character.Definition.Id.SubtypeId.String.Equals(settings.RequiredEntity, StringComparison.OrdinalIgnoreCase))
                {
                    Vector3D entityPosition = character.PositionComp.GetPosition();

                    //     var myEntity = entity as MyEntity;
                    // if (myEntity != null && myEntity.DefinitionId.HasValue &&
                    //     myEntity.DefinitionId.Value.SubtypeId.String.Equals(settings.RequiredEntity, StringComparison.OrdinalIgnoreCase))

                    //Vector3D entityPosition = myEntity.PositionComp.GetPosition();
                    LogError($"Entity Position: {entityPosition}, Distance: {Vector3D.Distance(entityPosition, blockPosition)}");

                    if (Vector3D.Distance(entityPosition, blockPosition) <= settings.RequiredEntityRadius)
                    {
                        for (int i = 0; i < spawnAmount; i++)
                        {
                            Vector3D spawnPosition = entityPosition + GetRandomOffset();
                            LogError($"Spawning bot {settings.EntityID.FirstOrDefault()} at {spawnPosition}");
                            MyVisualScriptLogicProvider.SpawnBot(settings.EntityID.FirstOrDefault(), spawnPosition);
                        }
                    }
                }
            }
        }

        private Vector3D GetRandomOffset()
        {
            Random random = new Random();
            double offsetX = random.NextDouble() * 2 - 1; // Random value between -1 and 1
            double offsetY = random.NextDouble() * 2 - 1; // Random value between -1 and 1
            double offsetZ = random.NextDouble() * 2 - 1; // Random value between -1 and 1
            return new Vector3D(offsetX, offsetY, offsetZ);
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

        private void SpawnItems(IMySlimBlock block, CustomEntitySpawner itemSettings, Vector3D? position = null)
        {
            LogError("Starting SpawnItems");

            for (int i = 0; i < itemSettings.ItemTypes.Count; i++)
            {
                var itemType = itemSettings.ItemTypes[i];
                var itemId = itemSettings.ItemIds[i];
                double minAmount = itemSettings.MinItemAmount[i];
                double maxAmount = itemSettings.MaxItemAmount[i];

                double amount = itemSettings.UseWeightedDrops
                    ? GetWeightedRandomNumber(minAmount, GenerateProbabilities(minAmount, maxAmount))
                    : GetRandomDouble(minAmount, maxAmount);

                if (itemSettings.SpawnInsideInventory)
                {
                    PlaceItemInCargo(block, itemType, itemId, amount, itemSettings.StackItems);
                }
                else
                {
                    if (position.HasValue)
                    {
                        SpawnItemNearPosition(position.Value, itemType, itemId, amount, itemSettings.StackItems, itemSettings);
                    }
                    else
                    {
                        SpawnItemNearBlock(block, itemType, itemId, amount, itemSettings.StackItems, itemSettings);
                    }
                }
            }
        }
        private double GetRandomDouble(double minAmount, double maxAmount)
        {
            return minAmount + (randomGenerator.NextDouble() * (maxAmount - minAmount));
        }


        private void PlaceItemInCargo(IMySlimBlock block, string itemType, string itemId, double amount, bool stackItems)
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
                        VRage.MyFixedPoint fixedAmount = (VRage.MyFixedPoint)amount;
                        if (stackItems)
                        {
                            inventory.AddItems(fixedAmount, itemObjectBuilder);
                        }
                        else
                        {
                            for (int i = 0; i < (int)amount; i++)
                            {
                                inventory.AddItems(1, itemObjectBuilder);
                            }
                            // Handle any fractional part left
                            double fractionalPart = amount - (int)amount;
                            if (fractionalPart > 0)
                            {
                                inventory.AddItems((VRage.MyFixedPoint)fractionalPart, itemObjectBuilder);
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


        private void SpawnItemNearBlock(IMySlimBlock block, string itemType, string itemId, double amount, bool stackItems, CustomEntitySpawner settings)
        {
            LogError("Starting SpawnItemNearBlock");

            // Check if item type mapping exists
            if (!settings.itemTypeMappings.ContainsKey(itemType))
            {
                LogError($"Item type mapping not found for: {itemType}");
                return;
            }

            // Create item definition and object builder
            var itemDefinition = new MyDefinitionId(settings.itemTypeMappings[itemType], itemId);
            var itemObjectBuilder = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(itemDefinition);

            if (itemObjectBuilder == null)
            {
                LogError($"Failed to create object builder for item: {itemType}, ID: {itemId}");
                return;
            }

            // Round amount to the nearest integer
            int roundedAmount = (int)Math.Round(amount);

            List<Vector3D> dropPositions = new List<Vector3D>();
            List<Quaternion> dropRotations = new List<Quaternion>();

            // Calculate drop positions and rotations
            if (stackItems)
            {
                dropPositions.Add(CalculateDropPosition(block, settings));
                dropRotations.Add(GenerateRandomRotation());
                LogError($"Calculated single drop position: {dropPositions[0]} and rotation for stacked items.");
            }
            else
            {
                for (int j = 0; j < roundedAmount; j++)
                {
                    dropPositions.Add(CalculateDropPosition(block, settings));
                    dropRotations.Add(GenerateRandomRotation());
                }
                LogError($"Calculated {dropPositions.Count} drop positions and rotations for non-stacked items.");
            }

            // Spawn items at calculated positions
            for (int i = 0; i < dropPositions.Count; i++)
            {
                MyFloatingObjects.Spawn(
                    new MyPhysicalInventoryItem((VRage.MyFixedPoint)(stackItems ? amount : 1), itemObjectBuilder),
                    dropPositions[i], dropRotations[i].Up, block.FatBlock.WorldMatrix.Up
                );
                LogError($"Spawned item at position: {dropPositions[i]}, with rotation.");
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

            Vector3D dropPosition = basePosition + offset;
            LogError($"Calculated drop position: {dropPosition}");
            return dropPosition;
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
                    BlockId = iniParser.Get(section, nameof(BlockId)).ToString(),
                    BlockType = iniParser.Get(section, nameof(BlockType)).ToString(),
                    Enabled = iniParser.Get(section, nameof(Enabled)).ToBoolean(true),
                    PlayerDistanceCheck = iniParser.Get(section, nameof(PlayerDistanceCheck)).ToInt32(1000),
                    EnableEntitySpawning = iniParser.Get(section, nameof(EnableEntitySpawning)).ToBoolean(true),
                    EnableItemSpawning = iniParser.Get(section, nameof(EnableItemSpawning)).ToBoolean(true),
                    SpawnItemsWithEntities = iniParser.Get(section, nameof(SpawnItemsWithEntities)).ToBoolean(false),
                    MinEntityAmount = iniParser.Get(section, nameof(MinEntityAmount)).ToInt32(0),
                    MaxEntityAmount = iniParser.Get(section, nameof(MaxEntityAmount)).ToInt32(0),
                    UseWeightedDrops = iniParser.Get(section, nameof(UseWeightedDrops)).ToBoolean(false),
                    MaxEntitiesInArea = iniParser.Get(section, nameof(MaxEntitiesInArea)).ToInt32(30),
                    MaxEntitiesRadius = iniParser.Get(section, nameof(MaxEntitiesRadius)).ToDouble(100),
                    StackItems = iniParser.Get(section, nameof(StackItems)).ToBoolean(true),
                    SpawnInsideInventory = iniParser.Get(section, nameof(SpawnInsideInventory)).ToBoolean(true),
                    DamageAmount = (float)iniParser.Get(section, nameof(DamageAmount)).ToDouble(0),
                    Repair = iniParser.Get(section, nameof(Repair)).ToBoolean(false),
                    MinHealthPercentage = (float)iniParser.Get(section, nameof(MinHealthPercentage)).ToDouble(0),
                    MaxHealthPercentage = (float)iniParser.Get(section, nameof(MaxHealthPercentage)).ToDouble(1),
                    MinHeight = iniParser.Get(section, nameof(MinHeight)).ToDouble(0),
                    MaxHeight = iniParser.Get(section, nameof(MaxHeight)).ToDouble(0),
                    MinRadius = iniParser.Get(section, nameof(MinRadius)).ToDouble(1),
                    MaxRadius = iniParser.Get(section, nameof(MaxRadius)).ToDouble(1),
                    SpawnTriggerInterval = iniParser.Get(section, nameof(SpawnTriggerInterval)).ToInt32(5),
                    EnableAirtightAndOxygen = iniParser.Get(section, nameof(EnableAirtightAndOxygen)).ToBoolean(false),
                    RequiredEntity = iniParser.Get(section, nameof(RequiredEntity)).ToString("Wolf"),
                    RequiredEntityRadius = iniParser.Get(section, nameof(RequiredEntityRadius)).ToDouble(100),
                    RequiredEntityNumber = iniParser.Get(section, nameof(RequiredEntityNumber)).ToInt32(0),
                    RequireEntityNumberForTotalEntities = iniParser.Get(section, nameof(RequireEntityNumberForTotalEntities)).ToBoolean(false),
                    RequireEntityCenterOn = iniParser.Get(section, nameof(RequireEntityCenterOn)).ToBoolean(false),
                    EnableWaterAPI = iniParser.Get(section, nameof(EnableWaterAPI)).ToBoolean(false),
                    MinWaterDepth = iniParser.Get(section, nameof(MinWaterDepth)).ToDouble(0),
                    MaxWaterDepth = iniParser.Get(section, nameof(MaxWaterDepth)).ToDouble(10)
                };

                blockSettings.EntityID.AddRange(iniParser.Get(section, nameof(blockSettings.EntityID)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                blockSettings.ItemTypes.AddRange(iniParser.Get(section, nameof(blockSettings.ItemTypes)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                blockSettings.ItemIds.AddRange(iniParser.Get(section, nameof(blockSettings.ItemIds)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                blockSettings.MinItemAmount.AddRange(iniParser.Get(section, nameof(blockSettings.MinItemAmount)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(double.Parse));
                blockSettings.MaxItemAmount.AddRange(iniParser.Get(section, nameof(blockSettings.MaxItemAmount)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(double.Parse));
                blockSettings.RequiredItemTypes.AddRange(iniParser.Get(section, nameof(blockSettings.RequiredItemTypes)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                blockSettings.RequiredItemIds.AddRange(iniParser.Get(section, nameof(blockSettings.RequiredItemIds)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                blockSettings.RequiredItemAmounts.AddRange(iniParser.Get(section, nameof(blockSettings.RequiredItemAmounts)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(double.Parse));
                blockSettings.PermanentRequiredItemTypes.AddRange(iniParser.Get(section, nameof(blockSettings.PermanentRequiredItemTypes)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                blockSettings.PermanentRequiredItemIds.AddRange(iniParser.Get(section, nameof(blockSettings.PermanentRequiredItemIds)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                blockSettings.PermanentRequiredItemAmounts.AddRange(iniParser.Get(section, nameof(blockSettings.PermanentRequiredItemAmounts)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse));

                //NPC Death spawns
                // Settings for the entity that triggers the spawn on death
                blockSettings.NPCDeathEntity = iniParser.Get(section, nameof(NPCDeathEntity)).ToString();
                blockSettings.NPCDeathEntityRadius = iniParser.Get(section, nameof(NPCDeathEntityRadius)).ToDouble(20.0);
                blockSettings.NPCDeathEntityMinAmount = iniParser.Get(section, nameof(NPCDeathEntityMinAmount)).ToInt32(1);
                blockSettings.NPCDeathEntityMaxAmount = iniParser.Get(section, nameof(NPCDeathEntityMaxAmount)).ToInt32(1);

                // Settings for the entity to spawn upon death of NPCDeathEntity
                blockSettings.EnableNPCDeathSpawn = iniParser.Get(section, nameof(EnableNPCDeathSpawn)).ToBoolean(false);
                blockSettings.NPCDeathSpawnEntity = iniParser.Get(section, nameof(NPCDeathSpawnEntity)).ToString();
                blockSettings.NPCDeathSpawnMinAmount = iniParser.Get(section, nameof(NPCDeathSpawnMinAmount)).ToInt32(1);
                blockSettings.NPCDeathSpawnMaxAmount = iniParser.Get(section, nameof(NPCDeathSpawnMaxAmount)).ToInt32(1);
                blockSettings.NPCDeathSpawnRadius = iniParser.Get(section, nameof(NPCDeathSpawnRadius)).ToDouble(10.0);

                // Settings for items to spawn upon NPC death
                blockSettings.EnableNPCDeathItemSpawn = iniParser.Get(section, nameof(EnableNPCDeathItemSpawn)).ToBoolean(false);
                blockSettings.NPCDeathItemTypes.AddRange(iniParser.Get(section, nameof(NPCDeathItemTypes)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                blockSettings.NPCDeathItemIds.AddRange(iniParser.Get(section, nameof(NPCDeathItemIds)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                blockSettings.NPCDeathItemMinAmount = iniParser.Get(section, nameof(NPCDeathItemMinAmount)).ToInt32(1);
                blockSettings.NPCDeathItemMaxAmount = iniParser.Get(section, nameof(NPCDeathItemMaxAmount)).ToInt32(1);
                blockSettings.NPCDeathItemRadius = iniParser.Get(section, nameof(NPCDeathItemRadius)).ToDouble(5.0);

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

        private void SpawnItemNearPosition(Vector3D position, string itemType, string itemId, double amount, bool stackItems, CustomEntitySpawner settings)
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
            Quaternion rotation = Quaternion.CreateFromYawPitchRoll(
                (float)(randomGenerator.NextDouble() * Math.PI * 2),
                (float)(randomGenerator.NextDouble() * Math.PI * 2),
                (float)(randomGenerator.NextDouble() * Math.PI * 2)
            );
            LogError($"Generated random rotation: {rotation}");
            return rotation;
        }



        private List<double> GenerateProbabilities(double minAmount, double maxAmount)
        {
            LogError("Starting GenerateProbabilities");
            double range = maxAmount - minAmount;
            double middle = (range + 1) / 2;
            var probabilities = new List<double>();

            for (int i = 1; i <= range; i++)
            {
                double weight = i <= middle ? i : range - i + 1;
                probabilities.Add(weight);
            }
            return probabilities;
        }


        private double GetWeightedRandomNumber(double minAmount, List<double> probabilities)
        {
            LogError("Starting GetWeightedRandomNumber");
            double totalWeight = probabilities.Sum();
            if (totalWeight == 0)
            {
                return minAmount;
            }

            double randomValue = randomGenerator.NextDouble() * totalWeight;
            double cumulativeWeight = 0;

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
            if (MyAPIGateway.Utilities.IsDedicated)
            {
                // Only run on dedicated server
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
            else if (MyAPIGateway.Session.IsServer)
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
            if (EnableLogging == false) return;
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
                    Enabled = iniParser.Get(section, nameof(Enabled)).ToBoolean(true),
                    PlayerDistanceCheck = iniParser.Get(section, nameof(PlayerDistanceCheck)).ToInt32(1000),
                    EnableEntitySpawning = iniParser.Get(section, nameof(EnableEntitySpawning)).ToBoolean(true),
                    EnableItemSpawning = iniParser.Get(section, nameof(EnableItemSpawning)).ToBoolean(true),
                    SpawnItemsWithEntities = iniParser.Get(section, nameof(SpawnItemsWithEntities)).ToBoolean(false),
                    MinEntityAmount = iniParser.Get(section, nameof(MinEntityAmount)).ToInt32(0),
                    MaxEntityAmount = iniParser.Get(section, nameof(MaxEntityAmount)).ToInt32(0),
                    UseWeightedDrops = iniParser.Get(section, nameof(UseWeightedDrops)).ToBoolean(false),
                    MaxEntitiesInArea = iniParser.Get(section, nameof(MaxEntitiesInArea)).ToInt32(30),
                    MaxEntitiesRadius = iniParser.Get(section, nameof(MaxEntitiesRadius)).ToDouble(100),
                    StackItems = iniParser.Get(section, nameof(StackItems)).ToBoolean(true),
                    SpawnInsideInventory = iniParser.Get(section, nameof(SpawnInsideInventory)).ToBoolean(true),
                    DamageAmount = (float)iniParser.Get(section, nameof(DamageAmount)).ToDouble(0),
                    Repair = iniParser.Get(section, nameof(Repair)).ToBoolean(false),
                    MinHealthPercentage = (float)iniParser.Get(section, nameof(MinHealthPercentage)).ToDouble(0),
                    MaxHealthPercentage = (float)iniParser.Get(section, nameof(MaxHealthPercentage)).ToDouble(1),
                    MinHeight = iniParser.Get(section, nameof(MinHeight)).ToDouble(0),
                    MaxHeight = iniParser.Get(section, nameof(MaxHeight)).ToDouble(0),
                    MinRadius = iniParser.Get(section, nameof(MinRadius)).ToDouble(1),
                    MaxRadius = iniParser.Get(section, nameof(MaxRadius)).ToDouble(1),
                    SpawnTriggerInterval = iniParser.Get(section, nameof(SpawnTriggerInterval)).ToInt32(5),
                    EnableAirtightAndOxygen = iniParser.Get(section, nameof(EnableAirtightAndOxygen)).ToBoolean(false),
                    RequiredEntity = iniParser.Get(section, nameof(RequiredEntity)).ToString("Wolf"),
                    RequiredEntityRadius = iniParser.Get(section, nameof(RequiredEntityRadius)).ToDouble(100),
                    RequiredEntityNumber = iniParser.Get(section, nameof(RequiredEntityNumber)).ToInt32(0),
                    RequireEntityNumberForTotalEntities = iniParser.Get(section, nameof(RequireEntityNumberForTotalEntities)).ToBoolean(false),
                    RequireEntityCenterOn = iniParser.Get(section, nameof(RequireEntityCenterOn)).ToBoolean(false),
                    EnableWaterAPI = iniParser.Get(section, nameof(EnableWaterAPI)).ToBoolean(false),
                    MinWaterDepth = iniParser.Get(section, nameof(MinWaterDepth)).ToDouble(0),
                    MaxWaterDepth = iniParser.Get(section, nameof(MaxWaterDepth)).ToDouble(10)
                };

                blockSettings.EntityID.AddRange(iniParser.Get(section, nameof(blockSettings.EntityID)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                blockSettings.ItemTypes.AddRange(iniParser.Get(section, nameof(blockSettings.ItemTypes)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                blockSettings.ItemIds.AddRange(iniParser.Get(section, nameof(blockSettings.ItemIds)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                blockSettings.MinItemAmount.AddRange(iniParser.Get(section, nameof(blockSettings.MinItemAmount)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(double.Parse));
                blockSettings.MaxItemAmount.AddRange(iniParser.Get(section, nameof(blockSettings.MaxItemAmount)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(double.Parse));
                blockSettings.RequiredItemTypes.AddRange(iniParser.Get(section, nameof(blockSettings.RequiredItemTypes)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                blockSettings.RequiredItemIds.AddRange(iniParser.Get(section, nameof(blockSettings.RequiredItemIds)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                blockSettings.RequiredItemAmounts.AddRange(iniParser.Get(section, nameof(blockSettings.RequiredItemAmounts)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(double.Parse));
                blockSettings.PermanentRequiredItemTypes.AddRange(iniParser.Get(section, nameof(blockSettings.PermanentRequiredItemTypes)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                blockSettings.PermanentRequiredItemIds.AddRange(iniParser.Get(section, nameof(blockSettings.PermanentRequiredItemIds)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                blockSettings.PermanentRequiredItemAmounts.AddRange(iniParser.Get(section, nameof(blockSettings.PermanentRequiredItemAmounts)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse));
                
                //NPC Death spawns
                // Settings for the entity that triggers the spawn on death
                blockSettings.NPCDeathEntity = iniParser.Get(section, nameof(NPCDeathEntity)).ToString();
                blockSettings.NPCDeathEntityRadius = iniParser.Get(section, nameof(NPCDeathEntityRadius)).ToDouble(20.0);
                blockSettings.NPCDeathEntityMinAmount = iniParser.Get(section, nameof(NPCDeathEntityMinAmount)).ToInt32(1);
                blockSettings.NPCDeathEntityMaxAmount = iniParser.Get(section, nameof(NPCDeathEntityMaxAmount)).ToInt32(1);

                // Settings for the entity to spawn upon death of NPCDeathEntity
                blockSettings.EnableNPCDeathSpawn = iniParser.Get(section, nameof(EnableNPCDeathSpawn)).ToBoolean(false);
                blockSettings.NPCDeathSpawnEntity = iniParser.Get(section, nameof(NPCDeathSpawnEntity)).ToString();
                blockSettings.NPCDeathSpawnMinAmount = iniParser.Get(section, nameof(NPCDeathSpawnMinAmount)).ToInt32(1);
                blockSettings.NPCDeathSpawnMaxAmount = iniParser.Get(section, nameof(NPCDeathSpawnMaxAmount)).ToInt32(1);
                blockSettings.NPCDeathSpawnRadius = iniParser.Get(section, nameof(NPCDeathSpawnRadius)).ToDouble(10.0);

                // Settings for items to spawn upon NPC death
                blockSettings.EnableNPCDeathItemSpawn = iniParser.Get(section, nameof(EnableNPCDeathItemSpawn)).ToBoolean(false);
                blockSettings.NPCDeathItemTypes.AddRange(iniParser.Get(section, nameof(NPCDeathItemTypes)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                blockSettings.NPCDeathItemIds.AddRange(iniParser.Get(section, nameof(NPCDeathItemIds)).ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                blockSettings.NPCDeathItemMinAmount = iniParser.Get(section, nameof(NPCDeathItemMinAmount)).ToInt32(1);
                blockSettings.NPCDeathItemMaxAmount = iniParser.Get(section, nameof(NPCDeathItemMaxAmount)).ToInt32(1);
                blockSettings.NPCDeathItemRadius = iniParser.Get(section, nameof(NPCDeathItemRadius)).ToDouble(5.0);


                BlockSpawnSettings.Add(blockSettings);

            }
        }

    }
}





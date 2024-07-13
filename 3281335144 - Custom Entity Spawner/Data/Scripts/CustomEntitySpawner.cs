using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Configuration;
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
using Sandbox.Engine.Utils;
using VRage.ObjectBuilders;

using Sandbox.Common.ObjectBuilders;
using PEPCO.iSurvival.CustomEntitySpawner;
using System.Collections.Immutable;
using Sandbox.Game.Entities.Character;

namespace PEPCO.iSurvival.CustomEntitySpawner
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class CustomEntitySpawner : MySessionComponentBase
    {
        private static HashSet<string> validBotIds = new HashSet<string>();
        private int updateTickCounter = 0;
        private long totalUpdateTicks = 0;
        private bool isLoading = true;
        private int loadingTickCount = 100;
        private static readonly Random randomGenerator = new Random();

        private HashSet<string> loadedFiles = new HashSet<string>();

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

; Block settings
; BlockId specifies the unique identifier for the block.
; Example: SmallBlockSmallContainer for a small cargo container.
; List of options: Any valid block ID.
;BlockId=SmallBlockSmallContainer

; BlockType specifies the type of block for which this configuration applies.
; Example: MyObjectBuilder_CargoContainer for cargo containers.
; List of options: MyObjectBuilder_CargoContainer, MyObjectBuilder_Refinery, MyObjectBuilder_Assembler, etc.
;BlockType=MyObjectBuilder_CargoContainer

; Enabled specifies whether this configuration is active.
; Values: true or false
; Example: true
;Enabled=true

; PlayerDistanceCheck is the maximum distance from a player for spawning entities.
; Example: 100 (100 meters)
; List of options: Any positive integer, -1 to disable the check.
;PlayerDistanceCheck=100

; Entity spawning settings
; EnableEntitySpawning specifies whether entities should be spawned.
; Values: true or false
;EnableEntitySpawning=true

; EntityID specifies the ID of the entities to spawn.
; Example: Wolf
; List of options: Any valid entity ID such as Wolf, Spider, etc.
;EntityID=Wolf

; MinEntityAmount is the minimum number of entities to spawn when conditions are met.
; Example: 1
; List of options: Any positive integer.
;MinEntityAmount=1

; MaxEntityAmount is the maximum number of entities to spawn when conditions are met.
; Example: 1
; List of options: Any positive integer.
;MaxEntityAmount=1

; MaxEntitiesInArea is the maximum number of entities allowed in the area for spawning.
; Example: 30
; List of options: Any positive integer.
;MaxEntitiesInArea=30

; MaxEntitiesRadius is the radius (in meters) within which the MaxEntitiesInArea limit is checked.
; This radius is spherical, meaning it is measured in 3D space.
; Example: 100 (100 meters)
; List of options: Any positive float value.
;MaxEntitiesRadius=100


; Item spawning settings
; EnableItemSpawning specifies whether items should be spawned.
; Values: true or false
;EnableItemSpawning=true

; ItemTypes specifies the types of items to spawn.
; Example: MyObjectBuilder_Component
; List of options: MyObjectBuilder_Component, MyObjectBuilder_Ore, MyObjectBuilder_Ingot, MyObjectBuilder_ConsumableItem, etc.
;ItemTypes=MyObjectBuilder_Component

; ItemIds specifies the IDs of the items to spawn.
; Example: SteelPlate
; List of options: Any valid item ID such as SteelPlate, Iron, etc.
;ItemIds=SteelPlate

; MinItemAmount is the minimum number of items to spawn when conditions are met.
; Example: 1
; List of options: Any positive integer.
;MinItemAmount=1

; MaxItemAmount is the maximum number of items to spawn when conditions are met.
; Example: 1
; List of options: Any positive integer.
;MaxItemAmount=1

; UseWeightedDrops determines if the number of items spawned should use weighted probabilities.
; Values: true or false
; Example: false
;UseWeightedDrops=false

; StackItems specifies whether items should be stacked when spawned.
; Values: true or false
;StackItems=false

; SpawnInsideInventory specifies whether items should be spawned inside the inventory of the block.
; Values: true or false
;SpawnInsideInventory=false

; SpawnItemsWithEntities specifies whether items should be spawned only when an entity is spawned.
; Values: true or false
;SpawnItemsWithEntities=true

; Environmental conditions
; DamageAmount is the amount of damage to apply to the block each time entities are spawned.
; Example: 0
; List of options: Any non-negative float value.
;DamageAmount=0

; MinHealthPercentage is the minimum health percentage the block must have to allow spawning.
; Example: 0.2 (20%)
; List of options: Any float value between 0 and 1.
;MinHealthPercentage=0.2

; MaxHealthPercentage is the maximum health percentage the block can have to allow spawning.
; Example: 1 (100%)
; List of options: Any float value between 0 and 1.
;MaxHealthPercentage=1

; MinHeight is the minimum height offset for spawning entities.
; Example: 0.5
; List of options: Any non-negative float value.
;MinHeight=0.5

; MaxHeight is the maximum height offset for spawning entities.
; Example: 2.0
; List of options: Any non-negative float value.
;MaxHeight=2.0

; MinRadius is the minimum radius for spawning entities around the block.
; Example: 0.5
; List of options: Any non-negative float value.
;MinRadius=0.5

; MaxRadius is the maximum radius for spawning entities around the block.
; Example: 2.0
; List of options: Any non-negative float value.
;MaxRadius=2.0

; SpawnTriggerInterval is the interval in update ticks for triggering entity spawn.
; Example: 3 (every 3 updates)
; List of options: Any positive integer.
;SpawnTriggerInterval=3

; EnableAirtightAndOxygen determines if airtight and oxygen levels are considered for spawning.
; Values: true or false
; Example: false
;EnableAirtightAndOxygen=false

; Required items for spawning (to be removed)
; RequiredItemTypes specifies the types of items required in the inventory for spawning (to be removed).
; Example: MyObjectBuilder_Component
; List of options: MyObjectBuilder_Component, MyObjectBuilder_Ore, MyObjectBuilder_Ingot, MyObjectBuilder_ConsumableItem, etc.
;RequiredItemTypes=MyObjectBuilder_Component,MyObjectBuilder_Component

; RequiredItemIds specifies the IDs of the required items (to be removed).
; Example: SteelPlate
; List of options: Any valid item ID such as SteelPlate, Iron, etc.
;RequiredItemIds=SteelPlate,InteriorPlate

; RequiredItemAmounts specifies the amounts of the required items (to be removed).
; Example: 5
; List of options: Any positive integer.
;RequiredItemAmounts=1,1

; Permanent required items for spawning (not removed)
; PermanentRequiredItemTypes specifies the types of items required in the inventory for spawning (not removed).
; Example: MyObjectBuilder_Ore
; List of options: MyObjectBuilder_Component, MyObjectBuilder_Ore, MyObjectBuilder_Ingot, MyObjectBuilder_ConsumableItem, etc.
;PermanentRequiredItemTypes=MyObjectBuilder_Ore,MyObjectBuilder_Ore

; PermanentRequiredItemIds specifies the IDs of the permanent required items (not removed).
; Example: Iron
; List of options: Any valid item ID such as Iron, SteelPlate, etc.
;PermanentRequiredItemIds=Iron,Gold

; PermanentRequiredItemAmounts specifies the amounts of the permanent required items (not removed).
; Example: 10
; List of options: Any positive integer.
;PermanentRequiredItemAmounts=1,1

; Required entities in the vicinity for spawning
; RequiredEntity specifies the entity type required in the vicinity for spawning.
; Example: Wolf
; List of options: Any valid entity ID such as Wolf, Spider, etc.
;RequiredEntity=Wolf

; RequiredEntityRadius is the radius within which the required entity must be present.
; Example: 10 (10 meters)
; List of options: Any positive float value.
;RequiredEntityRadius=10

; RequiredEntityNumber is the number of required entities needed for spawning.
; Example: 0 (no specific number required)
; List of options: Any positive integer.
;RequiredEntityNumber=0

; RequireEntityNumberForTotalEntities determines if the required entity number is for the total entities.
; Values: true or false
; Example: false
;RequireEntityNumberForTotalEntities=false

; MaxEntitiesInArea is the maximum number of entities allowed in the area for spawning.
; Example: 30
; List of options: Any positive integer.
;MaxEntitiesInArea=30
";

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            if (MyAPIGateway.Session.IsServer)
            {

                try
                {
                    EnsureDefaultIniFilesExist();
                    CopyAllCESFilesToWorldStorage();
                    LoadAllFilesFromWorldStorage();
                    settings.Load();
                    LoadValidBotIds();
                    InitializeBotSpawnerConfig();
                    MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
                }
                catch (Exception ex)
                {
                    MyAPIGateway.Utilities.ShowMessage("Custom Entity Spawner", $"Initialization error: {ex.Message}");
                    LogError($"Initialization error: {ex.Message}");
                }
            }
        }

        private void EnsureDefaultIniFilesExist()
        {
            if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(CustomEntitySpawnerSettings.GlobalFileName, typeof(CustomEntitySpawnerSettings)))
            {
                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(CustomEntitySpawnerSettings.GlobalFileName, typeof(CustomEntitySpawnerSettings)))
                {
                    writer.Write(DefaultGlobalIniContent);
                }
            }

            if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(CustomEntitySpawnerSettings.EntitySpawnerFileName, typeof(CustomEntitySpawnerSettings)))
            {
                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(CustomEntitySpawnerSettings.EntitySpawnerFileName, typeof(CustomEntitySpawnerSettings)))
                {
                    writer.Write(DefaultEntitySpawnerIniContent);
                }
            }
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
            validBotIds.Clear();
            base.UnloadData();
        }

        private void OnMessageEntered(string messageText, ref bool sendToOthers)
        {
            var player = MyAPIGateway.Session.Player;

            if (player == null || !MyAPIGateway.Session.IsUserAdmin(player.SteamUserId))
            {
                MyAPIGateway.Utilities.ShowMessage("BotSpawner", "Only admins can use this command.");
                return;
            }

            var command = messageText.Split(' ');
            switch (command[0].ToLower())
            {
                case "/pepco":
                    HandlePepcoCommand(command, ref sendToOthers);
                    break;
            }
        }

        private void HandlePepcoCommand(string[] command, ref bool sendToOthers)
        {
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
                    if (command.Length > 2 && command[2].ToLower() == "dead")
                    {
                        CleanupDeadEntities();
                        sendToOthers = false;
                    }
                    break;
                case "show":
                    if (command.Length > 2 && command[2].ToLower() == "all")
                    {
                        ShowAllEntities();
                        sendToOthers = false;
                    }
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
            if (command.Length == 3)
            {
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
                    default:
                        MyAPIGateway.Utilities.ShowMessage("PEPCO", "Unknown CES command. Use /pepco help for a list of commands.");
                        break;
                }
            }
        }

        private void HandleKillCommand(string[] command, ref bool sendToOthers)
        {
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

                if (MyAPIGateway.Session.IsServer)
                {
                    if (entityName.Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        KillAllEntities(radius);
                    }
                    else
                    {
                        KillEntitiesByName(entityName, radius);
                    }
                }
                sendToOthers = false;
            }
            else
            {
                MyAPIGateway.Utilities.ShowMessage("BotSpawner", "Usage: /pepco kill <EntityName|all> [Radius]");
            }
        }

        private void ShowHelp()
        {
            var commands = new List<string>
    {
        "/pepco help - Displays this help message.",
        "/pepco CES reload - Reloads the Custom Entity Spawner settings.",
        "/pepco CES list - Lists all blocks and their spawn settings.",
        "/pepco kill <EntityName|all> [Radius] - Kills entities by name or all entities within a radius.",
        "/pepco cleanup dead - Cleans up dead entities.",
        "/pepco show all - Shows all entities.",
        "/pepco spawn <BotID> - Spawns a bot with the specified ID.",
        "/pepco listBots - Lists all valid bot IDs."
    };

            foreach (var command in commands)
            {
                MyAPIGateway.Utilities.ShowMessage("PEPCO Commands", command);
            }
        }

        private void ShowAllEntities()
        {
            var entities = new HashSet<IMyEntity>();
            IMyCharacter playerCharacter = MyAPIGateway.Session.Player.Character;
            MyAPIGateway.Entities.GetEntities(entities);
            foreach (var entity in entities)
            {
                var character = entity as IMyCharacter;
                if (entity is IMyCharacter && character != playerCharacter && !character.IsDead)
                {
                    string entityType = entity.GetType().Name;
                    string entityName = entity.DisplayName ?? "Unnamed";

                    MyAPIGateway.Utilities.ShowMessage("Entity", $"Type: {entityType}, Name: {entityName}");

                }
            }
        }
        private void ListAllBlocksAndSpawns()
        {
            if (settings.BlockSpawnSettings.Count == 0)
            {
                MyAPIGateway.Utilities.ShowMessage("CustomEntitySpawner", "No block spawn settings found.");
                return;
            }

            foreach (var blockSettings in settings.BlockSpawnSettings)
            {
                string message = $"BlockId: {blockSettings.BlockId}, BlockType: {blockSettings.BlockType}, Entities: {string.Join(", ", blockSettings.EntityID)}";
                MyAPIGateway.Utilities.ShowMessage("CustomEntitySpawner", message);
            }
        }

        private void CleanupDeadEntities()
        {
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
            var character = entity as IMyCharacter;
            if (character != null && character.IsDead)
            {
                return true;
            }
            return false;
        }

        private void KillEntitiesByName(string entityName, double radius)
        {
            var entities = new HashSet<IMyEntity>();
            Vector3D playerPosition = MyAPIGateway.Session.Player.GetPosition();
            IMyCharacter playerCharacter = MyAPIGateway.Session.Player.Character;

            MyAPIGateway.Entities.GetEntities(entities, e => e is IMyCharacter && e.DisplayName != null && e.DisplayName.IndexOf(entityName, StringComparison.OrdinalIgnoreCase) >= 0);

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

            MyAPIGateway.Utilities.ShowMessage("BotSpawner", $"Killed {entitiesKilled} entities with name: {entityName} within {radius} meters.");
        }

        private void KillAllEntities(double radius)
        {
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

        private int CountEntitiesInRadius(Vector3D position, double radius)
        {
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

        public override void UpdateBeforeSimulation()
        {
            if (isLoading && loadingTickCount-- > 0)
            {
                return;
            }
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
                    LogError($"Update error: {ex.Message}");
                }
            }
            if (settings.CleanupInterval != 0)
            {
                if (++cleanupTickCounter >= settings.CleanupInterval)
                {
                    cleanupTickCounter = 0;
                    try
                    {
                        CleanupDeadEntities();
                    }
                    catch (Exception ex)
                    {
                        LogError($"Cleanup error: {ex.Message}");
                    }
                }

            }
        }

        private static void LoadValidBotIds()
        {
            var botDefinitions = MyDefinitionManager.Static.GetBotDefinitions();
            foreach (var botDefinition in botDefinitions)
            {
                validBotIds.Add(botDefinition.Id.SubtypeName);
            }
        }

        private static void ListValidBotIds()
        {
            foreach (var botId in validBotIds)
            {
                MyAPIGateway.Utilities.ShowMessage("BotSpawner", botId);
            }
        }

        private void SpawnEntitiesNearBlocks(ref int entitiesSpawned)
        {
            long baseUpdateCycles = totalUpdateTicks / settings.BaseUpdateInterval;
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var entity in MyEntities.GetEntities())
            {
                var grid = entity as IMyCubeGrid;
                if (grid != null)
                {
                    var blocks = new List<IMySlimBlock>();
                    grid.GetBlocks(blocks, b => b.FatBlock != null && IsValidBlock(b.FatBlock.BlockDefinition.TypeIdString, b.FatBlock.BlockDefinition.SubtypeId));

                    foreach (var block in blocks)
                    {
                        foreach (var blockSettings in settings.BlockSpawnSettings)
                        {
                            if (block.FatBlock.BlockDefinition.TypeIdString == blockSettings.BlockType && block.FatBlock.BlockDefinition.SubtypeId == blockSettings.BlockId)
                            {
                                int spawnIterations;
                                if (blockSettings.Enabled && baseUpdateCycles % blockSettings.SpawnTriggerInterval == 0 && IsEnvironmentSuitable(grid, block, out spawnIterations))
                                {
                                    if (!IsPlayerInRange(block, players, blockSettings.PlayerDistanceCheck))
                                    {
                                        continue;
                                    }

                                    float blockHealthPercentage = block.Integrity / block.MaxIntegrity;
                                    if (blockHealthPercentage >= blockSettings.MinHealthPercentage && blockHealthPercentage <= blockSettings.MaxHealthPercentage)
                                    {
                                        if (CheckInventoryForRequiredItems(block, blockSettings))
                                        {
                                            if (blockSettings.EnableEntitySpawning)
                                            {
                                                int currentEntityCount = GetTotalEntityCount();
                                                ShowMessageIfLoggingEnabled($"Total Entities: {currentEntityCount}");

                                                if (currentEntityCount >= settings.GlobalMaxEntities)
                                                {
                                                    ShowMessageIfLoggingEnabled($"Entities:{currentEntityCount}. Global max entities limit exceeded.");
                                                    return;
                                                }
                                            }

                                            for (int i = 0; i < spawnIterations; i++)
                                            {
                                                if (blockSettings.EnableEntitySpawning)
                                                {
                                                    int entitySpawnAmount = randomGenerator.Next(blockSettings.MinEntityAmount, blockSettings.MaxEntityAmount + 1);

                                                    if (entitySpawnAmount > 0)
                                                    {
                                                        SpawnEntities(block, blockSettings, entitySpawnAmount);
                                                        entitiesSpawned++;
                                                    }
                                                }

                                                if (!blockSettings.SpawnItemsWithEntities && blockSettings.EnableItemSpawning)
                                                {
                                                    int itemSpawnAmount = blockSettings.UseWeightedDrops ?
                                                        GetWeightedRandomNumber(blockSettings.MinItemAmount, GenerateProbabilities(blockSettings.MinItemAmount, blockSettings.MaxItemAmount)) :
                                                        randomGenerator.Next(blockSettings.MinItemAmount, blockSettings.MaxItemAmount + 1);

                                                    if (itemSpawnAmount > 0)
                                                    {
                                                        SpawnItems(block, blockSettings, itemSpawnAmount);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }



        private int GetTotalEntityCount()
        {
            var entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities, entity =>
            {

                IMyCharacter playerCharacter = MyAPIGateway.Session.Player.Character;
                // Check if entity is IMyCharacter but not playerCharacter or dead
                var character = entity as IMyCharacter;
                if (entity is IMyCharacter && character != playerCharacter && !character.IsDead)
                {
                    return true;
                }

                return false; // exclude other entities
            });

            return entities.Count;
        }
        private bool IsPlayerInRange(IMySlimBlock block, List<IMyPlayer> players, int playerDistanceCheck)
        {
            if (playerDistanceCheck == -1)
            {
                return true;
            }

            if (playerDistanceCheck == 0)
            {
                return players.Count > 0;
            }

            Vector3D blockPosition = block.FatBlock.GetPosition();

            foreach (var player in players)
            {
                var controlledEntity = player.Controller?.ControlledEntity?.Entity;
                if (controlledEntity != null && controlledEntity is IMyCharacter && player == MyAPIGateway.Session.LocalHumanPlayer)
                {
                    Vector3D playerPosition = controlledEntity.GetPosition();
                    double distance = Vector3D.Distance(playerPosition, blockPosition);
                    if (distance <= playerDistanceCheck)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsValidBlock(string typeId, string subtypeId)
        {
            return settings.BlockSpawnSettings.Exists(s => s.BlockType == typeId && s.BlockId == subtypeId);
        }

        private BotSpawnerConfig GetSpawnSettingsForBlock(string typeId, string subtypeId)
        {
            return settings.BlockSpawnSettings.Find(s => s.BlockType == typeId && s.BlockId == subtypeId);
        }

        private bool IsEnvironmentSuitable(IMyCubeGrid grid, IMySlimBlock block, out int spawnIterations)
        {
            spawnIterations = 0;
            var blockSettings = GetSpawnSettingsForBlock(block.FatBlock.BlockDefinition.TypeIdString, block.FatBlock.BlockDefinition.SubtypeId);
            if (blockSettings == null)
                return true;

            var functionalBlock = block.FatBlock as IMyFunctionalBlock;
            if (functionalBlock != null)
            {
                if (!functionalBlock.IsFunctional || !functionalBlock.Enabled || !functionalBlock.IsWorking)
                {
                    return false;
                }
            }

            if (blockSettings.EnableAirtightAndOxygen)
            {
                bool isAirtight = grid.IsRoomAtPositionAirtight(block.FatBlock.Position);
                double oxygenLevel = MyAPIGateway.Session.OxygenProviderSystem.GetOxygenInPoint(block.FatBlock.GetPosition());
                if (!isAirtight && oxygenLevel <= 0.5)
                    return false;
            }

            if (!string.IsNullOrEmpty(blockSettings.RequiredEntity))
            {
                int entityCount = GetEntityCountInRadius(block.FatBlock.GetPosition(), blockSettings.RequiredEntityRadius);

                if (blockSettings.MaxEntitiesInArea > 0 && entityCount >= blockSettings.MaxEntitiesInArea)
                {
                    return false;
                }

                if (blockSettings.RequireEntityNumberForTotalEntities)
                {
                    if (entityCount >= blockSettings.RequiredEntityNumber)
                    {
                        spawnIterations = entityCount / blockSettings.RequiredEntityNumber;
                        return true;
                    }
                }
                else
                {
                    spawnIterations = entityCount >= blockSettings.RequiredEntityNumber ? 1 : 0;
                    return entityCount >= blockSettings.RequiredEntityNumber;
                }
            }

            // New condition to check MaxEntitiesInArea within MaxEntitiesRadius
            if (blockSettings.MaxEntitiesInArea > 0)
            {
                int entityCount = GetEntityCountInRadius(block.FatBlock.GetPosition(), blockSettings.MaxEntitiesRadius);
                if (entityCount >= blockSettings.MaxEntitiesInArea)
                {
                    return false;
                }
            }

            return true;
        }



        private int GetEntityCountInRadius(Vector3D position, double radius)
        {
            var entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities, e => e is IMyCharacter);

            int entityCount = 0;
            foreach (var entity in entities)
            {
                if (Vector3D.Distance(entity.GetPosition(), position) <= radius)
                {
                    entityCount++;
                }
            }
            return entityCount;
        }

        private bool CheckInventoryForRequiredItems(IMySlimBlock block, BotSpawnerConfig blockSettings)
        {
            var inventory = block.FatBlock.GetInventory() as IMyInventory;
            if (inventory == null)
            {
                return true;
            }

            for (int i = 0; i < blockSettings.RequiredItemTypes.Count; i++)
            {
                var requiredItemType = new MyDefinitionId(itemTypeMappings[blockSettings.RequiredItemTypes[i]], blockSettings.RequiredItemIds[i]);
                var itemAmount = (VRage.MyFixedPoint)blockSettings.RequiredItemAmounts[i];

                if (!inventory.ContainItems(itemAmount, requiredItemType))
                {
                    return false;
                }
            }

            for (int i = 0; i < blockSettings.PermanentRequiredItemTypes.Count; i++)
            {
                var permanentRequiredItemType = new MyDefinitionId(itemTypeMappings[blockSettings.PermanentRequiredItemTypes[i]], blockSettings.PermanentRequiredItemIds[i]);
                var permanentItemAmount = (VRage.MyFixedPoint)blockSettings.PermanentRequiredItemAmounts[i];

                if (!inventory.ContainItems(permanentItemAmount, permanentRequiredItemType))
                {
                    return false;
                }
            }

            return true;
        }

        private void RemoveItemsFromInventory(IMySlimBlock block, BotSpawnerConfig blockSettings, int dropAmount)
        {
            var inventory = block.FatBlock.GetInventory() as IMyInventory;
            if (inventory == null)
                return;

            for (int i = 0; i < blockSettings.RequiredItemTypes.Count; i++)
            {
                var requiredItemType = new MyDefinitionId(itemTypeMappings[blockSettings.RequiredItemTypes[i]], blockSettings.RequiredItemIds[i]);
                var totalAmountToRemove = (VRage.MyFixedPoint)(blockSettings.RequiredItemAmounts[i] * dropAmount);

                inventory.RemoveItemsOfType(totalAmountToRemove, requiredItemType);
            }
        }

        private void SpawnEntities(IMySlimBlock block, BotSpawnerConfig settings, int spawnAmount)
        {
            if (settings.EnableEntitySpawning)
            {
                for (int i = 0; i < settings.EntityID.Count; i++)
                {
                    string entityID = settings.EntityID[i];
                    for (int j = 0; j < spawnAmount; j++)
                    {
                        int entityAmount = randomGenerator.Next(settings.MinEntityAmount, settings.MaxEntityAmount + 1);
                        Vector3D spawnPosition = CalculateDropPosition(block, settings);
                        Quaternion dropRotation = Quaternion.CreateFromYawPitchRoll(
                            (float)(randomGenerator.NextDouble() * Math.PI * 2),
                            (float)(randomGenerator.NextDouble() * Math.PI * 2),
                            (float)(randomGenerator.NextDouble() * Math.PI * 2)
                        );
                        MyVisualScriptLogicProvider.SpawnBot(entityID, spawnPosition);

                        if (settings.SpawnItemsWithEntities && settings.EnableItemSpawning)
                        {
                            SpawnItems(block, settings, spawnAmount);
                        }
                    }
                }
            }
        }



        private void SpawnItems(IMySlimBlock block, BotSpawnerConfig settings, int spawnAmount)
        {
            for (int k = 0; k < settings.ItemTypes.Count; k++)
            {
                var itemType = settings.ItemTypes[k];
                var itemId = settings.ItemIds[k];
                var amount = settings.UseWeightedDrops ?
                    GetWeightedRandomNumber(settings.MinItemAmount, GenerateProbabilities(settings.MinItemAmount, settings.MaxItemAmount)) :
                    randomGenerator.Next(settings.MinItemAmount, settings.MaxItemAmount + 1);

                if (settings.SpawnInsideInventory)
                {
                    PlaceItemInCargo(block, itemType, itemId, amount, settings.StackItems);
                }
                else
                {
                    SpawnItemNearBlock(block, itemType, itemId, amount, settings.StackItems, settings);
                }
            }
        }

        private void PlaceItemInCargo(IMySlimBlock block, string itemType, string itemId, int amount, bool stackItems)
        {
            var inventory = block.FatBlock.GetInventory() as IMyInventory;
            if (inventory != null)
            {
                var itemDefinition = new MyDefinitionId(itemTypeMappings[itemType], itemId);
                var itemObjectBuilder = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(itemDefinition);
                var physicalItem = new MyPhysicalInventoryItem((VRage.MyFixedPoint)amount, itemObjectBuilder);

                if (stackItems)
                {
                    inventory.AddItems(amount, physicalItem.Content);
                }
                else
                {
                    for (int i = 0; i < amount; i++)
                    {
                        inventory.AddItems(1, physicalItem.Content);
                    }
                }
            }
        }
        private void SpawnItemNearBlock(IMySlimBlock block, string itemType, string itemId, int amount, bool stackItems, BotSpawnerConfig settings)
        {
            var itemDefinition = new MyDefinitionId(itemTypeMappings[itemType], itemId);
            var itemObjectBuilder = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(itemDefinition);

            if (stackItems)
            {
                Vector3D dropPosition = CalculateDropPosition(block, settings);
                Quaternion dropRotation = Quaternion.CreateFromYawPitchRoll(
                    (float)(randomGenerator.NextDouble() * Math.PI * 2),
                    (float)(randomGenerator.NextDouble() * Math.PI * 2),
                    (float)(randomGenerator.NextDouble() * Math.PI * 2)
                );

                MyFloatingObjects.Spawn(
                    new MyPhysicalInventoryItem((VRage.MyFixedPoint)amount, itemObjectBuilder),
                    dropPosition, dropRotation.Up, block.FatBlock.WorldMatrix.Up
                );
            }
            else
            {
                for (int j = 0; j < amount; j++)
                {
                    Vector3D dropPosition = CalculateDropPosition(block, settings);
                    Quaternion dropRotation = Quaternion.CreateFromYawPitchRoll(
                        (float)(randomGenerator.NextDouble() * Math.PI * 2),
                        (float)(randomGenerator.NextDouble() * Math.PI * 2),
                        (float)(randomGenerator.NextDouble() * Math.PI * 2)
                    );

                    MyFloatingObjects.Spawn(
                        new MyPhysicalInventoryItem((VRage.MyFixedPoint)1, itemObjectBuilder),
                        dropPosition, dropRotation.Up, block.FatBlock.WorldMatrix.Up
                    );
                }
            }
        }

        private Vector3D CalculateDropPosition(IMySlimBlock block, BotSpawnerConfig settings)
        {
            double heightOffset = randomGenerator.NextDouble() * (settings.MaxHeight - settings.MinHeight) + settings.MinHeight;
            double radius = randomGenerator.NextDouble() * (settings.MaxRadius - settings.MinRadius) + settings.MinRadius;
            double angle = randomGenerator.NextDouble() * Math.PI * 2;

            double xOffset = radius * Math.Cos(angle);
            double yOffset = radius * Math.Sin(angle);

            return block.FatBlock.GetPosition() +
                   (block.FatBlock.WorldMatrix.Up * heightOffset) +
                   (block.FatBlock.WorldMatrix.Right * xOffset) +
                   (block.FatBlock.WorldMatrix.Forward * yOffset);
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

        private void InitializeBotSpawnerConfig()
        {
            settings.BlockSpawnSettings.Clear();
            settings.Load();

            foreach (var botSpawnerConfig in settings.BlockSpawnSettings)
            {
                LogError($"Initialized settings for {botSpawnerConfig.BlockId}: " +
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
                         $"RequireEntityNumberForTotalEntities={botSpawnerConfig.RequireEntityNumberForTotalEntities}");
            }

            LogError("Drop settings initialized.");
        }

        private void ReloadSettings()
        {
            try
            {
                MyAPIGateway.Utilities.ShowMessage("PEPCO", "Reloading settings...");
                LogError("Reloading settings...");

                EnsureDefaultIniFilesExist();
                CopyAllCESFilesToWorldStorage();
                LoadAllFilesFromWorldStorage();
                InitializeBotSpawnerConfig();

                MyAPIGateway.Utilities.ShowMessage("PEPCO", "Settings reloaded successfully.");
                LogError("Settings reloaded successfully.");
            }
            catch (Exception ex)
            {
                MyAPIGateway.Utilities.ShowMessage("PEPCO", $"Error reloading settings: {ex.Message}");
                LogError($"Error reloading settings: {ex.Message}");
            }
        }

        private void CopyAllCESFilesToWorldStorage()
        {
            LogError("Starting CopyAllCESFilesToWorldStorage");
            try
            {
                foreach (var modItem in MyAPIGateway.Session.Mods)
                {
                    string modFilePath = "Data/CES.ini";
                    LogError($"Checking for file: {modFilePath} in mod: {modItem.PublishedFileId}");

                    if (MyAPIGateway.Utilities.FileExistsInModLocation(modFilePath, modItem))
                    {
                        LogError($"File found: {modFilePath} in mod: {modItem.PublishedFileId}");

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

                            LogError($"File copied to World Storage successfully from mod {modItem.PublishedFileId}.");
                        }
                        else
                        {
                            LogError($"File already exists: {worldStorageFilePath} for mod: {modItem.PublishedFileId}");
                        }
                    }
                    else
                    {
                        LogError($"File not found: {modFilePath} in mod: {modItem.PublishedFileId}");
                    }
                }
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLine($"Error copying files to World Storage: {ex.Message}");
                LogError($"Error copying files to World Storage: {ex.Message}");
            }
        }

        private void LoadAllFilesFromWorldStorage()
        {
            try
            {
                HashSet<string> loadedFiles = new HashSet<string>(); // Set to track loaded files

                List<string> knownConfigFiles = new List<string>
        {
            "GlobalConfig.ini",
            "CustomEntitySpawner.ini"
        };

                List<string> additionalFilePatterns = new List<string>
        {
            "_CES.ini"
        };

                settings.BlockSpawnSettings.Clear(); // Clear existing settings to avoid duplicates

                // Load known config files
                foreach (string configFileName in knownConfigFiles)
                {
                    LogError($"Looking at {configFileName}");
                    if (MyAPIGateway.Utilities.FileExistsInWorldStorage(configFileName, typeof(CustomEntitySpawner)) && !loadedFiles.Contains(configFileName))
                    {
                        LogError($"Found in world storage: {configFileName}");
                        using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(configFileName, typeof(CustomEntitySpawner)))
                        {
                            string fileContent = reader.ReadToEnd();
                            if (configFileName.Equals("GlobalConfig.ini", StringComparison.OrdinalIgnoreCase))
                            {
                                LogError($"Loading GlobalConfig: {configFileName}");
                                LoadGlobalConfig(fileContent);
                            }
                            else if (configFileName.Equals("CustomEntitySpawner.ini", StringComparison.OrdinalIgnoreCase))
                            {
                                LogError($"Loading CustomEntitySpawnerConfig: {configFileName}");
                                LoadCustomEntitySpawnerConfig(fileContent);
                            }
                            loadedFiles.Add(configFileName);
                            ShowMessageIfLoggingEnabled($"Loaded: {configFileName}");
                        }
                    }
                }

                // Load additional mod-specific config files
                foreach (var modItem in MyAPIGateway.Session.Mods)
                {
                    LogError($"Looking for: {modItem.PublishedFileId} in world storage folder");
                    foreach (string pattern in additionalFilePatterns)
                    {
                        string cesFileName = $"{modItem.PublishedFileId}{pattern}";
                        LogError($"Checking for: {cesFileName} in mod: {modItem.PublishedFileId}");
                        if (MyAPIGateway.Utilities.FileExistsInWorldStorage(cesFileName, typeof(CustomEntitySpawner)) && !loadedFiles.Contains(cesFileName))
                        {
                            LogError($"Found {cesFileName} in world storage");
                            using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(cesFileName, typeof(CustomEntitySpawner)))
                            {
                                string fileContent = reader.ReadToEnd();
                                LogError($"Loading config from: {cesFileName}");
                                LoadCustomEntitySpawnerConfig(fileContent);
                                loadedFiles.Add(cesFileName);
                                ShowMessageIfLoggingEnabled($"Addon Loaded: {cesFileName}");
                            }
                        }
                        else
                        {
                            LogError($"Not found: {cesFileName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLine($"Error loading files from World Storage: {ex.Message}");
                LogError($"Error loading files from World Storage: {ex.Message}");
            }
        }

        private void ShowMessageIfLoggingEnabled(string message)
        {
            if (settings.EnableLogging)
            {
                MyAPIGateway.Utilities.ShowMessage("CES:", message);
                LogError($"CES: {message}");

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
            settings.EnableLogging = iniParser.Get("Config", "EnableLogging").ToBoolean(true);
            settings.CleanupInterval = iniParser.Get("Config", "CleanupInterval").ToInt32(180);
            settings.GlobalMaxEntities = iniParser.Get("Config", "GlobalMaxEntities").ToInt32(100); // Load new setting
        }

        private void LoadCustomEntitySpawnerConfig(string fileContent)
        {
            try
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
                    if (section == "Config")
                        continue;

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
                        MaxEntitiesRadius = iniParser.Get(section, nameof(BotSpawnerConfig.MaxEntitiesRadius)).ToDouble(100), // Default value of 100
                        StackItems = iniParser.Get(section, nameof(BotSpawnerConfig.StackItems)).ToBoolean(),
                        SpawnInsideInventory = iniParser.Get(section, nameof(BotSpawnerConfig.SpawnInsideInventory)).ToBoolean(),
                        DamageAmount = (float)iniParser.Get(section, nameof(BotSpawnerConfig.DamageAmount)).ToDouble(),
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
                        RequireEntityNumberForTotalEntities = iniParser.Get(section, nameof(BotSpawnerConfig.RequireEntityNumberForTotalEntities)).ToBoolean()
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

                    LogError($"Loaded settings for section {section}: BlockId={botSpawnerConfig.BlockId}, BlockType={botSpawnerConfig.BlockType}, " +
                             $"Enabled={botSpawnerConfig.Enabled}, PlayerDistanceCheck={botSpawnerConfig.PlayerDistanceCheck}, " +
                             $"EnableEntitySpawning={botSpawnerConfig.EnableEntitySpawning}, EnableItemSpawning={botSpawnerConfig.EnableItemSpawning}, " +
                             $"SpawnItemsWithEntities={botSpawnerConfig.SpawnItemsWithEntities}, MinEntityAmount={botSpawnerConfig.MinEntityAmount}, " +
                             $"MaxEntityAmount={botSpawnerConfig.MaxEntityAmount}, MinItemAmount={botSpawnerConfig.MinItemAmount}, " +
                             $"MaxItemAmount={botSpawnerConfig.MaxItemAmount}, UseWeightedDrops={botSpawnerConfig.UseWeightedDrops}, " +
                             $"MaxEntitiesInArea={botSpawnerConfig.MaxEntitiesInArea}, MaxEntitiesRadius={botSpawnerConfig.MaxEntitiesRadius}, " +
                             $"StackItems={botSpawnerConfig.StackItems}, SpawnInsideInventory={botSpawnerConfig.SpawnInsideInventory}, " +
                             $"DamageAmount={botSpawnerConfig.DamageAmount}, MinHealthPercentage={botSpawnerConfig.MinHealthPercentage}, " +
                             $"MaxHealthPercentage={botSpawnerConfig.MaxHealthPercentage}, MinHeight={botSpawnerConfig.MinHeight}, " +
                             $"MaxHeight={botSpawnerConfig.MaxHeight}, MinRadius={botSpawnerConfig.MinRadius}, MaxRadius={botSpawnerConfig.MaxRadius}, " +
                             $"SpawnTriggerInterval={botSpawnerConfig.SpawnTriggerInterval}, EnableAirtightAndOxygen={botSpawnerConfig.EnableAirtightAndOxygen}, " +
                             $"RequiredEntity={botSpawnerConfig.RequiredEntity}, RequiredEntityRadius={botSpawnerConfig.RequiredEntityRadius}, " +
                             $"RequiredEntityNumber={botSpawnerConfig.RequiredEntityNumber}, RequireEntityNumberForTotalEntities={botSpawnerConfig.RequireEntityNumberForTotalEntities}");
                }
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLine($"Error loading CustomEntitySpawner config: {ex.Message}");
                LogError($"Error loading CustomEntitySpawner config: {ex.Message}");
            }
        }






        private void LogError(string message)
        {
            if (!settings.EnableLogging)
                return;

            string logFilePath = "CustomEntitySpawner.log";
            string existingContent = "";

            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(logFilePath, typeof(CustomEntitySpawner)))
            {
                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(logFilePath, typeof(CustomEntitySpawner)))
                {
                    existingContent = reader.ReadToEnd();
                }
            }

            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(logFilePath, typeof(CustomEntitySpawner)))
            {
                if (!string.IsNullOrEmpty(existingContent))
                {
                    writer.Write(existingContent);
                }
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
    }

    public class CustomEntitySpawnerSettings
    {
        public const string GlobalFileName = "GlobalConfig.ini";
        public const string EntitySpawnerFileName = "CustomEntitySpawner.ini";
        private const string IniSection = "Config";

        public int BaseUpdateInterval { get; set; } = 60;
        public bool EnableLogging { get; set; } = true;
        public int CleanupInterval { get; set; } = 18000;
        public int GlobalMaxEntities { get; set; } = 32;

        public List<BotSpawnerConfig> BlockSpawnSettings { get; set; } = new List<BotSpawnerConfig>();

        public void Load()
        {
            MyIni iniParser = new MyIni();

            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(GlobalFileName, typeof(CustomEntitySpawnerSettings)))
            {
                using (TextReader file = MyAPIGateway.Utilities.ReadFileInWorldStorage(GlobalFileName, typeof(CustomEntitySpawnerSettings)))
                {
                    string text = file.ReadToEnd();

                    MyIniParseResult result;
                    if (!iniParser.TryParse(text, out result))
                        throw new Exception($"Config error: {result.ToString()}");

                    BaseUpdateInterval = iniParser.Get(IniSection, nameof(BaseUpdateInterval)).ToInt32(BaseUpdateInterval);
                    EnableLogging = iniParser.Get(IniSection, nameof(EnableLogging)).ToBoolean(EnableLogging);
                    CleanupInterval = iniParser.Get(IniSection, nameof(CleanupInterval)).ToInt32(CleanupInterval);
                    GlobalMaxEntities = iniParser.Get(IniSection, nameof(GlobalMaxEntities)).ToInt32(GlobalMaxEntities); // Load new setting
                }
            }
            else
            {
                throw new Exception("GlobalConfig.ini not found.");
            }

            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(EntitySpawnerFileName, typeof(CustomEntitySpawnerSettings)))
            {
                using (TextReader file = MyAPIGateway.Utilities.ReadFileInWorldStorage(EntitySpawnerFileName, typeof(CustomEntitySpawnerSettings)))
                {
                    string text = file.ReadToEnd();

                    MyIniParseResult result;
                    if (!iniParser.TryParse(text, out result))
                        throw new Exception($"Config error: {result.ToString()}");

                    LoadBlockSpawnSettings(iniParser);
                }
            }
            else
            {
                throw new Exception("CustomEntitySpawner.ini not found.");
            }

            foreach (var modItem in MyAPIGateway.Session.Mods)
            {
                string cesFileName = $"{modItem.PublishedFileId}_CES.ini";
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(cesFileName, typeof(CustomEntitySpawnerSettings)))
                {
                    using (TextReader file = MyAPIGateway.Utilities.ReadFileInWorldStorage(cesFileName, typeof(CustomEntitySpawnerSettings)))
                    {
                        string text = file.ReadToEnd();

                        MyIniParseResult result;
                        if (!iniParser.TryParse(text, out result))
                            throw new Exception($"Config error in {cesFileName}: {result.ToString()}");

                        LoadBlockSpawnSettings(iniParser);
                    }
                }
            }
        }

        private void LoadBlockSpawnSettings(MyIni iniParser)
        {
            List<string> sections = new List<string>();
            iniParser.GetSections(sections);

            foreach (var section in sections)
            {
                if (section == IniSection)
                    continue;

                var botSpawnerConfig = new BotSpawnerConfig
                {
                    BlockId = iniParser.Get(section, nameof(BotSpawnerConfig.BlockId)).ToString(section),
                    BlockType = iniParser.Get(section, nameof(BotSpawnerConfig.BlockType)).ToString(),
                    Enabled = iniParser.Get(section, nameof(BotSpawnerConfig.Enabled)).ToBoolean(),
                    PlayerDistanceCheck = iniParser.Get(section, nameof(BotSpawnerConfig.PlayerDistanceCheck)).ToInt32(),
                    EnableEntitySpawning = iniParser.Get(section, nameof(BotSpawnerConfig.EnableEntitySpawning)).ToBoolean(true),
                    EnableItemSpawning = iniParser.Get(section, nameof(BotSpawnerConfig.EnableItemSpawning)).ToBoolean(true),
                    MinEntityAmount = iniParser.Get(section, nameof(BotSpawnerConfig.MinEntityAmount)).ToInt32(),
                    MaxEntityAmount = iniParser.Get(section, nameof(BotSpawnerConfig.MaxEntityAmount)).ToInt32(),
                    MinItemAmount = iniParser.Get(section, nameof(BotSpawnerConfig.MinItemAmount)).ToInt32(),
                    MaxItemAmount = iniParser.Get(section, nameof(BotSpawnerConfig.MaxItemAmount)).ToInt32(),
                    UseWeightedDrops = iniParser.Get(section, nameof(BotSpawnerConfig.UseWeightedDrops)).ToBoolean(),
                    MaxEntitiesInArea = iniParser.Get(section, nameof(BotSpawnerConfig.MaxEntitiesInArea)).ToInt32(),
                    MaxEntitiesRadius = iniParser.Get(section, nameof(BotSpawnerConfig.MaxEntitiesRadius)).ToDouble(100), // Default value of 100
                    StackItems = iniParser.Get(section, nameof(BotSpawnerConfig.StackItems)).ToBoolean(),
                    SpawnInsideInventory = iniParser.Get(section, nameof(BotSpawnerConfig.SpawnInsideInventory)).ToBoolean(),
                    DamageAmount = (float)iniParser.Get(section, nameof(BotSpawnerConfig.DamageAmount)).ToDouble(),
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
                    RequireEntityNumberForTotalEntities = iniParser.Get(section, nameof(BotSpawnerConfig.RequireEntityNumberForTotalEntities)).ToBoolean()
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

                BlockSpawnSettings.Add(botSpawnerConfig);
            }
        }
    }

    public class BotSpawnerConfig
    {
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
    }
}


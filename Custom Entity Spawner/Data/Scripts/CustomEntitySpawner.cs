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
using static VRage.Game.MyObjectBuilder_Checkpoint;
using VRage.Game.Entity;
using VRage.ModAPI;
using Sandbox.Engine.Utils;
using VRage.ObjectBuilders;

using Sandbox.Common.ObjectBuilders;
using PEPCO.iSurvival.CustomEntitySpawner;

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

        private int cleanupTickCounter = 0;
        private const int CleanupInterval = 180; // Cleanup every 600 ticks (adjust as needed)

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
; ==============================================
[Config]
BaseUpdateInterval=60    
EnableLogging=false
";

        private const string DefaultEntitySpawnerIniContent = @"
; ==============================================
; HOW TO USE CustomEntitySpawner.ini
; ==============================================
;[LargeBlockSmallContainer]
;BlockType=MyObjectBuilder_CargoContainer
;MinAmount=1
;MaxAmount=5
;UseWeightedDrops=false
;DamageAmount=0
;MinHealthPercentage=0.2
;MaxHealthPercentage=1
;MinHeight=0.5
;MaxHeight=2.0
;MinRadius=0.5
;MaxRadius=2.0
;SpawnTriggerInterval=3
;EnableAirtightAndOxygen=false
;Enabled=true
;PlayerDistanceCheck=10
;EntityID=Wolf
;RequiredItemTypes=MyObjectBuilder_Component
;RequiredItemIds=SteelPlate
;RequiredItemAmounts=0
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
            //MyAPIGateway.Utilities.ShowMessage("STARTTEST", "EnsureDefaultIniFilesExist");
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

            if (messageText.StartsWith("/pepco spawn"))
            {
                var parameters = messageText.Split(' ');
                if (parameters.Length == 3)
                {
                    string botId = parameters[2];
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
            else if (messageText.Equals("/pepco listBots"))
            {
                ListValidBotIds();
            }
            else if (messageText.Equals("/pepco CES reload", StringComparison.OrdinalIgnoreCase))
            {
                if (MyAPIGateway.Session.IsServer)
                {
                    try
                    {
                        ReloadSettings();
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error reloading settings: {ex.Message}");
                    }
                }
                sendToOthers = false;
            }
            else if (messageText.StartsWith("/pepco kill ", StringComparison.OrdinalIgnoreCase))
            {
                var parameters = messageText.Split(' ');
                if (parameters.Length >= 3)
                {
                    string entityName = parameters[2];
                    double radius = 10;
                    if (parameters.Length == 4)
                    {
                        double parsedRadius;
                        if (double.TryParse(parameters[3], out parsedRadius))
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
            else if (messageText.Equals("/pepco cleanup dead", StringComparison.OrdinalIgnoreCase))
            {
                if (MyAPIGateway.Session.IsServer)
                {
                    try
                    {
                        CleanupDeadEntities();
                        MyAPIGateway.Utilities.ShowMessage("BotSpawner", "Cleanup of dead entities completed.");
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error during cleanup: {ex.Message}");
                    }
                }
                sendToOthers = false;
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

            // Add other criteria for different types of entities if necessary
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
                //MyAPIGateway.Utilities.ShowMessage("PEPCO", $"updateTickCounter >= settings.BaseUpdateInterval");
                updateTickCounter = 0;
                try
                {
                    //MyAPIGateway.Utilities.ShowMessage("PEPCO", $"UpdateBeforeSimulations:Try SpawnEntitiesNearBlocks");
                    int entitiesSpawned = 0;
                    SpawnEntitiesNearBlocks(ref entitiesSpawned);
                }
                catch (Exception ex)
                {
                    LogError($"Update error: {ex.Message}");
                }
            }
            if (++cleanupTickCounter >= CleanupInterval)
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


            ////MyAPIGateway.Utilities.ShowMessage("STARTTEST", "SpawnEntitiesNearBlocks");
            long baseUpdateCycles = totalUpdateTicks / settings.BaseUpdateInterval;
            //MyAPIGateway.Utilities.ShowMessage("PEPCO", $"Start SpawnEntitiesNearBlocks");
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

                        var blockSettings = GetSpawnSettingsForBlock(block.FatBlock.BlockDefinition.TypeIdString, block.FatBlock.BlockDefinition.SubtypeId);

                        if (blockSettings != null && blockSettings.Enabled && baseUpdateCycles % blockSettings.SpawnTriggerInterval == 0 && IsEnvironmentSuitable(grid, block))
                        {

                            if (!IsPlayerInRange(block, players, blockSettings.PlayerDistanceCheck))
                            {
                                continue;
                            }
                            float blockHealthPercentage = block.Integrity / block.MaxIntegrity;
                            if (blockHealthPercentage >= blockSettings.MinHealthPercentage && blockHealthPercentage <= blockSettings.MaxHealthPercentage)
                            {

                                //MyAPIGateway.Utilities.ShowMessage("SPAWNTEST", $"Before CheckInventoryForRequiredItems");
                                if (CheckInventoryForRequiredItems(block, blockSettings))
                                {
                                    //MyAPIGateway.Utilities.ShowMessage("SPAWNTEST", $"After CheckInventoryForRequiredItems {block}");                                        

                                    int spawnAmount = SpawnEntities(block, blockSettings);
                                    if (spawnAmount > 0)
                                    {
                                        RemoveItemsFromInventory(block, blockSettings, spawnAmount);
                                        block.DoDamage(blockSettings.DamageAmount * spawnAmount, MyDamageType.Grind, true);
                                    }
                                }
                            }
                        }
                    }
                }

            }
        }

        private bool IsPlayerInRange(IMySlimBlock block, List<IMyPlayer> players, int playerDistanceCheck)
        {
            // Always on if playerDistanceCheck is -1
            if (playerDistanceCheck == -1)
            {
                return true;
            }

            // Any player online if playerDistanceCheck is 0
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
            ////MyAPIGateway.Utilities.ShowMessage("STARTTEST", "IsValidBlock");
            return settings.BlockSpawnSettings.Exists(s => s.BlockType == typeId && s.BlockId == subtypeId);
        }

        private BotSpawnerConfig GetSpawnSettingsForBlock(string typeId, string subtypeId)
        {
            return settings.BlockSpawnSettings.Find(s => s.BlockType == typeId && s.BlockId == subtypeId);
        }

        private bool IsEnvironmentSuitable(IMyCubeGrid grid, IMySlimBlock block)
        {
            ////MyAPIGateway.Utilities.ShowMessage("STARTTEST", "IsEnvironmentSuitable");
            var blockSettings = GetSpawnSettingsForBlock(block.FatBlock.BlockDefinition.TypeIdString, block.FatBlock.BlockDefinition.SubtypeId);
            if (blockSettings == null || !blockSettings.EnableAirtightAndOxygen)
                return true;

            bool isAirtight = grid.IsRoomAtPositionAirtight(block.FatBlock.Position);
            double oxygenLevel = MyAPIGateway.Session.OxygenProviderSystem.GetOxygenInPoint(block.FatBlock.GetPosition());

            return isAirtight || oxygenLevel > 0.5;
        }
        private bool CheckInventoryForRequiredItems(IMySlimBlock block, BotSpawnerConfig blockSettings)
        {
            //MyAPIGateway.Utilities.ShowMessage("STARTTEST", "CheckInventoryForRequiredItems");
            var inventory = block.FatBlock.GetInventory() as IMyInventory;
            if (inventory == null)
            {
                //LogError($"Block {block.FatBlock.BlockDefinition.SubtypeId} has no inventory. Skipping required item check.");
                return true; // Skip check if no inventory
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

            return true;
        }
        private void RemoveItemsFromInventory(IMySlimBlock block, BotSpawnerConfig blockSettings, int dropAmount)
        {
            //MyAPIGateway.Utilities.ShowMessage("STARTTEST", "RemoveItemsFromInventory");
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
        private int SpawnEntities(IMySlimBlock block, BotSpawnerConfig settings)
        {
            //MyAPIGateway.Utilities.ShowMessage("STARTTEST", "SpawnEntities");

            int spawnAmount;
            if (settings.UseWeightedDrops)
            {
                //MyAPIGateway.Utilities.ShowMessage("PEPCO", $"UseWeightedDrops");
                spawnAmount = GetWeightedRandomNumber(settings.MinAmount, GenerateProbabilities(settings.MinAmount, settings.MaxAmount));

            }
            else
            {
                //MyAPIGateway.Utilities.ShowMessage("PEPCO", $"Didn't UseWeightedDrops");
                spawnAmount = randomGenerator.Next(settings.MinAmount, settings.MaxAmount + 1);
            }

            for (int i = 0; i < settings.EntityID.Count; i++)
            {
                string EntityID = settings.EntityID[i];
                {

                    for (int j = 0; j < spawnAmount; j++)
                    {
                        Vector3D spawnPosition = CalculateDropPosition(block, settings);
                        Quaternion dropRotation = Quaternion.CreateFromYawPitchRoll(
                            (float)(randomGenerator.NextDouble() * Math.PI * 2),
                            (float)(randomGenerator.NextDouble() * Math.PI * 2),
                            (float)(randomGenerator.NextDouble() * Math.PI * 2)
                        );
                        MyVisualScriptLogicProvider.SpawnBot(EntityID, spawnPosition);
                        //MyAPIGateway.Utilities.ShowMessage("PEPCO", $"{EntityID} {spawnPosition}");
                        //  MyFloatingObjects.Spawn(
                        //          new MyPhysicalInventoryItem((VRage.MyFixedPoint)1, dropItem),
                        //          dropPosition, dropRotation.Up, block.FatBlock.WorldMatrix.Up
                        //           MyVisualScriptLogicProvider.SpawnBot(EntityID, dropPosition, dropRotation.Up, block.FatBlock.WorldMatrix.Up);
                        //    MyAPIGateway.Utilities.ShowMessage("BotSpawner", $"Spawned bot: {} at {dropPosition}");

                        //            public static void SpawnBot(string EntityID, BotSpawnerConfig blockSettings)
                        //    {
                        //       Random random = new Random();
                        //      double randomHeight = random.NextDouble() * (blockSettings.MaxHeight - blockSettings.MinHeight) + blockSettings.MinHeight;
                        //     double randomRadius = random.NextDouble() * (blockSettings.MaxRadius - blockSettings.MinRadius) + blockSettings.MinRadius;
                        //      double angle = random.NextDouble() * Math.PI * 2;
                        //      Vector3D offset = new Vector3D(
                        //      randomRadius * Math.Cos(angle),
                        //      randomHeight,
                        //         randomRadius * Math.Sin(angle)
                        //     );
                        //
                        //     Vector3D spawnPosition = MyAPIGateway.Session.Player.GetPosition() + offset;
                        //     MyVisualScriptLogicProvider.SpawnBot(EntityID, spawnPosition);
                        //     MyAPIGateway.Utilities.ShowMessage("BotSpawner", $"Spawned bot: {EntityID} at {spawnPosition}");
                        // }
                        //      );

                    }
                }
            }

            return spawnAmount;
        }
        private Vector3D CalculateDropPosition(IMySlimBlock block, BotSpawnerConfig blockSettings)
        {
            //MyAPIGateway.Utilities.ShowMessage("STARTTEST", "CalculateDropPosition");
            double heightOffset = randomGenerator.NextDouble() * (blockSettings.MaxHeight - blockSettings.MinHeight) + blockSettings.MinHeight;
            double radius = randomGenerator.NextDouble() * (blockSettings.MaxRadius - blockSettings.MinRadius) + blockSettings.MinRadius;
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
            //MyAPIGateway.Utilities.ShowMessage("STARTTEST", "GenerateProbabilities");
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
            //MyAPIGateway.Utilities.ShowMessage("STARTTEST", "GetWeightedRandomNumber");
            int totalWeight = probabilities.Sum();
            int randomValue = randomGenerator.Next(1, totalWeight + 1);
            int cumulativeWeight = 0;

            for (int i = 0; i < probabilities.Count; i++)
            {
                cumulativeWeight += probabilities[i];
                if (randomValue <= cumulativeWeight)
                    return minAmount + i;
            }
            return minAmount; // In case something goes wrong, return the minimum amount
        }
        private void InitializeBotSpawnerConfig()
        {
            //MyAPIGateway.Utilities.ShowMessage("STARTTEST", "InitializeBotSpawnerConfig");
            // Clear existing drop settings
            settings.BlockSpawnSettings.Clear();

            // Re-load settings from the loaded configuration
            settings.Load();

            // Log initialized settings
            foreach (var botSpawnerConfig in settings.BlockSpawnSettings)
            {
                LogError($"Initialized settings for {botSpawnerConfig.BlockId}: " +
                         $"BlockType={botSpawnerConfig.BlockType}, MinAmount={botSpawnerConfig.MinAmount}, MaxAmount={botSpawnerConfig.MaxAmount}, " +
                         $"DamageAmount={botSpawnerConfig.DamageAmount}, MinHealthPercentage={botSpawnerConfig.MinHealthPercentage}, " +
                         $"MaxHealthPercentage={botSpawnerConfig.MaxHealthPercentage}, MinHeight={botSpawnerConfig.MinHeight}, " +
                         $"MaxHeight={botSpawnerConfig.MaxHeight}, MinRadius={botSpawnerConfig.MinRadius}, MaxRadius={botSpawnerConfig.MaxRadius}, " +
                         $"SpawnTriggerInterval={botSpawnerConfig.SpawnTriggerInterval}, EnableAirtightAndOxygen={botSpawnerConfig.EnableAirtightAndOxygen}, " +
                         $"Enabled={botSpawnerConfig.Enabled}, " +
                         $"EntityID={string.Join(",", botSpawnerConfig.EntityID)}, " +
                         $"RequiredItemTypes={string.Join(",", botSpawnerConfig.RequiredItemTypes)}, RequiredItemIds={string.Join(",", botSpawnerConfig.RequiredItemIds)}, " +
                         $"RequiredItemAmounts={string.Join(",", botSpawnerConfig.RequiredItemAmounts)}");
            }

            LogError("Drop settings initialized.");
        }



        private void ReloadSettings()
        {
            //MyAPIGateway.Utilities.ShowMessage("STARTTEST", "ReloadSettings");
            try
            {


                MyAPIGateway.Utilities.ShowMessage("PEPCO", "Reloading settings...");
                LogError("Reloading settings...");

                // Ensure default INI files exist
                //MyAPIGateway.Utilities.ShowMessage("PEPCO", "Ensure default INI files exist...");
                LogError("Ensure default INI files exist...");
                EnsureDefaultIniFilesExist();

                // Copy all CES files to world storage
                //MyAPIGateway.Utilities.ShowMessage("PEPCO", "Copying all CES files to world storage...");
                LogError("Copying all CES files to world storage...");
                CopyAllCESFilesToWorldStorage();

                // Reload all files from world storage
                LoadAllFilesFromWorldStorage();

                // Reload the main settings
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
            //MyAPIGateway.Utilities.ShowMessage("STARTTEST", "CopyAllCESFilesToWorldStorage");
            LogError("Starting CopyAllCESFilesToWorldStorage");
            try
            {
                // Iterate through all mods in the current session
                foreach (var modItem in MyAPIGateway.Session.Mods)
                {
                    // Construct the file path
                    string modFilePath = "Data/CES.ini";
                    LogError($"Checking for file: {modFilePath} in mod: {modItem.PublishedFileId}");

                    // Check if the file exists in the mod data folder
                    if (MyAPIGateway.Utilities.FileExistsInModLocation(modFilePath, modItem))
                    {
                        LogError($"File found: {modFilePath} in mod: {modItem.PublishedFileId}");

                        // Determine the destination file path in world storage
                        string worldStorageFilePath = $"{modItem.PublishedFileId}_CES.ini";

                        // Check if the file already exists in world storage
                        if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(worldStorageFilePath, typeof(CustomEntitySpawner)))
                        {
                            // Read the file from the mod data folder
                            string fileContent;
                            using (var reader = MyAPIGateway.Utilities.ReadFileInModLocation(modFilePath, modItem))
                            {
                                fileContent = reader.ReadToEnd();
                            }

                            // Write the file to world storage
                            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(worldStorageFilePath, typeof(CustomEntitySpawner)))
                            {
                                writer.Write(fileContent);
                            }

                            //MyAPIGateway.Utilities.ShowMessage("CES.ini", $"File copied to World Storage successfully from mod {modItem.PublishedFileId}.");
                            LogError($"File copied to World Storage successfully from mod {modItem.PublishedFileId}.");
                        }
                        else
                        {
                            //MyAPIGateway.Utilities.ShowMessage("CES.ini", $"File already exists in World Storage for mod {modItem.PublishedFileId}.");
                            LogError($"File already exists: {worldStorageFilePath} for mod: {modItem.PublishedFileId}");
                        }
                    }
                    else
                    {
                        //MyAPIGateway.Utilities.ShowMessage("CES.ini", $"File not found in Mod Data folder for mod {modItem.PublishedFileId}.");
                        LogError($"File not found: {modFilePath} in mod: {modItem.PublishedFileId}");
                    }
                }
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLine($"Error copying files to World Storage: {ex.Message}");
                //MyAPIGateway.Utilities.ShowMessage("CES.ini", $"Error copying files to World Storage: {ex.Message}");
                LogError($"Error copying files to World Storage: {ex.Message}");
            }
        }

        private void LoadAllFilesFromWorldStorage()
        {
            //MyAPIGateway.Utilities.ShowMessage("STARTTEST", "LoadAllFilesFromWorldStorage");
            try
            {
                // List of known configuration file names
                List<string> knownConfigFiles = new List<string>
        {
            "GlobalConfig.ini",
            "CustomEntitySpawner.ini"
        };

                // Additional file patterns to check for
                List<string> additionalFilePatterns = new List<string>
        {
            "_CES.ini"
        };

                // Clear existing settings
                settings.BlockSpawnSettings.Clear();

                // Load known config files
                foreach (string configFileName in knownConfigFiles)
                {
                    //MyAPIGateway.Utilities.ShowMessage("CES.ini", $"Looking at {configFileName}.");
                    LogError($"Looking at {configFileName}");
                    if (MyAPIGateway.Utilities.FileExistsInWorldStorage(configFileName, typeof(CustomEntitySpawner)))
                    {
                        //MyAPIGateway.Utilities.ShowMessage("CES.ini", $"Looking in world Storage for {configFileName}.");
                        LogError($"Looking in world Storage for {configFileName}");
                        using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(configFileName, typeof(CustomEntitySpawner)))
                        {
                            string fileContent = reader.ReadToEnd();
                            if (configFileName.Equals("GlobalConfig.ini", StringComparison.OrdinalIgnoreCase))
                            {
                                //MyAPIGateway.Utilities.ShowMessage("GlobalConfig.ini", $"Loading GlobalConfig: {configFileName}.");
                                LogError($"Loading GlobalConfig: {configFileName}");
                                LoadGlobalConfig(fileContent);
                            }
                            else if (configFileName.Equals("CustomEntitySpawner.ini", StringComparison.OrdinalIgnoreCase))
                            {
                                //MyAPIGateway.Utilities.ShowMessage("CustomItemSpawnerConfig.ini", $"Loading CustomItemSpawnerConfig: {configFileName}.");
                                LogError($"Loading CustomItemSpawnerConfig: {configFileName}");
                                LoadCustomEntitySpawnerConfig(fileContent);
                            }
                        }
                    }
                }

                // Load additional CES files
                foreach (var modItem in MyAPIGateway.Session.Mods)
                {
                    //MyAPIGateway.Utilities.ShowMessage("CES.ini", $"Looking for: {modItem.PublishedFileId} in world storage folder");
                    LogError($"Looking for: {modItem.PublishedFileId} in world storage folder");
                    foreach (string pattern in additionalFilePatterns)
                    {
                        string cesFileName = $"{modItem.PublishedFileId}{pattern}";
                        //MyAPIGateway.Utilities.ShowMessage("CES.ini", $"Looking for: {modItem.PublishedFileId} in {cesFileName}");
                        LogError($"Looking for: {modItem.PublishedFileId} in {cesFileName}");
                        if (MyAPIGateway.Utilities.FileExistsInWorldStorage(cesFileName, typeof(CustomEntitySpawner)))
                        {
                            //MyAPIGateway.Utilities.ShowMessage("CES.ini", $"Found {modItem.PublishedFileId} in {cesFileName}");
                            LogError($"Found {modItem.PublishedFileId} in {cesFileName}");
                            using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(cesFileName, typeof(CustomEntitySpawner)))
                            {
                                //MyAPIGateway.Utilities.ShowMessage("CES.ini", $"Loading: {modItem.PublishedFileId} in {cesFileName}");
                                LogError($"Loading: {modItem.PublishedFileId} in {cesFileName}");
                                string fileContent = reader.ReadToEnd();
                                LogError($"Contents:, {fileContent}");
                                LoadCustomEntitySpawnerConfig(fileContent);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLine($"Error loading files from World Storage: {ex.Message}");
                //MyAPIGateway.Utilities.ShowMessage("CES.ini", $"Error loading files from World Storage: {ex.Message}");
                LogError($"Error loading files from World Storage: {ex.Message}");
            }
        }



        private void LoadGlobalConfig(string fileContent)
        {
            //MyAPIGateway.Utilities.ShowMessage("STARTTEST", "LoadGlobalConfig");
            // Parse and load global configuration settings from fileContent
            MyIni iniParser = new MyIni();
            MyIniParseResult result;
            if (!iniParser.TryParse(fileContent, out result))
            {
                throw new Exception($"Config error: {result.ToString()}");
            }

            // Load specific global settings
            settings.BaseUpdateInterval = iniParser.Get("Config", "BaseUpdateInterval").ToInt32(60);
            settings.EnableLogging = iniParser.Get("Config", "EnableLogging").ToBoolean(true);
        }

        private void LoadCustomEntitySpawnerConfig(string fileContent)
        {
            //MyAPIGateway.Utilities.ShowMessage("STARTTEST", "LoadCustomEntitySpawnerConfig");
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
                        BlockId = section,
                        BlockType = iniParser.Get(section, nameof(BotSpawnerConfig.BlockType)).ToString(),
                        MinAmount = iniParser.Get(section, nameof(BotSpawnerConfig.MinAmount)).ToInt32(),
                        MaxAmount = iniParser.Get(section, nameof(BotSpawnerConfig.MaxAmount)).ToInt32(),
                        UseWeightedDrops = iniParser.Get(section, nameof(BotSpawnerConfig.UseWeightedDrops)).ToBoolean(),
                        DamageAmount = (float)iniParser.Get(section, nameof(BotSpawnerConfig.DamageAmount)).ToDouble(),
                        MinHealthPercentage = (float)iniParser.Get(section, nameof(BotSpawnerConfig.MinHealthPercentage)).ToDouble(),
                        MaxHealthPercentage = (float)iniParser.Get(section, nameof(BotSpawnerConfig.MaxHealthPercentage)).ToDouble(),
                        MinHeight = iniParser.Get(section, nameof(BotSpawnerConfig.MinHeight)).ToDouble(),
                        MaxHeight = iniParser.Get(section, nameof(BotSpawnerConfig.MaxHeight)).ToDouble(),
                        MinRadius = iniParser.Get(section, nameof(BotSpawnerConfig.MinRadius)).ToDouble(),
                        MaxRadius = iniParser.Get(section, nameof(BotSpawnerConfig.MaxRadius)).ToDouble(),
                        SpawnTriggerInterval = iniParser.Get(section, nameof(BotSpawnerConfig.SpawnTriggerInterval)).ToInt32(),
                        EnableAirtightAndOxygen = iniParser.Get(section, nameof(BotSpawnerConfig.EnableAirtightAndOxygen)).ToBoolean(),
                        Enabled = iniParser.Get(section, nameof(BotSpawnerConfig.Enabled)).ToBoolean(),
                        PlayerDistanceCheck = iniParser.Get(section, nameof(BotSpawnerConfig.PlayerDistanceCheck)).ToInt32()
                    };

                    botSpawnerConfig.EntityID.AddRange(iniParser.Get(section, nameof(BotSpawnerConfig.EntityID)).ToString().Split(','));
                    botSpawnerConfig.RequiredItemTypes.AddRange(iniParser.Get(section, nameof(BotSpawnerConfig.RequiredItemTypes)).ToString().Split(','));
                    botSpawnerConfig.RequiredItemIds.AddRange(iniParser.Get(section, nameof(BotSpawnerConfig.RequiredItemIds)).ToString().Split(','));
                    botSpawnerConfig.RequiredItemAmounts.AddRange(iniParser.Get(section, nameof(BotSpawnerConfig.RequiredItemAmounts)).ToString().Split(',').Select(int.Parse));

                    settings.BlockSpawnSettings.Add(botSpawnerConfig);

                    // Log loaded settings
                    LogError($"Loaded settings for {section}: " +
                             $"BlockType={botSpawnerConfig.BlockType}, MinAmount={botSpawnerConfig.MinAmount}, MaxAmount={botSpawnerConfig.MaxAmount}, " +
                             $"DamageAmount={botSpawnerConfig.DamageAmount}, MinHealthPercentage={botSpawnerConfig.MinHealthPercentage}, " +
                             $"MaxHealthPercentage={botSpawnerConfig.MaxHealthPercentage}, MinHeight={botSpawnerConfig.MinHeight}, " +
                             $"MaxHeight={botSpawnerConfig.MaxHeight}, MinRadius={botSpawnerConfig.MinRadius}, MaxRadius={botSpawnerConfig.MaxRadius}, " +
                             $"SpawnTriggerInterval={botSpawnerConfig.SpawnTriggerInterval}, EnableAirtightAndOxygen={botSpawnerConfig.EnableAirtightAndOxygen}, " +
                             $"Enabled={botSpawnerConfig.Enabled}, " +
                             $"PlayerDistanceCheck={botSpawnerConfig.PlayerDistanceCheck}, " +
                             $"EntityID={string.Join(",", botSpawnerConfig.EntityID)}, " +
                             $"RequiredItemTypes={string.Join(",", botSpawnerConfig.RequiredItemTypes)}, RequiredItemIds={string.Join(",", botSpawnerConfig.RequiredItemIds)}, " +
                             $"RequiredItemAmounts={string.Join(",", botSpawnerConfig.RequiredItemAmounts)}");
                }
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLine($"Error loading CustomEntitySpawner config: {ex.Message}");
                //MyAPIGateway.Utilities.ShowMessage("CustomEntitySpawner.ini", $"Error loading CustomEntitySpawner config: {ex.Message}");
                LogError($"Error loading CustomEntitySpawner config: {ex.Message}");
            }
        }


        private void LogError(string message)
        {
            if (!settings.EnableLogging)
                return;

            string logFilePath = "CustomEntitySpawner.log";
            string existingContent = "";

            // Read the existing content if the file exists
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(logFilePath, typeof(CustomEntitySpawner)))
            {
                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(logFilePath, typeof(CustomEntitySpawner)))
                {
                    existingContent = reader.ReadToEnd();
                }
            }

            // Append the new log entry
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
        public bool EnableLogging { get; set; } = true; // Default value is true
        public List<BotSpawnerConfig> BlockSpawnSettings { get; set; } = new List<BotSpawnerConfig>();

        public void Load()
        {
            //MyAPIGateway.Utilities.ShowMessage("STARTTEST", "Load");
            MyIni iniParser = new MyIni();

            // Load global settings
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(GlobalFileName, typeof(CustomEntitySpawnerSettings)))
            {
                using (TextReader file = MyAPIGateway.Utilities.ReadFileInWorldStorage(GlobalFileName, typeof(CustomEntitySpawnerSettings)))
                {
                    string text = file.ReadToEnd();

                    MyIniParseResult result;
                    if (!iniParser.TryParse(text, out result))
                        throw new Exception($"Config error: {result.ToString()}");

                    // Load base settings
                    BaseUpdateInterval = iniParser.Get(IniSection, nameof(BaseUpdateInterval)).ToInt32(BaseUpdateInterval);
                    EnableLogging = iniParser.Get(IniSection, nameof(EnableLogging)).ToBoolean(EnableLogging);
                }
            }
            else
            {
                throw new Exception("GlobalConfig.ini not found.");
            }

            // Load block drop settings from CustomEntitySpawner.ini
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

            // Load block drop settings from _CES.ini files
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
            //MyAPIGateway.Utilities.ShowMessage("STARTTEST", "LoadBlockSpawnSettings");
            List<string> sections = new List<string>();
            iniParser.GetSections(sections);

            foreach (var section in sections)
            {
                if (section == IniSection)
                    continue;

                var botSpawnerConfig = new BotSpawnerConfig
                {
                    BlockId = section,
                    BlockType = iniParser.Get(section, nameof(BotSpawnerConfig.BlockType)).ToString(),
                    MinAmount = iniParser.Get(section, nameof(BotSpawnerConfig.MinAmount)).ToInt32(),
                    MaxAmount = iniParser.Get(section, nameof(BotSpawnerConfig.MaxAmount)).ToInt32(),
                    UseWeightedDrops = iniParser.Get(section, nameof(BotSpawnerConfig.UseWeightedDrops)).ToBoolean(),
                    DamageAmount = (float)iniParser.Get(section, nameof(BotSpawnerConfig.DamageAmount)).ToDouble(),
                    MinHealthPercentage = (float)iniParser.Get(section, nameof(BotSpawnerConfig.MinHealthPercentage)).ToDouble(),
                    MaxHealthPercentage = (float)iniParser.Get(section, nameof(BotSpawnerConfig.MaxHealthPercentage)).ToDouble(),
                    MinHeight = iniParser.Get(section, nameof(BotSpawnerConfig.MinHeight)).ToDouble(),
                    MaxHeight = iniParser.Get(section, nameof(BotSpawnerConfig.MaxHeight)).ToDouble(),
                    MinRadius = iniParser.Get(section, nameof(BotSpawnerConfig.MinRadius)).ToDouble(),
                    MaxRadius = iniParser.Get(section, nameof(BotSpawnerConfig.MaxRadius)).ToDouble(),
                    SpawnTriggerInterval = iniParser.Get(section, nameof(BotSpawnerConfig.SpawnTriggerInterval)).ToInt32(),
                    EnableAirtightAndOxygen = iniParser.Get(section, nameof(BotSpawnerConfig.EnableAirtightAndOxygen)).ToBoolean(),
                    Enabled = iniParser.Get(section, nameof(BotSpawnerConfig.Enabled)).ToBoolean(),
                    PlayerDistanceCheck = iniParser.Get(section, nameof(BotSpawnerConfig.PlayerDistanceCheck)).ToInt32()
                };

                botSpawnerConfig.EntityID.AddRange(iniParser.Get(section, nameof(BotSpawnerConfig.EntityID)).ToString().Split(','));
                botSpawnerConfig.RequiredItemTypes.AddRange(iniParser.Get(section, nameof(BotSpawnerConfig.RequiredItemTypes)).ToString().Split(','));
                botSpawnerConfig.RequiredItemIds.AddRange(iniParser.Get(section, nameof(BotSpawnerConfig.RequiredItemIds)).ToString().Split(','));
                botSpawnerConfig.RequiredItemAmounts.AddRange(iniParser.Get(section, nameof(BotSpawnerConfig.RequiredItemAmounts)).ToString().Split(',').Select(int.Parse));

                BlockSpawnSettings.Add(botSpawnerConfig);
            }
        }
    }

    public class BotSpawnerConfig
    {
        public string BlockId { get; set; }
        public string BlockType { get; set; }
        public List<string> EntityID { get; set; } = new List<string>();
        public int MinAmount { get; set; } = 1;
        public int MaxAmount { get; set; } = 1;
        public bool UseWeightedDrops { get; set; } = false;
        public float DamageAmount { get; set; } = 0;
        public float MinHealthPercentage { get; set; } = 0.1f;
        public float MaxHealthPercentage { get; set; } = 1.0f;
        public double MinHeight { get; set; } = 1.0;
        public double MaxHeight { get; set; } = 2.0;
        public double MinRadius { get; set; } = 0.5;
        public double MaxRadius { get; set; } = 2;
        public int SpawnTriggerInterval { get; set; } = 10;
        public bool EnableAirtightAndOxygen { get; set; } = false;
        public bool Enabled { get; set; } = true;
        public List<string> RequiredItemTypes { get; set; } = new List<string>();
        public List<string> RequiredItemIds { get; set; } = new List<string>();
        public List<int> RequiredItemAmounts { get; set; } = new List<int>();
        public int PlayerDistanceCheck { get; set; } = 1000;

    }
}

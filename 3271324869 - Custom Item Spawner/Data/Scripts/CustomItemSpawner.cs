﻿using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;
using VRage.Game;
using VRage.Game.Entity;
using VRageMath;
using VRage.Utils;
using Sandbox.Game.Entities;
using System.Linq;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.ModAPI;
using System.IO;
using PEPCO.iSurvival.CustomItemSpawner;

namespace PEPCO.iSurvival.CustomItemSpawner
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class CustomItemSpawner : MySessionComponentBase
    {
        private int updateTickCounter = 0;
        private long totalUpdateTicks = 0;
        private bool isLoading = true;
        private int loadingTickCount = 3;
        private static readonly Random randomGenerator = new Random();

        private const string ModDataFile = "CIS.ini";
        private const string WorldStorageFolder = "CustomItemSpawner";
        private const string WorldStorageFile = "CIS.ini";

        private static readonly Dictionary<string, Type> itemTypeMappings = new Dictionary<string, Type>
        {
            { "MyObjectBuilder_ConsumableItem", typeof(MyObjectBuilder_ConsumableItem) },
            { "MyObjectBuilder_Ore", typeof(MyObjectBuilder_Ore) },
            { "MyObjectBuilder_Ingot", typeof(MyObjectBuilder_Ingot) },
            { "MyObjectBuilder_Component", typeof(MyObjectBuilder_Component) }
        };

        public static CustomItemSpawnerSettings settings = new CustomItemSpawnerSettings();

        private const string DefaultGlobalIniContent = @"
; ==============================================
; HOW TO USE GlobalConfig.ini
; ==============================================
; This configuration file defines the global settings for the Custom Item Spawner mod.
;
; [Config]
; BaseUpdateInterval - Defines the base interval in ticks for updates.
;
[Config]
BaseUpdateInterval=60
";

        private const string DefaultItemSpawnerIniContent = @"
; ==============================================
; HOW TO USE CustomItemSpawner.ini
; ==============================================
; This configuration file defines the behavior of the Custom Item Spawner mod.
; Each section represents a different type of block and its settings for item drops.
;
; [SectionName]
; BlockType - The type of block (e.g., MyObjectBuilder_CubeBlock).
; MinAmount - Minimum number of items to drop.
; MaxAmount - Maximum number of items to drop.
; DamageAmount - Amount of damage to apply to the block per drop.
; MinHealthPercentage - Minimum health percentage required to apply damage.
; MaxHealthPercentage - Maximum health percentage allowed for spawning.
; MinHeight, MaxHeight - The height range where items will spawn.
; MinRadius, MaxRadius - The radius range around the block where items will spawn.
; SpawnTriggerInterval - How often (in seconds) the drops will occur.
; EnableAirtightAndOxygen - Whether to check for airtightness and oxygen level.
; Enabled - Whether the settings for this block type are enabled.
; StackItems - Whether to stack dropped items together.
; SpawnInsideInventory - Whether to spawn items inside the block's inventory.
; ItemTypes - Comma-separated list of item types to drop (e.g., MyObjectBuilder_ConsumableItem).
; ItemIds - Comma-separated list of item IDs to drop (e.g., Apple).
; RequiredItemTypes - Comma-separated list of required item types to be present in the block's inventory.
; RequiredItemIds - Comma-separated list of required item IDs.
; RequiredItemAmounts - Comma-separated list of amounts for each required item.
;
; Example:
; [LargeBlockSmallContainer]
; BlockType=MyObjectBuilder_CargoContainer
; MinAmount=1
; MaxAmount=5
; DamageAmount=10.0
; MinHealthPercentage=0.2
; MaxHealthPercentage=1
; MinHeight=1.0
; MaxHeight=5.0
; MinRadius=2.0
; MaxRadius=5.0
; SpawnTriggerInterval=2
; EnableAirtightAndOxygen=true
; Enabled=true
; StackItems=false
; SpawnInsideInventory=true
; ItemTypes=MyObjectBuilder_Ingot,MyObjectBuilder_Ingot
; ItemIds=Iron,Gold
; RequiredItemTypes=MyObjectBuilder_Ore,MyObjectBuilder_Ore
; RequiredItemIds=Iron,Ice
; RequiredItemAmounts=1,0

[AppleTreeFarmLG]
BlockType=MyObjectBuilder_CubeBlock
MinAmount=1
MaxAmount=10
DamageAmount=0
MinHealthPercentage=0.2
MaxHealthPercentage=1
MinHeight=1.0
MaxHeight=3.0
MinRadius=0.5
MaxRadius=3.0
SpawnTriggerInterval=3
EnableAirtightAndOxygen=true
Enabled=true
StackItems=false
SpawnInsideInventory=false
ItemTypes=MyObjectBuilder_ConsumableItem
ItemIds=Apple
RequiredItemTypes=NA
RequiredItemIds=NA
RequiredItemAmounts=0
";

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            if (MyAPIGateway.Session.IsServer)
            {
                try
                {
                    EnsureDefaultIniFilesExist();
                    LoadAllFilesFromWorldStorage();
                    settings.Load();
                    InitializeDropSettings();
                    CopyAllCISFilesToWorldStorage();
                    MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
                }
                catch (Exception ex)
                {
                    MyAPIGateway.Utilities.ShowMessage("Custom Item Spawner", $"Initialization error: {ex.Message}");
                    LogError($"Initialization error: {ex.Message}");
                }
            }
        }

        private void EnsureDefaultIniFilesExist()
        {
            if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(CustomItemSpawnerSettings.GlobalFileName, typeof(CustomItemSpawnerSettings)))
            {
                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(CustomItemSpawnerSettings.GlobalFileName, typeof(CustomItemSpawnerSettings)))
                {
                    writer.Write(DefaultGlobalIniContent);
                }
            }

            if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(CustomItemSpawnerSettings.ItemSpawnerFileName, typeof(CustomItemSpawnerSettings)))
            {
                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(CustomItemSpawnerSettings.ItemSpawnerFileName, typeof(CustomItemSpawnerSettings)))
                {
                    writer.Write(DefaultItemSpawnerIniContent);
                }
            }
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
        }

        private void OnMessageEntered(string messageText, ref bool sendToOthers)
        {
            if (messageText.Equals("/pepco farm reload", StringComparison.OrdinalIgnoreCase))
            {
                if (MyAPIGateway.Session.IsServer)
                {
                    try
                    {
                        ReloadSettings();
                        MyAPIGateway.Utilities.ShowMessage("PEPCO", "Settings reloaded successfully.");
                    }
                    catch (Exception ex)
                    {
                        MyAPIGateway.Utilities.ShowMessage("PEPCO", $"Error reloading settings: {ex.Message}");
                        LogError($"Error reloading settings: {ex.Message}");
                    }
                }
                sendToOthers = false;
            }
        }

        public override void UpdateBeforeSimulation()
        {
            if (isLoading && loadingTickCount-- > 0)
                return;

            isLoading = false;
            totalUpdateTicks++;

            if (++updateTickCounter >= settings.BaseUpdateInterval)
            {
                updateTickCounter = 0;
                try
                {
                    DropItemsNearBlocks();
                }
                catch (Exception ex)
                {
                    LogError($"Update error: {ex.Message}");
                }
            }
        }

        private void DropItemsNearBlocks()
        {
            long baseUpdateCycles = totalUpdateTicks / settings.BaseUpdateInterval;

            foreach (var entity in MyEntities.GetEntities())
            {
                var grid = entity as IMyCubeGrid;
                if (grid != null)
                {
                    var blocks = new List<IMySlimBlock>();
                    grid.GetBlocks(blocks, b => b.FatBlock != null && IsValidBlock(b.FatBlock.BlockDefinition.TypeIdString, b.FatBlock.BlockDefinition.SubtypeId));

                    foreach (var block in blocks)
                    {
                        var blockSettings = GetDropSettingsForBlock(block.FatBlock.BlockDefinition.TypeIdString, block.FatBlock.BlockDefinition.SubtypeId);
                        if (blockSettings != null && blockSettings.Enabled && baseUpdateCycles % blockSettings.SpawnTriggerInterval == 0 && IsEnvironmentSuitable(grid, block))
                        {
                            float blockHealthPercentage = block.Integrity / block.MaxIntegrity;
                            if (blockHealthPercentage >= blockSettings.MinHealthPercentage && blockHealthPercentage <= blockSettings.MaxHealthPercentage)
                            {
                                if (CheckInventoryForRequiredItems(block, blockSettings))
                                {
                                    int dropAmount = DropItems(block, blockSettings);
                                    if (dropAmount > 0)
                                    {
                                        RemoveItemsFromInventory(block, blockSettings, dropAmount);
                                        block.DoDamage(blockSettings.DamageAmount * dropAmount, MyDamageType.Grind, true);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool IsValidBlock(string typeId, string subtypeId)
        {
            return settings.BlockDropSettings.Exists(s => s.BlockType == typeId && s.BlockId == subtypeId);
        }

        private DropSettings GetDropSettingsForBlock(string typeId, string subtypeId)
        {
            return settings.BlockDropSettings.Find(s => s.BlockType == typeId && s.BlockId == subtypeId);
        }

        private bool IsEnvironmentSuitable(IMyCubeGrid grid, IMySlimBlock block)
        {
            var blockSettings = GetDropSettingsForBlock(block.FatBlock.BlockDefinition.TypeIdString, block.FatBlock.BlockDefinition.SubtypeId);
            if (blockSettings == null || !blockSettings.EnableAirtightAndOxygen)
                return true;

            bool isAirtight = grid.IsRoomAtPositionAirtight(block.FatBlock.Position);
            double oxygenLevel = MyAPIGateway.Session.OxygenProviderSystem.GetOxygenInPoint(block.FatBlock.GetPosition());

            return isAirtight || oxygenLevel > 0.5;
        }

        private bool CheckInventoryForRequiredItems(IMySlimBlock block, DropSettings blockSettings)
        {
            var inventory = block.FatBlock.GetInventory() as IMyInventory;
            if (inventory == null)
            {
                LogError($"Block {block.FatBlock.BlockDefinition.SubtypeId} has no inventory. Skipping required item check.");
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

        private void RemoveItemsFromInventory(IMySlimBlock block, DropSettings blockSettings, int dropAmount)
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

        private int DropItems(IMySlimBlock block, DropSettings settings)
        {
            int dropAmount = GetWeightedRandomNumber(GenerateProbabilities(settings.MinAmount, settings.MaxAmount));
            for (int i = 0; i < settings.ItemTypes.Count; i++)
            {
                string itemType = settings.ItemTypes[i];
                string itemId = settings.ItemIds[i];
                var dropItem = CreateObjectBuilder(itemType, itemId);

                if (settings.SpawnInsideInventory && block.FatBlock.HasInventory)
                {
                    var inventory = block.FatBlock.GetInventory() as IMyInventory;
                    if (inventory != null)
                    {
                        inventory.AddItems((VRage.MyFixedPoint)dropAmount, dropItem);
                    }
                }
                else
                {
                    if (settings.StackItems)
                    {
                        Vector3D dropPosition = CalculateDropPosition(block, settings);
                        Quaternion dropRotation = Quaternion.CreateFromYawPitchRoll(
                            (float)(randomGenerator.NextDouble() * Math.PI * 2),
                            (float)(randomGenerator.NextDouble() * Math.PI * 2),
                            (float)(randomGenerator.NextDouble() * Math.PI * 2)
                        );

                        MyFloatingObjects.Spawn(
                            new MyPhysicalInventoryItem((VRage.MyFixedPoint)dropAmount, dropItem),
                            dropPosition, dropRotation.Up, block.FatBlock.WorldMatrix.Up
                        );
                    }
                    else
                    {
                        for (int j = 0; j < dropAmount; j++)
                        {
                            Vector3D dropPosition = CalculateDropPosition(block, settings);
                            Quaternion dropRotation = Quaternion.CreateFromYawPitchRoll(
                                (float)(randomGenerator.NextDouble() * Math.PI * 2),
                                (float)(randomGenerator.NextDouble() * Math.PI * 2),
                                (float)(randomGenerator.NextDouble() * Math.PI * 2)
                            );

                            MyFloatingObjects.Spawn(
                                new MyPhysicalInventoryItem((VRage.MyFixedPoint)1, dropItem),
                                dropPosition, dropRotation.Up, block.FatBlock.WorldMatrix.Up
                            );
                        }
                    }
                }
            }

            return dropAmount;
        }

        private Vector3D CalculateDropPosition(IMySlimBlock block, DropSettings blockSettings)
        {
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

        private MyObjectBuilder_PhysicalObject CreateObjectBuilder(string itemType, string itemId)
        {
            Type type;
            if (!itemTypeMappings.TryGetValue(itemType, out type))
                throw new Exception($"Unsupported item type: {itemType}");

            var dropItem = MyObjectBuilderSerializer.CreateNewObject(type) as MyObjectBuilder_PhysicalObject;
            dropItem.SubtypeName = itemId;
            return dropItem;
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

        private int GetWeightedRandomNumber(List<int> probabilities)
        {
            int totalWeight = probabilities.Sum();
            int randomValue = randomGenerator.Next(1, totalWeight + 1);
            int cumulativeWeight = 0;

            for (int i = 0; i < probabilities.Count; i++)
            {
                cumulativeWeight += probabilities[i];
                if (randomValue <= cumulativeWeight)
                    return i + 1;
            }
            return 1;
        }

        private void InitializeDropSettings()
        {
            // Placeholder for any additional initialization logic.
        }

        private void ReloadSettings()
        {
            try
            {
                LoadAllFilesFromWorldStorage();
                settings.Load();
                InitializeDropSettings();
            }
            catch (Exception ex)
            {
                MyAPIGateway.Utilities.ShowMessage("PEPCO", $"Error reloading settings: {ex.Message}");
                LogError($"Error reloading settings: {ex.Message}");
            }
        }

        private void CopyAllCISFilesToWorldStorage()
        {
            LogError("Starting CopyAllCISFilesToWorldStorage");
            try
            {
                // Iterate through all mods in the current session
                foreach (var modItem in MyAPIGateway.Session.Mods)
                {
                    // Construct the file path
                    string modFilePath = "Data/CIS.ini";
                    LogError($"Checking for file: {modFilePath} in mod: {modItem.PublishedFileId}");

                    // Check if the file exists in the mod data folder
                    if (MyAPIGateway.Utilities.FileExistsInModLocation(modFilePath, modItem))
                    {
                        LogError($"File found: {modFilePath} in mod: {modItem.PublishedFileId}");

                        // Determine the destination file path in world storage
                        string worldStorageFilePath = $"{modItem.PublishedFileId}_CIS.ini";

                        // Check if the file already exists in world storage
                        if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(worldStorageFilePath, typeof(CustomItemSpawner)))
                        {
                            // Read the file from the mod data folder
                            string fileContent;
                            using (var reader = MyAPIGateway.Utilities.ReadFileInModLocation(modFilePath, modItem))
                            {
                                fileContent = reader.ReadToEnd();
                            }

                            // Write the file to world storage
                            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(worldStorageFilePath, typeof(CustomItemSpawner)))
                            {
                                writer.Write(fileContent);
                            }

                            MyAPIGateway.Utilities.ShowMessage("CIS.ini", $"File copied to World Storage successfully from mod {modItem.PublishedFileId}.");
                            LogError($"File copied to World Storage successfully from mod {modItem.PublishedFileId}.");
                        }
                        else
                        {
                            MyAPIGateway.Utilities.ShowMessage("CIS.ini", $"File already exists in World Storage for mod {modItem.PublishedFileId}.");
                            LogError($"File already exists: {worldStorageFilePath} for mod: {modItem.PublishedFileId}");
                        }
                    }
                    else
                    {
                        // MyAPIGateway.Utilities.ShowMessage("CIS.ini", $"File not found in Mod Data folder for mod {modItem.PublishedFileId}.");
                        LogError($"File not found: {modFilePath} in mod: {modItem.PublishedFileId}");
                    }
                }
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLine($"Error copying files to World Storage: {ex.Message}");
                MyAPIGateway.Utilities.ShowMessage("CIS.ini", $"Error copying files to World Storage: {ex.Message}");
                LogError($"Error copying files to World Storage: {ex.Message}");
            }
        }


        private void LogError(string message)
        {
            string logFilePath = "CustomItemSpawner.log";
            string existingContent = "";

            // Read the existing content if the file exists
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(logFilePath, typeof(CustomItemSpawner)))
            {
                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(logFilePath, typeof(CustomItemSpawner)))
                {
                    existingContent = reader.ReadToEnd();
                }
            }

            // Append the new log entry
            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(logFilePath, typeof(CustomItemSpawner)))
            {
                if (!string.IsNullOrEmpty(existingContent))
                {
                    writer.Write(existingContent);
                }
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }

        private void LoadAllFilesFromWorldStorage()
        {
            try
            {
                // List of known configuration file names
                List<string> knownConfigFiles = new List<string>
        {
            "GlobalConfig.ini",
            "CustomItemSpawner.ini"
        };

                // Additional file patterns to check for
                List<string> additionalFilePatterns = new List<string>
        {
            "_CIS.ini"
        };

                // Load known config files
                foreach (string configFileName in knownConfigFiles)
                {
                    if (MyAPIGateway.Utilities.FileExistsInWorldStorage(configFileName, typeof(CustomItemSpawner)))
                    {
                        using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(configFileName, typeof(CustomItemSpawner)))
                        {
                            string fileContent = reader.ReadToEnd();
                            if (configFileName.Equals("GlobalConfig.ini", StringComparison.OrdinalIgnoreCase))
                            {
                                LoadGlobalConfig(fileContent);
                            }
                            else if (configFileName.Equals("CustomItemSpawner.ini", StringComparison.OrdinalIgnoreCase))
                            {
                                LoadCustomItemSpawnerConfig(fileContent);
                            }
                        }
                    }
                }

                // Load additional CIS files
                foreach (var modItem in MyAPIGateway.Session.Mods)
                {
                    foreach (string pattern in additionalFilePatterns)
                    {
                        string cisFileName = $"{modItem.PublishedFileId}{pattern}";
                        if (MyAPIGateway.Utilities.FileExistsInWorldStorage(cisFileName, typeof(CustomItemSpawner)))
                        {
                            using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(cisFileName, typeof(CustomItemSpawner)))
                            {
                                string fileContent = reader.ReadToEnd();
                                LoadCustomItemSpawner(fileContent, cisFileName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLine($"Error loading files from World Storage: {ex.Message}");
                MyAPIGateway.Utilities.ShowMessage("CIS.ini", $"Error loading files from World Storage: {ex.Message}");
                LogError($"Error loading files from World Storage: {ex.Message}");
            }
        }

        private void LoadGlobalConfig(string fileContent)
        {
            // Parse and load global configuration settings from fileContent
            MyIni iniParser = new MyIni();
            MyIniParseResult result;
            if (!iniParser.TryParse(fileContent, out result))
            {
                throw new Exception($"Config error: {result.ToString()}");
            }

            // Load specific global settings
            settings.BaseUpdateInterval = iniParser.Get("Config", "BaseUpdateInterval").ToInt32(60);
        }

        private void LoadCustomItemSpawnerConfig(string fileContent)
        {
            // Parse and load custom item spawner configuration settings from fileContent
            MyIni iniParser = new MyIni();
            MyIniParseResult result;
            if (!iniParser.TryParse(fileContent, out result))
            {
                throw new Exception($"Config error: {result.ToString()}");
            }

            // Load specific item spawner settings
            List<string> sections = new List<string>();
            iniParser.GetSections(sections);

            foreach (var section in sections)
            {
                if (section == "Config")
                    continue;

                var dropSettings = new DropSettings
                {
                    BlockId = section,
                    BlockType = iniParser.Get(section, nameof(DropSettings.BlockType)).ToString(),
                    MinAmount = iniParser.Get(section, nameof(DropSettings.MinAmount)).ToInt32(),
                    MaxAmount = iniParser.Get(section, nameof(DropSettings.MaxAmount)).ToInt32(),
                    DamageAmount = (float)iniParser.Get(section, nameof(DropSettings.DamageAmount)).ToDouble(),
                    MinHealthPercentage = (float)iniParser.Get(section, nameof(DropSettings.MinHealthPercentage)).ToDouble(),
                    MaxHealthPercentage = (float)iniParser.Get(section, nameof(DropSettings.MaxHealthPercentage)).ToDouble(),
                    MinHeight = iniParser.Get(section, nameof(DropSettings.MinHeight)).ToDouble(),
                    MaxHeight = iniParser.Get(section, nameof(DropSettings.MaxHeight)).ToDouble(),
                    MinRadius = iniParser.Get(section, nameof(DropSettings.MinRadius)).ToDouble(),
                    MaxRadius = iniParser.Get(section, nameof(DropSettings.MaxRadius)).ToDouble(),
                    SpawnTriggerInterval = iniParser.Get(section, nameof(DropSettings.SpawnTriggerInterval)).ToInt32(),
                    EnableAirtightAndOxygen = iniParser.Get(section, nameof(DropSettings.EnableAirtightAndOxygen)).ToBoolean(),
                    Enabled = iniParser.Get(section, nameof(DropSettings.Enabled)).ToBoolean(),
                    StackItems = iniParser.Get(section, nameof(DropSettings.StackItems)).ToBoolean(),
                    SpawnInsideInventory = iniParser.Get(section, nameof(DropSettings.SpawnInsideInventory)).ToBoolean()
                };

                dropSettings.ItemTypes.AddRange(iniParser.Get(section, nameof(DropSettings.ItemTypes)).ToString().Split(','));
                dropSettings.ItemIds.AddRange(iniParser.Get(section, nameof(DropSettings.ItemIds)).ToString().Split(','));
                dropSettings.RequiredItemTypes.AddRange(iniParser.Get(section, nameof(DropSettings.RequiredItemTypes)).ToString().Split(','));
                dropSettings.RequiredItemIds.AddRange(iniParser.Get(section, nameof(DropSettings.RequiredItemIds)).ToString().Split(','));
                dropSettings.RequiredItemAmounts.AddRange(iniParser.Get(section, nameof(DropSettings.RequiredItemAmounts)).ToString().Split(',').Select(int.Parse));

                settings.BlockDropSettings.Add(dropSettings);
            }
        }

        private void LoadCustomItemSpawner(string fileContent, string fileName)
        {
            // Parse and load custom item spawner configuration settings from fileContent
            MyIni iniParser = new MyIni();
            MyIniParseResult result;
            if (!iniParser.TryParse(fileContent, out result))
            {
                throw new Exception($"Config error in file {fileName}: {result.ToString()}");
            }

            // Load specific item spawner settings
            List<string> sections = new List<string>();
            iniParser.GetSections(sections);

            foreach (var section in sections)
            {
                if (section == "Config")
                    continue;

                var dropSettings = new DropSettings
                {
                    BlockId = section,
                    BlockType = iniParser.Get(section, nameof(DropSettings.BlockType)).ToString(),
                    MinAmount = iniParser.Get(section, nameof(DropSettings.MinAmount)).ToInt32(),
                    MaxAmount = iniParser.Get(section, nameof(DropSettings.MaxAmount)).ToInt32(),
                    DamageAmount = (float)iniParser.Get(section, nameof(DropSettings.DamageAmount)).ToDouble(),
                    MinHealthPercentage = (float)iniParser.Get(section, nameof(DropSettings.MinHealthPercentage)).ToDouble(),
                    MaxHealthPercentage = (float)iniParser.Get(section, nameof(DropSettings.MaxHealthPercentage)).ToDouble(),
                    MinHeight = iniParser.Get(section, nameof(DropSettings.MinHeight)).ToDouble(),
                    MaxHeight = iniParser.Get(section, nameof(DropSettings.MaxHeight)).ToDouble(),
                    MinRadius = iniParser.Get(section, nameof(DropSettings.MinRadius)).ToDouble(),
                    MaxRadius = iniParser.Get(section, nameof(DropSettings.MaxRadius)).ToDouble(),
                    SpawnTriggerInterval = iniParser.Get(section, nameof(DropSettings.SpawnTriggerInterval)).ToInt32(),
                    EnableAirtightAndOxygen = iniParser.Get(section, nameof(DropSettings.EnableAirtightAndOxygen)).ToBoolean(),
                    Enabled = iniParser.Get(section, nameof(DropSettings.Enabled)).ToBoolean(),
                    StackItems = iniParser.Get(section, nameof(DropSettings.StackItems)).ToBoolean(),
                    SpawnInsideInventory = iniParser.Get(section, nameof(DropSettings.SpawnInsideInventory)).ToBoolean()
                };

                dropSettings.ItemTypes.AddRange(iniParser.Get(section, nameof(DropSettings.ItemTypes)).ToString().Split(','));
                dropSettings.ItemIds.AddRange(iniParser.Get(section, nameof(DropSettings.ItemIds)).ToString().Split(','));
                dropSettings.RequiredItemTypes.AddRange(iniParser.Get(section, nameof(DropSettings.RequiredItemTypes)).ToString().Split(','));
                dropSettings.RequiredItemIds.AddRange(iniParser.Get(section, nameof(DropSettings.RequiredItemIds)).ToString().Split(','));
                dropSettings.RequiredItemAmounts.AddRange(iniParser.Get(section, nameof(DropSettings.RequiredItemAmounts)).ToString().Split(',').Select(int.Parse));

                settings.BlockDropSettings.Add(dropSettings);
            }
        }


    }

    public class CustomItemSpawnerSettings
    {
        public const string GlobalFileName = "GlobalConfig.ini";
        public const string ItemSpawnerFileName = "CustomItemSpawner.ini";
        private const string IniSection = "Config";

        public int BaseUpdateInterval { get; set; } = 300;
        public List<DropSettings> BlockDropSettings { get; set; } = new List<DropSettings>();

        public void Load()
        {
            MyIni iniParser = new MyIni();

            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(GlobalFileName, typeof(CustomItemSpawnerSettings)))
            {
                using (TextReader file = MyAPIGateway.Utilities.ReadFileInWorldStorage(GlobalFileName, typeof(CustomItemSpawnerSettings)))
                {
                    string text = file.ReadToEnd();

                    MyIniParseResult result;
                    if (!iniParser.TryParse(text, out result))
                        throw new Exception($"Config error: {result.ToString()}");

                    // Load base settings
                    BaseUpdateInterval = iniParser.Get(IniSection, nameof(BaseUpdateInterval)).ToInt32(BaseUpdateInterval);
                }
            }
            else
            {
                throw new Exception("GlobalConfig.ini not found.");
            }

            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(ItemSpawnerFileName, typeof(CustomItemSpawnerSettings)))
            {
                using (TextReader file = MyAPIGateway.Utilities.ReadFileInWorldStorage(ItemSpawnerFileName, typeof(CustomItemSpawnerSettings)))
                {
                    string text = file.ReadToEnd();

                    MyIniParseResult result;
                    if (!iniParser.TryParse(text, out result))
                        throw new Exception($"Config error: {result.ToString()}");

                    // Load block drop settings
                    List<string> sections = new List<string>();
                    iniParser.GetSections(sections);

                    foreach (var section in sections)
                    {
                        if (section == IniSection)
                            continue;

                        var dropSettings = new DropSettings
                        {
                            BlockId = section,
                            BlockType = iniParser.Get(section, nameof(DropSettings.BlockType)).ToString(),
                            MinAmount = iniParser.Get(section, nameof(DropSettings.MinAmount)).ToInt32(),
                            MaxAmount = iniParser.Get(section, nameof(DropSettings.MaxAmount)).ToInt32(),
                            DamageAmount = (float)iniParser.Get(section, nameof(DropSettings.DamageAmount)).ToDouble(),
                            MinHealthPercentage = (float)iniParser.Get(section, nameof(DropSettings.MinHealthPercentage)).ToDouble(),
                            MaxHealthPercentage = (float)iniParser.Get(section, nameof(DropSettings.MaxHealthPercentage)).ToDouble(),
                            MinHeight = iniParser.Get(section, nameof(DropSettings.MinHeight)).ToDouble(),
                            MaxHeight = iniParser.Get(section, nameof(DropSettings.MaxHeight)).ToDouble(),
                            MinRadius = iniParser.Get(section, nameof(DropSettings.MinRadius)).ToDouble(),
                            MaxRadius = iniParser.Get(section, nameof(DropSettings.MaxRadius)).ToDouble(),
                            SpawnTriggerInterval = iniParser.Get(section, nameof(DropSettings.SpawnTriggerInterval)).ToInt32(),
                            EnableAirtightAndOxygen = iniParser.Get(section, nameof(DropSettings.EnableAirtightAndOxygen)).ToBoolean(),
                            Enabled = iniParser.Get(section, nameof(DropSettings.Enabled)).ToBoolean(),
                            StackItems = iniParser.Get(section, nameof(DropSettings.StackItems)).ToBoolean(),
                            SpawnInsideInventory = iniParser.Get(section, nameof(DropSettings.SpawnInsideInventory)).ToBoolean()
                        };

                        dropSettings.ItemTypes.AddRange(iniParser.Get(section, nameof(DropSettings.ItemTypes)).ToString().Split(','));
                        dropSettings.ItemIds.AddRange(iniParser.Get(section, nameof(DropSettings.ItemIds)).ToString().Split(','));
                        dropSettings.RequiredItemTypes.AddRange(iniParser.Get(section, nameof(DropSettings.RequiredItemTypes)).ToString().Split(','));
                        dropSettings.RequiredItemIds.AddRange(iniParser.Get(section, nameof(DropSettings.RequiredItemIds)).ToString().Split(','));
                        dropSettings.RequiredItemAmounts.AddRange(iniParser.Get(section, nameof(DropSettings.RequiredItemAmounts)).ToString().Split(',').Select(int.Parse));

                        BlockDropSettings.Add(dropSettings);
                    }
                }
            }
            else
            {
                throw new Exception("CustomItemSpawner.ini not found.");
            }
        }

        public void Save()
        {
            MyIni iniParser = new MyIni();

            // Save base settings
            iniParser.Set(IniSection, nameof(BaseUpdateInterval), BaseUpdateInterval);

            // Save block drop settings
            foreach (var dropSetting in BlockDropSettings)
            {
                var section = dropSetting.BlockId;
                iniParser.Set(section, nameof(DropSettings.BlockType), dropSetting.BlockType);
                iniParser.Set(section, nameof(DropSettings.ItemTypes), string.Join(",", dropSetting.ItemTypes));
                iniParser.Set(section, nameof(DropSettings.ItemIds), string.Join(",", dropSetting.ItemIds));
                iniParser.Set(section, nameof(DropSettings.MinAmount), dropSetting.MinAmount);
                iniParser.Set(section, nameof(DropSettings.MaxAmount), dropSetting.MaxAmount);
                iniParser.Set(section, nameof(DropSettings.DamageAmount), dropSetting.DamageAmount);
                iniParser.Set(section, nameof(DropSettings.MinHeight), dropSetting.MinHeight);
                iniParser.Set(section, nameof(DropSettings.MaxHeight), dropSetting.MaxHeight);
                iniParser.Set(section, nameof(DropSettings.MinRadius), dropSetting.MinRadius);
                iniParser.Set(section, nameof(DropSettings.MaxRadius), dropSetting.MaxRadius);
                iniParser.Set(section, nameof(DropSettings.SpawnTriggerInterval), dropSetting.SpawnTriggerInterval);
                iniParser.Set(section, nameof(DropSettings.EnableAirtightAndOxygen), dropSetting.EnableAirtightAndOxygen);
                iniParser.Set(section, nameof(DropSettings.Enabled), dropSetting.Enabled);
                iniParser.Set(section, nameof(DropSettings.StackItems), dropSetting.StackItems);
                iniParser.Set(section, nameof(DropSettings.SpawnInsideInventory), dropSetting.SpawnInsideInventory);
                iniParser.Set(section, nameof(DropSettings.RequiredItemTypes), string.Join(",", dropSetting.RequiredItemTypes));
                iniParser.Set(section, nameof(DropSettings.RequiredItemIds), string.Join(",", dropSetting.RequiredItemIds));
                iniParser.Set(section, nameof(DropSettings.RequiredItemAmounts), string.Join(",", dropSetting.RequiredItemAmounts));

                using (TextWriter file = MyAPIGateway.Utilities.WriteFileInWorldStorage(ItemSpawnerFileName, typeof(CustomItemSpawnerSettings)))
                {
                    file.Write(iniParser.ToString());
                }
            }
        }
    }

    public class DropSettings
    {
        public string BlockId { get; set; }
        public string BlockType { get; set; }
        public List<string> ItemTypes { get; set; } = new List<string>();
        public List<string> ItemIds { get; set; } = new List<string>();
        public int MinAmount { get; set; }
        public int MaxAmount { get; set; }
        public float DamageAmount { get; set; }
        public float MinHealthPercentage { get; set; } = 0.1f;
        public float MaxHealthPercentage { get; set; } = 1.0f;
        public double MinHeight { get; set; }
        public double MaxHeight { get; set; }
        public double MinRadius { get; set; }
        public double MaxRadius { get; set; }
        public int SpawnTriggerInterval { get; set; }
        public bool EnableAirtightAndOxygen { get; set; }
        public bool Enabled { get; set; }
        public bool StackItems { get; set; }
        public bool SpawnInsideInventory { get; set; }
        public List<string> RequiredItemTypes { get; set; } = new List<string>();
        public List<string> RequiredItemIds { get; set; } = new List<string>();
        public List<int> RequiredItemAmounts { get; set; } = new List<int>();
    }
}

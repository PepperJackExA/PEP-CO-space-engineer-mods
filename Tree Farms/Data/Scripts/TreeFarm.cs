using Sandbox.Common.ObjectBuilders;
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
using System.IO;
using Sandbox.Game.Entities;
using System.Linq;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace PEPCO.iSurvival.TreeFarm
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class TreeFarm : MySessionComponentBase
    {
        private int updateTickCounter = 0;
        private long totalUpdateTicks = 0;
        private bool isLoading = true;
        private int loadingTickCount = 3;
        private static readonly Random randomGenerator = new Random();
        private static readonly Dictionary<string, string> blockTypeMappings = new Dictionary<string, string>
        {
            { "CubeBlock", "MyObjectBuilder_CubeBlock" },
            { "Motor", "MyObjectBuilder_MotorStator" },
            { "Piston", "MyObjectBuilder_PistonBase" }
        };
        private static readonly Dictionary<string, Type> itemTypeMappings = new Dictionary<string, Type>
        {
            { "Consumable", typeof(MyObjectBuilder_ConsumableItem) },
            { "Ore", typeof(MyObjectBuilder_Ore) },
            { "Ingot", typeof(MyObjectBuilder_Ingot) },
            { "Component", typeof(MyObjectBuilder_Component) }
        };
        public static TreeFarmSettings settings = new TreeFarmSettings();

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            if (MyAPIGateway.Session.IsServer)
            {
                settings.Load();
                settings.LoadAllDropSettings();
                MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
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
                    settings.Load();
                    settings.LoadAllDropSettings();
                    MyAPIGateway.Utilities.ShowMessage("TreeFarm", "Configuration reloaded.");
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
                DropItemsNearBlocks();
            }
        }

        private void DropItemsNearBlocks()
        {
            long baseUpdateCycles = totalUpdateTicks / settings.BaseUpdateInterval;

            foreach (var entity in MyEntities.GetEntities())
            {
                IMyCubeGrid grid = entity as IMyCubeGrid;
                if (grid != null)
                {
                    var blocks = new List<IMySlimBlock>();
                    grid.GetBlocks(blocks, b => b.FatBlock != null && IsValidBlock(b.FatBlock.BlockDefinition.TypeIdString, b.FatBlock.BlockDefinition.SubtypeId));

                    foreach (var block in blocks)
                    {
                        var blockSettings = GetDropSettingsForBlock(block.FatBlock.BlockDefinition.TypeIdString, block.FatBlock.BlockDefinition.SubtypeId);
                        if (blockSettings != null && blockSettings.Enabled && baseUpdateCycles % blockSettings.SpawnTriggerInterval == 0 && IsEnvironmentSuitable(grid, block))
                        {
                            int dropAmount = DropItems(block, blockSettings);
                            block.DoDamage(blockSettings.DamageAmount * dropAmount, MyDamageType.Grind, true);
                        }
                    }
                }
            }
        }

        private bool IsValidBlock(string typeId, string subtypeId)
        {
            return settings.BlockDropSettings.Exists(s => blockTypeMappings[s.BlockType] == typeId && s.BlockId == subtypeId);
        }

        private DropSettings GetDropSettingsForBlock(string typeId, string subtypeId)
        {
            return settings.BlockDropSettings.Find(s => blockTypeMappings[s.BlockType] == typeId && s.BlockId == subtypeId);
        }

        private bool IsEnvironmentSuitable(IMyCubeGrid grid, IMySlimBlock block)
        {
            var settings = GetDropSettingsForBlock(block.FatBlock.BlockDefinition.TypeIdString, block.FatBlock.BlockDefinition.SubtypeId);
            if (settings == null || !settings.EnableAirtightAndOxygen)
                return true;

            bool isAirtight = grid.IsRoomAtPositionAirtight(block.FatBlock.Position);
            double oxygenLevel = MyAPIGateway.Session.OxygenProviderSystem.GetOxygenInPoint(block.FatBlock.GetPosition());

            return isAirtight || oxygenLevel > 0.5;
        }

        private int DropItems(IMySlimBlock block, DropSettings settings)
        {
            int dropAmount = GetWeightedRandomNumber(GenerateProbabilities(settings.MinAmount, settings.MaxAmount));
            for (int i = 0; i < dropAmount; i++)
            {
                Vector3D dropPosition = CalculateDropPosition(block, settings);
                Quaternion dropRotation = Quaternion.CreateFromYawPitchRoll(
                    (float)(randomGenerator.NextDouble() * Math.PI * 2),
                    (float)(randomGenerator.NextDouble() * Math.PI * 2),
                    (float)(randomGenerator.NextDouble() * Math.PI * 2)
                );

                var dropItem = CreateObjectBuilder(settings.ItemType, settings.ItemId);
                MyFloatingObjects.Spawn(
                    new MyPhysicalInventoryItem((VRage.MyFixedPoint)1, dropItem),
                    dropPosition, dropRotation.Up, block.FatBlock.WorldMatrix.Up
                );
            }
            return dropAmount;
        }

        private Vector3D CalculateDropPosition(IMySlimBlock block, DropSettings settings)
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

        private MyObjectBuilder_PhysicalObject CreateObjectBuilder(string itemType, string itemName)
        {
            Type type;
            if (!itemTypeMappings.TryGetValue(itemType, out type))
                throw new Exception($"Unsupported item type: {itemType}");

            var dropItem = MyObjectBuilderSerializer.CreateNewObject(type) as MyObjectBuilder_PhysicalObject;
            dropItem.SubtypeName = itemName;
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

        public class TreeFarmSettings
        {
            private const string VariableId = nameof(TreeFarmSettings);
            private const string GlobalFileName = "TreeFarmSettings.ini";
            private const string IniSection = "Config";
            private const string FileExtension = ".ini";

            public const int DEFAULT_UPDATE_INTERVAL = 300;
            public int BaseUpdateInterval = DEFAULT_UPDATE_INTERVAL;
            public List<ulong> playerExceptions = new List<ulong>();

            public List<DropSettings> BlockDropSettings = new List<DropSettings>
            {
                new DropSettings("CubeBlock", "AppleTreeFarmLG", "Consumable", "Apple", 1, 5, 0.1f, 1.0, 3.0, 0.5, 2.0, 10, true, true),
                new DropSettings("CubeBlock", "AppleTreeFarm", "Consumable", "Apple", 1, 5, 0.1f, 1.0, 3.0, 0.5, 2.0, 10, true, true)
            };

            public TreeFarmSettings() { }

            public void Load()
            {
                if (MyAPIGateway.Session.IsServer)
                    LoadOnHost();
                else
                    LoadOnClient();
            }

            public void LoadOnHost()
            {
                MyIni iniParser = new MyIni();

                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(GlobalFileName, typeof(TreeFarmSettings)))
                {
                    using (TextReader file = MyAPIGateway.Utilities.ReadFileInWorldStorage(GlobalFileName, typeof(TreeFarmSettings)))
                    {
                        string text = file.ReadToEnd();

                        MyIniParseResult result;
                        if (!iniParser.TryParse(text, out result))
                            throw new Exception($"Config error: {result.ToString()}");

                        LoadGlobalSettings(iniParser);
                    }
                }

                SaveConfig();
            }

            public void LoadOnClient()
            {
                string text;
                if (!MyAPIGateway.Utilities.GetVariable<string>(VariableId, out text))
                    throw new Exception("No config found in sandbox.sbc!");

                MyIni iniParser = new MyIni();
                MyIniParseResult result;
                if (!iniParser.TryParse(text, out result))
                    throw new Exception($"Config error: {result.ToString()}");

                LoadGlobalSettings(iniParser);
            }

            public void LoadAllDropSettings()
            {
                foreach (var dropSetting in BlockDropSettings)
                {
                    LoadDropSetting(dropSetting.BlockId);
                }
            }

            public void LoadDropSetting(string blockId)
            {
                MyIni iniParser = new MyIni();
                string fileName = blockId + FileExtension;
                MyLog.Default.WriteLineAndConsole($"Loading settings from file: {fileName}");

                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, typeof(TreeFarmSettings)))
                {
                    using (TextReader file = MyAPIGateway.Utilities.ReadFileInWorldStorage(fileName, typeof(TreeFarmSettings)))
                    {
                        string text = file.ReadToEnd();

                        MyIniParseResult result;
                        if (!iniParser.TryParse(text, out result))
                        {
                            MyLog.Default.WriteLineAndConsole($"Config error: {result.ToString()}");
                            throw new Exception($"Config error: {result.ToString()}");
                        }

                        LoadDropSetting(iniParser, blockId);
                    }
                }
                else
                {
                    MyLog.Default.WriteLineAndConsole($"Settings file not found: {fileName}");
                }
            }

            public void SaveConfig()
            {
                MyIni iniParser = new MyIni();
                SaveGlobalSettings(iniParser);

                string saveText = iniParser.ToString();
                MyAPIGateway.Utilities.SetVariable<string>(VariableId, saveText);

                if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(GlobalFileName, typeof(TreeFarmSettings)))
                {
                    using (TextWriter file = MyAPIGateway.Utilities.WriteFileInWorldStorage(GlobalFileName, typeof(TreeFarmSettings)))
                    {
                        file.Write(saveText);
                    }
                }

                SaveAllDropSettings();
            }


            public void SaveAllDropSettings()
            {
                foreach (var dropSetting in BlockDropSettings)
                {
                    SaveDropSetting(dropSetting);
                }
            }

            public void SaveDropSetting(DropSettings dropSetting)
            {
                MyIni iniParser = new MyIni();
                SaveDropSetting(iniParser, dropSetting);

                string fileName = dropSetting.BlockId + FileExtension;
                MyLog.Default.WriteLineAndConsole($"Saving settings to file: {fileName}");

                if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, typeof(TreeFarmSettings)))
                {
                    using (TextWriter file = MyAPIGateway.Utilities.WriteFileInWorldStorage(fileName, typeof(TreeFarmSettings)))
                    {
                        file.Write(iniParser.ToString());
                    }
                }
            }


            private void LoadGlobalSettings(MyIni iniParser)
            {
                BaseUpdateInterval = iniParser.Get(IniSection, nameof(BaseUpdateInterval)).ToInt32(BaseUpdateInterval);
                var configList = iniParser.Get(IniSection, nameof(playerExceptions)).ToString().Trim().Split('\n');
                playerExceptions.Clear();
                foreach (var config in configList)
                {
                    ulong playerId;
                    if (ulong.TryParse(config, out playerId) && playerId > 0)
                    {
                        playerExceptions.Add(playerId);
                    }
                }
            }

            private void SaveGlobalSettings(MyIni iniParser)
            {
                iniParser.Set(IniSection, nameof(BaseUpdateInterval), BaseUpdateInterval);
                iniParser.Set(IniSection, nameof(playerExceptions), string.Join("\n", playerExceptions));
            }

            private void LoadDropSetting(MyIni iniParser, string blockId)
            {
                string blockType = iniParser.Get(IniSection, $"{blockId}_BlockType").ToString();
                string itemType = iniParser.Get(IniSection, $"{blockId}_ItemType").ToString();
                string itemId = iniParser.Get(IniSection, $"{blockId}_ItemId").ToString();
                int minAmount = iniParser.Get(IniSection, $"{blockId}_MinAmount").ToInt32();
                int maxAmount = iniParser.Get(IniSection, $"{blockId}_MaxAmount").ToInt32();
                float damageAmount = (float)iniParser.Get(IniSection, $"{blockId}_DamageAmount").ToDouble();
                double minHeight = iniParser.Get(IniSection, $"{blockId}_MinHeight").ToDouble();
                double maxHeight = iniParser.Get(IniSection, $"{blockId}_MaxHeight").ToDouble();
                double minRadius = iniParser.Get(IniSection, $"{blockId}_MinRadius").ToDouble();
                double maxRadius = iniParser.Get(IniSection, $"{blockId}_MaxRadius").ToDouble();
                int spawnTriggerInterval = iniParser.Get(IniSection, $"{blockId}_SpawnTriggerInterval").ToInt32();
                bool enableAirtightAndOxygen = iniParser.Get(IniSection, $"{blockId}_EnableAirtightAndOxygen").ToBoolean(true);
                bool enabled = iniParser.Get(IniSection, $"{blockId}_Enabled").ToBoolean(true);

                var existingSetting = BlockDropSettings.Find(s => s.BlockId == blockId);
                if (existingSetting != null)
                {
                    existingSetting.BlockType = blockType;
                    existingSetting.ItemType = itemType;
                    existingSetting.ItemId = itemId;
                    existingSetting.MinAmount = minAmount;
                    existingSetting.MaxAmount = maxAmount;
                    existingSetting.DamageAmount = damageAmount;
                    existingSetting.MinHeight = minHeight;
                    existingSetting.MaxHeight = maxHeight;
                    existingSetting.MinRadius = minRadius;
                    existingSetting.MaxRadius = maxRadius;
                    existingSetting.SpawnTriggerInterval = spawnTriggerInterval;
                    existingSetting.EnableAirtightAndOxygen = enableAirtightAndOxygen;
                    existingSetting.Enabled = enabled;
                }
                else
                {
                    BlockDropSettings.Add(new DropSettings(blockType, blockId, itemType, itemId, minAmount, maxAmount, damageAmount, minHeight, maxHeight, minRadius, maxRadius, spawnTriggerInterval, enableAirtightAndOxygen, enabled));
                }
            }

            private void SaveDropSetting(MyIni iniParser, DropSettings dropSetting)
            {
                string blockId = dropSetting.BlockId;

                iniParser.Set(IniSection, $"{blockId}_BlockType", dropSetting.BlockType);
                iniParser.Set(IniSection, $"{blockId}_ItemType", dropSetting.ItemType);
                iniParser.Set(IniSection, $"{blockId}_ItemId", dropSetting.ItemId);
                iniParser.Set(IniSection, $"{blockId}_MinAmount", dropSetting.MinAmount);
                iniParser.Set(IniSection, $"{blockId}_MaxAmount", dropSetting.MaxAmount);
                iniParser.Set(IniSection, $"{blockId}_DamageAmount", dropSetting.DamageAmount);
                iniParser.Set(IniSection, $"{blockId}_MinHeight", dropSetting.MinHeight);
                iniParser.Set(IniSection, $"{blockId}_MaxHeight", dropSetting.MaxHeight);
                iniParser.Set(IniSection, $"{blockId}_MinRadius", dropSetting.MinRadius);
                iniParser.Set(IniSection, $"{blockId}_MaxRadius", dropSetting.MaxRadius);
                iniParser.Set(IniSection, $"{blockId}_SpawnTriggerInterval", dropSetting.SpawnTriggerInterval);
                iniParser.Set(IniSection, $"{blockId}_EnableAirtightAndOxygen", dropSetting.EnableAirtightAndOxygen);
                iniParser.Set(IniSection, $"{blockId}_Enabled", dropSetting.Enabled);
            }
        }

        public class DropSettings
        {
            public string BlockType { get; set; }
            public string BlockId { get; set; }
            public string ItemType { get; set; }
            public string ItemId { get; set; }
            public int MinAmount { get; set; }
            public int MaxAmount { get; set; }
            public float DamageAmount { get; set; }
            public double MinHeight { get; set; }
            public double MaxHeight { get; set; }
            public double MinRadius { get; set; }
            public double MaxRadius { get; set; }
            public int SpawnTriggerInterval { get; set; }
            public bool EnableAirtightAndOxygen { get; set; } = true;
            public bool Enabled { get; set; } = true;

            public DropSettings(string blockType, string blockId, string itemType, string itemId, int minAmount, int maxAmount, float damageAmount, double minHeight, double maxHeight, double minRadius, double maxRadius, int spawnTriggerInterval, bool EnableAirtightAndOxygen, bool Enabled)
            {
                BlockType = blockType;
                BlockId = blockId;
                ItemType = itemType;
                ItemId = itemId;
                MinAmount = minAmount;
                MaxAmount = maxAmount;
                DamageAmount = damageAmount;
                MinHeight = minHeight;
                MaxHeight = maxHeight;
                MinRadius = minRadius;
                MaxRadius = maxRadius;
                SpawnTriggerInterval = spawnTriggerInterval;
            }
        }
    }
}

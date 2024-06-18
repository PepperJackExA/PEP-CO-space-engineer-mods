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

        private static readonly Dictionary<string, Type> itemTypeMappings = new Dictionary<string, Type>
        {
            { "MyObjectBuilder_ConsumableItem", typeof(MyObjectBuilder_ConsumableItem) },
            { "MyObjectBuilder_Ore", typeof(MyObjectBuilder_Ore) },
            { "MyObjectBuilder_Ingot", typeof(MyObjectBuilder_Ingot) },
            { "MyObjectBuilder_Component", typeof(MyObjectBuilder_Component) }
        };

        public static TreeFarmSettings settings = new TreeFarmSettings();

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            if (MyAPIGateway.Session.IsServer)
            {
                try
                {
                    settings.Load();
                    InitializeDropSettings();
                    MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
                    MyAPIGateway.Utilities.ShowMessage("TreeFarm", "TreeFarm mod initialized.");
                }
                catch (Exception ex)
                {
                    MyAPIGateway.Utilities.ShowMessage("TreeFarm", $"Initialization error: {ex.Message}");
                }
            }
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
            MyAPIGateway.Utilities.ShowMessage("TreeFarm", "TreeFarm mod unloaded.");
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
                        MyAPIGateway.Utilities.ShowMessage("TreeFarm", "Configuration reloaded.");
                    }
                    catch (Exception ex)
                    {
                        MyAPIGateway.Utilities.ShowMessage("TreeFarm", $"Reload error: {ex.Message}");
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
                    MyAPIGateway.Utilities.ShowMessage("TreeFarm", $"Update error: {ex.Message}");
                }
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
            return settings.BlockDropSettings.Exists(s => s.BlockType == typeId && s.BlockId == subtypeId);
        }

        private DropSettings GetDropSettingsForBlock(string typeId, string subtypeId)
        {
            return settings.BlockDropSettings.Find(s => s.BlockType == typeId && s.BlockId == subtypeId);
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
            if (settings.StackItems)
            {
                Vector3D dropPosition = CalculateDropPosition(block, settings);
                Quaternion dropRotation = Quaternion.CreateFromYawPitchRoll(
                    (float)(randomGenerator.NextDouble() * Math.PI * 2),
                    (float)(randomGenerator.NextDouble() * Math.PI * 2),
                    (float)(randomGenerator.NextDouble() * Math.PI * 2)
                );

                var dropItem = CreateObjectBuilder(settings.ItemType, settings.ItemId);
                MyFloatingObjects.Spawn(
                    new MyPhysicalInventoryItem((VRage.MyFixedPoint)dropAmount, dropItem),
                    dropPosition, dropRotation.Up, block.FatBlock.WorldMatrix.Up
                );

                MyAPIGateway.Utilities.ShowMessage("TreeFarm", $"Dropped {dropAmount} items of {settings.ItemId} at {dropPosition}");
            }
            else
            {
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

                    MyAPIGateway.Utilities.ShowMessage("TreeFarm", $"Dropped item {settings.ItemId} at {dropPosition}");
                }
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
            {
                MyAPIGateway.Utilities.ShowMessage("TreeFarm", $"Unsupported item type: {itemType}");
                throw new Exception($"Unsupported item type: {itemType}");
            }

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

        private void InitializeDropSettings()
        {
            // Reinitialize any cached or in-memory settings if needed.
            // Currently, this function is a placeholder for any additional initialization logic.
        }

        private void ReloadSettings()
        {
            settings.Load();
            InitializeDropSettings();  // Reinitialize settings
        }
    }
}

public class TreeFarmSettings
{
    private const string GlobalFileName = "TreeFarmSettings.ini";
    private const string IniSection = "Config";

    public int BaseUpdateInterval { get; set; } = 300;
    public List<DropSettings> BlockDropSettings { get; set; } = new List<DropSettings>();

    public void Load()
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

                // Load base settings
                BaseUpdateInterval = iniParser.Get(IniSection, nameof(BaseUpdateInterval)).ToInt32(BaseUpdateInterval);

                // Load block drop settings
                List<string> sections = new List<string>();
                iniParser.GetSections(sections);

                BlockDropSettings.Clear();  // Clear existing settings before loading new ones

                foreach (var section in sections)
                {
                    if (section == IniSection)
                        continue;

                    var dropSettings = new DropSettings
                    {
                        BlockId = section,
                        BlockType = iniParser.Get(section, nameof(DropSettings.BlockType)).ToString(),
                        ItemType = iniParser.Get(section, nameof(DropSettings.ItemType)).ToString(),
                        ItemId = iniParser.Get(section, nameof(DropSettings.ItemId)).ToString(),
                        MinAmount = iniParser.Get(section, nameof(DropSettings.MinAmount)).ToInt32(),
                        MaxAmount = iniParser.Get(section, nameof(DropSettings.MaxAmount)).ToInt32(),
                        DamageAmount = (float)iniParser.Get(section, nameof(DropSettings.DamageAmount)).ToDouble(),
                        MinHeight = iniParser.Get(section, nameof(DropSettings.MinHeight)).ToDouble(),
                        MaxHeight = iniParser.Get(section, nameof(DropSettings.MaxHeight)).ToDouble(),
                        MinRadius = iniParser.Get(section, nameof(DropSettings.MinRadius)).ToDouble(),
                        MaxRadius = iniParser.Get(section, nameof(DropSettings.MaxRadius)).ToDouble(),
                        SpawnTriggerInterval = iniParser.Get(section, nameof(DropSettings.SpawnTriggerInterval)).ToInt32(),
                        EnableAirtightAndOxygen = iniParser.Get(section, nameof(DropSettings.EnableAirtightAndOxygen)).ToBoolean(),
                        Enabled = iniParser.Get(section, nameof(DropSettings.Enabled)).ToBoolean(),
                        StackItems = iniParser.Get(section, nameof(DropSettings.StackItems)).ToBoolean()
                    };

                    BlockDropSettings.Add(dropSettings);
                }

                MyAPIGateway.Utilities.ShowMessage("TreeFarm", "Settings loaded successfully.");
            }
        }
        else
        {
            MyAPIGateway.Utilities.ShowMessage("TreeFarm", "Settings file not found.");
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
            iniParser.Set(section, nameof(DropSettings.ItemType), dropSetting.ItemType);
            iniParser.Set(section, nameof(DropSettings.ItemId), dropSetting.ItemId);
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
        }

        using (TextWriter file = MyAPIGateway.Utilities.WriteFileInWorldStorage(GlobalFileName, typeof(TreeFarmSettings)))
        {
            file.Write(iniParser.ToString());
        }
    }
}

public class DropSettings
{
    public string BlockId { get; set; }
    public string BlockType { get; set; }
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
    public bool EnableAirtightAndOxygen { get; set; }
    public bool Enabled { get; set; }
    public bool StackItems { get; set; }  // New property for stacking items
}

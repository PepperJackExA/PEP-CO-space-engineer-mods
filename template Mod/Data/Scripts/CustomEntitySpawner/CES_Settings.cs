using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using VRage.Game.ModAPI.Ingame.Utilities;
using System.Linq;

namespace PEPCO.iSurvival.CustomEntitySpawner
{
    public class CustomEntitySpawnerSettings
    {
        // Constants for file names
        public const string GlobalFileName = "GlobalConfig.ini";
        public const string EntitySpawnerFileName = "CustomEntitySpawner.ini";
        private const string IniSection = "Config";

        // Settings properties
        public int BaseUpdateInterval { get; set; } = 60;
        public bool EnableLogging { get; set; } = true;
        public int CleanupInterval { get; set; } = 18000;
        public int GlobalMaxEntities { get; set; } = 32;

        public List<BotSpawnerConfig> BlockSpawnSettings { get; set; } = new List<BotSpawnerConfig>();

        // Load settings from configuration files
        public void Load()
        {
            MyIni iniParser = new MyIni();

            // Load Global Configuration
            LoadIniFile(GlobalFileName, iniParser);
            BaseUpdateInterval = iniParser.Get(IniSection, nameof(BaseUpdateInterval)).ToInt32(BaseUpdateInterval);
            EnableLogging = iniParser.Get(IniSection, nameof(EnableLogging)).ToBoolean(EnableLogging);
            CleanupInterval = iniParser.Get(IniSection, nameof(CleanupInterval)).ToInt32(CleanupInterval);
            GlobalMaxEntities = iniParser.Get(IniSection, nameof(GlobalMaxEntities)).ToInt32(GlobalMaxEntities);

            // Load Entity Spawner Configuration
            LoadIniFile(EntitySpawnerFileName, iniParser);
            LoadBlockSpawnSettings(iniParser);

            // Load configurations from all mods
            foreach (var modItem in MyAPIGateway.Session.Mods)
            {
                string cesFileName = $"{modItem.PublishedFileId}_CES.ini";
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(cesFileName, typeof(CustomEntitySpawnerSettings)))
                {
                    LoadIniFile(cesFileName, iniParser);
                    LoadBlockSpawnSettings(iniParser);
                }
            }
        }

        // Helper method to load an INI file
        private void LoadIniFile(string fileName, MyIni iniParser)
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, typeof(CustomEntitySpawnerSettings)))
            {
                using (TextReader file = MyAPIGateway.Utilities.ReadFileInWorldStorage(fileName, typeof(CustomEntitySpawnerSettings)))
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

        // Load block spawn settings from INI sections
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
                    PlayerDistanceCheck = iniParser.Get(section, nameof(BotSpawnerConfig.PlayerDistanceCheck)).ToInt32(1000),
                    EnableEntitySpawning = iniParser.Get(section, nameof(BotSpawnerConfig.EnableEntitySpawning)).ToBoolean(false),
                    EnableItemSpawning = iniParser.Get(section, nameof(BotSpawnerConfig.EnableItemSpawning)).ToBoolean(false),
                    SpawnItemsWithEntities = iniParser.Get(section, nameof(BotSpawnerConfig.SpawnItemsWithEntities)).ToBoolean(false),
                    MinEntityAmount = iniParser.Get(section, nameof(BotSpawnerConfig.MinEntityAmount)).ToInt32(1),
                    MaxEntityAmount = iniParser.Get(section, nameof(BotSpawnerConfig.MaxEntityAmount)).ToInt32(1),
                    MinItemAmount = iniParser.Get(section, nameof(BotSpawnerConfig.MinItemAmount)).ToInt32(1),
                    MaxItemAmount = iniParser.Get(section, nameof(BotSpawnerConfig.MaxItemAmount)).ToInt32(1),
                    UseWeightedDrops = iniParser.Get(section, nameof(BotSpawnerConfig.UseWeightedDrops)).ToBoolean(false),
                    MaxEntitiesInArea = iniParser.Get(section, nameof(BotSpawnerConfig.MaxEntitiesInArea)).ToInt32(30),
                    MaxEntitiesRadius = iniParser.Get(section, nameof(BotSpawnerConfig.MaxEntitiesRadius)).ToDouble(100),
                    StackItems = iniParser.Get(section, nameof(BotSpawnerConfig.StackItems)).ToBoolean(false),
                    SpawnInsideInventory = iniParser.Get(section, nameof(BotSpawnerConfig.SpawnInsideInventory)).ToBoolean(false),
                    DamageAmount = (float)iniParser.Get(section, nameof(BotSpawnerConfig.DamageAmount)).ToDouble(0),
                    Repair = iniParser.Get(section, nameof(BotSpawnerConfig.Repair)).ToBoolean(false),
                    MinHealthPercentage = (float)iniParser.Get(section, nameof(BotSpawnerConfig.MinHealthPercentage)).ToDouble(0),
                    MaxHealthPercentage = (float)iniParser.Get(section, nameof(BotSpawnerConfig.MaxHealthPercentage)).ToDouble(1),
                    MinHeight = iniParser.Get(section, nameof(BotSpawnerConfig.MinHeight)).ToDouble(0),
                    MaxHeight = iniParser.Get(section, nameof(BotSpawnerConfig.MaxHeight)).ToDouble(2),
                    MinRadius = iniParser.Get(section, nameof(BotSpawnerConfig.MinRadius)).ToDouble(0),
                    MaxRadius = iniParser.Get(section, nameof(BotSpawnerConfig.MaxRadius)).ToDouble(2),
                    SpawnTriggerInterval = iniParser.Get(section, nameof(BotSpawnerConfig.SpawnTriggerInterval)).ToInt32(5),
                    EnableAirtightAndOxygen = iniParser.Get(section, nameof(BotSpawnerConfig.EnableAirtightAndOxygen)).ToBoolean(false),
                    RequiredEntity = iniParser.Get(section, nameof(BotSpawnerConfig.RequiredEntity)).ToString("NA"),
                    RequiredEntityRadius = iniParser.Get(section, nameof(BotSpawnerConfig.RequiredEntityRadius)).ToDouble(100),
                    RequiredEntityNumber = iniParser.Get(section, nameof(BotSpawnerConfig.RequiredEntityNumber)).ToInt32(0),
                    RequireEntityNumberForTotalEntities = iniParser.Get(section, nameof(BotSpawnerConfig.RequireEntityNumberForTotalEntities)).ToBoolean(false),
                    RequireEntityCenterOn = iniParser.Get(section, nameof(BotSpawnerConfig.RequireEntityCenterOn)).ToBoolean(false)
                };

                // Parse and add lists
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
    }
}

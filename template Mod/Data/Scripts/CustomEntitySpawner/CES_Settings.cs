using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using VRage.Game.ModAPI.Ingame.Utilities;
using System.Linq;
using PEPCO.LogError;
using ProtoBuf;
using VRage.Game;

namespace PEPCO.iSurvival.CustomEntitySpawner
{
    public class SharedData
    {
        
    }

    public class CustomEntitySpawnerSettings
    {

        public static CustomEntitySpawnerSettings chat = new CustomEntitySpawnerSettings();
        public static CustomEntitySpawner CESspawner = new CustomEntitySpawner();
        public static PEPCO_LogError log = new PEPCO_LogError();

        public string GlobalFileName = "GlobalConfig.ini";
        public string EntitySpawnerFileName = "CustomEntitySpawner.ini";
        private string IniSection = "Config";

        public bool scriptPaused = false;

        public Dictionary<string, Type> itemTypeMappings = new Dictionary<string, Type>
        {
            { "MyObjectBuilder_ConsumableItem", typeof(MyObjectBuilder_ConsumableItem) },
            { "MyObjectBuilder_Ore", typeof(MyObjectBuilder_Ore) },
            { "MyObjectBuilder_Ingot", typeof(MyObjectBuilder_Ingot) },
            { "MyObjectBuilder_Component", typeof(MyObjectBuilder_Component) }
        };
        public int BaseUpdateInterval { get; set; } = 60;
        public bool EnableLogging { get; set; } = true;
        public int CleanupInterval { get; set; } = 18000;
        public int GlobalMaxEntities { get; set; } = 32;

        public List<BotSpawnerConfig> BlockSpawnSettings { get; set; } = new List<BotSpawnerConfig>();

        
        public string DefaultGlobalIniContent = @"
; ==============================================
; HOW TO USE GlobalConfig.ini
; ==============================================C
;BaseUpdateInterval 60 = 1 second
;EnableLogging will enable the storage log file and some ingame messages
;CleanupInterval 60 = 1 second
;GlobalMaxEntities Required Items will be lost if this is not set lower then server Max entities setting.
[Config]
BaseUpdateInterval=60    
EnableLogging=true
CleanupInterval=0
GlobalMaxEntities=30
";

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
        public void UpdateBlockSettings(long entityId, BotSpawnerConfig settings)
        {
            // Find the existing block setting by EntityId and update it
            var existingSetting = BlockSpawnSettings.FirstOrDefault(s => s.BlockId == settings.BlockId);
            if (existingSetting != null)
            {
                existingSetting = settings;
            }
            else
            {
                BlockSpawnSettings.Add(settings);
            }
        }
        public void Load()
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
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(cesFileName, typeof(CustomEntitySpawnerSettings)))
                {
                    LoadIniFile(cesFileName, iniParser);
                    LoadBlockSpawnSettings(iniParser);
                }
            }
        }

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
        [ProtoMember(1)]
        public string BlockId { get; set; }
        [ProtoMember(2)]
        public string BlockType { get; set; }
        [ProtoMember(3)]
        public List<string> EntityID { get; set; } = new List<string>();
        [ProtoMember(4)]
        public List<string> ItemTypes { get; set; } = new List<string>();
        [ProtoMember(5)]
        public List<string> ItemIds { get; set; } = new List<string>();
        [ProtoMember(6)]
        public bool StackItems { get; set; } = false;
        [ProtoMember(7)]
        public bool SpawnInsideInventory { get; set; } = false;
        [ProtoMember(8)]
        public bool EnableEntitySpawning { get; set; } = true;
        [ProtoMember(9)]
        public bool EnableItemSpawning { get; set; } = true;
        [ProtoMember(10)]
        public bool SpawnItemsWithEntities { get; set; } = false;
        [ProtoMember(11)]
        public int MinEntityAmount { get; set; } = 1;
        [ProtoMember(12)]
        public int MaxEntityAmount { get; set; } = 1;
        [ProtoMember(13)]
        public int MinItemAmount { get; set; } = 1;
        [ProtoMember(14)]
        public int MaxItemAmount { get; set; } = 1;
        [ProtoMember(15)]
        public bool UseWeightedDrops { get; set; } = false;
        [ProtoMember(16)]
        public float DamageAmount { get; set; } = 0;
        [ProtoMember(17)]
        public bool Repair { get; set; } = false;
        [ProtoMember(18)]
        public float MinHealthPercentage { get; set; } = 0.1f;
        [ProtoMember(19)]
        public float MaxHealthPercentage { get; set; } = 1.0f;
        [ProtoMember(20)]
        public double MinHeight { get; set; } = 1.0;
        [ProtoMember(21)]
        public double MaxHeight { get; set; } = 2.0;
        [ProtoMember(22)]
        public double MinRadius { get; set; } = 0.5;
        [ProtoMember(23)]
        public double MaxRadius { get; set; } = 2.0;
        [ProtoMember(24)]
        public int SpawnTriggerInterval { get; set; } = 10;
        [ProtoMember(25)]
        public bool EnableAirtightAndOxygen { get; set; } = false;
        [ProtoMember(26)]
        public bool Enabled { get; set; } = true;
        [ProtoMember(27)]
        public List<string> RequiredItemTypes { get; set; } = new List<string>();
        [ProtoMember(28)]
        public List<string> RequiredItemIds { get; set; } = new List<string>();
        [ProtoMember(29)]
        public List<int> RequiredItemAmounts { get; set; } = new List<int>();
        [ProtoMember(30)]
        public List<string> PermanentRequiredItemTypes { get; set; } = new List<string>();
        [ProtoMember(31)]
        public List<string> PermanentRequiredItemIds { get; set; } = new List<string>();
        [ProtoMember(32)]
        public List<int> PermanentRequiredItemAmounts { get; set; } = new List<int>();
        [ProtoMember(33)]
        public int PlayerDistanceCheck { get; set; } = 1000;
        [ProtoMember(34)]
        public string RequiredEntity { get; set; } = "";
        [ProtoMember(35)]
        public double RequiredEntityRadius { get; set; } = 10;
        [ProtoMember(36)]
        public int RequiredEntityNumber { get; set; } = 0;
        [ProtoMember(37)]
        public bool RequireEntityNumberForTotalEntities { get; set; } = false;
        [ProtoMember(38)]
        public int MaxEntitiesInArea { get; set; } = 10;
        [ProtoMember(39)]
        public double MaxEntitiesRadius { get; set; } = 100;
        [ProtoMember(40)]
        public bool RequireEntityCenterOn { get; set; } = false;

        public BotSpawnerConfig() { }

        public BotSpawnerConfig(string blockId, string blockType)
        {
            BlockId = blockId;
            BlockType = blockType;
        }
    }

}

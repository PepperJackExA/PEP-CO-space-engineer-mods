using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace PEPCO.iSurvival.CustomEntitySpawner.Config
{
    public class BlockSettings
    {
        public string BlockId { get; set; } = "DefaultBlockId";
        public string BlockType { get; set; } = "MyObjectBuilder_Default";
        public bool Enabled { get; set; } = true;
        public int DefaultSpawnRate { get; set; } = 10;  // Default setting example
        
        // Load specific block settings or defaults if missing
        public void LoadBlockConfig(MyIni ini, string section)
        {
            BlockId = ini.Get(section, "BlockId").ToString(BlockId);
            BlockType = ini.Get(section, "BlockType").ToString(BlockType);
            Enabled = ini.Get(section, "Enabled").ToBoolean(Enabled);
            DefaultSpawnRate = ini.Get(section, "SpawnRate").ToInt32(DefaultSpawnRate);
        }

        // Write default block settings into a specified section
        public void WriteDefaultBlockSettingsToIni(MyIni ini, string section)
        {
            ini.Set(section, "BlockId", BlockId);
            ini.Set(section, "BlockType", BlockType);
            ini.Set(section, "Enabled", Enabled);
            ini.Set(section, "SpawnRate", DefaultSpawnRate);

        }

        public bool EnableCustomSpawning { get; set; } = false;
        public string EntityName { get; set; } = "Wolf";  // Default name, can be customized in settings
        public int SpawnCount { get; set; } = 1;          // Default spawn count, customizable
        public double SpawnRadius { get; set; } = 10.0;   // Default radius
        public double SpawnHeight { get; set; } = 5.0;    // Default height

        public void LoadSettings(MyIni ini)
        {
            EnableCustomSpawning = ini.Get("CustomSpawning", "EnableCustomSpawning").ToBoolean(false);
            EntityName = ini.Get("CustomSpawning", "EntityName").ToString("Wolf");
            SpawnCount = ini.Get("CustomSpawning", "SpawnCount").ToInt32(1);
            SpawnRadius = ini.Get("CustomSpawning", "SpawnRadius").ToDouble(10.0);
            SpawnHeight = ini.Get("CustomSpawning", "SpawnHeight").ToDouble(5.0);
        }

    }
}

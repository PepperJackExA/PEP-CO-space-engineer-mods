using Sandbox.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;
using System.Collections.Generic;

namespace PEPCO.iSurvival.CustomEntitySpawner.Config
{
    public static class IniFileHandler
    {
        public static void EnsureDefaultIniFilesExist()
        {
            string filePath = "CustomEntitySpawnerConfig.ini";
            if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(filePath, typeof(IniFileHandler)))
            {
                var ini = new MyIni();

                // Write global default settings
                var globalSettings = new GlobalSettings();
                globalSettings.WriteDefaultSettingsToIni(ini);

                // Write block-specific default settings for known blocks
                var defaultBlockSettings = new BlockSettings();
                defaultBlockSettings.WriteDefaultBlockSettingsToIni(ini, "SmallBlockSmallContainerSpawn");
                defaultBlockSettings.WriteDefaultBlockSettingsToIni(ini, "LargeBlockLargeContainerSpawn");

                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(filePath, typeof(IniFileHandler)))
                {
                    writer.Write(ini.ToString());
                }
            }
        }

        public static void LoadAllFilesFromWorldStorage(List<BlockSettings> blockSettingsList)
        {
            string filePath = "CustomEntitySpawnerConfig.ini";
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(filePath, typeof(IniFileHandler)))
            {
                var ini = new MyIni();
                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(filePath, typeof(IniFileHandler)))
                {
                    ini.TryParse(reader.ReadToEnd());

                    // Manually load each known section, or add new ones as required
                    LoadSectionIfExists(ini, "SmallBlockSmallContainerSpawn", blockSettingsList);
                    LoadSectionIfExists(ini, "LargeBlockLargeContainerSpawn", blockSettingsList);
                }
            }
        }

        private static void LoadSectionIfExists(MyIni ini, string sectionName, List<BlockSettings> blockSettingsList)
        {
            if (ini.ContainsSection(sectionName))
            {
                var blockSettings = new BlockSettings();
                blockSettings.LoadBlockConfig(ini, sectionName);
                blockSettingsList.Add(blockSettings);
            }
            else
            {
                // If section is missing, load default settings
                var defaultSettings = new BlockSettings();
                blockSettingsList.Add(defaultSettings); // Add default block settings
            }
        }
    }
}

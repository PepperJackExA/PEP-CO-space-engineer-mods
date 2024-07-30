using Sandbox.ModAPI;
using VRage.Utils;
using System.IO;

namespace PEPCO.Template
{
    public static class ConfigSettings
    {
        public static int SomeSetting = 60;
        private const string ConfigFileName = "PEPCO_Template_Config.ini";

        public static void Load()
        {
            if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(ConfigFileName, typeof(ConfigSettings)))
            {
                CreateDefaultConfig();
            }

            ReadConfig();
        }

        private static void CreateDefaultConfig()
        {
            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(ConfigFileName, typeof(ConfigSettings)))
            {
                writer.Write("[Settings]\n");
                writer.Write("SomeSetting=60\n");
            }
        }

        private static void ReadConfig()
        {
            using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(ConfigFileName, typeof(ConfigSettings)))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("SomeSetting="))
                    {
                        SomeSetting = int.Parse(line.Substring("SomeSetting=".Length));
                    }
                }
            }
        }
    }
}

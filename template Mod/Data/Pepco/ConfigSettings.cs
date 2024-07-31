using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace Pepco
{
    public class ConfigSettings
    {
        private const string ConfigFileName = "ServerConfig.ini";
        private MyIni configIni = new MyIni();

        public string Setting1 { get; private set; } = "Text";
        public int Setting2 { get; private set; } = 16;

        public void LoadConfig()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(ConfigFileName, typeof(ConfigSettings)))
            {
                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(ConfigFileName, typeof(ConfigSettings)))
                {
                    string content = reader.ReadToEnd();
                    configIni.TryParse(content);
                }

                MyIniValue setting1 = configIni.Get("General", "Setting1");
                if (setting1.IsEmpty == false)
                {
                    Setting1 = setting1.ToString();
                }

                MyIniValue setting2Value = configIni.Get("General", "Setting2");
                int setting2;
                if (setting2Value.IsEmpty == false && int.TryParse(setting2Value.ToString(), out setting2))
                {
                    Setting2 = setting2;
                }
            }
            else
            {
                SaveConfig();
            }
        }

        public void SaveConfig()
        {
            configIni.Set("General", "Setting1", Setting1);
            configIni.Set("General", "Setting2", Setting2);

            
            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(ConfigFileName, typeof(ConfigSettings)))
            {
                writer.Write(configIni.ToString());
            }
        }

        public void UpdateConfig(string key, string value)
        {
            switch (key.ToLower())
            {
                case "setting1":
                    Setting1 = value;
                    break;
                case "setting2":
                    int setting2;
                    if (int.TryParse(value, out setting2))
                    {
                        Setting2 = setting2;
                    }
                    break;
                default:
                    MyAPIGateway.Utilities.ShowMessage("Config", $"Unknown config key: {key}");
                    return;
            }
            SaveConfig();
        }
    }
}

using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame.Utilities;
using System;
using VRage.Utils;

namespace PEPCO
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class SingleFileMod : MySessionComponentBase
    {
        private const string ClientConfigFileName = "PEPCOClientSettings.ini";
        private const string ServerConfigFileName = "PEPCOServerSettings.ini";

        public static int ClientExampleSetting { get; private set; } = 42;
        public static int ServerExampleSetting { get; private set; } = 84;

        public override void BeforeStart()
        {
            if (MyAPIGateway.Session.IsServer)
            {
                CreateServerConfigIfNotExists();
                LoadServerConfig();
            }
            LoadClientConfig();
            CommandHandler.Init();
        }

        protected override void UnloadData()
        {
            if (MyAPIGateway.Session.IsServer)
            {
                SaveServerConfig();
            }
            SaveClientConfig();
            CommandHandler.Unload();
        }

        private static void CreateServerConfigIfNotExists()
        {
            if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(ServerConfigFileName, typeof(SingleFileMod)))
            {
                SaveServerConfig();
            }
        }

        private static void LoadConfig()
        {
            if (MyAPIGateway.Session.IsServer)
            {
                LoadServerConfig();
            }
            LoadClientConfig();
        }

        private static void LoadClientConfig()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(ClientConfigFileName, typeof(SingleFileMod)))
            {
                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(ClientConfigFileName, typeof(SingleFileMod)))
                {
                    var text = reader.ReadToEnd();
                    var ini = new MyIni();
                    MyIniParseResult result;
                    if (!ini.TryParse(text, out result))
                    {
                        MyLog.Default.WriteLineAndConsole($"Failed to parse client settings: {result}");
                        return;
                    }

                    ClientExampleSetting = ini.Get("ClientSettings", "ExampleSetting").ToInt32(ClientExampleSetting);
                }
            }
        }

        private static void LoadServerConfig()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(ServerConfigFileName, typeof(SingleFileMod)))
            {
                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(ServerConfigFileName, typeof(SingleFileMod)))
                {
                    var text = reader.ReadToEnd();
                    var ini = new MyIni();
                    MyIniParseResult result;
                    if (!ini.TryParse(text, out result))
                    {
                        MyLog.Default.WriteLineAndConsole($"Failed to parse server settings: {result}");
                        return;
                    }

                    ServerExampleSetting = ini.Get("ServerSettings", "ExampleSetting").ToInt32(ServerExampleSetting);
                }
            }
        }

        private static void SaveClientConfig()
        {
            var ini = new MyIni();
            ini.Set("ClientSettings", "ExampleSetting", ClientExampleSetting);

            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(ClientConfigFileName, typeof(SingleFileMod)))
            {
                writer.Write(ini.ToString());
            }
        }

        private static void SaveServerConfig(MyIni ini = null)
        {
            if (ini == null)
            {
                ini = new MyIni();
                ini.Set("ServerSettings", "ExampleSetting", ServerExampleSetting);
            }

            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(ServerConfigFileName, typeof(SingleFileMod)))
            {
                writer.Write(ini.ToString());
            }
        }

        private static void UpdateServerSetting(string section, string key, string value)
        {
            var ini = new MyIni();
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(ServerConfigFileName, typeof(SingleFileMod)))
            {
                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(ServerConfigFileName, typeof(SingleFileMod)))
                {
                    var text = reader.ReadToEnd();
                    ini.TryParse(text);
                }
            }
            ini.Set(section, key, value);

            SaveServerConfig(ini);
            LoadServerConfig(); // Reload config after updating
        }

        public static class CommandHandler
        {
            public static void Init()
            {
                MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
            }

            public static void Unload()
            {
                MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
            }

            private static void OnMessageEntered(string messageText, ref bool sendToOthers)
            {
                if (messageText.StartsWith("/pepco", StringComparison.OrdinalIgnoreCase))
                {
                    var args = messageText.Split(' ');

                    if (args.Length > 1 && args[1].Equals("reload", StringComparison.OrdinalIgnoreCase))
                    {
                        LoadConfig();
                        MyAPIGateway.Utilities.ShowMessage("PEPCO", "Configuration reloaded.");
                        sendToOthers = false;
                    }
                    else if (args.Length > 2 && args[1].Equals("setserver", StringComparison.OrdinalIgnoreCase))
                    {
                        var settingArgs = args[2].Split('=');
                        if (settingArgs.Length == 2)
                        {
                            if (MyAPIGateway.Session.IsServer)
                            {
                                UpdateServerSetting("ServerSettings", settingArgs[0], settingArgs[1]);
                                MyAPIGateway.Utilities.ShowMessage("PEPCO", $"Server setting {settingArgs[0]} updated to {settingArgs[1]}.");
                                sendToOthers = false;
                            }
                            else
                            {
                                MyAPIGateway.Utilities.ShowMessage("PEPCO", "You are not on the server.");
                                sendToOthers = false;
                            }
                        }
                    }
                    else if (args.Length > 2 && args[1].Equals("setclient", StringComparison.OrdinalIgnoreCase))
                    {
                        var settingArgs = args[2].Split('=');
                        if (settingArgs.Length == 2)
                        {
                            UpdateClientSetting("ClientSettings", settingArgs[0], settingArgs[1]);
                            MyAPIGateway.Utilities.ShowMessage("PEPCO", $"Client setting {settingArgs[0]} updated to {settingArgs[1]}.");
                            sendToOthers = false;
                        }
                    }
                    else if (args.Length > 2 && args[1].Equals("show", StringComparison.OrdinalIgnoreCase))
                    {
                        if (args.Length == 4 && args[2].Equals("settings", StringComparison.OrdinalIgnoreCase))
                        {
                            ShowSettings(args[3]);
                        }
                        else if (args.Length == 3 && args[2].Equals("settings", StringComparison.OrdinalIgnoreCase))
                        {
                            ShowSettings("all");
                        }
                        sendToOthers = false;
                    }
                }
            }

            private static void UpdateClientSetting(string section, string key, string value)
            {
                var ini = new MyIni();
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(ClientConfigFileName, typeof(SingleFileMod)))
                {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(ClientConfigFileName, typeof(SingleFileMod)))
                    {
                        var text = reader.ReadToEnd();
                        ini.TryParse(text);
                    }
                }
                ini.Set(section, key, value);

                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(ClientConfigFileName, typeof(SingleFileMod)))
                {
                    writer.Write(ini.ToString());
                }
                LoadClientConfig(); // Reload config after updating
            }

            private static void ShowSettings(string type)
            {
                switch (type.ToLower())
                {
                    case "all":
                        ShowClientSettings();
                        if (MyAPIGateway.Session.IsServer)
                        {
                            ShowServerSettings();
                        }
                        break;
                    case "client":
                        ShowClientSettings();
                        break;
                    case "server":
                        if (MyAPIGateway.Session.IsServer)
                        {
                            ShowServerSettings();
                        }
                        else
                        {
                            MyAPIGateway.Utilities.ShowMessage("PEPCO", "You are not on the server.");
                        }
                        break;
                    default:
                        MyAPIGateway.Utilities.ShowMessage("PEPCO", "Invalid option. Use 'all', 'client', or 'server'.");
                        break;
                }
            }

            private static void ShowClientSettings()
            {
                MyAPIGateway.Utilities.ShowMessage("PEPCO", $"Client ExampleSetting: {ClientExampleSetting}");
            }

            private static void ShowServerSettings()
            {
                MyAPIGateway.Utilities.ShowMessage("PEPCO", $"Server ExampleSetting: {ServerExampleSetting}");
            }
        }
    }
}

using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame.Utilities;
using System;
using VRage.Utils;
using ProtoBuf;
using System.Collections.Generic;
using static PEPCO.SingleFileMod;

namespace PEPCO
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class SingleFileMod : MySessionComponentBase
    {
        private const string ClientConfigFileName = "PEPCOClientSettings.ini";
        private const string ServerConfigFileName = "PEPCOServerSettings.ini";

        private static MyIni clientIni = new MyIni();
        private static MyIni serverIni = new MyIni();

        public static int ClientExampleSetting { get; private set; } = 42;
        public static int ServerExampleSetting { get; private set; } = 84;

        public override void BeforeStart()
        {
            try
            {
                if (MyAPIGateway.Session.IsServer)
                {
                    CreateServerConfigIfNotExists();
                    LoadServerConfig();
                }
                LoadClientConfig();
                CommandHandler.Init();
                Networking.Init();
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"Error in BeforeStart: {e.Message}");
            }
        }

        protected override void UnloadData()
        {
            try
            {
                if (MyAPIGateway.Session.IsServer)
                {
                    SaveServerConfig();
                }
                SaveClientConfig();
                CommandHandler.Unload();
                Networking.Close();
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"Error in UnloadData: {e.Message}");
            }
        }

        private static void CreateServerConfigIfNotExists()
        {
            try
            {
                if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(ServerConfigFileName, typeof(SingleFileMod)))
                {
                    SaveServerConfig();
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"Error in CreateServerConfigIfNotExists: {e.Message}");
            }
        }

        private static void LoadConfig()
        {
            try
            {
                if (MyAPIGateway.Session.IsServer)
                {
                    LoadServerConfig();
                }
                LoadClientConfig();
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"Error in LoadConfig: {e.Message}");
            }
        }

        private static void LoadClientConfig()
        {
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(ClientConfigFileName, typeof(SingleFileMod)))
                {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(ClientConfigFileName, typeof(SingleFileMod)))
                    {
                        var text = reader.ReadToEnd();
                        MyIniParseResult result;
                        if (!clientIni.TryParse(text, out result))
                        {
                            MyLog.Default.WriteLineAndConsole($"Failed to parse client settings: {result}");
                            return;
                        }

                        ClientExampleSetting = clientIni.Get("ClientSettings", "ExampleSetting").ToInt32(ClientExampleSetting);
                    }
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"Error in LoadClientConfig: {e.Message}");
            }
        }

        private static void LoadServerConfig()
        {
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(ServerConfigFileName, typeof(SingleFileMod)))
                {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(ServerConfigFileName, typeof(SingleFileMod)))
                    {
                        var text = reader.ReadToEnd();
                        MyIniParseResult result;
                        if (!serverIni.TryParse(text, out result))
                        {
                            MyLog.Default.WriteLineAndConsole($"Failed to parse server settings: {result}");
                            return;
                        }

                        ServerExampleSetting = serverIni.Get("ServerSettings", "ExampleSetting").ToInt32(ServerExampleSetting);
                    }
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"Error in LoadServerConfig: {e.Message}");
            }
        }

        private static void SaveClientConfig()
        {
            try
            {
                clientIni.Set("ClientSettings", "ExampleSetting", ClientExampleSetting);

                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(ClientConfigFileName, typeof(SingleFileMod)))
                {
                    writer.Write(clientIni.ToString());
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"Error in SaveClientConfig: {e.Message}");
            }
        }

        private static void SaveServerConfig(MyIni ini = null)
        {
            try
            {
                if (ini == null)
                {
                    ini = serverIni;
                    ini.Set("ServerSettings", "ExampleSetting", ServerExampleSetting);
                }

                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(ServerConfigFileName, typeof(SingleFileMod)))
                {
                    writer.Write(ini.ToString());
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"Error in SaveServerConfig: {e.Message}");
            }
        }

        public static void UpdateServerSetting(string section, string key, string value)
        {
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(ServerConfigFileName, typeof(SingleFileMod)))
                {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(ServerConfigFileName, typeof(SingleFileMod)))
                    {
                        var text = reader.ReadToEnd();
                        serverIni.TryParse(text);
                    }
                }
                serverIni.Set(section, key, value);

                SaveServerConfig(serverIni);
                LoadServerConfig(); // Reload config after updating

                // Synchronize settings to clients
                var settings = new NavigationScreenBlockSettings
                {
                    ExampleSetting = ServerExampleSetting
                };
                var packet = new PacketBlockSettings();
                packet.Send(0, settings); // Sending with dummy entityId 0
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"Error updating server setting: {e.Message}");
            }
        }

        private static void UpdateClientSetting(string section, string key, string value)
        {
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(ClientConfigFileName, typeof(SingleFileMod)))
                {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(ClientConfigFileName, typeof(SingleFileMod)))
                    {
                        var text = reader.ReadToEnd();
                        clientIni.TryParse(text);
                    }
                }
                clientIni.Set(section, key, value);

                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(ClientConfigFileName, typeof(SingleFileMod)))
                {
                    writer.Write(clientIni.ToString());
                }
                LoadClientConfig(); // Reload config after updating
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"Error updating client setting: {e.Message}");
            }
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
                try
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
                                    var packet = new PacketUpdateServerSetting("ServerSettings", settingArgs[0], settingArgs[1]);
                                    packet.Send();
                                    MyAPIGateway.Utilities.ShowMessage("PEPCO", $"Request to update server setting {settingArgs[0]} to {settingArgs[1]} sent to server.");
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
                catch (Exception e)
                {
                    MyLog.Default.WriteLineAndConsole($"Error in OnMessageEntered: {e.Message}");
                }
            }

            private static void ShowSettings(string type)
            {
                try
                {
                    switch (type.ToLower())
                    {
                        case "all":
                            ShowClientSettings();
                            if (MyAPIGateway.Multiplayer.IsServer)
                            {
                                ShowServerSettings();
                            }
                            break;
                        case "client":
                            ShowClientSettings();
                            break;
                        case "server":
                            if (MyAPIGateway.Multiplayer.IsServer)
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
                catch (Exception e)
                {
                    MyLog.Default.WriteLineAndConsole($"Error in ShowSettings: {e.Message}");
                }
            }

            private static void ShowClientSettings()
            {
                try
                {
                    MyAPIGateway.Utilities.ShowMessage("PEPCO", "Client Settings:");
                    var clientSections = new List<string>();
                    clientIni.GetSections(clientSections);
                    foreach (var section in clientSections)
                    {
                        var keys = new List<MyIniKey>();
                        clientIni.GetKeys(section, keys);
                        foreach (var key in keys)
                        {
                            MyAPIGateway.Utilities.ShowMessage("PEPCO", $"{section}.{key.Name}: {clientIni.Get(key).ToString()}");
                        }
                    }
                }
                catch (Exception e)
                {
                    MyLog.Default.WriteLineAndConsole($"Error in ShowClientSettings: {e.Message}");
                }
            }

            private static void ShowServerSettings()
            {
                try
                {
                    MyAPIGateway.Utilities.ShowMessage("PEPCO", "Server Settings:");
                    var serverSections = new List<string>();
                    serverIni.GetSections(serverSections);
                    foreach (var section in serverSections)
                    {
                        var keys = new List<MyIniKey>();
                        serverIni.GetKeys(section, keys);
                        foreach (var key in keys)
                        {
                            MyAPIGateway.Utilities.ShowMessage("PEPCO", $"{section}.{key.Name}: {serverIni.Get(key).ToString()}");
                        }
                    }
                }
                catch (Exception e)
                {
                    MyLog.Default.WriteLineAndConsole($"Error in ShowServerSettings: {e.Message}");
                }
            }
        }

        [ProtoContract(UseProtoMembersOnly = true)]
        public class PacketBlockSettings : PacketBase
        {
            [ProtoMember(1)]
            public long EntityId;

            [ProtoMember(2)]
            public NavigationScreenBlockSettings Settings;

            public PacketBlockSettings() { } // Empty constructor required for deserialization

            public void Send(long entityId, NavigationScreenBlockSettings settings)
            {
                EntityId = entityId;
                Settings = settings;

                if (MyAPIGateway.Multiplayer.IsServer)
                    Networking.RelayToClients(this);
                else
                    Networking.SendToServer(this);
            }

            public override void Received(ref bool relay)
            {
                try
                {
                    ClientExampleSetting = Settings.ExampleSetting;
                    relay = true;
                }
                catch (Exception e)
                {
                    MyLog.Default.WriteLineAndConsole($"Error in PacketBlockSettings.Received: {e.Message}");
                }
            }
        }

        public class NavigationScreenBlockSettings
        {
            public int ExampleSetting { get; set; }
        }

        public static class Networking
        {
            private const ushort NetworkId = 32988;

            public static void Init()
            {
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(NetworkId, MessageHandler);
            }

            public static void Close()
            {
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(NetworkId, MessageHandler);
            }

            public static void RelayToClients<T>(T obj)
            {
                try
                {
                    byte[] data = MyAPIGateway.Utilities.SerializeToBinary(obj);
                    MyAPIGateway.Multiplayer.SendMessageToOthers(NetworkId, data);
                }
                catch (Exception e)
                {
                    MyLog.Default.WriteLineAndConsole($"Error in RelayToClients: {e.Message}");
                }
            }

            public static void SendToServer<T>(T obj)
            {
                try
                {
                    byte[] data = MyAPIGateway.Utilities.SerializeToBinary(obj);
                    MyAPIGateway.Multiplayer.SendMessageToServer(NetworkId, data);
                }
                catch (Exception e)
                {
                    MyLog.Default.WriteLineAndConsole($"Error in SendToServer: {e.Message}");
                }
            }

            private static void MessageHandler(ushort handlerId, byte[] data, ulong senderId, bool fromServer)
            {
                try
                {
                    var packet = MyAPIGateway.Utilities.SerializeFromBinary<PacketBase>(data);
                    bool relay = false;
                    packet.Received(ref relay);
                    if (relay && MyAPIGateway.Multiplayer.IsServer)
                    {
                        RelayToClients(packet);
                    }
                }
                catch (Exception e)
                {
                    MyLog.Default.WriteLineAndConsole($"Error in MessageHandler: {e.Message}");
                }
            }
        }
    }

    [ProtoContract]
    [ProtoInclude(100, typeof(PacketUpdateServerSetting))]
    [ProtoInclude(101, typeof(PacketBlockSettings))]
    public class PacketBase
    {
        [ProtoMember(1)]
        public string PacketType { get; set; }

        public virtual void Received(ref bool relay)
        {
            // Default implementation (can be overridden in derived classes)
        }
    }

    [ProtoContract(UseProtoMembersOnly = true)]
    public class PacketUpdateServerSetting : PacketBase
    {
        [ProtoMember(1)]
        public string Section { get; set; }

        [ProtoMember(2)]
        public string Key { get; set; }

        [ProtoMember(3)]
        public string Value { get; set; }

        public PacketUpdateServerSetting() { } // Empty constructor required for deserialization

        public PacketUpdateServerSetting(string section, string key, string value)
        {
            Section = section;
            Key = key;
            Value = value;
        }

        public void Send()
        {
            if (MyAPIGateway.Multiplayer.IsServer)
                Networking.RelayToClients(this);
            else
                Networking.SendToServer(this);
        }

        public override void Received(ref bool relay)
        {
            try
            {
                if (MyAPIGateway.Multiplayer.IsServer)
                {
                    SingleFileMod.UpdateServerSetting(Section, Key, Value);
                    relay = true; // Relay to clients if needed
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"Error in PacketUpdateServerSetting.Received: {e.Message}");
            }
        }
    }
}

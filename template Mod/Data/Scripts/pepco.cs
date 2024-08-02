using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame.Utilities;
using System;
using System.Collections.Generic;
using VRage.Utils;
using ProtoBuf;

namespace PEPCO
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class SingleFileMod : MySessionComponentBase
    {
        private const string ClientConfigFileName = "PEPCOClientSettings.ini";
        private const string ServerConfigFileName = "PEPCOServerSettings.ini";
        public static MyIni clientIni = new MyIni();
        public static MyIni serverIni = new MyIni();

        public static int ClientExampleSetting { get; private set; } = 42;
        public static int ServerExampleSetting { get; private set; } = 84;

        public override void BeforeStart()
        {
            if (MyAPIGateway.Session.IsServer)
            {
                CreateServerConfigIfNotExists();
                LoadServerConfig();
            }
            else
            {
                LoadClientConfig();
            }
            CommandHandler.Init();
            Networking.Init();
        }

        protected override void UnloadData()
        {
            if (MyAPIGateway.Session.IsServer)
            {
                SaveServerConfig();
            }
            else
            {
                SaveClientConfig();
            }
            CommandHandler.Unload();
            Networking.Close();
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

        private static void LoadServerConfig()
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

        private static void SaveClientConfig()
        {
            var ini = new MyIni();
            ini.Set("ClientSettings", "ExampleSetting", ClientExampleSetting);

            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(ClientConfigFileName, typeof(SingleFileMod)))
            {
                writer.Write(ini.ToString());
            }
        }

        private static void SaveServerConfig()
        {
            var ini = new MyIni();
            ini.Set("ServerSettings", "ExampleSetting", ServerExampleSetting);

            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(ServerConfigFileName, typeof(SingleFileMod)))
            {
                writer.Write(ini.ToString());
            }
        }

        public static void UpdateServerSetting(string section, string key, string value)
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

            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(ServerConfigFileName, typeof(SingleFileMod)))
            {
                writer.Write(ini.ToString());
            }
            LoadServerConfig(); // Reload config after updating
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

        public static void SendServerSettingsToClient(ulong recipientId)
        {
            var settings = new Dictionary<string, Dictionary<string, string>>();
            var serverSections = new List<string>();
            serverIni.GetSections(serverSections);
            foreach (var section in serverSections)
            {
                var keys = new List<MyIniKey>();
                serverIni.GetKeys(section, keys);
                var sectionDict = new Dictionary<string, string>();
                foreach (var key in keys)
                {
                    sectionDict[key.Name] = serverIni.Get(key).ToString();
                }
                settings[section] = sectionDict;
            }

            var packet = new PacketServerSettings(settings);
            packet.Send(recipientId);
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
                                }
                                else
                                {
                                    var packet = new PacketUpdateServerSetting("ServerSettings", settingArgs[0], settingArgs[1]);
                                    packet.Send();
                                    MyAPIGateway.Utilities.ShowMessage("PEPCO", $"Request to update server setting {settingArgs[0]} to {settingArgs[1]} sent to server.");
                                }
                                sendToOthers = false;
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
                        else if (args.Length > 2 && args[1].Equals("show", StringComparison.OrdinalIgnoreCase) && args[2].Equals("server", StringComparison.OrdinalIgnoreCase))
                        {
                            var requestPacket = new PacketRequestServerSettings();
                            requestPacket.Send();
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
                                var requestPacket = new PacketRequestServerSettings();
                                requestPacket.Send();
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
    }

    public static class Networking
    {
        private const ushort NetworkId = 1234;

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

        public static void SendToClient<T>(T obj, ulong recipientId)
        {
            try
            {
                byte[] data = MyAPIGateway.Utilities.SerializeToBinary(obj);
                MyAPIGateway.Multiplayer.SendMessageTo(NetworkId, data, recipientId);
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"Error in SendToClient: {e.Message}");
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

    [ProtoContract(UseProtoMembersOnly = true)]
    public abstract class PacketBase
    {
        public PacketBase() { } // Parameterless constructor for ProtoBuf

        public abstract void Received(ref bool relay);
    }

    [ProtoContract(UseProtoMembersOnly = true)]
    public class PacketServerSettings : PacketBase
    {
        [ProtoMember(1)]
        public Dictionary<string, Dictionary<string, string>> Settings { get; set; }

        public PacketServerSettings() { } // Empty constructor required for deserialization

        public PacketServerSettings(Dictionary<string, Dictionary<string, string>> settings)
        {
            Settings = settings;
        }

        public void Send(ulong recipientId)
        {
            if (MyAPIGateway.Multiplayer.IsServer)
                Networking.SendToClient(this, recipientId);
            else
                MyLog.Default.WriteLineAndConsole("Attempted to send server settings from a non-server instance.");
        }

        public override void Received(ref bool relay)
        {
            try
            {
                MyAPIGateway.Utilities.ShowMessage("PEPCO", "Server Settings:");
                foreach (var section in Settings)
                {
                    foreach (var key in section.Value)
                    {
                        MyAPIGateway.Utilities.ShowMessage("PEPCO", $"{section.Key}.{key.Key}: {key.Value}");
                    }
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"Error in PacketServerSettings.Received: {e.Message}");
            }
        }
    }

    [ProtoContract(UseProtoMembersOnly = true)]
    public class PacketUpdateServerSetting : PacketBase
    {
        [ProtoMember(1)]
        public string Section;

        [ProtoMember(2)]
        public string Key;

        [ProtoMember(3)]
        public string Value;

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
            if (MyAPIGateway.Session.IsServer)
            {
                SingleFileMod.UpdateServerSetting(Section, Key, Value);
                relay = true;
            }
        }
    }

    [ProtoContract(UseProtoMembersOnly = true)]
    public class PacketRequestServerSettings : PacketBase
    {
        public PacketRequestServerSettings() { } // Empty constructor required for deserialization

        public void Send()
        {
            if (!MyAPIGateway.Multiplayer.IsServer)
                Networking.SendToServer(this);
        }

        public override void Received(ref bool relay)
        {
            if (MyAPIGateway.Session.IsServer)
            {
                SingleFileMod.SendServerSettingsToClient(MyAPIGateway.Multiplayer.MyId);
                relay = false;
            }
        }
    }
}

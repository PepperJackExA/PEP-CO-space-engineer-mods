using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using System;

namespace PEPCO.Template
{
    public static class CommandHandler
    {
        public static void OnMessageEntered(string messageText, ref bool sendToOthers)
        {
            var player = MyAPIGateway.Session.Player;

            if (player == null || !MyAPIGateway.Session.IsUserAdmin(player.SteamUserId))
            {
                MyAPIGateway.Utilities.ShowMessage("PEPCO", "Only admins can use this command.");
                return;
            }

            if (messageText.StartsWith("/pepco "))
            {
                var command = messageText.Substring("/pepco ".Length).ToLower();

                switch (command)
                {
                    case "reload":
                        ConfigSettings.Load();
                        MyAPIGateway.Utilities.ShowMessage("PEPCO", "Configuration reloaded.");
                        break;

                    case "settings":
                        MyAPIGateway.Utilities.ShowMessage("PEPCO", $"SomeSetting: {ConfigSettings.SomeSetting}");
                        break;

                    default:
                        MyAPIGateway.Utilities.ShowMessage("PEPCO", "Unknown command.");
                        break;
                }

                sendToOthers = false;
            }
        }
    }
}

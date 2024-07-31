using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Utils;

namespace Pepco
{
    public class CommandHandler
    {
        private ConfigSettings configSettings;

        public CommandHandler(ConfigSettings settings)
        {
            configSettings = settings;
            MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
        }

        private void OnMessageEntered(string messageText, ref bool sendToOthers)
        {
            if (!messageText.StartsWith("/pepco config")) return;

            sendToOthers = false;
            var parameters = messageText.Split(' ');

            if (parameters.Length == 4)
            {
                string key = parameters[2];
                string value = parameters[3];

                configSettings.UpdateConfig(key, value);
                MyAPIGateway.Utilities.ShowMessage("Config", $"Updated {key} to {value}");
            }
            else
            {
                MyAPIGateway.Utilities.ShowMessage("Config", "Usage: /pepco config <key> <value>");
            }
        }

        public void Unload()
        {
            MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
        }
    }
}

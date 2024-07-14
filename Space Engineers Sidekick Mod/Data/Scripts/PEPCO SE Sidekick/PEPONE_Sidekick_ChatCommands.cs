using Sandbox.Game;
using Digi;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEPONE_Sidekick
{
    public class ChatCommands : IDisposable
    {
        const string MainCommand = "/sidekick";
        readonly PEPONE_SidekickSession Mod;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatCommands"/> class.
        /// </summary>
        /// <param name="mod">The PEPONE_SidekickSession instance.</param>
        public ChatCommands(PEPONE_SidekickSession mod)
        {
            Mod = mod;

            MyAPIGateway.Utilities.MessageEntered += MessageEntered;
        }

        /// <summary>
        /// Disposes the ChatCommands instance.
        /// </summary>
        public void Dispose()
        {
            MyAPIGateway.Utilities.MessageEntered -= MessageEntered;
        }

        /// <summary>
        /// Handles the entered chat message.
        /// </summary>
        /// <param name="messageText">The entered message text.</param>
        /// <param name="sendToOthers">A reference to a boolean value indicating whether to send the message to others.</param>
        private void MessageEntered(string messageText, ref bool sendToOthers)
        {
            try
            {
                //If the message doesn't start with the main command, return
                if (!messageText.ToUpper().StartsWith(MainCommand.ToUpper()))
                    return;
                //Remove the main command from the message
                else messageText = messageText.Substring(MainCommand.Length).Trim().ToUpper();

                sendToOthers = false;


                switch (messageText)
                {
                    case "FILE":
                        Mod.ExportBlocks(true);
                        break;
                    case "HELP":
                        MyVisualScriptLogicProvider.OpenSteamOverlayLocal("https://steamcommunity.com/sharedfiles/filedetails/?id=3286043993");
                        break;
                    case "GRID":
                        Mod.ExportGrid();
                        break;
                    default:
                        Mod.ExportBlocks(false);
                        break;
                }

                
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}

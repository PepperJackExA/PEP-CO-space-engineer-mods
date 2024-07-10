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


        public ChatCommands(PEPONE_SidekickSession mod)
        {
            Mod = mod;

            MyAPIGateway.Utilities.MessageEntered += MessageEntered;
        }

        public void Dispose()
        {
            MyAPIGateway.Utilities.MessageEntered -= MessageEntered;
        }

        void MessageEntered(string messageText, ref bool sendToOthers)
        {
            try
            {
                if (!messageText.ToUpper().StartsWith(MainCommand.ToUpper()))
                    return;

                sendToOthers = false;

                bool exportToFile = messageText.ToUpper().EndsWith("FILE");

                Mod.ExportBlocks(exportToFile);

            }
            catch (Exception ex) {
                Log.Error(ex);
            }

        }
    }
}

using PEPCO.iSurvival.CustomEntitySpawner.Config;
using System.Collections.Generic;

namespace PEPCO.iSurvival.CustomEntitySpawner.Utilities
{
    public class CommandHandler
    {
        private GlobalSettings globalSettings;
        private List<BlockSettings> blockSettings;

        public CommandHandler(GlobalSettings globalSettings, List<BlockSettings> blockSettings)
        {
            this.globalSettings = globalSettings;
            this.blockSettings = blockSettings;
        }

        public void OnMessageEntered(string messageText)
        {
            // Parse and handle commands
        }
    }
}

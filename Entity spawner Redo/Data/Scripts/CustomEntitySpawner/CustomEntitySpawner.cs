using Sandbox.ModAPI;
using VRage.Game.Components;
using PEPCO.iSurvival.CustomEntitySpawner.Config;
using PEPCO.iSurvival.CustomEntitySpawner.Spawning;
using PEPCO.iSurvival.CustomEntitySpawner.NPC;
using PEPCO.iSurvival.CustomEntitySpawner.Utilities;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace PEPCO.iSurvival.CustomEntitySpawner
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class CustomEntitySpawner : MySessionComponentBase
    {
        private GlobalSettings globalSettings;
        private List<BlockSettings> blockSpawnSettings;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            globalSettings = new GlobalSettings();
            blockSpawnSettings = new List<BlockSettings>();

            var ini = new MyIni();

            globalSettings.LoadGlobalConfig(ini);
            IniFileHandler.EnsureDefaultIniFilesExist();
            IniFileHandler.LoadAllFilesFromWorldStorage(blockSpawnSettings);
        }

        public override void UpdateBeforeSimulation()
        {
            if (globalSettings.ScriptPaused) return;
            // Spawn handling logic
        }

        protected override void UnloadData()
        {
            Logger.Cleanup();
        }
    }
}

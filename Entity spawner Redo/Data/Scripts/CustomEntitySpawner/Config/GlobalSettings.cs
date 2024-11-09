using Sandbox.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace PEPCO.iSurvival.CustomEntitySpawner.Config
{
    public class GlobalSettings
    {
        public int BaseUpdateInterval { get; set; } = 60;
        public bool EnableLogging { get; set; } = false;
        public int CleanupInterval { get; set; } = 18000;
        public int GlobalMaxEntities { get; set; } = 32;
        public bool ScriptPaused { get; set; } = false;

        public void LoadGlobalConfig(MyIni ini)
        {
            ScriptPaused = ini.Get("General", "ScriptPaused").ToBoolean(ScriptPaused);
        }
        public void WriteDefaultSettingsToIni(MyIni ini)
        {
            ini.Set("General", "ScriptPaused", ScriptPaused);
        }
    }
}

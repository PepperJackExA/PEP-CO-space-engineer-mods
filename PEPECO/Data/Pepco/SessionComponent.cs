using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;

namespace Pepco
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class SessionComponent : MySessionComponentBase
    {
        private ConfigSettings configSettings;
        private CommandHandler commandHandler;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);

            configSettings = new ConfigSettings();
            configSettings.LoadConfig();

            commandHandler = new CommandHandler(configSettings);
        }
        protected override void UnloadData()
        {
            commandHandler.Unload();
            base.UnloadData();
        }
    }
}

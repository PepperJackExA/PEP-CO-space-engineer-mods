using Sandbox.ModAPI;
using VRage.Game.Components;
using VRageMath;
using Draygo.API;
using VRage.Utils;
using System.Text;
using VRage.Game;

namespace MyHudMod
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class MyHudScript : MySessionComponentBase
    {
        private HudAPIv2 hudAPI;
        private HudAPIv2.HUDMessage myMessage;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);

            if (MyAPIGateway.Session.IsServer)
                return;

            hudAPI = new HudAPIv2(OnRegistered);
        }

        private void OnRegistered()
        {
            myMessage = new HudAPIv2.HUDMessage(
                new StringBuilder("Hello, Space Engineers!"),
                new Vector2D(0.5, 0.5),
                null,
                -1,
                1.0,
                true,
                false,
                null,
                HudAPIv2.DefaultHUDBlendType,
                HudAPIv2.DefaultFont
            );
        }

        public override void UpdateAfterSimulation()
        {
            if (myMessage != null)
            {
                myMessage.Visible = true;
            }
        }

        protected override void UnloadData()
        {
            if (hudAPI != null)
            {
                hudAPI.Unload();
                hudAPI = null;
            }

            base.UnloadData();
        }
    }
}

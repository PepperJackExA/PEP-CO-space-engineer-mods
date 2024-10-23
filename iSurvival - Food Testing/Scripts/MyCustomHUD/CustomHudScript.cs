using Sandbox.ModAPI;
using System.Text;
using VRage.Game;
using VRage.Game.Components;
using VRage.ModAPI;
using VRageMath;
using Draygo.API;  // Reference to access HudAPIv2

namespace MyHudMod
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class MyHudMod : MySessionComponentBase
    {
        private HudAPIv2 hudAPI;
        private HudAPIv2.HUDMessage hudMessage;
        private HudAPIv2.BillBoardHUDMessage billboardMessage;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            // Initialize the HUD API
            hudAPI = new HudAPIv2(OnHudApiRegistered);
        }

        private void OnHudApiRegistered()
        {
            if (!hudAPI.Heartbeat)
            {
                MyAPIGateway.Utilities.ShowMessage("MyHudMod", "HudAPI not registered.");
                return;
            }

            // Create a simple HUD text message
            hudMessage = new HudAPIv2.HUDMessage(
                Message: new StringBuilder("Welcome to My Hud Mod!"),  // Correct parameter name
                Origin: new Vector2D(0, -0.8),                         // Screen position
                Blend: HudAPIv2.DefaultHUDBlendType,
                TimeToLive: 0,                                         // 0 means it will stay indefinitely
                Shadowing: true,
                Scale: 1.0                                             // Scale of the text
            );

            
            billboardMessage.BillBoardColor = Color.Green;

            // Optional: Adjust elements on screen resize
            hudAPI.OnScreenDimensionsChanged = OnScreenDimensionsChanged;
        }

        private void OnScreenDimensionsChanged()
        {
            // Adjust positions or sizes here based on the new screen dimensions
            hudMessage.Message.Clear().Append("Screen resized!");
        }

        protected override void UnloadData()
        {
            // Clean up HUD elements and API when the mod is unloaded
            hudMessage?.Flush(); // Use Remove() instead of DeleteMessage
            billboardMessage?.Flush(); // Use Remove() instead of DeleteMessage
            hudAPI?.Close();
        }
    }
}

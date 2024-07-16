using Sandbox.Game.Localization;
using Sandbox.ModAPI.Interfaces.Terminal;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using VRage.Utils;
using Digi;
using VRageMath;
using Sandbox.Game.Entities.Cube;
using SubmarineStuff;

namespace SubmarineStuff
{
    public class SubmarineStuffControls
    {

        const string IdPrefix = "SubmarineStuffEngine_"; // highly recommended to tag your properties/actions like this to avoid colliding with other mods'

        static bool Done = false;

        public static void DoOnce(IMyModContext context) // called by GyroLogic.cs
        {
            if (Done)
                return;
            Done = true;

            // these are all the options and they're not all required so use only what you need.
            CreateControls();
            CreateActions(context);
        }

        static bool CustomVisibleCondition(IMyTerminalBlock b)
        {
            // only visible for the blocks having this gamelogic comp
            return b?.GameLogic?.GetAs<MyBallastTankBlock>() != null;
        }

        static void CreateControls()
        {
            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyTerminalBlock>(""); // separators don't store the id
                c.SupportsMultipleBlocks = true;
                c.Visible = CustomVisibleCondition;

                MyAPIGateway.TerminalControls.AddControl<IMyTerminalBlock>(c);
            }
            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyTerminalBlock>(IdPrefix + "SubmarineStuffLabel");
                c.Label = MyStringId.GetOrCompute("Ballast");
                c.SupportsMultipleBlocks = true;
                c.Visible = CustomVisibleCondition;

                MyAPIGateway.TerminalControls.AddControl<IMyTerminalBlock>(c);
            }

            IMyTerminalControlSlider slider;
            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyTerminalBlock>(IdPrefix + "BallastTrimSlider");
                c.Title = MyStringId.GetOrCompute("Ballast trim");
                c.Tooltip = MyStringId.GetOrCompute("This changes the boyancy of your ballast tank!");
                c.SupportsMultipleBlocks = true;
                c.Visible = CustomVisibleCondition;

                c.Setter = (b, v) =>
                {
                    var logic = b?.GameLogic?.GetAs<MyBallastTankBlock>();
                    if (logic != null)
                        logic.ballastTank_Fill = MathHelper.Clamp(v, 0f, 100f); // just a heads up that the given value here is not clamped by the game, a mod or PB can give lower or higher than the limits!
                };
                c.Getter = (b) => b?.GameLogic?.GetAs<MyBallastTankBlock>()?.ballastTank_Fill ?? 0;

                c.SetLimits(0f, 100f);
                //c.SetLimits((b) => 0, (b) => 10); // overload with callbacks to define limits based on the block instance.
                //c.SetDualLogLimits(0, 10, 2); // all these also have callback overloads
                //c.SetLogLimits(0, 10);

                // called when the value changes so that you can display it next to the label
                c.Writer = (b, sb) =>
                {
                    var logic = b?.GameLogic?.GetAs<MyBallastTankBlock>();
                    if (logic != null)
                    {
                        float val = logic.ballastTank_Fill;
                        sb.Append(Math.Round(val, 2)).Append("%");
                    }
                };
                slider = c;
                MyAPIGateway.TerminalControls.AddControl<IMyTerminalBlock>(c);
            }

            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyTerminalBlock>(IdPrefix + "BallastTrimReset");
                c.Title = MyStringId.GetOrCompute("Emergency blow");
                c.Tooltip = MyStringId.GetOrCompute("Resets the boyancy to max!");
                c.SupportsMultipleBlocks = true;
                c.Visible = CustomVisibleCondition;

                c.Action = (b) =>
                {
                    var logic = b?.GameLogic?.GetAs<MyBallastTankBlock>();
                    if (logic != null)
                        logic.ballastTank_Fill = 0f;
                    slider.UpdateVisual();
                };

                MyAPIGateway.TerminalControls.AddControl<IMyTerminalBlock>(c);
            }

        }

        static void CreateActions(IMyModContext context)
        {
            // yes, there's only one type of action
            {
                var a = MyAPIGateway.TerminalControls.CreateAction<IMyTerminalBlock>(IdPrefix + "SubmarineStuffAction");

                a.Name = new StringBuilder("Submarine Stuff Action");

                // If the action is visible for grouped blocks (as long as they all have this action).
                a.ValidForGroups = true;

                // The icon shown in the list and top-right of the block icon in toolbar.
                a.Icon = @"Textures\GUI\Icons\Actions\CharacterToggle.dds";
                // For paths inside the mod folder you need to supply an absolute path which can be retrieved from a session or gamelogic comp's ModContext.
                //a.Icon = Path.Combine(context.ModPath, @"Textures\YourIcon.dds");

                // Called when the toolbar slot is triggered
                // Should not be unassigned.
                a.Action = (b) => {
                    var logic = b?.GameLogic?.GetAs<MyBallastTankBlock>();
                    logic.ballastTank_Fill = 0f;
                };

                // The status of the action, shown in toolbar icon text and can also be read by mods or PBs.
                a.Writer = (b, sb) =>
                {
                    var logic = b?.GameLogic?.GetAs<MyBallastTankBlock>();
                    sb.Append($"{logic.ballastTank_Fill}%");
                };

                // What toolbar types to NOT allow this action for.
                // Can be left unassigned to allow all toolbar types.
                // The below are the options used by jumpdrive's Jump action as an example.
                //a.InvalidToolbarTypes = new List<MyToolbarType>()
                //{
                //    MyToolbarType.ButtonPanel,
                //    MyToolbarType.Character,
                //    MyToolbarType.Seat
                //};
                // PB checks if it's valid for ButtonPanel before allowing the action to be invoked.

                // Wether the action is to be visible for the given block instance.
                // Can be left unassigned as it defaults to true.
                // Warning: gets called per tick while in toolbar for each block there, including each block in groups.
                //   It also can be called by mods or PBs.
                a.Enabled = CustomVisibleCondition;

                MyAPIGateway.TerminalControls.AddAction<IMyTerminalBlock>(a);
            }
        }
    }
}

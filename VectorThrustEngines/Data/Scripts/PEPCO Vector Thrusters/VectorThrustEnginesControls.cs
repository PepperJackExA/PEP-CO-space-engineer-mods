using PEPCO.VectorThrustEngines;
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

namespace VectorThrustEngines.Data.Scripts.PEPONE_Vector_Thrusters
{
    public class VectorThrustEnginesControls
    {

        const string IdPrefix = "PEPCOVectorThrustEngine_"; // highly recommended to tag your properties/actions like this to avoid colliding with other mods'

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
            return b?.GameLogic?.GetAs<ThrustBlock>() != null;
        }

        static void CreateControls()
        {
            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyThrust>(""); // separators don't store the id
                c.SupportsMultipleBlocks = true;
                c.Visible = CustomVisibleCondition;

                MyAPIGateway.TerminalControls.AddControl<IMyThrust>(c);
            }
            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyThrust>(IdPrefix + "VectorThrustLabel");
                c.Label = MyStringId.GetOrCompute("Vector Thrust");
                c.SupportsMultipleBlocks = true;
                c.Visible = CustomVisibleCondition;

                MyAPIGateway.TerminalControls.AddControl<IMyThrust>(c);
            }
            //Toggle button for thrust vectoring on/off
            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyThrust>(IdPrefix + "VectorThrustOnOff");
                c.Title = MyStringId.GetOrCompute("Vector Thrust");
                c.Tooltip = MyStringId.GetOrCompute("This enables your thruster to direct thrust in different directions!");
                c.SupportsMultipleBlocks = true; // wether this control should be visible when multiple blocks are selected (as long as they all have this control).

                // callbacks to determine if the control should be visible or not-grayed-out(Enabled) depending on whatever custom condition you want, given a block instance.
                // optional, they both default to true.
                c.Visible = CustomVisibleCondition;
                //c.Enabled = CustomVisibleCondition;

                c.OnText = MySpaceTexts.SwitchText_On;
                c.OffText = MySpaceTexts.SwitchText_Off;

                // setters and getters should both be assigned on all controls that have them, to avoid errors in mods or PB scripts getting exceptions from them.
                c.Getter = (b) => b?.GameLogic?.GetAs<ThrustBlock>()?.VectorThrust_Toggle ?? true;
                c.Setter = (b, v) =>
                {
                    var logic = b?.GameLogic?.GetAs<ThrustBlock>();
                    if (logic != null)
                        logic.VectorThrust_Toggle = v;
                    ///Log.Info($"Toggled to{v}", $"Toggled to{v}", 3000);
                };

                MyAPIGateway.TerminalControls.AddControl<IMyThrust>(c);
            }

            //Toggle button for reversing thrust vectoring on/off
            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyThrust>(IdPrefix + "VectorThrustReverseOnOff");
                c.Title = MyStringId.GetOrCompute("Vector Thrust Reverse");
                c.Tooltip = MyStringId.GetOrCompute("This reverses your vector thruster (forward becomes backwards)!");
                c.SupportsMultipleBlocks = true; // wether this control should be visible when multiple blocks are selected (as long as they all have this control).

                // callbacks to determine if the control should be visible or not-grayed-out(Enabled) depending on whatever custom condition you want, given a block instance.
                // optional, they both default to true.
                c.Visible = CustomVisibleCondition;
                //c.Enabled = CustomVisibleCondition;

                c.OnText = MySpaceTexts.SwitchText_On;
                c.OffText = MySpaceTexts.SwitchText_Off;

                // setters and getters should both be assigned on all controls that have them, to avoid errors in mods or PB scripts getting exceptions from them.
                c.Getter = (b) => b?.GameLogic?.GetAs<ThrustBlock>()?.VectorThrustReverse_Toggle ?? true;
                c.Setter = (b, v) =>
                {
                    var logic = b?.GameLogic?.GetAs<ThrustBlock>();
                    if (logic != null)
                        logic.VectorThrustReverse_Toggle = v;
                    ///Log.Info($"Toggled to{v}", $"Toggled to{v}", 3000);
                };

                MyAPIGateway.TerminalControls.AddControl<IMyThrust>(c);
            }

            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyThrust>(IdPrefix + "VectorThrustAngleSlider");
                c.Title = MyStringId.GetOrCompute("Vector Thrust Angle");
                c.Tooltip = MyStringId.GetOrCompute("This changes the angle your thrusters thrusts!");
                c.SupportsMultipleBlocks = true;
                c.Visible = CustomVisibleCondition;

                c.Setter = (b, v) =>
                {
                    var logic = b?.GameLogic?.GetAs<ThrustBlock>();
                    if (logic != null)
                        logic.VectorThrust_Angle = MathHelper.Clamp(v, -90f, 90f); // just a heads up that the given value here is not clamped by the game, a mod or PB can give lower or higher than the limits!
                };
                c.Getter = (b) => b?.GameLogic?.GetAs<ThrustBlock>()?.VectorThrust_Angle ?? 0;

                c.SetLimits(-90f, 90f);
                //c.SetLimits((b) => 0, (b) => 10); // overload with callbacks to define limits based on the block instance.
                //c.SetDualLogLimits(0, 10, 2); // all these also have callback overloads
                //c.SetLogLimits(0, 10);

                // called when the value changes so that you can display it next to the label
                c.Writer = (b, sb) =>
                {
                    var logic = b?.GameLogic?.GetAs<ThrustBlock>();
                    if (logic != null)
                    {
                        float val = logic.VectorThrust_Angle;
                        sb.Append(Math.Round(val, 2)).Append(" degrees");
                    }
                };

                MyAPIGateway.TerminalControls.AddControl<IMyThrust>(c);
            }

        }

        static void CreateActions(IMyModContext context)
        {
            // yes, there's only one type of action
            {
                var a = MyAPIGateway.TerminalControls.CreateAction<IMyThrust>(IdPrefix + "VectorThrustAction");

                a.Name = new StringBuilder("Vector Thrust Action");

                // If the action is visible for grouped blocks (as long as they all have this action).
                a.ValidForGroups = true;

                // The icon shown in the list and top-right of the block icon in toolbar.
                a.Icon = @"Textures\GUI\Icons\Actions\CharacterToggle.dds";
                // For paths inside the mod folder you need to supply an absolute path which can be retrieved from a session or gamelogic comp's ModContext.
                //a.Icon = Path.Combine(context.ModPath, @"Textures\YourIcon.dds");

                // Called when the toolbar slot is triggered
                // Should not be unassigned.
                a.Action = (b) => {
                    var logic = b?.GameLogic?.GetAs<ThrustBlock>();
                    logic.VectorThrust_Toggle = !logic.VectorThrust_Toggle;
                };

                // The status of the action, shown in toolbar icon text and can also be read by mods or PBs.
                a.Writer = (b, sb) =>
                {
                    var logic = b?.GameLogic?.GetAs<ThrustBlock>();
                    if (logic.VectorThrust_Toggle)
                    {
                        sb.Append($"On");
                    }
                    else
                    {
                        sb.Append($"Off");
                    }
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

                MyAPIGateway.TerminalControls.AddAction<IMyThrust>(a);
            }
        }
    }
}

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
using PEPCO;
using VRage.ModAPI;
using VRageRender.Messages;

namespace PEPCO
{
    public class NavigationScreenTerminalControls
    {

        const string IdPrefix = "NavigationScreen"; // highly recommended to tag your properties/actions like this to avoid colliding with other mods'

        static bool Done = false;

        public static void DoOnce(IMyModContext context) // called by NavigationScreenLogic.cs
        {
            if (Done)
                return;
            Done = true;

            // these are all the options and they're not all required so use only what you need.
            CreateControls();
        }

        static bool CustomVisibleCondition(IMyTerminalBlock b)
        {
            // only visible for the blocks having this gamelogic comp
            return b?.GameLogic?.GetAs<NavigationScreenLogic>() != null;
        }

        static void CreateControls()
        {
            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyTextPanel>(""); // separators don't store the id
                c.SupportsMultipleBlocks = false;
                c.Visible = CustomVisibleCondition;

                MyAPIGateway.TerminalControls.AddControl<IMyTextPanel>(c);
            }
            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyTextPanel>(IdPrefix + "MainLabel");
                c.Label = MyStringId.GetOrCompute("Navigation Map");
                c.SupportsMultipleBlocks = false;
                c.Visible = CustomVisibleCondition;

                MyAPIGateway.TerminalControls.AddControl<IMyTextPanel>(c);
            }

            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlColor, IMyTextPanel>(IdPrefix + "ChevronColor");
                c.Title = MyStringId.GetOrCompute("Chevron Color");
                c.Visible = CustomVisibleCondition;
                c.Getter = (b) => b?.GameLogic?.GetAs<NavigationScreenLogic>()?.NavigationScreenChevronColor ?? new Color(255,0,255);
                c.Setter = (b, v) => {

                    var logic = b?.GameLogic?.GetAs<NavigationScreenLogic>();
                    if (logic != null)
                        logic.NavigationScreenChevronColor = v;
                    
                };
                c.Tooltip = MyStringId.GetOrCompute("Changes the color of your chevron!");
                c.SupportsMultipleBlocks = false;

                MyAPIGateway.TerminalControls.AddControl<IMyTextPanel>(c);
            }

            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyTextPanel>(IdPrefix + "ChevronScale");
                c.Title = MyStringId.GetOrCompute("Chevron Scale");
                c.Tooltip = MyStringId.GetOrCompute("Changes the scale of the chevron on your map!");
                c.SupportsMultipleBlocks = false;
                c.Visible = CustomVisibleCondition;

                c.Setter = (b, v) =>
                {
                    var logic = b?.GameLogic?.GetAs<NavigationScreenLogic>();
                    if (logic != null)
                        logic.NavigationScreenChevronScale = MathHelper.Clamp(v, 0.1f, 10f); // just a heads up that the given value here is not clamped by the game, a mod or PB can give lower or higher than the limits!
                };
                c.Getter = (b) => b?.GameLogic?.GetAs<NavigationScreenLogic>()?.NavigationScreenChevronScale ?? 1f;

                c.SetLimits(0.1f, 10f);
                c.Writer = (b, sb) =>
                {
                    var logic = b?.GameLogic?.GetAs<NavigationScreenLogic>();
                    if (logic != null)
                    {
                        float val = logic.NavigationScreenChevronScale;
                        sb.Append(Math.Round(val, 2));
                    }
                };

                MyAPIGateway.TerminalControls.AddControl<IMyTextPanel>(c);
            }

            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyTextPanel>(IdPrefix + "ChevronStrength");
                c.Title = MyStringId.GetOrCompute("Chevron Strength");
                c.Tooltip = MyStringId.GetOrCompute("Changes the glow of the chevron on your map!");
                c.SupportsMultipleBlocks = false;
                c.Visible = CustomVisibleCondition;

                c.Setter = (b, v) =>
                {
                    var logic = b?.GameLogic?.GetAs<NavigationScreenLogic>();
                    if (logic != null)
                        logic.NavigationScreenChevronStrength = MathHelper.Clamp(v, 0.1f, 10f); // just a heads up that the given value here is not clamped by the game, a mod or PB can give lower or higher than the limits!
                };
                c.Getter = (b) => b?.GameLogic?.GetAs<NavigationScreenLogic>()?.NavigationScreenChevronStrength ?? 10f;

                c.SetLimits(0.1f, 10f);
                c.Writer = (b, sb) =>
                {
                    var logic = b?.GameLogic?.GetAs<NavigationScreenLogic>();
                    if (logic != null)
                    {
                        float val = logic.NavigationScreenChevronStrength;
                        sb.Append(Math.Round(val, 2));
                    }
                };

                MyAPIGateway.TerminalControls.AddControl<IMyTextPanel>(c);
            }

            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyTextPanel>(""); // separators don't store the id
                c.SupportsMultipleBlocks = false;
                c.Visible = CustomVisibleCondition;

                MyAPIGateway.TerminalControls.AddControl<IMyTextPanel>(c);
            }

            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMyTextPanel>(IdPrefix + "MapOffset");
                c.Title = MyStringId.GetOrCompute("Map direction offset");
                c.Tooltip = MyStringId.GetOrCompute("Check the tutorial if you have questions");
                c.SupportsMultipleBlocks = false;
                c.Visible = CustomVisibleCondition;

                c.Getter = (b) => b?.GameLogic?.GetAs<NavigationScreenLogic>()?.NavigationScreenOffset ?? 2;
                c.Setter = (b, key) => {
                

                    var logic = b?.GameLogic?.GetAs<NavigationScreenLogic>();
                    if (logic != null)
                        logic.NavigationScreenOffset = key;

                };
                c.ComboBoxContent = (list) =>
                {
                    list.Add(new MyTerminalControlComboBoxItem() { Key = 0, Value = MyStringId.GetOrCompute("Block's front") });
                    list.Add(new MyTerminalControlComboBoxItem() { Key = 1, Value = MyStringId.GetOrCompute("Block's right") });
                    list.Add(new MyTerminalControlComboBoxItem() { Key = 2, Value = MyStringId.GetOrCompute("Block's rear") });
                    list.Add(new MyTerminalControlComboBoxItem() { Key = 3, Value = MyStringId.GetOrCompute("Block's left") });
                };

                MyAPIGateway.TerminalControls.AddControl<IMyTextPanel>(c);
            }

            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyTextPanel>(IdPrefix + "MapZoom");
                c.Title = MyStringId.GetOrCompute("Map Zoom");
                c.Tooltip = MyStringId.GetOrCompute("Zooms your map!");
                c.SupportsMultipleBlocks = false;
                c.Visible = CustomVisibleCondition;

                c.Setter = (b, v) =>
                {
                    var logic = b?.GameLogic?.GetAs<NavigationScreenLogic>();
                    if (logic != null)
                        logic.NavigationScreenZoom = MathHelper.Clamp((float)Math.Round(v, 1), 1f, 10f); // just a heads up that the given value here is not clamped by the game, a mod or PB can give lower or higher than the limits!
                };
                c.Getter = (b) => b?.GameLogic?.GetAs<NavigationScreenLogic>()?.NavigationScreenZoom ?? 1f;

                c.SetLimits(1f, 10f);
                c.Writer = (b, sb) =>
                {
                    var logic = b?.GameLogic?.GetAs<NavigationScreenLogic>();
                    if (logic != null)
                    {
                        float val = logic.NavigationScreenZoom;
                        sb.Append(Math.Round(val,1) * 100 + "%");
                    }
                };

                MyAPIGateway.TerminalControls.AddControl<IMyTextPanel>(c);
            }




        }
    }
}

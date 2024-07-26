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

namespace PEPCO
{
    public class MercatorMapTerminalControls
    {

        const string IdPrefix = "mercatorMap"; // highly recommended to tag your properties/actions like this to avoid colliding with other mods'

        static bool Done = false;

        public static void DoOnce(IMyModContext context) // called by MercatorMapLogic.cs
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
            return b?.GameLogic?.GetAs<MercatorMapLogic>() != null;
        }

        static void CreateControls()
        {
            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyTerminalBlock>(""); // separators don't store the id
                c.SupportsMultipleBlocks = false;
                c.Visible = CustomVisibleCondition;

                MyAPIGateway.TerminalControls.AddControl<IMyTerminalBlock>(c);
            }
            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyTerminalBlock>(IdPrefix + "MainLabel");
                c.Label = MyStringId.GetOrCompute("Navigation Map");
                c.SupportsMultipleBlocks = false;
                c.Visible = CustomVisibleCondition;

                MyAPIGateway.TerminalControls.AddControl<IMyTerminalBlock>(c);
            }

            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlColor, IMyTerminalBlock>(IdPrefix + "ChevronColor");
                c.Title = MyStringId.GetOrCompute("Chevron Color");
                c.Visible = CustomVisibleCondition;
                c.Getter = (b) => b?.GameLogic?.GetAs<MercatorMapLogic>()?.mercatorMapChevronColor ?? new Color(255,0,255,1);
                c.Setter = (b, color) => {

                    var logic = b?.GameLogic?.GetAs<MercatorMapLogic>();
                    if (logic != null)
                        logic.mercatorMapChevronColor = color;
                    c.Tooltip = MyStringId.GetOrCompute("Changes the color of your chevron!");
                    c.SupportsMultipleBlocks = false;
                };

                MyAPIGateway.TerminalControls.AddControl<IMyTerminalBlock>(c);
            }

            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyTerminalBlock>(IdPrefix + "ChevronScale");
                c.Title = MyStringId.GetOrCompute("Chevron Scale");
                c.Tooltip = MyStringId.GetOrCompute("Changes the scale of the chevron on your map!");
                c.SupportsMultipleBlocks = false;
                c.Visible = CustomVisibleCondition;

                c.Setter = (b, v) =>
                {
                    var logic = b?.GameLogic?.GetAs<MercatorMapLogic>();
                    if (logic != null)
                        logic.mercatorMapChevronScale = MathHelper.Clamp(v, 0.1f, 10f); // just a heads up that the given value here is not clamped by the game, a mod or PB can give lower or higher than the limits!
                };
                c.Getter = (b) => b?.GameLogic?.GetAs<MercatorMapLogic>()?.mercatorMapChevronScale ?? 1;

                c.SetLimits(0.1f, 10f);
                c.Writer = (b, sb) =>
                {
                    var logic = b?.GameLogic?.GetAs<MercatorMapLogic>();
                    if (logic != null)
                    {
                        float val = logic.mercatorMapChevronScale;
                        sb.Append(Math.Round(val, 2));
                    }
                };

                MyAPIGateway.TerminalControls.AddControl<IMyTerminalBlock>(c);
            }

            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyTerminalBlock>(IdPrefix + "ChevronStrength");
                c.Title = MyStringId.GetOrCompute("Chevron Strength");
                c.Tooltip = MyStringId.GetOrCompute("Changes the glow of the chevron on your map!");
                c.SupportsMultipleBlocks = false;
                c.Visible = CustomVisibleCondition;

                c.Setter = (b, v) =>
                {
                    var logic = b?.GameLogic?.GetAs<MercatorMapLogic>();
                    if (logic != null)
                        logic.mercatorMapChevronStrength = MathHelper.Clamp(v, 0.1f, 10f); // just a heads up that the given value here is not clamped by the game, a mod or PB can give lower or higher than the limits!
                };
                c.Getter = (b) => b?.GameLogic?.GetAs<MercatorMapLogic>()?.mercatorMapChevronStrength ?? 10;

                c.SetLimits(0.1f, 10f);
                c.Writer = (b, sb) =>
                {
                    var logic = b?.GameLogic?.GetAs<MercatorMapLogic>();
                    if (logic != null)
                    {
                        float val = logic.mercatorMapChevronStrength;
                        sb.Append(Math.Round(val, 2));
                    }
                };

                MyAPIGateway.TerminalControls.AddControl<IMyTerminalBlock>(c);
            }

            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyTerminalBlock>(""); // separators don't store the id
                c.SupportsMultipleBlocks = false;
                c.Visible = CustomVisibleCondition;

                MyAPIGateway.TerminalControls.AddControl<IMyTerminalBlock>(c);
            }

            {
                var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMyTerminalBlock>(IdPrefix + "MapOffset");
                c.Title = MyStringId.GetOrCompute("Map direction offset");
                c.Tooltip = MyStringId.GetOrCompute("Check the tutorial if you have questions");
                c.SupportsMultipleBlocks = false;
                c.Visible = CustomVisibleCondition;

                c.Getter = (b) => b?.GameLogic?.GetAs<MercatorMapLogic>()?.mercatorMapOffset ?? 2;
                c.Setter = (b, key) => {
                

                    var logic = b?.GameLogic?.GetAs<MercatorMapLogic>();
                    if (logic != null)
                        logic.mercatorMapOffset = key;

                };
                c.ComboBoxContent = (list) =>
                {
                    list.Add(new MyTerminalControlComboBoxItem() { Key = 0, Value = MyStringId.GetOrCompute("Block's front") });
                    list.Add(new MyTerminalControlComboBoxItem() { Key = 1, Value = MyStringId.GetOrCompute("Block's right") });
                    list.Add(new MyTerminalControlComboBoxItem() { Key = 2, Value = MyStringId.GetOrCompute("Block's rear") });
                    list.Add(new MyTerminalControlComboBoxItem() { Key = 3, Value = MyStringId.GetOrCompute("Block's left") });
                };

                MyAPIGateway.TerminalControls.AddControl<IMyTerminalBlock>(c);
            }


        }
    }
}

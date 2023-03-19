using Sandbox.ModAPI;
using VRage.Game.Components;
using System.Collections.Generic;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Game.ModAPI;
using Sandbox.Common.ObjectBuilders;
using VRage.ObjectBuilders;

namespace PEP_CO.AntennaRanges
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_RadioAntenna), false)]
    public sealed class Loader : MyGameLogicComponent
    {
        private static bool InitedSession;
        MyObjectBuilder_EntityBase builder;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            builder = objectBuilder;
            NeedsUpdate |= VRage.ModAPI.MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }
        public override void UpdateOnceBeforeFrame()
        {
            if (MyAPIGateway.Session != null && !InitedSession)
            {
                InitAntennae();
            }
        }

        public static readonly Dictionary<string, float> AntennaRanges = new Dictionary<string, float>
        {
            { "LBShortRadioAntenna", 2000 },
            { "AntennaCorner", 5000 },
            { "AntennaSlope", 5000 },
            { "Antenna45Corner", 5000 },
            { "AntennaCube", 5000 },
            { "OmnidirectionalAntenna", 7500 },
            { "LargeBlockRadioAntenna", 10000 },
            { "LargeBlockRadioAntennaDish", 50000 },

            { "OmnidirectionalAntennaSmall", 200 },
            { "SmallBlockRadioAntenna", 500 },
            { "SBLongRadioAntenna", 2000 },
            { "SBAngledRadioAntenna", 2000 },


        };
        public static void InitAntennae()
        {
            List<IMyTerminalControl> antennactrls = new List<IMyTerminalControl>();
            MyAPIGateway.TerminalControls.GetControls<IMyRadioAntenna>(out antennactrls);
            IMyTerminalControlSlider RadiusSlider = antennactrls.Find(x => x.Id == "Radius") as IMyTerminalControlSlider;

            RadiusSlider.SetLogLimits((Antenna) => 1, (Antenna) => (Antenna as IMyRadioAntenna).GetMaxRange());
			InitedSession = true;
        }
    }

    public static class Extensions
    {
        /// <summary>
        /// Returns &lt;Description&gt; tag contents from block's .sbc definition. Broken by Keens.
        /// </summary>
        public static string GetCustomDefinition(this IMyCubeBlock Block)
        {
            return (Block as Sandbox.Game.Entities.MyCubeBlock).BlockDefinition.DescriptionString;
        }

        private static float GetDefaultRange(this IMyRadioAntenna Antenna)
        {
            return Antenna.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Large ? 50000 : 5000;
        }

        public static float GetMaxRange(this IMyRadioAntenna Antenna)
        {
            string Subtype = Antenna.BlockDefinition.SubtypeName;
            if (Loader.AntennaRanges.ContainsKey(Subtype))
                return Loader.AntennaRanges[Subtype];
            else
            {
                float Range = ParseMaxRange(Antenna);
                Loader.AntennaRanges.Add(Subtype, Range);
                return Range;
            }
        }

        private static float ParseMaxRange(this IMyRadioAntenna Antenna)
        {
            float range = Antenna.GetDefaultRange();
            string description = Antenna.GetCustomDefinition();
            if (description == null)
            {
                MyAPIGateway.Utilities.ShowMessage("AntennaRanges", $"Cannot parse definition for {Antenna.CustomName}: description is null. Blame Keens for breaking BlockDefinition's Description field.");
                return range;
            }
            range = ParseDefinitionForMaxRange(description);
            return range;
        }

        private static float ParseDefinitionForMaxRange(string Description)
        {
            float range = 0;
            var description = Description.Replace("\r\n", "\n").Trim().Split('\n');

            foreach (string DescriptionLine in description)
            {
                if (DescriptionLine.Trim().StartsWith("MaxRange:"))
                    float.TryParse(DescriptionLine.Split(':')[1].Trim(), out range);
            }
            return range;
        }
    }
}
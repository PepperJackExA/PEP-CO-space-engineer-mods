using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Jakaria.API;
using Sandbox.Game.Localization;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;


namespace SubmarineStuff
{
    public static class RemoteControlTerminalControls
    {
        const string IdPrefix = "SubmarineStuffEngine_"; // highly recommended to tag your properties/actions like this to avoid colliding with other mods'

        static bool Done = false;

        // just to clarify, don't store your states/values here, those should be per block and not static.

        public static void DoOnce(IMyModContext context) // called by GyroLogic.cs
        {
            if (Done)
                return;
            Done = true;

            // these are all the options and they're not all required so use only what you need.

            CreateProperties();
        }

        static bool CustomVisibleCondition(IMyTerminalBlock b)
        {
            // only visible for the blocks having this gamelogic comp
            return b?.GameLogic?.GetAs<RemoteControlLogic>() != null;
        }
        static void CreateProperties()
        {
            {
                var p = MyAPIGateway.TerminalControls.CreateProperty<Vector3, IMyRemoteControl>(IdPrefix + "BuoyancyForce");

                p.Getter = (b) =>
                {
                    var logic = b?.GameLogic?.GetAs<RemoteControlLogic>();
                    if (logic != null) return logic.BuoyancyForce;
                    else return Vector3.Zero;
                };
                p.Setter = (b, v) =>
                {
                };

                MyAPIGateway.TerminalControls.AddControl<IMyRemoteControl>(p);
            }

            {
                var p = MyAPIGateway.TerminalControls.CreateProperty<Vector3, IMyRemoteControl>(IdPrefix + "CenterOfBuoyancy");

                p.Getter = (b) =>
                {
                    var logic = b?.GameLogic?.GetAs<RemoteControlLogic>();
                    if (logic != null) return logic.CenterOfBuoyancy;
                    else return Vector3.Zero;
                };
                p.Setter = (b, v) =>
                {
                };

                MyAPIGateway.TerminalControls.AddControl<IMyRemoteControl>(p);
            }

            {
                var p = MyAPIGateway.TerminalControls.CreateProperty<Vector3, IMyRemoteControl>(IdPrefix + "FluidVelocity");

                p.Getter = (b) =>
                {
                    var logic = b?.GameLogic?.GetAs<RemoteControlLogic>();
                    if (logic != null) return logic.FluidVelocity;
                    else return Vector3.Zero;
                };
                p.Setter = (b, v) =>
                {
                };

                MyAPIGateway.TerminalControls.AddControl<IMyRemoteControl>(p);
            }

            {
                var p = MyAPIGateway.TerminalControls.CreateProperty<float, IMyRemoteControl>(IdPrefix + "FluidDepth");

                p.Getter = (b) =>
                {
                    var logic = b?.GameLogic?.GetAs<RemoteControlLogic>();
                    if (logic != null) return logic.FluidDepth;
                    else return float.PositiveInfinity;
                };
                p.Setter = (b, v) =>
                {
                };

                MyAPIGateway.TerminalControls.AddControl<IMyRemoteControl>(p);
            }

            {
                var p = MyAPIGateway.TerminalControls.CreateProperty<float, IMyRemoteControl>(IdPrefix + "PercentUnderwater");

                p.Getter = (b) =>
                {
                    var logic = b?.GameLogic?.GetAs<RemoteControlLogic>();
                    if (logic != null) return logic.PercentUnderwater;
                    else return float.PositiveInfinity;
                };
                p.Setter = (b, v) =>
                {
                };

                MyAPIGateway.TerminalControls.AddControl<IMyRemoteControl>(p);
            }

            {
                var p = MyAPIGateway.TerminalControls.CreateProperty<float, IMyRemoteControl>(IdPrefix + "FluidPressure");

                p.Getter = (b) =>
                {
                    var logic = b?.GameLogic?.GetAs<RemoteControlLogic>();
                    if (logic != null) return logic.FluidPressure;
                    else return float.PositiveInfinity;
                };
                p.Setter = (b, v) =>
                {
                };

                MyAPIGateway.TerminalControls.AddControl<IMyRemoteControl>(p);
            }

            {
                var p = MyAPIGateway.TerminalControls.CreateProperty<float, IMyRemoteControl>(IdPrefix + "BuoyancyRatio");

                p.Getter = (b) =>
                {
                    var logic = b?.GameLogic?.GetAs<RemoteControlLogic>();
                    if (logic != null) return logic.BuoyancyRatio;
                    else return float.PositiveInfinity;
                };
                p.Setter = (b, v) =>
                {
                };

                MyAPIGateway.TerminalControls.AddControl<IMyRemoteControl>(p);
            }



        }

    }
}

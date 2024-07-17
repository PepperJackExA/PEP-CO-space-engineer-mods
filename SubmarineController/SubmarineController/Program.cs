using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        List<IMyTerminalBlock> _tanks = new List<IMyTerminalBlock>();
        List<IMyRemoteControl> _remoteControls = new List<IMyRemoteControl>();

        IMyRemoteControl primaryRemoteControl;

        public Program()
        {
            //Collect all the tanks on the grid with subtype == BallastTank
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(_tanks, b => b.BlockDefinition.SubtypeId == "BallastTank");
            Echo(_tanks.Count.ToString());

            GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(_remoteControls);
            if (_remoteControls.Count > 0)
            {
                primaryRemoteControl = _remoteControls[0];
            }

            //Echo a concatenated string of the _tanks' names
            string tankNames = "";
            foreach (IMyTerminalBlock tank in _tanks)
            {
                if (tank.GetProperty("SubmarineStuffEngine_BallastTrimSlider") != null)
                {
                    tankNames += tank.CustomName + "\n" + tank.GetValueFloat("SubmarineStuffEngine_BallastTrimSlider").ToString() + "\n";
                }
                tank.SetValueFloat("SubmarineStuffEngine_BallastTrimSlider", 0.5f);
            }

            tankNames += "Primary Remote Control: " + primaryRemoteControl.GetValue<Vector3>("SubmarineStuffEngine_Boyancy");
            Echo(tankNames);
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked,
            // or the script updates itself. The updateSource argument
            // describes where the update came from. Be aware that the
            // updateSource is a  bitfield  and might contain more than 
            // one update type.
            // 
            // The method itself is required, but the arguments above
            // can be removed if not needed.
        }
    }
}

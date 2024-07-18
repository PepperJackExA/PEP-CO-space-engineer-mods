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

            Runtime.UpdateFrequency = UpdateFrequency.Update100;

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

            //Echo a concatenated string of the _tanks' names
            string output = "";

            output += $"UZAR did this:\n" +
                $"boyancyForce: {primaryRemoteControl.GetValue<float>("SubmarineStuffEngine_BuoyancyRatio")}\n" +
                $"";
            Echo(output);
            Me.GetSurface(0).WriteText(output);
        }
    }
}

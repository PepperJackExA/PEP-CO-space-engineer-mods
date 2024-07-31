using System.Collections.Generic;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage;
using VRage.Utils;
using Digi;
using VRage.Scripting;

namespace PEPCO
{
    // In this example we're hiding the "Detect asteroids" terminal control and terminal action. Also bonus, enforcing it to stay false.
    //  All this only on a specific sensor block to show doing it properly without breaking other mods trying to do the same.
    //
    // This is also compatible with multiple mods doing the same thing on the same type, but for different subtypes.
    //   For example another mod could have the same on largegrid sensor to hide a different control, or even the same control, it would work properly.


    // You will need the internal IDs for the terminal properties and/or actions you wish to edit, pick one:
    // - Use the commented-out code I left in the Edit*() methods below and run the game, it will write to SE's log.
    // - For vanilla: With a decompiler go to the block's class and find CreateTerminalControls().
    // - For mods: Open the mod's downloaded folder (by searching its workshopid in the steam folder) and dive through its .cs files to find where they're declaring them.

    // For important notes about terminal controls see: https://github.com/THDigi/SE-ModScript-Examples/blob/master/Data/Scripts/Examples/TerminalControls/Adding/GyroTerminalControls.cs#L21-L35
    public static class HideNavigationScreenControls
    {
        static bool Done = false;

        //Add a hashset with control Ids to be hidden
        static HashSet<string> HiddenControls = new HashSet<string>()
        {
            "Script",
            "PanelList",
            "Content",
            "ScriptForegroundColor",
            "ScriptBackgroundColor",
            "ShowTextPanel",
            "Font",
            "FontSize",
            "FontColor",
            "alignment",
            "TextPaddingSlider",
            "BackgroundColor",
            "ImageList",
            "SelectTextures",
            "ChangeIntervalSlider",
            "SelectedImageList",
            "RemoveSelectedTextures",
            "PreserveAspectRatio",
            "Rotate",
            "Title",

        };

        public static void DoOnce() // called by SensorLogic.cs
        {
            if (Done)
                return;

            Done = true;

            EditControls();
        }

        static bool AppendedCondition(IMyTerminalBlock block)
        {
            // if block has this gamelogic component then return false to hide the control/action.
            return block?.GameLogic?.GetAs<NavigationScreenLogic>() == null;
        }

        static void EditControls()
        {
            List<IMyTerminalControl> controls;

            // mind the IMySensorBlock input
            MyAPIGateway.TerminalControls.GetControls<IMyTextPanel>(out controls);

            foreach (IMyTerminalControl c in controls)
            {

                //lookup if id is in the hashset
                if (c.Id == "" || HiddenControls.Contains(c.Id))
                {
                    c.Visible = TerminalChainedDelegate.Create(c.Visible, AppendedCondition); // hides
                }
            }
        }

    }
}
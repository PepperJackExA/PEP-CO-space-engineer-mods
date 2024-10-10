using RichHudFramework.Client;
using RichHudFramework.Internal;
using RichHudFramework.IO;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Input;
using VRageMath;

namespace TextEditorExample
{
    /// <summary>
    /// Example Text Editor Mod used to demonstrate the usage of the Rich HUD Framework.
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class TextEditorMain : MySessionComponentBase
    {
        private TextEditor textEditor;
        private IBindGroup editorBinds;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            RichHudClient.Init("Text Editor Example", HudInit, ClientReset);
        }

        private void HudInit()
        {
            /* There are three(-ish) ways to register a HUD element to a parent element. By calling the parent's RegisterChild() method
             * and passing in the child element, by calling the child element's Register() method and passing in the parent or by
             * passing the parent into the child's constructor. 
             
             I'm using HighDpiRoot instead of Root to compensate for scaling at resolutions > 1080p.
           */
            textEditor = new TextEditor(HudMain.HighDpiRoot)
            {
                Visible = false, // I don't want this to be visible on init.
            };

            editorBinds = BindManager.GetOrCreateGroup("editorBinds");
            editorBinds.RegisterBinds(new BindGroupInitializer()
            {
                { "editorToggle", MyKeys.Home, MyKeys.Control }
            });

            editorBinds[0].NewPressed += ToggleEditor;
        }

        private void ToggleEditor(object sender, EventArgs args)
        {
            textEditor.Visible = !textEditor.Visible;
            HudMain.EnableCursor = textEditor.Visible;
        }

        public override void Draw()
        {
            if (RichHudClient.Registered)
            {
                /* If you need to update framework members externally, then 
                you'll need to make sure you don't start updating until your
                mod client has been registered. */
            }
        }

        private void ClientReset()
        {
            /* At this point, your client has been unregistered and all of 
            your framework members will stop working.

            This will be called in one of three cases:
            1) The game session is unloading.
            2) An unhandled exception has been thrown and caught on either the client
            or on master.
            3) RichHudClient.Reset() has been called manually.
            */
        }
    }
}

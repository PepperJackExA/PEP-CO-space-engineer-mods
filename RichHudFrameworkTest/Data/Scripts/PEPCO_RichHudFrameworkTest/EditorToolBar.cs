using System;
using VRageMath;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Rendering.Client;
using RichHudFramework.UI;
using EventHandler = RichHudFramework.EventHandler;
using Sandbox.ModAPI;

namespace TextEditorExample
{
    public partial class TextEditor
    {
        /// <summary>
        /// Text editor toolbar
        /// </summary>
        private class EditorToolBar : HudElementBase
        {
            


            // The width of the HudChain containing the controls is determined by the total width
            // of every element in the chain
            public float MinimumWidth => layout.Width + Padding.X;

            private readonly HudChain layout;
            private readonly EditorDropdown<string> itemList;
            private readonly EditorToggleButton craftButton;

            private static readonly string[] items = new string[] {"Hammer", "Pickaxe", "Brains", "Happiness"};
            private GlyphFormat _format;

            public EditorToolBar(HudParentBase parent = null) : base(parent)
            {
                var background = new TexturedBox(this)
                {
                    DimAlignment = DimAlignments.Both,
                    Color = new Color(41, 54, 62),
                };

                // Item list
                itemList = new EditorDropdown<string>()
                {
                    Height = 24f,
                    Width = 400f,
                };

                for (int n = 0; n < items.Length; n++)
                    itemList.Add(items[n], items[n]);

                // Font style toggle

                GlyphFormat buttonFormat = new GlyphFormat(Color.White, TextAlignment.Center, 1.0f);

                craftButton = new EditorToggleButton()
                {
                    Format = buttonFormat,
                    Text = "Craft",
                    Width = 100f,
                };

                /* HudChain is useful for organizing collections of elements into straight lines with regular spacing, 
                 * either vertically horizontally. In this case, I'm organizing elements horizontally from left to right
                 * in the same order indicated by the collection initializer below. 
                 * 
                 * HudChain and its related types, like ScrollBox and the SelectionBox types, are powerful tools for 
                 * organizing UI elements, especially when used in conjunction with oneanother. 
                 */
                layout = new HudChain(false, this) // Set to alignVertical false to align the elements horizontally
                {
                    // Automatically resize the height of the elements to match that of the chain and allow the chain to be
                    // wider than the total size of the members
                    SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.ClampChainAlignAxis,
                    // Match the height of the chain and its children to the toolbar
                    DimAlignment = DimAlignments.Height | DimAlignments.IgnorePadding,
                    // The width of the parent could very well be greater than the width of the controls.
                    ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH | ParentAlignments.UsePadding,
                    // The order the elements will appear on the toolbar from left to right.
                    CollectionContainer = { itemList, craftButton }
                };


                craftButton.MouseInput.LeftClicked += DoMagic;


                Height = 30f;
                Padding = new Vector2(16f, 0f);
                _format = GlyphFormat.White;
            }

            protected override void Layout()
            {
                // The width of the toolbar should not be less than the total width of the controls
                // it contains.
                Width = Math.Max(Width, layout.Width + Padding.X);
            }

            /// <summary>
            /// Updates formatting based on input from the toolbar controls
            /// </summary>
            private void DoMagic(object sender, EventArgs args)
            {
                if (itemList.Selection != null)
                {
                    string selectedItem = itemList.Selection.AssocMember;


                        //Do magic
                        MyAPIGateway.Utilities.ShowMessage(string.Empty, $"Crafting {selectedItem}!");
                }
                else
                {
                    MyAPIGateway.Utilities.ShowMessage(string.Empty, "Nothing selected! Dummy!\nTry crafting brains!");
                }
            }
        }
    }
}

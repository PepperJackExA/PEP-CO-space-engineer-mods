using System;
using VRageMath;
using RichHudFramework.UI;

namespace TextEditorExample
{
    public partial class TextEditor
    {
        /// <summary>
        /// A TextBoxButton modified to serve as a toggle button for the editor.
        /// </summary>
        private class EditorToggleButton : LabelBoxButton
        {
            /// <summary>
            /// Indicates whether the button will accept input
            /// </summary>
            public bool Enabled
            {
                get { return _mouseInput.Visible; }
                set
                {
                    disabledOverlay.Visible = !value;
                    _mouseInput.Visible = value;
                }
            }



            /// <summary>
            /// Button color when selected
            /// </summary>
            public Color SelectColor { get; set; }

            /// <summary>
            /// Disabled overlay color
            /// </summary>
            public Color DisabledColor
            {
                get { return disabledOverlay.Color; }
                set { disabledOverlay.Color = value; }
            }

            private readonly TexturedBox disabledOverlay;

            public EditorToggleButton(HudElementBase parent = null) : base(parent)
            {
                // This overlay will be drawn over the button when it's disabled
                disabledOverlay = new TexturedBox(this)
                { 
                    Visible = false,
                    DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding
                };

                Color = TerminalFormatting.OuterSpace;
                HighlightColor = TerminalFormatting.Atomic;
                SelectColor = new Color(58, 68, 77);
                DisabledColor = new Color(0, 0, 0, 80);

                AutoResize = false;
                Enabled = true;

                Size = new Vector2(42f, 30f);

                MouseInput.LeftClicked += LeftClick;
            }

            /// <summary>
            /// Updates button state when left clicked
            /// </summary>
            private void LeftClick(object sender, EventArgs args)
            {
                //if (Enabled)
                //{
                //    Selected = !Selected;

                //    if (Selected)
                //        Color = SelectColor;
                //    else
                //        Color = oldColor;
                //}
            }

            /// <summary>
            /// Updates button state when moused over
            /// </summary>
            protected override void CursorEnter(object sender, EventArgs args)
            {
                //if (Enabled && HighlightEnabled)
                //{
                //    if (!Selected)
                //        oldColor = Color;

                //    Color = HighlightColor;
                //}
            }

            /// <summary>
            /// Updates button state when cursor exits
            /// </summary>
            protected override void CursorExit(object sender, EventArgs args)
            {
                //if (Enabled && HighlightEnabled)
                //{
                //    if (Selected)
                //        Color = SelectColor;
                //    else
                //        Color = oldColor;
                //}
            }
        }
    }
}

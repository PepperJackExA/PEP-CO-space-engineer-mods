using System;
using VRageMath;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI;
using RichHudFramework;

namespace TextEditorExample
{
    /// <summary>
    /// Example Text Editor window
    /// </summary>
    public partial class TextEditor : WindowBase
    {
        private readonly EditorToolBar toolBar;

        /// <summary>
        /// Initializes a new Text Editor window and registers it to the specified parent element.
        /// You can leave the parent null and use the parent element's register method if you prefer.
        /// </summary>
        public TextEditor(HudParentBase parent = null) : base(parent)
        {

            toolBar = new EditorToolBar(header)
            {
                DimAlignment = DimAlignments.Width,
                ParentAlignment = ParentAlignments.Bottom,
            };


            // Window styling:
            BodyColor = new Color(41, 54, 62, 150);
            BorderColor = new Color(58, 68, 77);

            header.Format = new GlyphFormat(GlyphFormat.Blueish.Color, TextAlignment.Center, 1.08f);
            header.Height = 30f;

            HeaderText = "EME Crafting Menu";
            Size = new Vector2(500f, 60);
        }




        protected override void Layout()
        {
            base.Layout();

            // Set window minimum width to prevent it from becoming narrower than the toolbar's minimum width
            MinimumSize = new Vector2(Math.Max(toolBar.MinimumWidth, MinimumSize.X), MinimumSize.Y);

        }
    }
}

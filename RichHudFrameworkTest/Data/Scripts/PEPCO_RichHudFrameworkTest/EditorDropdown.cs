using VRageMath;
using RichHudFramework.UI;

namespace TextEditorExample
{
    public partial class TextEditor
    {
        /// <summary>
        /// Customized Dropdown whose proportions have been altered to fit in the toolbar.
        /// </summary>
        private class EditorDropdown<T> : Dropdown<T>
        {
            public EditorDropdown(HudParentBase parent = null) : base(parent)
            {
                // This dropdown was originally made to mimic the dropdown in the SE terminal,
                // meaning it's fairly large. Fortunately, shrinking it down to size is just
                // a matter of thinning down a few elements and changing some text formatting.
                ScrollBar scrollBar = listBox.hudChain.ScrollBar;

                scrollBar.Padding = new Vector2(12f, 8f);
                scrollBar.Width = 20f;

                // Shrink down the divider and arrow icon in the dropdown display
                display.divider.Padding = new Vector2(4f, 8f);
                display.arrow.Width = 22f;

                // By setting the Height to 0, we can ensure that the only determining factor
                // for the height of the dropdown list is the number of elements visible
                listBox.Height = 0f;
                listBox.MinVisibleCount = 4;

                // Make the text smaller
                Format = GlyphFormat.White.WithSize(0.8f);
            }
        }
    }
}

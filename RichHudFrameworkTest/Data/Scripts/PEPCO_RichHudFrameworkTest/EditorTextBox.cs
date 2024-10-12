using System;
using VRageMath;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI;

namespace TextEditorExample
{
    public partial class TextEditor
    {
        /// <summary>
        /// Scrollable text box for the text editor
        /// </summary>
        private class EditorTextBox : HudElementBase
        {
            public readonly TextBox text;
            private readonly ScrollBar verticalScroll, horizontalScroll;

            public EditorTextBox(HudParentBase parent = null) : base(parent)
            {             
                text = new TextBox(this)
                {
                    // Align the text box to the top left corner of the parent element and place it on the interior
                    ParentAlignment = ParentAlignments.Top | ParentAlignments.Left | ParentAlignments.Inner | ParentAlignments.UsePadding,
                    Padding = new Vector2(8f, 8f),

                    Format = GlyphFormat.White,
                    VertCenterText = false, // This is a text editor; I don't want the text centered.
                    AutoResize = false, // Allows the text box size to be set manually (or via DimAlignment)
                    ClearSelectionOnLoseFocus = false // Leaving this enabled creates problems when trying to reformat text
                };

                // These scroll bars will be used to control text scrolling via textboard text offset
                verticalScroll = new ScrollBar(this)
                {
                    // Align this element s.t. it will be to the right of the text box
                    ParentAlignment = ParentAlignments.Top | ParentAlignments.Right | ParentAlignments.Inner | ParentAlignments.UsePadding,
                    Padding = new Vector2(8f),
                    Width = 18f,
                    Vertical = true,
                };

                horizontalScroll = new ScrollBar(this)
                {
                    // Align this s.t. it will be below both the text box and the right scroll bar
                    ParentAlignment = ParentAlignments.Bottom | ParentAlignments.Inner | ParentAlignments.UsePadding,
                    Padding = new Vector2(8f),
                    Height = 18f,
                    Vertical = false,
                };
            }

            protected override void Layout()
            {
                // Update scroll bar and text box size to match changes parent size
                verticalScroll.Height = Height - horizontalScroll.Height - Padding.Y;
                horizontalScroll.Width = Width - Padding.X;

                text.Width = Width - verticalScroll.Width - Padding.X;
                text.Height = Height - horizontalScroll.Height - Padding.Y;

                // Update slider size to reflect the amount of text being displayed
                //
                // ScrollBars will automatically hide the slide if the slide and slide bar are the 
                // same size, meaning we don't need to worry about hiding the slides if the amount of
                // text is less than or equal to what will fit in the text box.
                ITextBoard textBoard = text.TextBoard;

                horizontalScroll.slide.SliderWidth = (textBoard.Size.X / textBoard.TextSize.X) * horizontalScroll.Width;
                verticalScroll.slide.SliderHeight = (textBoard.Size.Y / textBoard.TextSize.Y) * verticalScroll.Height;
            }

            protected override void HandleInput(Vector2 cursorPos)
            {
                /* TextBoard Offsets:
                
                The TextBoard allows you to set an offset for the text being rendered starting from the
                center of the element. Text outside the bounds of the element will not be drawn.
                Offset is measured in pixels and updates with changes to scale.
                 
                An offset in the negative direction on the X-axis will offset the text to the left; a positive
                offset will move the text to the right.
                
                On the Y-axis, a negative offset will move the text down and a positive offset will move it in
                the opposite direction.
                
                By default, the visible range of text will start at the first line on the first character.
                It starts in the upper left hand corner.
                */

                ITextBoard textBoard = text.TextBoard;
                IMouseInput horzControl = horizontalScroll.slide.MouseInput,
                    vertControl = verticalScroll.slide.MouseInput;

                // If the total width of the text is greater than the size of the element, then I can scroll
                // horiztonally. This value is negative because the text is starts at the right hand side
                // and I need to move it left.
                horizontalScroll.Max = Math.Max(0f, textBoard.TextSize.X - textBoard.Size.X);

                // Same principle, but vertical and moving up. TextBoards start at the first line which means
                // every line that follows lower than the last, so I need to move up.
                verticalScroll.Max = Math.Max(0f, textBoard.TextSize.Y - textBoard.Size.Y);

                // Update the ScrollBar positions to represent the current offset unless they're being clicked.
                if (!horzControl.IsLeftClicked)
                    horizontalScroll.Current = -textBoard.TextOffset.X;

                if (!vertControl.IsLeftClicked)
                    verticalScroll.Current = textBoard.TextOffset.Y;

                textBoard.TextOffset = new Vector2(-horizontalScroll.Current, verticalScroll.Current);
            }
        }
    }
}

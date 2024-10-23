using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using VRageMath;

namespace TextEditorExample
{
    public class TextEditorTerminalPage : TerminalPageBase
    {
        private TextEditor textEditor;

        public TextEditorTerminalPage() : base("Text Editor")
        {
            // Initialize the text editor
            textEditor = new TextEditor(this)
            {
                Visible = true,
                Size = new Vector2(600f, 400f),
                Offset = new Vector2(0f, 0f)
            };
        }

        public override void Update()
        {
            base.Update();
            // Add any additional update logic here
        }
    }
}

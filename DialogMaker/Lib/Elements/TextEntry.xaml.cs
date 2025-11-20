using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class TextEntry : UserControl
    {
        public TextEntry()
        {
            InitializeComponent();
        }

        public TextBox TextBox => _textInput;
        public Button Button => _confirmButton;
    }
}

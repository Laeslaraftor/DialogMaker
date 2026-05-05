using DialogMaker.Lib.Elements;
using System.Windows;

namespace DialogMaker.Lib.InputFields
{
    public class InvalidInputField : InputField
    {
        public InvalidInputField()
        {
            _view = new ToolTipView()
            {
                MessageType = MessageType.Error
            };
        }

        public override string Placeholder
        {
            get => field ?? string.Empty;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(Placeholder));
                    field = value;
                    _view.Text = value;
                    OnPropertyChanged(nameof(Placeholder));
                }
            }
        }
        public override object? Value
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(Value));
                    field = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public override FrameworkElement View => _view;

        private readonly ToolTipView _view;
    }
}

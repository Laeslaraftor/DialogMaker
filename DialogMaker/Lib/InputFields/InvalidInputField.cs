using System.Windows;
using DialogMaker.Lib.Elements;

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
                    InvokePropertyChanging(nameof(Placeholder));
                    field = value;
                    _view.Text = value;
                    InvokePropertyChanged(nameof(Placeholder));
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
                    InvokePropertyChanging(nameof(Value));
                    field = value;
                    InvokePropertyChanged(nameof(Value));
                }
            }
        }

        public override FrameworkElement View => _view;

        private readonly ToolTipView _view;
    }
}

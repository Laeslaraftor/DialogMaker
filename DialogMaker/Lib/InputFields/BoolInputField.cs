using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.InputFields
{
    public class BoolInputField : InputField
    {
        public BoolInputField()
        {
            _view = new();
            _placeholder = new()
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new(5,0,0,0)
            };
            _box = new();

            _view.ColumnDefinitions.Add(new()
            {
                Width = GridLength.Auto
            });
            _view.ColumnDefinitions.Add(new()
            {
                Width = GridLength.Auto
            });

            _view.Children.Add(_placeholder);
            _view.Children.Add(_box);

            Grid.SetColumn(_placeholder, 1);

            _box.Checked += OnBoxChecked;
            _box.Unchecked += OnBoxChecked;
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

                    if (_placeholder.Text != value)
                    {
                        _placeholder.Text = value;
                    }

                    InvokePropertyChanged(nameof(Placeholder));
                }
            }
        }
        public override object? Value
        {
            get => field;
            set
            {
                if (field?.Equals(value) != true)
                {
                    InvokePropertyChanging(nameof(Value));
                    field = value;

                    if (!_box.IsChecked.Equals(value))
                    {
                        _box.IsChecked = value?.Equals(true);
                    }

                    InvokePropertyChanged(nameof(Value));
                }
            }
        }

        public override FrameworkElement View => _view;

        private readonly Grid _view;
        private readonly TextBlock _placeholder;
        private readonly CheckBox _box;

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            _box.Checked -= OnBoxChecked;
            _box.Unchecked -= OnBoxChecked;
        }

        #endregion

        #region События

        private void OnBoxChecked(object sender, RoutedEventArgs e)
        {
            Value = _box.IsChecked;
        }

        #endregion
    }
}

using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.InputFields
{
    public abstract class SliderInputField : TextInputField
    {
        public SliderInputField() : base()
        {
            _container.ColumnDefinitions.Add(new());
            _container.ColumnDefinitions.Add(new());

            _container.Children.Add(Entry);
            _container.Children.Add(_slider);

            _slider.ValueChanged += OnSliderValueChanged;

            Entry.TextWrapping = TextWrapping.NoWrap;
            Grid.SetColumnSpan(Entry, 2);
        }

        public bool IsSlider
        {
            get => _slider.Visibility == Visibility.Visible;
            set
            {
                if (IsSlider != value)
                {
                    OnPropertyChanging(nameof(IsSlider));
                    _slider.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                    var secondColumnSize = GridLength.Auto;

                    if (value)
                    {
                        secondColumnSize = new(80);
                    }

                    _container.ColumnDefinitions[1].Width = secondColumnSize;
                    Grid.SetColumnSpan(Entry, value ? 1 : 2);
                    Grid.SetColumn(Entry, 2 - Grid.GetColumnSpan(Entry));
                    OnPropertyChanged(nameof(IsSlider));
                }
            }
        }
        public float MinValue
        {
            get => (float)_slider.Minimum;
            set
            {
                if ((float)_slider.Minimum != value)
                {
                    OnPropertyChanging(nameof(MinValue));
                    _slider.Minimum = value;
                    OnPropertyChanged(nameof(MinValue));
                }
            }
        }
        public float MaxValue
        {
            get => (float)_slider.Maximum;
            set
            {
                if ((float)_slider.Maximum != value)
                {
                    OnPropertyChanging(nameof(MaxValue));
                    _slider.Maximum = value;
                    OnPropertyChanged(nameof(MaxValue));
                }
            }
        }
        public override FrameworkElement View => _container;

        private readonly Grid _container = new();
        private readonly Slider _slider = new()
        {
            Visibility = Visibility.Collapsed,
            Margin = new(0, 0, 10, 0),
            Maximum = double.MaxValue
        };

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            _slider.ValueChanged -= OnSliderValueChanged;
        }

        #endregion

        #region События

        private void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Value = e.NewValue;
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName != nameof(Value))
            {
                return;
            }

            var value = Value;

            if (value is double ||
                value is float ||
                value is int)
            {
                try
                {
                    _slider.Value = System.Convert.ToDouble(value);
                }
                catch (Exception error)
                {
                    error.Log();
                }
            }
        }

        #endregion
    }
}

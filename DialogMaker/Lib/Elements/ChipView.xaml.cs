using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace DialogMaker.Lib.Elements
{
    public partial class ChipView : UserControl
    {
        public ChipView()
        {
            InitializeComponent();
        }

        public event EventHandler<RoutedEventArgs>? Click;

        public Brush? Color
        {
            get => GetValue(ColorProperty) as Brush;
            set => SetValue(ColorProperty, value); 
        }
        public Brush? TextColor
        {
            get => GetValue(TextColorProperty) as Brush;
            set => SetValue(TextColorProperty, value);
        }
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value); 
        }
        public ICommand? ClickCommand
        {
            get => GetValue(ClickCommandProperty) as ICommand;
            set => SetValue(ClickCommandProperty, value);   
        }
        public object? ClickCommandParameter
        {
            get => GetValue(ClickCommandParameterProperty);
            set => SetValue(ClickCommandParameterProperty, value);
        }
        public bool AutoSelect
        {
            get => (bool)GetValue(AutoSelectProperty);
            set => SetValue(AutoSelectProperty, value);
        }

        #region События

        private void OnBackgroundBorderSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _diagonalLine.Y1 = e.NewSize.Height;
            _diagonalLine.X1 = e.NewSize.Width;
        }

        private void OnMainIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool value = IsSelected;
            OnIsSelectedChanged(this, new(IsSelectedProperty, value, value));
        }
        private void OnButtonClicked(object sender, RoutedEventArgs e)
        {
            var command = ClickCommand;
            var commandParameter = ClickCommandParameter;

            if (command?.CanExecute(commandParameter) == false)
            {
                return;
            }
            if (AutoSelect)
            {
                IsSelected = !IsSelected;
            }

            command?.Execute(commandParameter);
            Click?.Invoke(this, e);
        }

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChipView view)
            {
                view._mainBorder.BorderBrush = e.NewValue as Brush;
            }
        }
        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChipView view)
            {
                view._text.Text = e.NewValue?.ToString();
            }
        }
        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChipView view && e.NewValue is bool value)
            {
                double opacity = value && view.IsEnabled ? 1 : 0;
                view._backgroundBorder.Opacity = opacity;
                view._text.Opacity = 1 - opacity;
            }
        }
        private static void OnTextColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChipView view)
            {
                view._text.Foreground = e.NewValue as Brush;
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(Brush),
            typeof(ChipView), new(OnColorChanged));
        public static readonly DependencyProperty TextColorProperty = DependencyProperty.Register(nameof(TextColor), typeof(Brush),
            typeof(ChipView), new(OnTextColorChanged));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string),
            typeof(ChipView), new(string.Empty, OnTextChanged));
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool),
            typeof(ChipView), new(false, OnIsSelectedChanged));
        public static readonly DependencyProperty ClickCommandProperty = DependencyProperty.Register(nameof(ClickCommand), typeof(ICommand),
            typeof(ChipView));
        public static readonly DependencyProperty ClickCommandParameterProperty = DependencyProperty.Register(nameof(ClickCommandParameter), typeof(object),
            typeof(ChipView));
        public static readonly DependencyProperty AutoSelectProperty = DependencyProperty.Register(nameof(AutoSelect), typeof(bool),
            typeof(ChipView), new(false));

        #endregion
    }
}

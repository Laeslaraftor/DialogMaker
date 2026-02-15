using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DialogMaker.Lib.Elements
{
    public partial class DiagramNodePort : UserControl
    {
        public DiagramNodePort()
        {
            InitializeComponent();
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public Brush Color
        {
            get => (Brush)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }
        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }
        public bool Invert
        {
            get => (bool)GetValue(InvertProperty);
            set => SetValue(InvertProperty, value);
        }
        public UIElement? ExtraControl
        {
            get => GetValue(ExtraControlProperty) as UIElement;
            set => SetValue(ExtraControlProperty, value);
        }
        public bool IsExtraControlVisible
        {
            get => (bool)GetValue(IsExtraControlVisibleProperty);
            set => SetValue(IsExtraControlVisibleProperty, value);
        }

        #region Управление

        public Point GetConnectorPosition(Visual relativeTo)
        {
            var size = _background.RenderSize / 2;
            return _background.GetPosition(relativeTo) + size;
        }
        public Rect GetConnectorRect(Visual relativeTo)
        {
            var scale = _border.GetVisualTreeScale();
            var position = _border.GetPosition(relativeTo);
            var size = _border.RenderSize;

            return new(position, size * scale);
        }

        #endregion

        #region События

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramNodePort view)
            {
                view._text.Text = e.NewValue?.ToString();
            }
        }
        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramNodePort view)
            {
                var colorBrush = e.NewValue as Brush;

                view._background.Background = colorBrush;
                view._border.BorderBrush = colorBrush;
                view._borderFade.Background = colorBrush;
                view._mainContainerBorder.BorderBrush = colorBrush;
            }
        }
        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramNodePort view && e.NewValue is bool value)
            {
                view._background.Opacity = value == true ? 1 : 0.1;
                Panel.SetZIndex(view._background, value == true ? 100 : 0);
            }
        }
        private static void OnInvertChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramNodePort view && e.NewValue is bool value)
            {
                int column = 1;
                GridLength mainColumnLength = GridLength.Auto;
                GridLength secondColumnLength = new(1, GridUnitType.Star);

                if (value)
                {
                    column = 0;
                    mainColumnLength = secondColumnLength;
                    secondColumnLength = GridLength.Auto;
                }

                view._firstColumn.Width = secondColumnLength;
                view._secondColumn.Width = mainColumnLength;

                view._mainContainer.Margin = value ? new(-6, 0, 0, 0) : new(0, 0, -6, 0);
                view._borderOffset.X = value ? -12 : 12;
                view._borderFadeScale.ScaleX = value ? 1 : -1;
                view._mainContainerBorderScale.ScaleX = view._borderFadeScale.ScaleX;
                Grid.SetColumn(view._border, column);
                Grid.SetColumn(view._mainContainer, 1 - column);
            }
        }
        private static void OnExtraControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramNodePort view)
            {
                view._fieldContainer.Content = e.NewValue as UIElement;
            }
        }
        private static void OnIsExtraControlVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramNodePort view && e.NewValue is bool value)
            {
                view._fieldContainer.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string),
            typeof(DiagramNodePort), new(string.Empty, OnTextChanged));
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(Brush),
            typeof(DiagramNodePort), new(Brushes.White, OnColorChanged));
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(nameof(IsActive), typeof(bool),
            typeof(DiagramNodePort), new(OnIsActiveChanged));
        public static readonly DependencyProperty InvertProperty = DependencyProperty.Register(nameof(Invert), typeof(bool),
            typeof(DiagramNodePort), new(OnInvertChanged));
        public static readonly DependencyProperty ExtraControlProperty = DependencyProperty.Register(nameof(ExtraControl), typeof(UIElement),
            typeof(DiagramNodePort), new(OnExtraControlChanged));
        public static readonly DependencyProperty IsExtraControlVisibleProperty = DependencyProperty.Register(nameof(IsExtraControlVisible), typeof(bool),
            typeof(DiagramNodePort), new(true, OnIsExtraControlVisiblePropertyChanged));

        #endregion
    }
}

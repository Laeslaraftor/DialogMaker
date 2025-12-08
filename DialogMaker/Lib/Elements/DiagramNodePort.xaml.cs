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
                view._background.Background = e.NewValue as Brush;
                view._border.BorderBrush = e.NewValue as Brush;
            }
        }
        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramNodePort view && e.NewValue is bool value)
            {
                view._background.Opacity = value == true ? 1 : 0.1;
            }
        }
        private static void OnInvertChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramNodePort view && e.NewValue is bool value)
            {
                int column = 0;

                if (value)
                {
                    column = 1;
                }

                Grid.SetColumn(view._text, column);
                Grid.SetColumn(view._border, 1 - column);
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

        #endregion
    }
}

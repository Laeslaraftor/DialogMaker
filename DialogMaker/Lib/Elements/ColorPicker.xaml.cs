using System.ComponentModel;
using DialogMaker.Core.Editor;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DialogMaker.Lib.Elements
{
    public partial class ColorPicker : UserControl
    {
        public ColorPicker()
        {
            InitializeComponent();
        }

        public event EventHandler<ValueChangedEventArgs<Color>>? ColorChanged;

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }
        public double BorderRadius
        {
            get => (double)GetValue(BorderRadiusProperty);
            set => SetValue(BorderRadiusProperty, value);
        }

        private readonly ColorPickerSelectionView _selectionView = new();
        private Window? _currentSelectionWindow;

        #region Управление

        private void UpdateOpacity()
        {
            _opacity.Width = RenderSize.Width * ((double)Color.A / 255);
        }

        #endregion

        #region События

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property.Name == nameof(IsEnabled))
            {
                _main.Cursor = IsEnabled ? Cursors.Hand : Cursors.Hand;
            }
        }
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            _clipGeometry.Rect = new(0, 0, sizeInfo.NewSize.Width, sizeInfo.NewSize.Height);
            UpdateOpacity();
        }

        private void OnButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_currentSelectionWindow != null)
            {
                _currentSelectionWindow.Focus();
                return;
            }

            var position = PointToScreen(Mouse.GetPosition((Button)sender));
            Point size = new(250, 300);
            Point halfSize = size / 2;

            Window window = new()
            {
                Width = size.X,
                Height = size.Y,
                Content = _selectionView,
                WindowStyle = WindowStyle.ToolWindow,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = Math.Max(position.X - halfSize.X, 0),
                Top = Math.Max(position.Y - halfSize.Y, 0)
            };

            try
            {
                window.Title = ToolTip?.ToString() ?? string.Empty;
            }
            catch (Exception error)
            {
                Logger.Log(error);
            }

            _currentSelectionWindow = window;

            void OnWindowClosing(object? sender, CancelEventArgs e)
            {
                window.Content = null;
                window.Closing -= OnWindowClosing;
                window.Deactivated -= OnWindowDeactivated;

                if (_currentSelectionWindow == window)
                {
                    _currentSelectionWindow = null;
                }
            }
            void OnWindowDeactivated(object? sender, EventArgs e)
            {
                //window.Close();
            }

            window.Closing += OnWindowClosing;
            window.Deactivated += OnWindowDeactivated;
            window.Show();
        }

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorPicker view && e.NewValue is Color color)
            {
                view._mainColor.Color = Color.FromArgb(255, color.R, color.G, color.B);
                view.UpdateOpacity();
                view.ColorChanged?.Invoke(d, new(e));
            }
        }
        private static void OnBorderRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorPicker view && e.NewValue is double radius)
            {
                view._clipGeometry.RadiusX = radius;
                view._clipGeometry.RadiusY = radius;
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(Color),
            typeof(ColorPicker), new(Colors.White, OnColorChanged));
        public static readonly DependencyProperty BorderRadiusProperty = DependencyProperty.Register(nameof(BorderRadius), typeof(double),
            typeof(ColorPicker), new(0d, OnBorderRadiusChanged));

        #endregion
    }
}

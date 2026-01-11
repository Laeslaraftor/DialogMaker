using DialogMaker.Lib.Controllers;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using Acly;

namespace DialogMaker.Lib.Elements
{
    public partial class ColorPickerSelectionView : UserControl
    {
        public ColorPickerSelectionView()
        {
            InitializeComponent();

            _channelController = new(_channelSlider)
            {
                Button = MouseButton.Left,
                ClampValue = true
            };
            _opacityController = new(_opacitySlider)
            {
                Button = MouseButton.Left,
                ClampValue = true
            };
            _colorController = new(_colorSlider)
            {
                Button = MouseButton.Left,
                ClampValue = true
            };

            _channelController.ValueChanged += OnSliderControllerValueChanged;
            _opacityController.ValueChanged += OnSliderControllerValueChanged;
            _colorController.ValueChanged += OnSliderControllerValueChanged;
        }

        public event EventHandler<ValueChangedEventArgs<Color>>? ColorChanged;

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        private readonly ViewAsSliderController _channelController;
        private readonly ViewAsSliderController _opacityController;
        private readonly ViewAsSliderController _colorController;
        private bool _skipNext;

        #region Управление

        private void SetColor(Color color, bool updateControls)
        {
            if (updateControls)
            {
                _skipNext = true;
                var opacityValue = _opacityController.Value;
                opacityValue.X = color.ScA;
                _opacityController.Value = opacityValue;

                var channelValue = _channelController.Value;
                channelValue.Y = GetColorPosition(color);
                _channelController.Value = channelValue;

                var colorValue = _colorController.Value;
                colorValue.Y = (color.ScR + color.ScB + color.ScG) / 3;
                colorValue.X = 1 - colorValue.Y;
                _colorController.Value = colorValue;
                _skipNext = false;
            }

            _mainColor.Color = GetColorAtPosition(_channelController.Value.Y);
            _preview.Color = color;
            _hexColor.Text = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);

            color.A = 255;
            _tansparentPreview.Color = color;

            _opacityTransform.X = _opacityController.Value.X * _opacitySlider.RenderSize.Width;
            _channelTransform.Y = _channelController.Value.Y * _channelSlider.RenderSize.Height;
            _colorTransform.X = _colorController.Value.X * _colorSlider.RenderSize.Width;
            _colorTransform.Y = _colorController.Value.Y * _colorSlider.RenderSize.Height;

            Color = color;
        }
        private void UpdateColor()
        {
            var intensity = _colorController.Value;
            var color = GetColorAtPosition(_channelController.Value.Y);

            color = color.Lerp(Colors.White, 1 - (float)intensity.X);
            color = color.Lerp(Colors.Black, (float)intensity.Y);
            color.A = (byte)(_opacityController.Value.X * 255);

            _skipNext = true;
            SetColor(color, false);
            _skipNext = false;
        }

        private Color GetColorAtPosition(double position)
        {
            var stops = _channelsGradient.GradientStops;
            GradientStop? lastStop = null;

            if (position >= 1 && stops.Count > 0)
            {
                return stops[^1].Color;
            }

            foreach (var stop in stops)
            {
                if (stop.Offset > position)
                {
                    if (lastStop != null)
                    {
                        double scale = (stop.Offset - position) / (stop.Offset - lastStop.Offset);
                        return stop.Color.Lerp(lastStop.Color, (float)scale);
                    }

                    break;
                }

                lastStop = stop;
            }

            return Colors.Transparent;
        }
        private double GetColorPosition(Color targetColor)
        {
            var gradientStops = _channelsGradient.GradientStops;

            // Если цвет точно совпадает с одной из остановок
            for (int i = 0; i < gradientStops.Count; i++)
            {
                var stop = gradientStops[i];

                if (stop.Color ==  targetColor)
                {
                    return stop.Offset;
                }
            }

            // Ищем между какими двумя остановками находится цвет
            for (int i = 0; i < gradientStops.Count - 1; i++)
            {
                var color1 = gradientStops[i].Color;
                var color2 = gradientStops[i + 1].Color;

                if (IsColorBetween(targetColor, color1, color2))
                {
                    // Вычисляем вес на основе расстояния между цветами
                    double distanceToColor1 = ColorDistance(targetColor, color1);
                    double distanceToColor2 = ColorDistance(targetColor, color2);
                    double totalDistance = distanceToColor1 + distanceToColor2;

                    // Если цвета очень близки, избегаем деления на ноль
                    if (totalDistance < 0.0001)
                        return gradientStops[i].Offset;

                    // Интерполируем позицию
                    double weight1 = 1 - (distanceToColor1 / totalDistance);
                    double offset1 = gradientStops[i].Offset;
                    double offset2 = gradientStops[i + 1].Offset;

                    return offset1 + (offset2 - offset1) * weight1;
                }
            }

            // Если цвет не найден, возвращаем 0
            return 0;
        }
        private bool IsColorBetween(Color target, Color color1, Color color2)
        {
            // Проверяем, находится ли цвет в RGB-кубе между двумя другими цветами
            return IsComponentBetween(target.R, color1.R, color2.R) &&
                   IsComponentBetween(target.G, color1.G, color2.G) &&
                   IsComponentBetween(target.B, color1.B, color2.B);
        }

        private bool IsComponentBetween(byte target, byte c1, byte c2)
        {
            int min = Math.Min(c1, c2);
            int max = Math.Max(c1, c2);
            return target >= min && target <= max;
        }
        private double ColorDistance(Color color1, Color color2)
        {
            // Евклидово расстояние в RGB пространстве
            double rDiff = color1.R - color2.R;
            double gDiff = color1.G - color2.G;
            double bDiff = color1.B - color2.B;

            return Math.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
        }


        #endregion

        #region События

        private void OnSliderControllerValueChanged(object? sender, ValueChangedEventArgs<Point> e)
        {
            UpdateColor();
        }

        private void OnHexColorConfirmedText(object sender, ValueChangedEventArgs<string> e)
        {
            try
            {
                Color = (Color)ColorConverter.ConvertFromString(e.NewValue);
            }
            catch (Exception error)
            {
                Debug.WriteLine(error);
                _hexColor.Text = e.OldValue;
            }
        }

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorPickerSelectionView view && !view._skipNext)
            {
                view.SetColor((Color)e.NewValue, true);
                view.ColorChanged?.Invoke(d, new(e));
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(Color),
            typeof(ColorPickerSelectionView), new(Colors.White, OnColorChanged));

        #endregion
    }
}

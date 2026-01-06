using Acly;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class PlayerView : UserControl
    {
        public PlayerView()
        {
            InitializeComponent();
            _playButton.Content = Icons.Play;
            _volumeSlider.Value = Volume;
        }

        public event EventHandler<ValueChangedEventArgs<TimeSpan>>? PositionChanged;
        public event EventHandler<ValueChangedEventArgs<double>>? VolumeChanged;
        public event EventHandler<ValueChangedEventArgs<bool>>? IsPlayingChanged;
        public event EventHandler<RoutedEventArgs>? ControlButtonClicked;

        public bool IsPlaying
        {
            get => (bool)GetValue(IsPlayingProperty);
            set => SetValue(IsPlayingProperty, value);
        }
        public TimeSpan Position
        {
            get => (TimeSpan)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }
        public TimeSpan Duration
        {
            get => (TimeSpan)GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }
        public double Volume
        {
            get => (double)GetValue(VolumeProperty);
            set => SetValue(VolumeProperty, value);
        }

        private bool _disablePositionEvent;

        #region Управление

        public void SetPositionWithNoNotify(TimeSpan position)
        {
            _disablePositionEvent = true;
            Position = position;
            _disablePositionEvent = false;
        }

        private void UpdateTime()
        {
            var position = Position;
            string format = TimeFormat;

            if (position.TotalHours > 1)
            {
                format = FullTimeFormat;
            }

            _timeText.Text = @$"{Position.ToString(format)} \ {Duration.ToString(format)}";
        }

        #endregion

        #region События

        private void OnPlayButtonClicked(object sender, RoutedEventArgs e)
        {
            IsPlaying = !IsPlaying;
            ControlButtonClicked?.Invoke(this, e);
        }
        private void OnTimelineValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Position = TimeSpan.FromSeconds(e.NewValue);
        }
        private void OnVolumeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Volume = Helper.Clamp01(e.NewValue);
        }

        private static void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PlayerView view && e.NewValue is bool value)
            {
                view._playButton.Content = value ? Icons.Pause : Icons.Play;
                view.IsPlayingChanged?.Invoke(view, new((bool)e.OldValue, value));
            }
        }
        private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PlayerView view)
            {
                view.UpdateTime();

                view._timeline.Value = ((TimeSpan)e.NewValue).TotalSeconds;

                if (!view._disablePositionEvent)
                {
                    view.PositionChanged?.Invoke(view, new(e));
                }
            }
        }
        private static void OnDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PlayerView view)
            {
                view._timeline.Maximum = ((TimeSpan)e.NewValue).TotalSeconds;
                view.UpdateTime();
            }
        }
        private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PlayerView view)
            {
                view.UpdateTime();

                view._volumeSlider.Value = (double)e.NewValue;
                view.VolumeChanged?.Invoke(view, new(e));
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register(nameof(IsPlaying), typeof(bool),
            typeof(PlayerView), new(false, OnIsPlayingChanged));
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(nameof(Position), typeof(TimeSpan),
            typeof(PlayerView), new(TimeSpan.Zero, OnPositionChanged));
        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(nameof(Duration), typeof(TimeSpan),
            typeof(PlayerView), new(TimeSpan.Zero, OnDurationChanged));
        public static readonly DependencyProperty VolumeProperty = DependencyProperty.Register(nameof(Volume), typeof(double),
            typeof(PlayerView), new(1d, OnVolumeChanged));

        #endregion

        #region Константы

        public const string TimeFormat = @"mm\:ss\.ff";
        public const string FullTimeFormat = @"hh\:mm\:ss\.ff";

        #endregion
    }
}

using Acly.Player;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Timer = System.Timers.Timer;

namespace DialogMaker.Lib.Elements
{
    public partial class MediaControl : UserControl, ISimplePlayer, IDisposable
    {
        public MediaControl()
        {
            InitializeComponent();
            _timer.Elapsed += OnTimerElapsed;
        }
        ~MediaControl()
        {
            Dispatcher.Invoke(Dispose);
        }

        public event EventHandler<ValueChangedEventArgs<TimeSpan>>? PositionChanged;
        public event SimplePlayerStateEvent? StateChanged;
        public event SimplePlayerEvent? SourceChanged;
        public event SimplePlayerEvent? SourceEnded;

        public bool IsDisposed
        {
            get => (bool)GetValue(IsDisposedProperty.DependencyProperty);
            private set => SetValue(IsDisposedProperty, value);
        }
        public SimplePlayerState State
        {
            get => (SimplePlayerState)GetValue(StateProperty.DependencyProperty);
            private set => SetValue(StateProperty, value);
        }
        public TimeSpan Position
        {
            get => (TimeSpan)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }
        public TimeSpan Duration
        {
            get => (TimeSpan)GetValue(DurationProperty.DependencyProperty);
            private set => SetValue(DurationProperty, value);
        }
        public double Speed
        {
            get => (double)GetValue(SpeedProperty);
            set => SetValue(SpeedProperty, value);
        }
        public double Volume
        {
            get => (double)GetValue(VolumeProperty);
            set => SetValue(VolumeProperty, value);
        }
        public bool Loop
        {
            get => (bool)GetValue(LoopProperty);
            set => SetValue(LoopProperty, value);
        }
        public bool AutoPlay
        {
            get => (bool)GetValue(AutoPlayProperty);
            set => SetValue(AutoPlayProperty, value);
        }
        public bool IsPlaying
        {
            get => (bool)GetValue(IsPlayingProperty.DependencyProperty);
            private set => SetValue(IsPlayingProperty, value);
        }
        public bool SourceSetted
        {
            get => (bool)GetValue(SourceSettedProperty.DependencyProperty);
            private set => SetValue(SourceSettedProperty, value);
        }

        float ISimplePlayer.Speed
        {
            get => (float)Speed;
            set => Speed = value;
        }
        float ISimplePlayer.Volume
        {
            get => (float)Volume;
            set => Volume = value;
        }

        private readonly Timer _timer = new()
        {
            Interval = 50,
            AutoReset = true,
        };
        private bool _disablePositionSync;

        #region Установка

        public void SetSource(string source)
        {
            try
            {
                _media.Source = new(source, UriKind.Absolute);
            }
            catch (Exception error)
            {
                error.Alert();
                return;
            }

            SourceSetted = true;
            SourceChanged?.Invoke(this);
        }

        Task ISimplePlayer.SetSource(byte[] Data)
        {
            throw new NotImplementedException();
        }
        Task ISimplePlayer.SetSource(Stream SourceStream)
        {
            throw new NotImplementedException();
        }
        async Task ISimplePlayer.SetSource(string SourceUrl)
        {
            SetSource(SourceUrl);
        }

        public void Close()
        {
            if (!SourceSetted)
            {
                return;
            }

            Stop();

            try
            {
                _media.Close();
            }
            catch (Exception error)
            {
                error.Alert();
            }

            SourceSetted = false;
        }

        #endregion

        #region Управление

        public void Play()
        {
            State = SimplePlayerState.Playing;
            _media.Play();
        }
        public void Pause()
        {
            if (!IsPlaying || !_media.CanPause)
            {
                return;
            }

            State = SimplePlayerState.Paused;
            _media.Pause();
        }
        public void Stop()
        {
            State = SimplePlayerState.Stopped;
            _media.Stop();
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            _timer.Elapsed -= OnTimerElapsed;
            _timer.Enabled = false;
            _timer.Dispose();

            GC.SuppressFinalize(this);
        }

        void ISimplePlayer.Release()
        {
            Dispose();
        }

        #endregion

        #region Дополнительно

        float[] ISimplePlayer.GetSpectrumData(int Size, SpectrumWindow Window)
        {
            throw new NotImplementedException();
        }
        float[] ISimplePlayer.GetSpectrumData(int Size, int SmoothAmount, SpectrumWindow Window)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region События

        private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    OnDispatchedTimerElapsed(sender, e);
                });
            }
            catch (Exception error)
            {
                Debug.WriteLine(error);
            }
        }
        private void OnDispatchedTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            var position = _media.Position;

            _disablePositionSync = true;
            Position = position;
            _disablePositionSync = false;
            Duration = _media.NaturalDuration.ToTimeSpan();

            _controls.SetPositionWithNoNotify(position);
        }
        private void OnControlsPositionChanged(object sender, ValueChangedEventArgs<TimeSpan> e)
        {
            Position = e.NewValue;
        }
        private void OnControlsControlButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!SourceSetted)
            {
                return;
            }
            if (IsPlaying)
            {
                Pause();
                return;
            }

            Play();
        }
        private void OnControlsVolumeChanged(object sender, ValueChangedEventArgs<double> e)
        {
            Volume = e.NewValue;
        }

        private void OnMediaOpened(object sender, RoutedEventArgs e)
        {
            _errorView.Visibility = Visibility.Collapsed;

            if (AutoPlay)
            {
                Play();
            }

            _controls.DarkShadow = _media.HasVideo;
        }
        private void OnMediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            _errorView.Visibility = Visibility.Visible;
            _errorView.Text = $"{e.ErrorException.GetType().Name}: {e.ErrorException.Message}";
        }
        private void OnMediaEnded(object sender, RoutedEventArgs e)
        {
            SourceEnded?.Invoke(this);

            if (Loop)
            {
                Position = TimeSpan.Zero;
                Play();
            }
        }

        private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MediaControl view && e.NewValue is SimplePlayerState state)
            {
                view.StateChanged?.Invoke(view, state);
                bool isPlaying = state == SimplePlayerState.Playing;

                view.IsPlaying = isPlaying;
                view._controls.IsPlaying = isPlaying;
            }
        }
        private static void OnSpeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MediaControl view)
            {
                view._media.SpeedRatio = (double)e.NewValue;
            }
        }
        private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MediaControl view && e.NewValue is double value)
            {
                view._controls.Volume = value;
                view._media.Volume = value;
            }
        }
        private static void OnDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MediaControl view)
            {
                view._controls.Duration = (TimeSpan)e.NewValue;
            }
        }
        private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not MediaControl view)
            {
                return;
            }

            view.PositionChanged?.Invoke(view, new(e));

            if (!view._disablePositionSync)
            {
                view._media.Position = (TimeSpan)e.NewValue;
            }
        }
        private static void OnSourceSettedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MediaControl view)
            {
                view._timer.Enabled = (bool)e.NewValue;
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyPropertyKey IsDisposedProperty = DependencyProperty.RegisterReadOnly(nameof(IsDisposed), typeof(bool),
            typeof(MediaControl), new(false));
        public static readonly DependencyPropertyKey StateProperty = DependencyProperty.RegisterReadOnly(nameof(State), typeof(SimplePlayerState),
            typeof(MediaControl), new(SimplePlayerState.Stopped, OnStateChanged));
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(nameof(Position), typeof(TimeSpan),
            typeof(MediaControl), new(TimeSpan.Zero, OnPositionChanged));
        public static readonly DependencyPropertyKey DurationProperty = DependencyProperty.RegisterReadOnly(nameof(Duration), typeof(TimeSpan),
            typeof(MediaControl), new(TimeSpan.Zero, OnDurationChanged));
        public static readonly DependencyProperty SpeedProperty = DependencyProperty.Register(nameof(Speed), typeof(double),
            typeof(MediaControl), new(1d, OnSpeedChanged));
        public static readonly DependencyProperty VolumeProperty = DependencyProperty.Register(nameof(Volume), typeof(double),
            typeof(MediaControl), new(1d, OnVolumeChanged));
        public static readonly DependencyProperty LoopProperty = DependencyProperty.Register(nameof(Loop), typeof(bool),
            typeof(MediaControl), new(false));
        public static readonly DependencyProperty AutoPlayProperty = DependencyProperty.Register(nameof(AutoPlay), typeof(bool),
            typeof(MediaControl), new(false));
        public static readonly DependencyPropertyKey IsPlayingProperty = DependencyProperty.RegisterReadOnly(nameof(IsPlaying), typeof(bool),
            typeof(MediaControl), new(false));
        public static readonly DependencyPropertyKey SourceSettedProperty = DependencyProperty.RegisterReadOnly(nameof(SourceSetted), typeof(bool),
            typeof(MediaControl), new(false, OnSourceSettedChanged));

        #endregion
    }
}

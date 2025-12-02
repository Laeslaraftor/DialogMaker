using DialogMaker.Lib.Elements;
using System.Drawing.Printing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static System.Net.Mime.MediaTypeNames;

namespace DialogMaker.Lib.Controllers
{
    public class ToolTipController : IDisposable
    {
        public ToolTipController(Window window, Visual container)
        {
            Window = window;
            Container = container;

            if (window.Content is not Panel panel)
            {
                throw new ArgumentException("Окно должно содержать панель!", nameof(window));
            }

            _panel = panel;
        }
        public ToolTipController() : this(MainWindow.Instance, MainWindow.Instance)
        {
        }
        public ToolTipController(Visual container) : this(MainWindow.Instance, container)
        {
        }
        public ToolTipController(Window window) : this(window, window)
        {
        }
        ~ToolTipController()
        {
            Dispose();
        }

        public Window Window { get; }
        public Visual Container { get; }

        private readonly ElementsPool<ToolTipView> _toolTips = new();
        private readonly Dictionary<Token, ToolTipView> _tokens = [];
        private readonly Panel _panel;

        #region Управление

        public async Task<Token> ShowAt(FrameworkElement element)
        {
            return await ShowAt(element, MessageType.Normal);
        }
        public async Task<Token> ShowAt(FrameworkElement element, MessageType type)
        {
            return await ShowAt(element, element.ToolTip?.ToString(), type);
        }
        public async Task<Token> ShowAt(UIElement element, string? text)
        {
            return await ShowAt(element, text, MessageType.Normal);
        }
        public async Task<Token> ShowAt(UIElement element, string? text, MessageType type)
        {
            var toolTip = GetView(text ?? string.Empty, type);

            _panel.Children.Add(toolTip);

            await Task.Delay(50);

            var position = element.GetPosition(Container);
            double offset = toolTip.RenderSize.Height + 5;

            if (0 > position.Y - offset)
            {
                position.Y += element.RenderSize.Height + 5;
            }
            else
            {
                position.Y -= offset;
            }

            Token result = new(toolTip, position);
            _tokens.Add(result, toolTip);

            result.Disposed += OnTokenDisposed;

            result.Show();

            return result;
        }

        public void Dispose()
        {
            List<Token> toolTips = [.. _tokens.Keys];

            foreach (var toolTip in toolTips)
            {
                toolTip.Dispose();
            }

            _toolTips.Dispose();

            GC.SuppressFinalize(this);
        }

        private ToolTipView GetView(string text, MessageType type)
        {
            var result = _toolTips.GetElement();
            result.VerticalAlignment = VerticalAlignment.Top;
            result.HorizontalAlignment = HorizontalAlignment.Left;
            result.Text = text;
            result.MessageType = type;
            result.Opacity = 0;

            Grid.SetColumnSpan(result, 100);
            Grid.SetRowSpan(result, 100);

            return result;
        }
        private void RemoveToolTip(ToolTipView view)
        {
            _panel.Children.Remove(view);
            _toolTips.Free(view);
        }

        #endregion

        #region События

        private void OnTokenDisposed(object? sender, EventArgs e)
        {
            if (sender is not Token token ||
                !_tokens.TryGetValue(token, out var view))
            {
                return;
            }

            token.Disposed -= OnTokenDisposed;
            _tokens.Remove(token);

            RemoveToolTip(view);
        }

        #endregion

        #region Классы

        public class Token : IDisposable
        {
            public Token(ToolTipView view, Point position)
            {
                _view = view;

                if (_view.RenderTransform is not TranslateTransform translation)
                {
                    translation = new();
                    _view.RenderTransform = translation;
                }

                translation.Y = position.Y;

                _translation = translation;
                _hideAnimation = new(position.X, position.X + 10, _animationsDuration, FillBehavior.HoldEnd)
                {
                    EasingFunction = _easing
                };
                _showAnimation = new(_hideAnimation.To.GetValueOrDefault(), position.X, _animationsDuration, FillBehavior.HoldEnd)
                {
                    EasingFunction = _easing
                };
            }

            public event EventHandler? Disposed;

            public string Text
            {
                get => _view.Text;
                set => _view.Text = value;
            }
            public MessageType MessageType
            {
                get => _view.MessageType;
                set => _view.MessageType = value;
            }

            private readonly ToolTipView _view;
            private readonly TranslateTransform _translation;
            private readonly DoubleAnimation _showAnimation;
            private readonly DoubleAnimation _hideAnimation;

            #region Управление

            public async void Show()
            {
                await ShowTask();
            }
            public async void Hide()
            {
                await HideTask();
            }
            public async void HideAndDispose()
            {
                await HideTask();
                Dispose();
            }
            public async Task ShowTask()
            {
                _view.BeginAnimation(UIElement.OpacityProperty, _opacityInAnimation);
                await PlayAnimation(_showAnimation);
            }
            public async Task HideTask()
            {
                _view.BeginAnimation(UIElement.OpacityProperty, _opacityOutAnimation);
                await PlayAnimation(_hideAnimation);
            }

            public void Dispose()
            {
                Disposed?.Invoke(this, EventArgs.Empty);
                GC.SuppressFinalize(this);
            }

            private async Task PlayAnimation(DoubleAnimation animation)
            {
                animation.Completed += OnAnimationCompleted;
                bool isPlaying = true;

                void OnAnimationCompleted(object? sender, EventArgs e)
                {
                    isPlaying = false;
                }

                _translation.BeginAnimation(TranslateTransform.XProperty, animation);

                while (isPlaying)
                {
                    await Task.Delay(50);
                }

                animation.Completed -= OnAnimationCompleted;
            }

            #endregion

            #region Статика

            private static readonly Duration _animationsDuration = TimeSpan.FromSeconds(0.1);
            private static readonly IEasingFunction _easing = new CubicEase()
            {
                EasingMode = EasingMode.EaseOut,
            };
            private static readonly DoubleAnimation _opacityInAnimation = new(0, 1, _animationsDuration, FillBehavior.HoldEnd)
            {
                EasingFunction = _easing
            };
            private static readonly DoubleAnimation _opacityOutAnimation = new(1, 0, _animationsDuration, FillBehavior.HoldEnd)
            {
                EasingFunction = _easing
            };

            #endregion
        }

        #endregion
    }
}

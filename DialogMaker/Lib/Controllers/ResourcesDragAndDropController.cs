using Acly;
using Acly.Numbers;
using DialogMaker.Core;
using DialogMaker.Editor;
using DialogMaker.Lib.Elements;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DialogMaker.Lib.Controllers
{
    public class ResourcesDragAndDropController : IDisposable
    {
        public ResourcesDragAndDropController(Window window)
        {
            if (window.Content is not Panel panel)
            {
                throw new ArgumentException("Дочерним элементом окна должна быть панель!", nameof(window));
            }

            _window = window;
            _content = panel;
            _toolTips = new(window);
            _dragViewTranslation = new();
            _dragViewScale = new()
            {
                ScaleX = 0,
                ScaleY = 0
            };
            _dragView = new()
            {
                RenderTransformOrigin = new(0.5, 0.5),
                RenderTransform = _dragViewScale,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            _dragViewContainer = new()
            {
                RenderTransformOrigin = new(0.5, 0.5),
                RenderTransform = _dragViewTranslation,
                Child = _dragView,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            Grid.SetColumnSpan(_dragViewContainer, 100);
            Grid.SetRowSpan(_dragViewContainer, 100);

            panel.Children.Add(_dragViewContainer);

            window.PreviewMouseDown += OnWindowPreviewMouseDown;
            window.PreviewMouseMove += OnWindowPreviewMouseMove;
            window.PreviewMouseUp += OnWindowPreviewMouseUp;
            _weightAnimation.Tick += OnWeightAnimationTick;
        }
        ~ResourcesDragAndDropController()
        {
            Dispose();
        }

        private ProjectResourceItem? CurrentItem
        {
            get => field;
            set
            {
                if (field == value)
                {
                    return;
                }

                if (field != null && _itemPreview != null)
                {
                    _dragView.Preview = null;
                    field.FreePreview(_itemPreview);
                }

                field = value;

                if (value != null)
                {
                    _itemPreview = value.GetPreview();
                    _dragView.Preview = _itemPreview;
                }
            }
        }
        private bool IsVisible
        {
            get => _dragViewScale.ScaleY != 0 && _dragViewScale.ScaleX != 0;
            set
            {
                if (IsVisible == value)
                {
                    return;
                }

                DoubleAnimation animation = _scaleDownAnimation;

                if (value)
                {
                    animation = _scaleUpAnimation;
                }

                _dragViewScale.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                _dragViewScale.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
            }
        }
        private Point Position
        {
            get => field;
            set
            {
                field = value;
                Point realPosition = value;

                if (EndPointWeight > 0)
                {
                    realPosition = new()
                    {
                        X = Helper.LerpUnclamped((float)value.X, (float)_endPosition.X, (float)EndPointWeight),
                        Y = Helper.LerpUnclamped((float)value.Y, (float)_endPosition.Y, (float)EndPointWeight),
                    };
                }

                _dragViewTranslation.X = realPosition.X;
                _dragViewTranslation.Y = realPosition.Y;
            }
        }
        private double EndPointWeight
        {
            get => field;
            set
            {
                field = Helper.Clamp01(value);
                Position = Position;
            }
        }
        private ReferenceView? EndView
        {
            get => field;
            set
            {
                if (field == value)
                {
                    return;
                }
                if (field != null)
                {
                    HideToolTip(field);
                }

                field = value;
                float weight = 0;

                if (value != null)
                {
                    _endPosition = value.GetPosition(_window);
                    weight = 1;
                    ShowToolTip(value);
                }

                WeightTo(weight);
            }
        }

        private readonly Window _window;
        private readonly Panel _content;
        private readonly Border _dragViewContainer;
        private readonly DragView _dragView;
        private readonly TranslateTransform _dragViewTranslation;
        private readonly ScaleTransform _dragViewScale;
        private readonly ToolTipController _toolTips;
        private readonly Dictionary<ReferenceView, ToolTipController.Token> _viewsToolTips = [];
        private Point _startPosition;
        private Point _endPosition;
        private object? _itemPreview;
        private bool _isHiding;

        #region Управление

        public void Dispose()
        {
            _window.PreviewMouseDown -= OnWindowPreviewMouseDown;
            _window.PreviewMouseMove -= OnWindowPreviewMouseMove;
            _window.PreviewMouseUp -= OnWindowPreviewMouseUp;
            _weightAnimation.Tick -= OnWeightAnimationTick;

            _content.Children.Remove(_dragView);

            GC.SuppressFinalize(this);
        }

        private void WeightTo(double weight)
        {
            _weightAnimation.Stop();

            _weightAnimation.From = (float)EndPointWeight;
            _weightAnimation.To = (float)weight;

            _weightAnimation.Start();
        }

        private async void ShowToolTip(ReferenceView view)
        {
            if (CurrentItem == null || _viewsToolTips.ContainsKey(view))
            {
                return;
            }

            var item = CurrentItem;
            var requestedType = view.RequestedResourceType;
            MessageType type = MessageType.Normal;
            string message = view.Placeholder;

            if (requestedType != null &&
                requestedType != item.ResourceType)
            {
                type = MessageType.Error;
                message = GetErrorMessage(requestedType.Value, item.ResourceType);
            }

            var token = await _toolTips.ShowAt(view, message, type);

            _viewsToolTips.Add(view, token);
        }
        private void HideToolTip(ReferenceView view)
        {
            if (_viewsToolTips.TryGetValue(view, out var token))
            {
                token.HideAndDispose();
                _viewsToolTips.Remove(view);
            }
        }
        private void HideAllToolTips()
        {
            if (_viewsToolTips.Count == 0)
            {
                return;
            }

            List<ReferenceView> views = [.. _viewsToolTips.Keys];

            foreach (var view in views)
            {
                HideToolTip(view);
            }
        }

        private async Task<ProjectResourceItem?> GetProjectItem(MouseEventArgs mouse)
        {
            ProjectResourceItem? item = null;

            await _window.Fetch(mouse, target =>
            {
                if (target is FrameworkElement element &&
                    element.DataContext is ProjectResourceItem projectItem)
                {
                    item = projectItem;
                }
            });

            return item;
        }
        private async Task<ReferenceView?> GetReferenceView(MouseEventArgs mouse)
        {
            ReferenceView? refenceView = null;

            await _window.Fetch(mouse, target =>
            {
                if (target is ReferenceView view)
                {
                    refenceView = view;
                }
            }, callback => false);

            return refenceView;
        }

        #endregion

        #region События

        private async void OnWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isHiding)
            {
                return;
            }

            var position = e.GetPosition(_window);
            var element = await _window.Fetch<Window, TextBox>(position);

            if (e.LeftButton == MouseButtonState.Released ||
                CurrentItem != null ||
                (element != null && element.Visibility == Visibility.Visible))
            {
                return;
            }

            CurrentItem = await GetProjectItem(e);
            _startPosition = position;
        }
        private async void OnWindowPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (CurrentItem == null || _isHiding)
            {
                return;
            }

            var position = e.GetPosition(_window);

            if (!IsVisible)
            {
                if (10 > _startPosition.Distance(position))
                {
                    return;
                }

                IsVisible = true;
            }

            Position = position - (_dragView.RenderSize / 2);

            EndView = await GetReferenceView(e);
        }
        private async void OnWindowPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed ||
                CurrentItem == null)
            {
                return;
            }
            if (!IsVisible)
            {
                CurrentItem = null;
                EndView = null;
                return;
            }
            if (EndView != null &&
                (EndView.RequestedResourceType == null ||
                EndView.RequestedResourceType == CurrentItem.ResourceType))
            {
                EndView.Item = CurrentItem;
            }

            IsVisible = false;
            EndView = null;
            _isHiding = true;

            HideAllToolTips();

            await Task.Delay(_animationsDuration);

            CurrentItem = null;
            _isHiding = false;
        }

        private async void OnWeightAnimationTick(ValueAnimation animation, float value)
        {
            await _window.Dispatcher.InvokeAsync(() =>
            {
                EndPointWeight = value;
            });
        }

        #endregion

        #region Статика

        private static readonly TimeSpan _animationsDuration = TimeSpan.FromSeconds(0.1);
        private static readonly CubicEase _easing = new()
        {
            EasingMode = EasingMode.EaseOut
        };
        private static readonly DoubleAnimation _scaleUpAnimation = new(0, 1, _animationsDuration, FillBehavior.HoldEnd)
        {
            EasingFunction = _easing
        };
        private static readonly DoubleAnimation _scaleDownAnimation = new(1, 0, _animationsDuration, FillBehavior.HoldEnd)
        {
            EasingFunction = _easing
        };
        private static readonly ValueAnimation _weightAnimation = new(0, 1)
        {
            Easing = Easing.EaseOutCubic,
            Duration = TimeSpan.FromSeconds(0.1)
        };

        private static string GetErrorMessage(DialogResourceType requestedType, DialogResourceType givenType)
        {
            return $"Требуется {requestedType}";
        }

        #endregion
    }
}

using DialogMaker.Core;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace DialogMaker.Lib.Controllers
{
    public class DragAndDropController : MouseController
    {
        public DragAndDropController(MouseController parentMouseController, Panel elementsContainer)
            : base(parentMouseController)
        {
            ElementsContainer = elementsContainer;
        }

        public event EventHandler<DragCheckEventArgs>? DragCheck;
        public event EventHandler<DragEventArgs<List<FrameworkElement>>>? DragBeginning;
        public event EventHandler<DragEventArgs<List<FrameworkElement>>>? DragUpdated;
        public event EventHandler<DragEventArgs<List<FrameworkElement>>>? DragEnded;
        public event EventHandler<DragRejectedEventArgs>? DragRejected;

        public Panel ElementsContainer { get; }

        private readonly List<FrameworkElement> _draggedElements = [];
        private Action<Point>? _draggedElementTransform;
        private Point _dragOffset;
        private MouseButton _dragMouseButton;

        #region Управление

        private async Task<Dictionary<MouseButton, DragHitTest>> HitTest(MouseEventArgs mouse)
        {
            Dictionary<MouseButton, DragHitTest> result = [];
            bool shouldRemoveLeftButton = false;
            int skipCount = 0;
            bool isMouseMoveControlHit = false;

            await ElementsContainer.Fetch(mouse, target =>
            {
                if (target is not FrameworkElement element)
                {
                    return;
                }
                if (isMouseMoveControlHit || IsMouseMoveControl(target))
                {
                    isMouseMoveControlHit = true;
                    return;
                }

                DragCheckEventArgs check = new(target, mouse);

                DragCheck?.Invoke(this, check);

                if (check.PotentialDragObject != target &&
                    check.PotentialDragObject is FrameworkElement newElement)
                {
                    element = newElement;
                }
                if ((element is TextBox ||
                    element is RichTextBox ||
                    element is ScrollBar) &&
                    check.DragMouseButton == MouseButton.Left)
                {
                    shouldRemoveLeftButton = true;
                    return;
                }
                if (!check.Ignore)
                {
                    result.ForceAdd(check.DragMouseButton, new(element, check.DragMouseButton));
                }
            }, callback =>
            {
                if (skipCount > 1 || isMouseMoveControlHit)
                {
                    return true;
                }

                skipCount++;

                return false;
            });

            if (isMouseMoveControlHit)
            {
                result.Clear();
                return result;
            }
            if (shouldRemoveLeftButton)
            {
                result.Remove(MouseButton.Left);
            }

            return result;
        }

        private async Task<bool> StartDrag(MouseEventArgs mouse)
        {
            if (_draggedElements.Count > 0)
            {
                StopDrag(mouse);
            }

            Point position = mouse.GetPosition(ElementsContainer);
            var dragHitTests = await HitTest(mouse);
            DragHitTest dragHitTest = new();

            foreach (var info in dragHitTests)
            {
                if (mouse.IsPressed(info.Key))
                {
                    dragHitTest = info.Value;
                    break;
                }
            }

            var uiElement = dragHitTest.Element;

            if (dragHitTest.IsEmpty ||
                uiElement == null)
            {
                return false;
            }

            _dragOffset = position;
            var scale = uiElement.GetVisualTreeScale();
            _draggedElements.Clear();
            _draggedElements.Add(uiElement);

            if (uiElement is ISelectable selectable && selectable.IsSelected)
            {
                foreach (var otherSelectable in selectable.GetOtherSelectables())
                {
                    if (otherSelectable.View != null)
                    {
                        _draggedElements.Add(otherSelectable.View);
                    }
                }
            }

            if (uiElement.Parent is Canvas)
            {
                SetTopZIndex(uiElement);

                _draggedElementTransform = p =>
                {
                    Point newMousePosition = (Point)(p - _dragOffset);
                    newMousePosition /= scale;

                    foreach (var element in _draggedElements)
                    {
                        var currentPosition = Canvas.GetElementPosition(element);
                        currentPosition += newMousePosition;

                        Canvas.SetLeft(element, currentPosition.X);
                        Canvas.SetTop(element, currentPosition.Y);
                    }

                    _dragOffset = p;
                };
            }
            else
            {
                List<TranslateTransform> translations = new(_draggedElements.Count);

                foreach (var element in _draggedElements)
                {
                    var translation = uiElement.GetTransform<TranslateTransform>();
                    translations.Add(translation);
                }

                _draggedElementTransform = p =>
                {
                    var delta = (Point)(p - _dragOffset);
                    delta /= scale;

                    foreach (var translation in translations)
                    {
                        translation.X += delta.X;
                        translation.Y += delta.Y;
                    }

                    _dragOffset = p;
                };
            }

            _dragMouseButton = dragHitTest.DragButton;

            DragBeginning?.Invoke(this, new(_draggedElements, mouse));

            return true;
        }
        private void StopDrag(MouseEventArgs mouse)
        {
            if (_draggedElements.Count == 0 ||
                mouse.IsPressed(_dragMouseButton))
            {
                return;
            }

            DragEnded?.Invoke(this, new(_draggedElements, mouse));

            _draggedElementTransform = null;
            _draggedElements.Clear();
        }

        private void SetTopZIndex(FrameworkElement element)
        {
            if (element.Parent is not Panel panel)
            {
                return;
            }

            int minIndex = int.MaxValue;

            foreach (UIElement child in panel.Children)
            {
                int zIndex = Panel.GetZIndex(child);

                if (zIndex >= 0)
                {
                    minIndex = Math.Min(zIndex, minIndex);
                }
            }
            foreach (UIElement child in panel.Children)
            {
                if (child is ISelectable)
                {
                    Panel.SetZIndex(child, minIndex);
                }
            }

            Panel.SetZIndex(element, minIndex + 1);
        }

        #endregion

        #region События

        protected override async void OnMouseDown(object? sender, MouseButtonEventArgs e)
        {
            if (e.Handled)
            {
                base.OnMouseDown(sender, e);
                DragRejected?.Invoke(this, new(e, DragRejectReason.AlreadyHandled));
                return;
            }

            var isStarted = await StartDrag(e);
            e.Handled = isStarted || e.Handled;

            base.OnMouseDown(sender, e);

            e.Handled = false;

            if (!isStarted)
            {
                DragRejected?.Invoke(this, new(e, DragRejectReason.ElementNotFound));
            }
        }
        protected override void OnMouseMove(object? sender, MouseEventArgs e)
        {
            base.OnMouseMove(sender, e);

            if (_draggedElementTransform == null ||
                _draggedElements.Count == 0 ||
                !e.IsPressed(_dragMouseButton))
            {
                return;
            }

            var position = e.GetPosition(ElementsContainer);
            _draggedElementTransform?.Invoke(position);

            DragUpdated?.Invoke(this, new(_draggedElements, e));
        }
        protected override void OnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            base.OnMouseUp(sender, e);
            StopDrag(e);
        }
        protected override void OnMouseLeave(object? sender, MouseEventArgs e)
        {
            base.OnMouseLeave(sender, e);
            StopDrag(e);
        }

        #endregion

        #region Классы

        private readonly struct DragHitTest
        {
            public DragHitTest(FrameworkElement element, MouseButton button)
            {
                IsEmpty = false;
                Element = element;
                DragButton = button;
            }

            public bool IsEmpty { get; } = true;
            public FrameworkElement Element { get; }
            public MouseButton DragButton { get; }
        }

        #endregion
    }
}

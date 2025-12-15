using DialogMaker.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace DialogMaker.Lib.Controllers
{
    public class DragAndDropController : Disposable
    {
        public DragAndDropController(Panel elementsContainer)
        {
            ElementsContainer = elementsContainer;
            elementsContainer.PreviewMouseDown += OnElementsContainerPreviewMouseDown;
            elementsContainer.PreviewMouseMove += OnElementsContainerPreviewMouseMove;
            elementsContainer.PreviewMouseUp += OnElementsContainerPreviewMouseUp;
            elementsContainer.MouseLeave += OnElementsContainerMouseLeave;
        }

        public event EventHandler<DragCheckEventArgs>? DragCheck;
        public event EventHandler<DragEventArgs>? DragBeginning;
        public event EventHandler<DragEventArgs>? DragUpdated;
        public event EventHandler<DragEventArgs>? DragEnded;
        public event EventHandler<MouseButtonEventArgs>? MouseDown;
        public event EventHandler<MouseEventArgs>? MouseMove;
        public event EventHandler<MouseButtonEventArgs>? MouseUp;

        public Panel ElementsContainer { get; }

        private UIElement? _draggedElement;
        private Action<Point>? _draggedElementTransform;
        private Point _dragOffset;
        private MouseButton _dragMouseButton;

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            ElementsContainer.PreviewMouseDown -= OnElementsContainerPreviewMouseDown;
            ElementsContainer.PreviewMouseMove -= OnElementsContainerPreviewMouseMove;
            ElementsContainer.PreviewMouseUp -= OnElementsContainerPreviewMouseUp;
            ElementsContainer.MouseLeave -= OnElementsContainerMouseLeave;
        }

        private async Task<Dictionary<MouseButton, DragHitTest>> HitTest(MouseEventArgs mouse)
        {
            Dictionary<MouseButton, DragHitTest> result = [];
            bool shouldRemoveLeftButton = false;

            await ElementsContainer.Fetch(mouse, target =>
            {
                if (target is not FrameworkElement element)
                {
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
            });

            if (shouldRemoveLeftButton)
            {
                result.Remove(MouseButton.Left);
            }

            return result;
        }

        private async Task<bool> StartDrag(MouseEventArgs mouse)
        {
            if (_draggedElement != null)
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

            if (uiElement.Parent is Canvas)
            {
                _draggedElementTransform = p =>
                {
                    var currentPosition = Canvas.GetElementPosition(uiElement);
                    Point newPosition = (Point)(p - _dragOffset);
                    newPosition /= scale;
                    newPosition += currentPosition;

                    Canvas.SetLeft(uiElement, newPosition.X);
                    Canvas.SetTop(uiElement, newPosition.Y);
                    _dragOffset = p;
                };
            }
            else
            {
                var translation = uiElement.GetTransform<TranslateTransform>();

                _draggedElementTransform = p =>
                {
                    var delta = (Point)(p - _dragOffset);
                    delta /= scale;

                    translation.X += delta.X;
                    translation.Y += delta.Y;

                    _dragOffset = p;
                };
            }

            _draggedElement = uiElement;
            _dragMouseButton = dragHitTest.DragButton;

            DragBeginning?.Invoke(this, new(uiElement, mouse));

            return true;
        }
        private void StopDrag(MouseEventArgs mouse)
        {
            if (_draggedElement == null ||
                mouse.IsPressed(_dragMouseButton))
            {
                return;
            }

            DragEnded?.Invoke(this, new(_draggedElement, mouse));

            _draggedElementTransform = null;
            _draggedElement = null;
        }

        #endregion

        #region События

        private async void OnElementsContainerPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseDown?.Invoke(this, e);

            await Task.Delay(25);

            if (e.Handled)
            {
                return;
            }

            var isStarted = await StartDrag(e);

            if (isStarted)
            {
                e.Handled = true;
            }
        }
        private void OnElementsContainerPreviewMouseMove(object sender, MouseEventArgs e)
        {
            MouseMove?.Invoke(this, e);

            if (_draggedElementTransform == null ||
                _draggedElement == null ||
                !e.IsPressed(_dragMouseButton))
            {
                return;
            }

            var position = e.GetPosition(ElementsContainer);
            _draggedElementTransform?.Invoke(position);

            DragUpdated?.Invoke(this, new(_draggedElement, e));
        }
        private void OnElementsContainerPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseUp?.Invoke(this, e);
            StopDrag(e);
        }
        private void OnElementsContainerMouseLeave(object sender, MouseEventArgs e)
        {
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

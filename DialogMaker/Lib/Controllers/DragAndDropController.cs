using DialogMaker.Editor;
using DialogMaker.ViewModels;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DialogMaker.Lib.Controllers
{
    public class DragAndDropController : INotifyPropertyChanged, IDisposable
    {
        public DragAndDropController(UIElement elementsContainer)
        {
            ElementsContainer = elementsContainer;
            elementsContainer.PreviewMouseDown += OnElementsContainerPreviewMouseDown;
            elementsContainer.PreviewMouseMove += OnElementsContainerPreviewMouseMove;
            elementsContainer.PreviewMouseUp += OnElementsContainerPreviewMouseUp;
            elementsContainer.MouseLeave += OnElementsContainerMouseLeave;
        }
        ~DragAndDropController()
        {
            Dispose();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<DragCheckEventArgs>? DragCheck;
        public event EventHandler<DragEventArgs>? DragBeginning;
        public event EventHandler<DragEventArgs>? DragUpdated;
        public event EventHandler<DragEventArgs>? DragEnded;

        public bool IsDisposed
        {
            get => _isDisposed;
            private set
            {
                if (_isDisposed != value)
                {
                    _isDisposed = value;
                    InvokePropertyChanged(nameof(IsDisposed));
                }
            }
        }
        public UIElement ElementsContainer { get; }

        private bool _isDisposed;
        private UIElement? _draggedElement;
        private Action<Vector>? _draggedElementTransform;
        private Point _dragOffset;

        #region Управление

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;

            ElementsContainer.PreviewMouseDown -= OnElementsContainerPreviewMouseDown;
            ElementsContainer.PreviewMouseMove -= OnElementsContainerPreviewMouseMove;
            ElementsContainer.PreviewMouseUp -= OnElementsContainerPreviewMouseUp;
            ElementsContainer.MouseLeave -= OnElementsContainerMouseLeave;
        }

        private async Task<FrameworkElement?> HitTest(MouseEventArgs mouse)
        {
            FrameworkElement? item = null;

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
                if (!check.Ignore)
                {
                    item = element;
                }
            });

            return item;
        }

        private async void StartDrag(MouseEventArgs mouse)
        {
            if (_draggedElement != null)
            {
                StopDrag(mouse);
            }

            Point position = mouse.GetPosition(ElementsContainer);
            FrameworkElement? uiElement = await HitTest(mouse);

            if (uiElement == null)
            {
                return;
            }

            _dragOffset = position;

            if (uiElement.Parent is Canvas)
            {
                double x = Canvas.GetLeft(uiElement);
                double y = Canvas.GetTop(uiElement);

                _dragOffset.X -= double.IsNaN(x) ? 0 : x;
                _dragOffset.Y -= double.IsNaN(y) ? 0 : y;
                _draggedElementTransform = p =>
                {
                    Canvas.SetLeft(uiElement, p.X);
                    Canvas.SetTop(uiElement, p.Y);
                };
            }
            else
            {
                if (uiElement.RenderTransform is not TranslateTransform translation)
                {
                    translation = new();
                    uiElement.RenderTransform = translation;
                }

                _dragOffset.X -= translation.X;
                _dragOffset.Y -= translation.Y;
                _draggedElementTransform = p =>
                {
                    translation.X = p.X;
                    translation.Y = p.Y;
                };
            }


            _draggedElement = uiElement;

            DragBeginning?.Invoke(this, new(uiElement, mouse));
        }
        private void StopDrag(MouseEventArgs mouse)
        {
            if (_draggedElement == null)
            {
                return;
            }

            DragEnded?.Invoke(this, new(_draggedElement, mouse));

            _draggedElementTransform = null;
            _draggedElement = null;
        }

        #endregion

        #region События

        private void InvokePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }

        private async void OnElementsContainerPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            await Task.Delay(50);

            if (!e.Handled)
            {
                StartDrag(e);
            }
        }
        private void OnElementsContainerPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_draggedElementTransform == null || _draggedElement == null)
            {
                return;
            }

            var position = e.GetPosition(ElementsContainer);
            _draggedElementTransform?.Invoke(position - _dragOffset);

            DragUpdated?.Invoke(this, new(_draggedElement, e));
        }
        private void OnElementsContainerPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            StopDrag(e);
        }
        private void OnElementsContainerMouseLeave(object sender, MouseEventArgs e)
        {
            StopDrag(e);
        }

        #endregion
    }
}

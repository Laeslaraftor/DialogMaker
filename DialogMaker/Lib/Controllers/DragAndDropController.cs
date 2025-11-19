using System.ComponentModel;
using System.Threading.Tasks;
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
        private TranslateTransform? _draggedElementTransform;
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

        private async Task<HitTestResult?> HitTest(MouseEventArgs mouse)
        {
            Point position = mouse.GetPosition(ElementsContainer);
            HitTestResult? testResult = null;

            HitTestFilterBehavior Filter(DependencyObject potentialHitTestTarget)
            {
                DragCheckEventArgs check = new(potentialHitTestTarget, mouse);

                DragCheck?.Invoke(this, check);

                if (check.Ignore)
                {
                    return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
                }

                return HitTestFilterBehavior.Continue;
            }
            HitTestResultBehavior Callback(HitTestResult result)
            {
                testResult = result;
                return HitTestResultBehavior.Stop;
            }

            VisualTreeHelper.HitTest(ElementsContainer, Filter, Callback, new PointHitTestParameters(position));

            while (testResult == null)
            {
                await Task.Delay(50);
            }

            return testResult;
        }

        private async void StartDrag(MouseEventArgs mouse)
        {
            if (_draggedElement != null)
            {
                StopDrag(mouse);
            }

            Point position = mouse.GetPosition(ElementsContainer);
            HitTestResult? hitTestResult = await HitTest(mouse);

            if (hitTestResult == null)
            {
                return;
            }

            if (hitTestResult.VisualHit is not UIElement uiElement)
            {
                return;
            }
            if (uiElement.RenderTransform is not TranslateTransform translation)
            {
                translation = new();
                uiElement.RenderTransform = translation;
            }

            _dragOffset = position;
            _dragOffset.X -= translation.X;
            _dragOffset.Y -= translation.Y;
            _draggedElementTransform = translation;
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

        private void OnElementsContainerPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            StartDrag(e);
        }
        private void OnElementsContainerPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_draggedElementTransform == null || _draggedElement == null)
            {
                return;
            }

            var position = e.GetPosition(ElementsContainer);
            _draggedElementTransform.X = position.X - _dragOffset.X;
            _draggedElementTransform.Y = position.Y - _dragOffset.Y;

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

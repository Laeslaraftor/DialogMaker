using DialogMaker.Core;
using DialogMaker.Lib.Elements;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Threading.Tasks;

namespace DialogMaker.Lib.Controllers
{
    public class MouseMultiselectController : Disposable
    {
        public MouseMultiselectController(DragAndDropController dragAndDrop)
        {
            DragAndDrop = dragAndDrop;

            dragAndDrop.ElementsContainer.Children.Add(_selection);

            Panel.SetZIndex(_selection, 1000);

            dragAndDrop.MouseDown += OnContainerPreviewMouseDown;
            dragAndDrop.MouseMove += OnContainerPreviewMouseMove;
            dragAndDrop.MouseUp += OnContainerPreviewMouseUp;
            dragAndDrop.DragBeginning += OnDragAndDropDragBeginning;
        }

        public event EventHandler? EmptyClick;
        public event EventHandler<SelectionEventArgs<ISelectable>>? Selected;

        public DragAndDropController DragAndDrop { get; }
        public Panel Container => DragAndDrop.ElementsContainer;
        public MouseButton MouseButton
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(MouseButton));
                    field = value;
                    InvokePropertyChanged(nameof(MouseButton));
                }
            }
        }

        private SelectionMode Mode => Keyboard.IsKeyDown(Key.LeftShift) ? SelectionMode.Multiple : SelectionMode.Single;

        private readonly SelectionBox _selection = new()
        {
            Visibility = Visibility.Collapsed,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };
        private bool _isSelecting;

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            DragAndDrop.ElementsContainer.Children.Add(_selection);

            DragAndDrop.MouseDown -= OnContainerPreviewMouseDown;
            DragAndDrop.MouseMove -= OnContainerPreviewMouseMove;
            DragAndDrop.MouseUp -= OnContainerPreviewMouseUp;
            DragAndDrop.DragBeginning -= OnDragAndDropDragBeginning;
        }

        private async void SelectPoint(MouseButtonEventArgs mouse, Point position)
        {
            var mode = Mode;
            ISelectable? selectable = null;
            int skipCount = 0;

            await Container.Fetch(position, obj =>
            {
                if (obj is ISelectable selectableObject)
                {
                    selectable = selectableObject;
                }
            }, callback =>
            {
                if (skipCount > 1 || selectable != null)
                {
                    return true;
                }

                skipCount++;

                return false;
            });

            if (selectable != null)
            {
                Selected?.Invoke(this, new(mode, selectable));
                return;
            }

            EmptyClick?.Invoke(this, EventArgs.Empty);
        }

        private void StopSelecting()
        {
            _isSelecting = false;
            _selection.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region События

        private void OnContainerPreviewMouseDown(object? sender, MouseButtonEventArgs e)
        {
            if (!e.IsPressed(MouseButton))
            {
                return;
            }

            var position = e.GetPosition(Container);
            SelectPoint(e, position);

            if (_isSelecting)
            {
                return;
            }

            _isSelecting = true;
            _selection.Visibility = Visibility.Visible;
            _selection.StartPoint = position;
            _selection.EndPoint = position;
        }
        private void OnContainerPreviewMouseMove(object? sender, MouseEventArgs e)
        {
            if (_isSelecting)
            {
                _selection.EndPoint = e.GetPosition(Container);
            }
        }
        private void OnContainerPreviewMouseUp(object? sender, MouseButtonEventArgs e)
        {
            if (!_isSelecting || e.IsPressed(MouseButton))
            {
                return;
            }

            StopSelecting();
        }
        private void OnDragAndDropDragBeginning(object? sender, DragEventArgs e)
        {
            StopSelecting();
        }

        #endregion
    }
}

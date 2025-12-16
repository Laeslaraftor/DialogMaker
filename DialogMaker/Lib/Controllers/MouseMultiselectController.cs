using DialogMaker.Lib.Elements;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace DialogMaker.Lib.Controllers
{
    public class MouseMultiselectController : MouseController
    {
        public MouseMultiselectController(DragAndDropController dragAndDrop)
            : base(dragAndDrop)
        {
            DragAndDrop = dragAndDrop;

            dragAndDrop.ElementsContainer.Children.Add(_selection);

            Panel.SetZIndex(_selection, 1000);
        }

        public event EventHandler? EmptyClick;
        public event EventHandler<SelectionEventArgs<ISelectable>>? Selected;

        public DragAndDropController DragAndDrop { get; }
        public Panel Container => DragAndDrop.ElementsContainer;
        public MouseButton MultiselectMouseButton
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(MultiselectMouseButton));
                    field = value;
                    InvokePropertyChanged(nameof(MultiselectMouseButton));
                }
            }
        }
        public MouseButton ExtraMouseButton
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(ExtraMouseButton));
                    field = value;
                    InvokePropertyChanged(nameof(ExtraMouseButton));
                }
            }
        }
        public int SelectionDepth
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(SelectionDepth));
                    field = value;
                    InvokePropertyChanged(nameof(SelectionDepth));
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
        private ISelectable? _lastForceSelectable;

        #region Управление

        protected override bool ValidateClickedElement(UIElement element)
        {
            return element is ISelectable;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            DragAndDrop.ElementsContainer.Children.Remove(_selection);
        }

        private async void SelectPoint(Point position, bool force = false)
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
                if (force)
                {
                    _lastForceSelectable = selectable;

                    if (selectable.IsSelected)
                    {
                        return;
                    }

                    mode = SelectionMode.Single;
                }

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
        private void CheckSelectionZone()
        {
            Rect zoneRect = new(_selection.StartPoint, _selection.EndPoint);
            var mainContainer = Container;

            void Check(Panel panel, int depth)
            {
                if (depth > SelectionDepth)
                {
                    return;
                }

                foreach (var element in panel.Children)
                {
                    if (element is Panel childPanel)
                    {
                        Check(childPanel, depth + 1);
                        continue;
                    }
                    if (element is not ISelectable selectable)
                    {
                        continue;
                    }

                    var selectableRect = selectable.GetViewRect(mainContainer);
                    selectable.IsSelected = zoneRect.IntersectsWith(selectableRect);
                }
            }

            Check(mainContainer, 0);
        }

        #endregion

        #region События

        protected override void OnMouseClick(object? sender, MouseClickEventArgs e)
        {
            base.OnMouseClick(sender, e);

            if (e.Element is ISelectable selectable)
            {
                if (selectable.IsSelected && _lastForceSelectable == selectable)
                {
                    _lastForceSelectable = null;
                    return;
                }

                Selected?.Invoke(this, new(Mode, selectable));
                return;
            }

            EmptyClick?.Invoke(this, EventArgs.Empty);
        }
        protected override void OnMouseDown(object? sender, MouseButtonEventArgs e)
        {
            base.OnMouseDown(sender, e);

            var position = e.GetPosition(Container);
            bool multiselectButtonPressed = e.IsPressed(MultiselectMouseButton);
            bool extraButtonPressed = e.IsPressed(ExtraMouseButton);


            if ((!e.Handled && multiselectButtonPressed) ||
                extraButtonPressed)
            {
                SelectPoint(position);
            }
            if (!multiselectButtonPressed || _isSelecting || e.Handled)
            {
                return;
            }

            _isSelecting = true;
            _selection.Visibility = Visibility.Visible;
            _selection.StartPoint = position;
            _selection.EndPoint = position;
        }
        protected override void OnMouseMove(object? sender, MouseEventArgs e)
        {
            base.OnMouseMove(sender, e);

            if (!e.IsPressed(MultiselectMouseButton))
            {
                return;
            }

            var position = e.GetPosition(Container);
            SelectPoint(position, true);

            if (!_isSelecting)
            {
                return;
            }

            _selection.EndPoint = position;
            CheckSelectionZone();
        }
        protected override void OnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            base.OnMouseUp(sender, e);

            if (!_isSelecting || e.IsPressed(MultiselectMouseButton))
            {
                return;
            }

            StopSelecting();
        }
        protected override void OnMouseLeave(object? sender, MouseEventArgs e)
        {
            base.OnMouseLeave(sender, e);
            StopSelecting();
        }

        #endregion
    }
}

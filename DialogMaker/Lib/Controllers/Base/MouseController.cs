using DialogMaker.Core;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DialogMaker.Lib.Controllers
{
    public class MouseController : Disposable
    {
        public MouseController()
        {
        }
        public MouseController(MouseController parent)
        {
            MouseHandler = parent;

            parent.MouseDown += OnMouseDown;
            parent.MouseMove += OnMouseMove;
            parent.MouseUp += OnMouseUp;
            parent.MouseLeave += OnMouseLeave;
            parent.MouseClick += OnMouseClick;
            parent.ValidateClickElement += OnValidateClickElement;
        }
        public MouseController(FrameworkElement element)
        {
            MouseHandler = element;

            element.PreviewMouseDown += OnMouseDown;
            element.PreviewMouseMove += OnMouseMove;
            element.PreviewMouseUp += OnMouseUp;
            element.MouseLeave += OnMouseLeave;
        }

        public event EventHandler<MouseButtonEventArgs>? MouseDown;
        public event EventHandler<MouseEventArgs>? MouseMove;
        public event EventHandler<MouseButtonEventArgs>? MouseUp;
        public event EventHandler<MouseClickEventArgs>? MouseClick;
        public event EventHandler<MouseEventArgs>? MouseLeave;
        public event EventHandler<ValidateEventArgs<UIElement>>? ValidateClickElement;

        public object? MouseHandler { get; }

        protected virtual int ClickFetchSkipsCount { get; } = 0;

        private UIElement? _lastMouseDownElement;

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            _lastMouseDownElement = null;

            if (MouseHandler is MouseController controller)
            {
                controller.MouseDown -= OnMouseDown;
                controller.MouseMove -= OnMouseMove;
                controller.MouseUp -= OnMouseUp;
                controller.MouseClick -= OnMouseClick;
                controller.MouseLeave -= OnMouseLeave;
                controller.ValidateClickElement -= OnValidateClickElement;
            }
            else if (MouseHandler is FrameworkElement element)
            {
                element.PreviewMouseDown -= OnMouseDown;
                element.PreviewMouseMove -= OnMouseMove;
                element.PreviewMouseUp -= OnMouseUp;
                element.MouseLeave -= OnMouseLeave;
            }
        }

        protected virtual bool ValidateClickedElement(UIElement element)
        {
            ValidateEventArgs<UIElement> validate = new(element)
            {
                IsValid = true
            };

            OnValidateClickElement(this, validate);

            return validate.IsValid;
        }
        protected bool IsMouseMoveControl(DependencyObject obj)
        {
            return obj is Track ||
                   obj is ScrollBar ||
                   obj is Slider ||
                   obj is TextBox ||
                   obj is RichTextBox ||
                   obj is Button;
        }

        private async Task<UIElement?> Fetch(MouseEventArgs mouse)
        {
            if (MouseHandler is not UIElement container)
            {
                return null;
            }

            UIElement? result = null;
            int skipCount = 0;
            int maxSkipCount = ClickFetchSkipsCount;

            await container.Fetch(mouse, obj =>
            {
                if (obj is UIElement element &&
                    ValidateClickedElement(element))
                {
                    result = element;
                }
            }, callback =>
            {
                if (result != null || 
                    maxSkipCount >= skipCount)
                {
                    return true;
                }

                skipCount++;

                return false;
            });

            return result;
        }

        #endregion

        #region События

        protected virtual async void OnMouseDown(object? sender, MouseButtonEventArgs e)
        {
            MouseDown?.Invoke(this, e);
            _lastMouseDownElement = await Fetch(e);            
        }
        protected virtual void OnMouseMove(object? sender, MouseEventArgs e)
        {
            MouseMove?.Invoke(this, e);
        }
        protected virtual async void OnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            MouseUp?.Invoke(this, e);

            if (_lastMouseDownElement == null)
            {
                return;
            }

            var pressedElement = _lastMouseDownElement;
            _lastMouseDownElement = null;
            var releasedElement = await Fetch(e);

            if (pressedElement == releasedElement)
            {
                OnMouseClick(sender, new(e, pressedElement));
            }
        }
        protected virtual void OnMouseClick(object? sender, MouseClickEventArgs e)
        {
            MouseClick?.Invoke(this, e);
        }
        protected virtual void OnMouseLeave(object? sender, MouseEventArgs e)
        {
            MouseLeave?.Invoke(this, e);
        }
        private void OnValidateClickElement(object? sender, ValidateEventArgs<UIElement> e)
        {
            if (sender != this)
            {
                e.IsValid = e.IsValid && ValidateClickedElement(e.Item);
            }

            ValidateClickElement?.Invoke(this, e);
        }

        #endregion
    }
}

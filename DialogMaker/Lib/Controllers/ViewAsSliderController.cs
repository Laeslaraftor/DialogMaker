using Acly;
using DialogMaker.Core;
using System.Windows;
using System.Windows.Input;

namespace DialogMaker.Lib.Controllers
{
    public class ViewAsSliderController : Disposable
    {
        public ViewAsSliderController(UIElement element)
        {
            Element = element;
            element.PreviewMouseDown += OnElementPreviewMouseDown;
        }

        public event EventHandler<ValueChangedEventArgs<Point>>? ValueChanged;

        public UIElement Element { get; }
        public Point Value
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Value));
                    var oldValue = field;
                    field = value;

                    ValueChanged?.Invoke(this, new(oldValue, value));
                    InvokePropertyChanged(nameof(value));
                }
            }
        }
        public MouseButton Button
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Button));
                    field = value;
                    InvokePropertyChanged(nameof(Button));
                }
            }
        }
        public bool ClampValue
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(ClampValue));
                    field = value;
                    InvokePropertyChanged(nameof(ClampValue));
                }
            }
        }

        private bool _eventAdded;

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Element.PreviewMouseDown -= OnElementPreviewMouseDown;
            RemoveHandleEvents();
        }

        #endregion

        #region События

        private void AddHandleEvents()
        {
            if (_eventAdded)
            {
                return;
            }

            _eventAdded = true;
            Element.PreviewMouseMove += OnElementPreviewMouseMove;
            Element.PreviewMouseUp += OnElementPreviewMouseUp;
        }
        private void RemoveHandleEvents()
        {
            Element.PreviewMouseMove -= OnElementPreviewMouseMove;
            Element.PreviewMouseUp -= OnElementPreviewMouseUp;
            _eventAdded = false;
        }

        private void OnElementPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.IsPressed(Button))
            {
                AddHandleEvents();
            }
        }
        private void OnElementPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!e.IsPressed(Button))
            {
                RemoveHandleEvents();
                return;
            }

            var position = e.GetPosition(Element);
            var value = position / Element.RenderSize;

            if (ClampValue)
            {
                value.X = Helper.Clamp01(value.X);
                value.Y = Helper.Clamp01(value.Y);
            }

            Value = value;
            e.Handled = true;
        }
        private void OnElementPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!e.IsPressed(Button))
            {
                RemoveHandleEvents();
            }
        }

        #endregion
    }
}

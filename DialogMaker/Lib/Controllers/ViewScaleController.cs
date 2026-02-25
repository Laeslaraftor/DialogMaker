using DialogMaker.Core;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DialogMaker.Lib.Controllers
{
    public class ViewScaleController : Disposable
    {
        public ViewScaleController(UIElement element)
        {
            Element = element;

            element.MouseWheel += OnElementPreviewMouseWheel;
        }

        public event EventHandler? ScaleChanged;

        public UIElement Element { get; }
        public UIElement? Container { get; set; }
        public double MaxScale { get; set; } = 2;
        public double MinScale { get; set; } = 0.25;
        public ScaleTransform? OverrideScaleTransform { get; set; }

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Element.MouseWheel -= OnElementPreviewMouseWheel;
        }

        #endregion

        #region События

        private void OnElementPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            var scale = OverrideScaleTransform;
            scale ??= Element.GetTransform<ScaleTransform>();
            double delta = (double)e.Delta / 2000;
            Point currentScale = new(scale.ScaleX, scale.ScaleY);
            Point newScale = Point.Clamp(currentScale + delta, MinScale, MaxScale);

            if (currentScale == newScale)
            {
                return;
            }

            scale.ScaleX = newScale.X;
            scale.ScaleY = newScale.Y;

            if (Container == null)
            {
                return;
            }

            var translation = Container.GetTransform<TranslateTransform>();
            var halfSize = Element.RenderSize / 2;
            Point position;

            if (0 > e.Delta)
            {
                position = (Point)halfSize;
            }
            else
            {
                position = e.GetPosition(Element);
            }

            Point origin = (position / Element.RenderSize) * Element.RenderSize;
            origin -= halfSize;
            origin *= (-delta * 2) / scale.ScaleX;

            translation.X += origin.X;
            translation.Y += origin.Y;

            ScaleChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}

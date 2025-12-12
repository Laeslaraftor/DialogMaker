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

            element.PreviewMouseWheel += OnElementPreviewMouseWheel;
        }

        public UIElement Element { get; }
        public UIElement? Container { get; set; }
        public ScaleTransform? OverrideScaleTransform { get; set; }

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Element.PreviewMouseWheel -= OnElementPreviewMouseWheel;
        }

        #endregion

        #region События

        private void OnElementPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scale = OverrideScaleTransform;
            scale ??= Element.GetTransform<ScaleTransform>();
            double delta = (double)e.Delta / 2000;

            scale.ScaleX += delta;
            scale.ScaleY += delta;

            if (Container != null)
            {
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
            }
        }

        #endregion
    }
}

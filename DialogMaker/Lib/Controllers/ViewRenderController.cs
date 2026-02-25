using DialogMaker.Core;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DialogMaker.Lib.Controllers
{
    public class ViewRenderController : ObservableObject
    {
        public event EventHandler<ValueChangedEventArgs<RenderTargetBitmap?>>? RendererChanged;

        public Size Size
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Size));
                    field = value;
                    InvokePropertyChanged(nameof(Size));
                }
            }
        }
        public Size Dpi
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Dpi));
                    field = value;
                    InvokePropertyChanged(nameof(Dpi));
                }
            }
        }
        public PixelFormat PixelFormat
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(PixelFormat));
                    field = value;
                    InvokePropertyChanged(nameof(PixelFormat));
                }
            }
        }
        public RenderTargetBitmap? Renderer
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Renderer));

                    var oldValue = field;
                    field = value;

                    RendererChanged?.Invoke(this, new(oldValue, value));
                    InvokePropertyChanged(nameof(Renderer));
                }
            }
        }

        private PixelFormat _rendererPixelFormat;
        private Size _rendererSize;
        private Size _rendererDpi;
        private bool _isRendering;

        #region Управление

        public void Render(Visual element)
        {
            if (_isRendering)
            {
                return;
            }

            _isRendering = true;
            var renderer = Renderer;

            if (renderer == null ||
                Size != _rendererSize ||
                Dpi != _rendererDpi ||
                _rendererPixelFormat != PixelFormat)
            {
                _rendererSize = Size;
                _rendererDpi = Dpi;
                _rendererPixelFormat = PixelFormat;
                renderer = new((int)Size.Width, (int)Size.Height, Dpi.Width, Dpi.Height, PixelFormat);
                Renderer = renderer;
            }
            else
            {
                renderer.Clear();
            }

            renderer.Render(element);

            _isRendering = false;
        }

        #endregion
    }
}

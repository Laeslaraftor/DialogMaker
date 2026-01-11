using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DialogMaker.Lib.Elements
{
    public class ClippingBorder : Border
    {
        public ClippingBorder() : base()
        {
            Clip = _clippingGeometry;
        }

        private readonly RectangleGeometry _clippingGeometry = new();

        #region События

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            _clippingGeometry.Rect = new(0, 0, sizeInfo.NewSize.Width, sizeInfo.NewSize.Height);
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property.Name == nameof(CornerRadius))
            {
                var radius = CornerRadius;
                _clippingGeometry.RadiusX = radius.TopLeft;
                _clippingGeometry.RadiusY = radius.BottomRight;
            }
        }

        #endregion
    }
}

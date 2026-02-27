using System.Windows;
using System.Windows.Media.Animation;

namespace DialogMaker.Lib
{
    public static class AnimationsHelper
    {
        private static readonly CubicEase _easingOut = new()
        {
            EasingMode = EasingMode.EaseOut
        };
        private static readonly DoubleAnimation _fadeInAnimation = new(0, 1, new(TimeSpan.FromSeconds(0.2)))
        {
            EasingFunction = _easingOut,
            FillBehavior = FillBehavior.HoldEnd
        };
        private static readonly DoubleAnimation _fadeOutAnimation = new(1, 0, new(TimeSpan.FromSeconds(0.2)))
        {
            EasingFunction = _easingOut,
            FillBehavior = FillBehavior.HoldEnd
        };

        public static void FadeIn(UIElement element)
        {
            element.BeginAnimation(UIElement.OpacityProperty, _fadeInAnimation);
        }
        public static void FadeOut(UIElement element)
        {
            element.BeginAnimation(UIElement.OpacityProperty, _fadeOutAnimation);
        }
    }
}

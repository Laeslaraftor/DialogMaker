using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DialogMaker.Lib
{
    public static class AnimationsHelper
    {
        public static readonly Duration DefaultDuration = new(TimeSpan.FromSeconds(0.2));

        private static readonly CubicEase _easingOut = new()
        {
            EasingMode = EasingMode.EaseOut
        };
        private static readonly DoubleAnimation _fadeInAnimation = new(0, 1, DefaultDuration)
        {
            EasingFunction = _easingOut,
            FillBehavior = FillBehavior.HoldEnd
        };
        private static readonly DoubleAnimation _fadeOutAnimation = new(1, 0, DefaultDuration)
        {
            EasingFunction = _easingOut,
            FillBehavior = FillBehavior.HoldEnd
        };
        private static readonly Dictionary<Duration, DoubleAnimation> _fadeInAnimations = new()
        {
            { DefaultDuration, _fadeInAnimation }
        };
        private static readonly Dictionary<Duration, DoubleAnimation> _fadeOutAnimations = new()
        {
            { DefaultDuration, _fadeOutAnimation }
        };

        public static void StopAnimations(UIElement element)
        {
            element.BeginAnimation(UIElement.OpacityProperty, null);
        }
        public static void FadeIn(UIElement element, HandoffBehavior behavior = HandoffBehavior.SnapshotAndReplace)
        {
            element.BeginAnimation(UIElement.OpacityProperty, _fadeInAnimation, behavior);
        }
        public static void FadeIn(UIElement element, Duration duration, HandoffBehavior behavior = HandoffBehavior.SnapshotAndReplace)
        {
            if (!_fadeInAnimations.TryGetValue(duration, out var animation))
            {
                animation = new(0, 1, duration)
                {
                    EasingFunction = _easingOut,
                    FillBehavior = FillBehavior.HoldEnd
                };
                _fadeInAnimations.Add(duration, animation);
            }

            element.BeginAnimation(UIElement.OpacityProperty, animation, behavior);
        }
        public static void FadeOut(UIElement element, HandoffBehavior behavior = HandoffBehavior.SnapshotAndReplace)
        {
            element.BeginAnimation(UIElement.OpacityProperty, _fadeOutAnimation, behavior);
        }
        public static void FadeOut(UIElement element, Duration duration, HandoffBehavior behavior = HandoffBehavior.SnapshotAndReplace)
        {
            if (!_fadeOutAnimations.TryGetValue(duration, out var animation))
            {
                animation = new(1, 0, duration)
                {
                    EasingFunction = _easingOut,
                    FillBehavior = FillBehavior.HoldEnd
                };
                _fadeOutAnimations.Add(duration, animation);
            }

            element.BeginAnimation(UIElement.OpacityProperty, animation, behavior);
        }
        public static void AnimateMargin(FrameworkElement element, Thickness from, Thickness to, Duration duration, HandoffBehavior behavior = HandoffBehavior.SnapshotAndReplace)
        {
            AnimateMargin(element, from, to, duration, _easingOut, behavior);
        }
        public static void AnimateMargin(FrameworkElement element, Thickness from, Thickness to, Duration duration, IEasingFunction easing, HandoffBehavior behavior = HandoffBehavior.SnapshotAndReplace)
        {
            ThicknessAnimation animation = new(from, to, duration, FillBehavior.HoldEnd)
            {
                EasingFunction = easing
            };
            element.BeginAnimation(FrameworkElement.MarginProperty, animation, behavior);
        }

        extension(UIElement element)
        {
            public void TranslateX(double from, double to, Duration duration)
            {
                element.TranslateX(from, to, duration, _easingOut);
            }
            public void TranslateX(double from, double to, Duration duration, IEasingFunction easing)
            {
                element.TranslateX(element.GetTransform<TranslateTransform>(), from, to, duration, easing);
            }
            public void TranslateX(double to, Duration duration)
            {
                var translation = element.GetTransform<TranslateTransform>();
                element.TranslateX(translation, translation.X, to, duration, _easingOut);
            }
            public void TranslateX(double to, Duration duration, IEasingFunction easing)
            {
                var translation = element.GetTransform<TranslateTransform>();
                element.TranslateX(translation, translation.X, to, duration, easing);
            }
            private void TranslateX(TranslateTransform translation, double from, double to, Duration duration, IEasingFunction easing)
            {
                DoubleAnimation animation = new(from, to, duration, FillBehavior.HoldEnd)
                {
                    EasingFunction = easing
                };
                translation.BeginAnimation(TranslateTransform.XProperty, animation);
            }
        }
    }
}

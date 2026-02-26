using System.Windows;
using System.Windows.Media;

namespace DialogMaker.Lib
{
    public static class FocusHelper
    {
        public static readonly DependencyProperty IgnoreFocusSwitchProperty = DependencyProperty.RegisterAttached("IgnoreFocusSwitch", typeof(bool),
            typeof(FocusHelper), new(false));
        public static readonly DependencyProperty IgnoreFocusHitProperty = DependencyProperty.RegisterAttached("IgnoreFocusHit", typeof(bool),
            typeof(FocusHelper), new(false));

        public static void SetIgnoreFocusSwitch(DependencyObject obj, bool value)
        {
            obj.SetValue(IgnoreFocusSwitchProperty, value);
        }
        public static bool GetIgnoreFocusSwitch(DependencyObject? obj)
        {
            if (obj == null)
            {
                return false;
            }

            return (bool)obj.GetValue(IgnoreFocusSwitchProperty);
        }
        public static void SetIgnoreFocusHit(DependencyObject obj, bool value)
        {
            obj.SetValue(IgnoreFocusHitProperty, value);
        }
        public static bool GetIgnoreFocusHit(DependencyObject? obj)
        {
            if (obj == null)
            {
                return false;
            }

            return (bool)obj.GetValue(IgnoreFocusHitProperty);
        }

        public static bool GetVisualTreeIgnoreFocusSwitch(DependencyObject? obj)
        {
            return GetVisualTreeValue(GetIgnoreFocusSwitch, obj);
        }
        public static bool GetVisualTreeIgnoreFocusHit(DependencyObject? obj)
        {
            return GetVisualTreeValue(GetIgnoreFocusHit, obj);
        }

        private static bool GetVisualTreeValue(Func<DependencyObject?, bool> getter, DependencyObject? obj)
        {
            if (obj == null)
            {
                return false;
            }

            bool result = getter(obj);

            while (!result)
            {
                obj = VisualTreeHelper.GetParent(obj);

                if (obj == null)
                {
                    break;
                }

                result = getter(obj);
            }

            return result;
        }
    }
}

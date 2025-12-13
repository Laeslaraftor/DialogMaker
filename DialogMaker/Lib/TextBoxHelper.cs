using DialogMaker.Lib.Elements;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace DialogMaker.Lib
{
    public static class TextBoxHelper
    {
        extension(TextBox view)
        {
            public string Placeholder
            {
                get => GetPlaceholder(view);
                set => SetPlaceholder(view, value);
            }
        }

        public static string GetPlaceholder(DependencyObject obj)
        {
            return (string)obj.GetValue(PlaceholderProperty);
        }
        public static void SetPlaceholder(DependencyObject obj, string value)
        {
            obj.SetValue(PlaceholderProperty, value);
        }


        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached(
                "Placeholder",
                typeof(string),
                typeof(TextBoxHelper),
                new FrameworkPropertyMetadata(
                    defaultValue: null,
                    propertyChangedCallback: OnPlaceholderChanged)
                );

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBoxControl)
            {
                if (!textBoxControl.IsLoaded)
                {
                    textBoxControl.Loaded -= OnTextBoxControlLoaded;
                    textBoxControl.Loaded += OnTextBoxControlLoaded;
                }

                textBoxControl.TextChanged -= OnTextBoxControlTextChanged;
                textBoxControl.TextChanged += OnTextBoxControlTextChanged;

                if (GetOrCreateAdorner(textBoxControl, out var adorner))
                {
                    adorner.InvalidateVisual();
                }
            }
        }

        private static void OnTextBoxControlLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBoxControl)
            {
                textBoxControl.Loaded -= OnTextBoxControlLoaded;
                GetOrCreateAdorner(textBoxControl, out _);
                OnTextBoxControlTextChanged(textBoxControl, null);
            }
        }

        private static void OnTextBoxControlTextChanged(object sender, TextChangedEventArgs? e)
        {
            if (sender is TextBox textBoxControl
                && GetOrCreateAdorner(textBoxControl, out var adorner))
            {

                if (textBoxControl.Text.Length > 0)
                {
                    adorner.Visibility = Visibility.Hidden;
                    return;
                }

                adorner.Visibility = Visibility.Visible;
            }
        }

        private static bool GetOrCreateAdorner(TextBox textBoxControl, [NotNullWhen(true)] out PlaceholderAdorner? adorner)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(textBoxControl);

            if (layer == null)
            {
                adorner = null;
                return false;
            }

            adorner = layer.GetAdorners(textBoxControl)?.OfType<PlaceholderAdorner>().FirstOrDefault();

            if (adorner == null)
            {
                adorner = new PlaceholderAdorner(textBoxControl);
                layer.Add(adorner);
            }

            return true;
        }
    }
}

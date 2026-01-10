using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class SimpleNode : UserControl
    {
        public SimpleNode()
        {
            InitializeComponent();
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public object? Preview
        {
            get => GetValue(PreviewProperty);
            set => SetValue(PreviewProperty, value);
        }
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        private readonly TextBlock _textContent = new()
        {
            TextWrapping = TextWrapping.Wrap
        };

        #region События

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SimpleNode view)
            {
                view._title.Text = e.NewValue?.ToString();
            }
        }
        private static void OnPreviewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SimpleNode view)
            {
                if (e.NewValue is UIElement)
                {
                    view._content.Content = e.NewValue;
                }
                else
                {
                    view._textContent.Text = e.NewValue?.ToString();
                    view._content.Content = view._textContent;
                }

                view._content.Visibility = e.NewValue == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SimpleNode view)
            {
                view._selectionOutline.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string),
            typeof(SimpleNode), new(string.Empty, OnTitleChanged));
        public static readonly DependencyProperty PreviewProperty = DependencyProperty.Register(nameof(Preview), typeof(object),
            typeof(SimpleNode), new(OnPreviewChanged));
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool),
            typeof(SimpleNode), new(OnIsSelectedChanged));

        #endregion
    }
}

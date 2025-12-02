using DialogMaker.Core;
using DialogMaker.Editor;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class ReferenceView : UserControl
    {
        public ReferenceView()
        {
            InitializeComponent();
        }

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }
        public ProjectResourceItem? Item
        {
            get => GetValue(ItemProperty) as ProjectResourceItem;
            set => SetValue(ItemProperty, value);
        }
        public DialogResourceType? RequestedResourceType
        {
            get => (DialogResourceType?)GetValue(RequestedResourceTypeProperty);
            set => SetValue(RequestedResourceTypeProperty, value);
        }

        private object? LastPreview
        {
            get => field;
            set
            {
                if (field == value)
                {
                    return;
                }

                string? text = value?.ToString();
                bool isEmpty = string.IsNullOrEmpty(text);
                _content.Content = value;
                _content.Visibility = isEmpty ? Visibility.Collapsed : Visibility.Visible;
                _placeholder.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;

                field = value;
            }
        }

        #region События

        private void OnItemChanged(ProjectResourceItem? oldItem, ProjectResourceItem? newItem)
        {
            if (LastPreview != null)
            {
                oldItem?.FreePreview(LastPreview);
            }
            if (newItem != null)
            {
                var requestedType = RequestedResourceType;

                if (requestedType != null && newItem.ResourceType != requestedType.Value)
                {
                    throw new ArgumentException($"Ресурс не соответствует запрошенному типу. Запрошен тип {requestedType}, а получен {newItem.ResourceType}", nameof(newItem));
                }

                LastPreview = newItem.GetPreview();
                return;
            }

            LastPreview = null;
        }

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ReferenceView view)
            {
                view._placeholder.Text = e.NewValue?.ToString();
            }
        }
        private static void OnItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ReferenceView view)
            {
                view.OnItemChanged(e.OldValue as ProjectResourceItem, e.NewValue as ProjectResourceItem);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(nameof(Placeholder), typeof(string),
            typeof(ReferenceView), new(string.Empty, OnPlaceholderChanged));
        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item), typeof(ProjectResourceItem),
            typeof(ReferenceView), new(OnItemChanged));
        public static readonly DependencyProperty RequestedResourceTypeProperty = DependencyProperty.Register(nameof(RequestedResourceType), typeof(DialogResourceType?),
            typeof(ReferenceView), new(null));

        #endregion
    }
}

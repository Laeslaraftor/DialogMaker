using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DialogMaker.Lib.Elements
{
    public partial class DropDownButton : ContentControl
    {
        public DropDownButton()
        {
            InitializeComponent();
        }

        public string? Text
        {
            get => GetValue(TextProperty) as string;
            set => SetValue(TextProperty, value);
        }

        #region События

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            e.Handled = true;
            base.OnContextMenuOpening(e);
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if (ContextMenu == null)
            {
                return;
            }

            ContextMenu.PlacementTarget = this;
            ContextMenu.Placement = PlacementMode.Bottom;
            ContextMenu.IsOpen = true;
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DropDownButton view)
            {
                view._text.Text = e.NewValue?.ToString();
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string),
            typeof(DropDownButton), new(string.Empty, OnTextChanged));

        #endregion
    }
}

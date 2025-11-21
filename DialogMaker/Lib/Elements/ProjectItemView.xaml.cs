using Acly.Tokens;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogMaker.Lib.Elements
{
    public partial class ProjectItemView : UserControl
    {
        public ProjectItemView()
        {
            InitializeComponent();
        }

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        public string ItemName
        {
            get => (string)GetValue(ItemNameProperty);
            set => SetValue(ItemNameProperty, value);
        }
        public bool CanChangeName
        {
            get => (bool)GetValue(CanChangeNameProperty);
            set => SetValue(CanChangeNameProperty, value);
        }

        private bool EditMode
        {
            get => _nameEditor.Visibility == Visibility.Visible;
            set
            {
                value &= CanChangeName;

                if (value == EditMode)
                {
                    return;
                }

                Visibility editorVisibility = Visibility.Hidden;

                if (value)
                {
                    editorVisibility = Visibility.Visible;
                }
                else
                {
                    RemoveLostFocusEvent();
                    ItemName = _nameEditor.Text;
                    Keyboard.ClearFocus();
                }

                _nameEditor.Text = ItemName;
                _name.Visibility = _nameEditor.Visibility;
                _nameEditor.Visibility = editorVisibility;

                if (value)
                {
                    AddLostFocusEvent();
                }
            }
        }

        private bool _lostFocusEventAdded;
        private Token? _lostFocusEventAddToken;

        #region Управление

        private async void AddLostFocusEvent()
        {
            if (_lostFocusEventAdded)
            {
                return;
            }

            Token token = new();
            _lostFocusEventAddToken = token;

            await Task.Delay(50);

            _nameEditor.Focus();

            if (_lostFocusEventAddToken != token)
            {
                return;
            }

            _lostFocusEventAdded = true;
            _nameEditor.LostFocus += OnNameEditorLostFocus;
        }
        private void RemoveLostFocusEvent()
        {
            _lostFocusEventAddToken = null;

            if (_lostFocusEventAdded)
            {
                _nameEditor.LostFocus -= OnNameEditorLostFocus;
                _lostFocusEventAdded = false;
            }
        }

        #endregion

        #region События

        private void OnNameMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2)
            {
                return;
            }

            EditMode = true;
        }
        private void OnNameEditorKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                e.Handled = true;
                EditMode = false;
            }
        }
        private void OnNameEditorLostFocus(object sender, RoutedEventArgs e)
        {
            RemoveLostFocusEvent();
            EditMode = _nameEditor.IsFocused;
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ProjectItemView view)
            {
                return;
            }

            string value = (string)e.NewValue;
            Visibility visibility = Visibility.Visible;

            if (string.IsNullOrEmpty(value))
            {
                visibility = Visibility.Collapsed;
            }

            view._icon.Text = value;
            view._icon.Visibility = visibility;
        }
        private static void OnItemNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ProjectItemView view)
            {
                return;
            }

            view._name.Text = (string)e.NewValue;
            view._nameEditor.Text = (string)e.NewValue;
        }
        private static void OnCanChangeNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ProjectItemView view)
            {
                return;
            }

            view._nameEditor.IsEnabled = (bool)e.NewValue;
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(string),
            typeof(ProjectItemView), new(string.Empty, OnIconChanged));
        public static readonly DependencyProperty ItemNameProperty = DependencyProperty.Register(nameof(ItemName), typeof(string),
            typeof(ProjectItemView), new(string.Empty, OnItemNameChanged));
        public static readonly DependencyProperty CanChangeNameProperty = DependencyProperty.Register(nameof(CanChangeName), typeof(bool),
            typeof(ProjectItemView), new(true, OnCanChangeNameChanged));

        #endregion
    }
}

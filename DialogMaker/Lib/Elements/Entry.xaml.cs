using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogMaker.Lib.Elements
{
    public partial class Entry : UserControl
    {
        public Entry()
        {
            InitializeComponent();
        }

        public event EventHandler<ValueChangedEventArgs<string>>? ConfirmedText;

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public TextBox TextBox => _text;

        private string _startFocusValue = string.Empty;

        #region Управление

        public static implicit operator TextBox(Entry entry)
        {
            return entry.TextBox;
        }

        #endregion

        #region События

        private void OnTextGotFocus(object sender, RoutedEventArgs e)
        {
            _startFocusValue = _text.Text;
        }
        private async void OnTextLostFocus(object sender, RoutedEventArgs e)
        {
            await Task.Delay(50);
            ConfirmedText?.Invoke(this, new(_startFocusValue, _text.Text));
        }
        private void OnTextKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter && !_text.AcceptsReturn) || e.Key == Key.Escape)
            {
                MainWindow.Instance.ClearFocus();
            }
        }
        private void OnTextTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = _text.Text;
            Visibility visibility = Visibility.Visible;

            if (text.Length > 0)
            {
                visibility = Visibility.Collapsed;
            }

            _placeholder.Visibility = visibility;
            Text = text;
        }

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Entry view)
            {
                view._placeholder.Text = e.NewValue as string;  
            }
        }
        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Entry view && !view._text.Text.Equals(e.NewValue))
            {
                view._text.Text = e.NewValue as string;
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(nameof(Placeholder), typeof(string),
            typeof(Entry), new(string.Empty, OnPlaceholderChanged));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string),
            typeof(Entry), new(string.Empty, OnTextChanged));

        #endregion
    }
}

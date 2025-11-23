using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogMaker.Lib.Elements
{
    public partial class LanguageView : UserControl
    {
        public LanguageView()
        {
            InitializeComponent();
        }

        public string LanguageId
        {
            get => (string)GetValue(LanguageIdProperty);
            set => SetValue(LanguageIdProperty, value);
        }
        public string LanguageName
        {
            get => (string)GetValue(LanguageNameProperty);
            set => SetValue(LanguageNameProperty, value);
        }
        public ICommand? IdEditCommand
        {
            get => GetValue(IdEditCommandProperty) as ICommand;
            set => SetValue(IdEditCommandProperty, value);
        }
        public object? IdEditCommandParameter
        {
            get => GetValue(IdEditCommandParameterProperty);
            set => SetValue(IdEditCommandParameterProperty, value);
        }
        public ICommand? NameEditCommand
        {
            get => GetValue(NameEditCommandProperty) as ICommand;
            set => SetValue(NameEditCommandProperty, value);
        }
        public object? NameEditCommandParameter
        {
            get => GetValue(NameEditCommandParameterProperty);
            set => SetValue(NameEditCommandParameterProperty, value);
        }

        #region События

        private static void OnLanguageIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LanguageView view)
            {
                view._id.Text = (string)e.NewValue;
            }
        }
        private static void OnLanguageNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LanguageView view)
            {
                view._name.Text = (string)e.NewValue;
            }
        }
        private static void IdEditCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LanguageView view)
            {
                view._id.EditCommand = e.NewValue as ICommand;
            }
        }
        private static void IdEditCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LanguageView view)
            {
                view._id.EditCommandParameter = e.NewValue;
            }
        }
        private static void NameEditCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LanguageView view)
            {
                view._name.EditCommand = e.NewValue as ICommand;
            }
        }
        private static void NameEditCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LanguageView view)
            {
                view._name.EditCommandParameter = e.NewValue;
            }
        }

        #endregion

        #region События

        public static readonly DependencyProperty LanguageIdProperty = DependencyProperty.Register(nameof(LanguageId), typeof(string),
           typeof(LanguageView), new(string.Empty, OnLanguageIdChanged));
        public static readonly DependencyProperty LanguageNameProperty = DependencyProperty.Register(nameof(LanguageName), typeof(string),
           typeof(LanguageView), new(string.Empty, OnLanguageNameChanged));
        public static readonly DependencyProperty IdEditCommandProperty = DependencyProperty.Register(nameof(IdEditCommand), typeof(ICommand),
           typeof(LanguageView), new(null, IdEditCommandChanged));
        public static readonly DependencyProperty IdEditCommandParameterProperty = DependencyProperty.Register(nameof(IdEditCommandParameter), typeof(object),
           typeof(LanguageView), new(null, IdEditCommandParameterChanged));
        public static readonly DependencyProperty NameEditCommandProperty = DependencyProperty.Register(nameof(NameEditCommand), typeof(ICommand),
           typeof(LanguageView), new(null, NameEditCommandChanged));
        public static readonly DependencyProperty NameEditCommandParameterProperty = DependencyProperty.Register(nameof(NameEditCommandParameter), typeof(object),
           typeof(LanguageView), new(null, NameEditCommandParameterChanged));

        #endregion
    }
}

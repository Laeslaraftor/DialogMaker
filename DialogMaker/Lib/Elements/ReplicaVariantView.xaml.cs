using DialogMaker.Editor;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DialogMaker.Lib.Elements
{
    public partial class ReplicaVariantView : UserControl
    {
        public ReplicaVariantView()
        {
            InitializeComponent();
        }

        public ProjectStringVariant? Variant
        {
            get => GetValue(VariantProperty) as ProjectStringVariant;
            set => SetValue(VariantProperty, value);
        }

        #region Управление

        private void SetVariant(ProjectStringVariant? oldValue, ProjectStringVariant? newValue)
        {
            if (oldValue == newValue)
            {
                return;
            }
            if (oldValue != null)
            {
                oldValue.PropertyChanged -= OnVariantPropertyChanged;
            }

            _textBox.Text = newValue?.Text ?? string.Empty;
            _comboBox.ItemsSource = newValue?.String.Project.Languages;
            ContextMenu = newValue?.ContextMenu;

            try
            {
                _voiceField.Item = newValue?.Voice?.Item;
            }
            catch (Exception error)
            {
                error.Log();
            }

            Binding languageBinding = new("LanguageIndex")
            {
                Mode = BindingMode.TwoWay
            };

            BindingOperations.SetBinding(_comboBox, ComboBox.SelectedIndexProperty, languageBinding);

            if (newValue != null)
            {
                newValue.PropertyChanged += OnVariantPropertyChanged;
            }
        }

        #endregion

        #region События

        private void OnVariantPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ProjectStringVariant variant)
            {
                return;
            }
            if (e.PropertyName == "Text")
            {
                _textBox.Text = variant.Text;
            }
            else if (e.PropertyName == "Voice")
            {
                _voiceField.Item = variant.Voice?.Item;
            }
        }

        private void OnReferenceViewItemChanged(object sender, ValueChangedEventArgs<ProjectResourceItem> e)
        {
            var variant = Variant;

            if (variant == null)
            {
                return;
            }
            if (e.NewValue == null)
            {
                variant.Voice = null;
                return;
            }

            try
            {
                variant.Voice = (ProjectFile)e.NewValue;
            }
            catch (Exception error)
            {
                error.Log();
            }
        }
        private void OnTextBoxConfirmedText(object sender, ValueChangedEventArgs<string> e)
        {
            Variant?.Text = e.NewValue;
        }


        private static void OnVariantChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ReplicaVariantView view)
            {
                view.SetVariant(e.OldValue as ProjectStringVariant, e.NewValue as ProjectStringVariant);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(nameof(Variant), typeof(ProjectStringVariant),
            typeof(ReplicaVariantView), new(OnVariantChanged));

        #endregion


    }
}

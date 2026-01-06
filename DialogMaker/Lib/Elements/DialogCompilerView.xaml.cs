using DialogMaker.Core.Executioning;
using DialogMaker.Core.Executioning.Builders;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class DialogCompilerView : UserControl
    {
        public DialogCompilerView()
        {
            InitializeComponent();
        }

        public DialogCodeBuilder? Builder
        {
            get => GetValue(BuilderProperty) as DialogCodeBuilder;
            set => SetValue(BuilderProperty, value);
        }
        public CompiledCodeInfo Code
        {
            get => (CompiledCodeInfo)GetValue(CodeProperty);
            set => SetValue(CodeProperty, value);
        }

        #region Управление

        private void SetBuilder(DialogCodeBuilder? oldValue, DialogCodeBuilder? newValue)
        {
            _sectionView.ItemsSource = newValue?.Sections;
        }
        private void SetCode(CompiledCodeInfo oldValue, CompiledCodeInfo newValue)
        {
            _resourcesList.ItemsSource = newValue.Context?.Resources;
            _variablesList.ItemsSource = newValue.Context?.Variables;

            if (newValue.ByteCode == null)
            {
                return;
            }

            try
            {
                var code = DialogByteCodeParser.Read(newValue.ByteCode, newValue.SectionPosition);
                _codeView.ItemsSource = code.Sections;
            }
            catch (Exception error)
            {
                error.Alert();
            }
        }

        #endregion

        #region События

        private static void OnBuilderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DialogCompilerView view)
            {
                view.SetBuilder(e.OldValue as DialogCodeBuilder, e.NewValue as DialogCodeBuilder);
            }
        }
        private static void OnCodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DialogCompilerView view)
            {
                view.SetCode((CompiledCodeInfo)e.OldValue, (CompiledCodeInfo)e.NewValue);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty BuilderProperty = DependencyProperty.Register(nameof(Builder), typeof(DialogCodeBuilder),
            typeof(DialogCompilerView), new(OnBuilderChanged));
        public static readonly DependencyProperty CodeProperty = DependencyProperty.Register(nameof(Code), typeof(CompiledCodeInfo),
            typeof(DialogCompilerView), new(OnCodeChanged));

        #endregion
    }
}

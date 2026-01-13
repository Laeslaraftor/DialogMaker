using DialogMaker.Core.Executioning;
using DialogMaker.Core.Executioning.Builders;
using DialogMaker.Editor.Runtime;
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
        public DialogCompilerOutput Code
        {
            get => (DialogCompilerOutput)GetValue(CodeProperty);
            set => SetValue(CodeProperty, value);
        }
        public DialogRuntimeResourcesController? ResourcesController
        {
            get => GetValue(ResourcesControllerProperty) as DialogRuntimeResourcesController;
            set => SetValue(ResourcesControllerProperty, value);
        }

        #region Управление

        private void SetResourcesController(DialogRuntimeResourcesController? oldValue, DialogRuntimeResourcesController? newValue)
        {
            _resourcesList.ItemsSource = newValue?.Items;
        }
        private void SetBuilder(DialogCodeBuilder? oldValue, DialogCodeBuilder? newValue)
        {
            _sectionView.ItemsSource = newValue?.Sections;
        }
        private void SetCode(DialogCompilerOutput oldValue, DialogCompilerOutput newValue)
        {
            if (newValue.Code == null)
            {
                return;
            }

            try
            {
                var code = DialogByteCodeData.Read(newValue.Write());
                _codeView.ItemsSource = code.Sections;
                _dialogPlayer.DialogExecutor = new(code, newValue.Context);
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
                view.SetCode((DialogCompilerOutput)e.OldValue, (DialogCompilerOutput)e.NewValue);
            }
        }
        private static void OnResourcesControllerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DialogCompilerView view)
            {
                view.SetResourcesController(e.OldValue as DialogRuntimeResourcesController, e.NewValue as DialogRuntimeResourcesController);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty BuilderProperty = DependencyProperty.Register(nameof(Builder), typeof(DialogCodeBuilder),
            typeof(DialogCompilerView), new(OnBuilderChanged));
        public static readonly DependencyProperty CodeProperty = DependencyProperty.Register(nameof(Code), typeof(DialogCompilerOutput),
            typeof(DialogCompilerView), new(OnCodeChanged));
        public static readonly DependencyProperty ResourcesControllerProperty = DependencyProperty.Register(nameof(ResourcesController), typeof(DialogRuntimeResourcesController),
            typeof(DialogCompilerView), new(OnResourcesControllerChanged));

        #endregion
    }
}

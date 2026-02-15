using DialogMaker.Core.Executioning;
using DialogMaker.Editor;
using DialogMaker.Editor.Runtime;
using DialogMaker.Lib.InputFields;
using Newtonsoft.Json.Linq;
using System.IO;
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
        ~DialogCompilerView()
        {
            _codeBuffer.Dispose();
        }

        public ProjectDialog? Dialog
        {
            get => GetValue(DialogProperty) as ProjectDialog;
            set => SetValue(DialogProperty, value);
        }
        public bool IsCompiling
        {
            get => (bool)GetValue(IsCompilingProperty.DependencyProperty);
            private set => SetValue(IsCompilingProperty, value);
        }
        public bool IsCompiled
        {
            get => (bool)GetValue(IsCompiledProperty.DependencyProperty);
            private set => SetValue(IsCompiledProperty, value);
        }
        public bool IsStarted
        {
            get => (bool)GetValue(IsStartedProperty.DependencyProperty);
            private set => SetValue(IsStartedProperty, value);
        }
        public bool IsPaused
        {
            get => (bool)GetValue(IsPausedProperty.DependencyProperty);
            private set => SetValue(IsPausedProperty, value);
        }

        private DialogRuntimeResourcesController? _lastResourcesController;
        private readonly MemoryStream _codeBuffer = new();

        #region Управление

        public async void Compile()
        {
            var dialog = Dialog;

            if (IsCompiling || dialog == null)
            {
                return;
            }

            Clear();

            IsCompiling = true;            
            DialogCompiler compiler = new(DialogActionsMap.Create(dialog.Original));

            try
            {
                var compileOutput = await Task.Run(compiler.Compile);
                _codeBuffer.Position = 0;
                _codeBuffer.SetLength(0);

                compileOutput.Write(_codeBuffer);
                _codeBuffer.Position = 0;

                var code = DialogByteCodeData.Read(_codeBuffer);

                _codeView.ItemsSource = code.Sections;
                _dialogPlayer.DialogExecutor?.Dispose();
                _dialogPlayer.DialogExecutor = new(code, compileOutput.Context);
                _lastResourcesController = new(new(dialog.Original, compileOutput.Metadata.LocalValues));
                _resourcesList.ItemsSource = _lastResourcesController.Items;
                IsCompiled = true;
            }
            catch (Exception error)
            {
                _errorView.Text = $"{error.GetType().Name}: {error.Message}";
                _errorView.Visibility = Visibility.Visible;
            }

            IsCompiling = false;
        }

        private void Clear()
        {
            IsStarted = false;
            IsPaused = false;
            IsCompiled = false;
            IsCompiling = false;
            _codeView.ItemsSource = null;
            _dialogPlayer.DialogExecutor?.Dispose();
            _dialogPlayer.DialogExecutor = null;
            _resourcesList.ItemsSource = null;
            _lastResourcesController?.Dispose();
            _lastResourcesController = null;
            _errorView.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region События

        private static void OnDialogChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DialogCompilerView view)
            {
                return;
            }

            view.Clear();

            if (e.NewValue is not ProjectDialog dialog)
            {
                return;
            }
        }
        private static void OnIsCompilingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DialogCompilerView view)
            {
                view._progressGrid.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private static void OnIsCompiledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DialogCompilerView view || e.NewValue is not bool value)
            {
                return;
            }

            view._mainGrid.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty DialogProperty = DependencyProperty.Register(nameof(Dialog), typeof(ProjectDialog),
            typeof(DialogCompilerView), new(OnDialogChanged));
        public static readonly DependencyPropertyKey IsCompilingProperty = DependencyProperty.RegisterReadOnly(nameof(IsCompiling), typeof(bool),
            typeof(DialogCompilerView), new(false, OnIsCompilingChanged));
        public static readonly DependencyPropertyKey IsCompiledProperty = DependencyProperty.RegisterReadOnly(nameof(IsCompiled), typeof(bool),
            typeof(DialogCompilerView), new(false, OnIsCompiledChanged));
        public static readonly DependencyPropertyKey IsPausedProperty = DependencyProperty.RegisterReadOnly(nameof(IsPaused), typeof(bool),
            typeof(DialogCompilerView), new(false));
        public static readonly DependencyPropertyKey IsStartedProperty = DependencyProperty.RegisterReadOnly(nameof(IsStarted), typeof(bool),
            typeof(DialogCompilerView), new(false));

        #endregion
    }
}

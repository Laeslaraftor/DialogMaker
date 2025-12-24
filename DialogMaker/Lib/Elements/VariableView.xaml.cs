using DialogMaker.Editor;
using DialogMaker.Lib.Controllers;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class VariableView : UserControl, IDisposable
    {
        public VariableView()
        {
            InitializeComponent();
            _idBlock.EditCommand = ProjectResourceItem.IdEditCommand;
        }
        ~VariableView()
        {
            Dispose();
        }

        public ProjectVariable? Variable
        {
            get => GetValue(VariableProperty) as ProjectVariable;
            set => SetValue(VariableProperty, value);
        }

        private PropertyEditorController? _editorController;

        #region Управление

        public void Dispose()
        {
            _editorController?.Dispose();
            GC.SuppressFinalize(this);
        }

        private void SetVariable(ProjectVariable? oldValue, ProjectVariable? newValue)
        {
            if (oldValue == newValue)
            {
                return;
            }
            if (_editorController != null)
            {
                _editorController.View.RemoveFromParent();
                _editorController.Dispose();
                _editorController = null;
            }

            _idBlock.DataContext = newValue;
            _idBlock.EditCommandParameter = newValue;
            _flagsView.Value = newValue?.Original.Resources.Flags;
            ContextMenu = newValue?.ContextMenu;

            if (newValue == null)
            {
                return;
            }

            _typeView.Type = newValue.ValueType;
            var editor = newValue.CreateInputField();

            if (editor != null)
            {
                Grid.SetRow(editor.View, 1);
                Grid.SetColumnSpan(editor.View, _mainGrid.ColumnDefinitions.Count);
                _mainGrid.Children.Add(editor.View);
                _editorController = editor;
            }
        }

        #endregion

        #region События

        private static void OnVariableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VariableView view)
            {
                view.SetVariable(e.OldValue as ProjectVariable, e.NewValue as ProjectVariable);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty VariableProperty = DependencyProperty.Register(nameof(Variable), typeof(ProjectVariable),
            typeof(VariableView), new(OnVariableChanged));

        #endregion
    }
}

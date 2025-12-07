using Acly;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Editor.Menus;
using DialogMaker.Lib;
using System.Collections;
using System.ComponentModel;
using System.Windows.Controls;

namespace DialogMaker.Editor
{
    public class ProjectDialog : ProjectStructureItem
    {
        public ProjectDialog(ProjectPack pack, DialogProjectDialog dialog) : base(pack.Project, dialog)
        {
            Original = dialog;
            Pack = pack;

            _nodesConverter = new(this);
            _nodesSync = new(dialog.Nodes, Nodes, _nodesConverter);

            dialog.PropertyChanged += OnDialogPropertyChanged;
        }

        public DialogProjectDialog Original { get; }
        public ProjectPack Pack { get; }
        public override string Icon => Icons.Message;
        public override string Name
        {
            get => Original.Name;
            set => Original.Name = value;
        }
        public override ContextMenu? ContextMenu
        {
            get
            {
                field ??= new DialogContextMenu(Original);
                return field;
            }
        }
        public ContextMenu EditorContextMenu
        {
            get
            {
                field ??= new DialogEditorContextMenu(this);
                return field;
            }
        }
        public override IEnumerable? Children => Nodes;
        public EditableCollection<DialogProjectNode> Nodes { get; } = [];

        private readonly ProjectNodeConverter _nodesConverter;
        private readonly CollectionSynchronizer<DialogProjectDialogNode, DialogProjectNode> _nodesSync;

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            _nodesSync.Dispose();

            Original.PropertyChanged -= OnDialogPropertyChanged;
        }

        #endregion

        #region События

        private void OnDialogPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Name))
            {
                InvokePropertyChanged(nameof(Name));
            }
        }

        #endregion
    }
}

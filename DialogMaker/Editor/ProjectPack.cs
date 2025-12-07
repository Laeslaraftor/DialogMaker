using Acly;
using DialogMaker.Core.Editor;
using DialogMaker.Editor.Menus;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;

namespace DialogMaker.Editor
{
    public class ProjectPack : ProjectStructureItem
    {
        public ProjectPack(ProjectController project, DialogProjectPack pack) : base(project, pack)
        {
            Original = pack;

            _dialogsConverter = new(this);
            _dialogsSync = new(pack.Dialogs, _dialogs, _dialogsConverter);

            pack.PropertyChanged += OnPackPropertyChanged;
        }

        public DialogProjectPack Original { get; }
        public override string Icon => string.Empty;
        public override string Name
        {
            get => Original.Name;
            set => Original.Name = value;
        }
        public override ContextMenu? ContextMenu
        {
            get
            {
                field ??= new DialogPackContextMenu(Original);
                return field;
            }
        }
        public override IEnumerable? Children => _dialogs;

        private readonly ProjectDialogConverter _dialogsConverter;
        private readonly CollectionSynchronizer<DialogProjectDialog, ProjectDialog> _dialogsSync;
        private readonly ObservableCollection<ProjectDialog> _dialogs = [];

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            _dialogsSync.Dispose();

            Original.PropertyChanged -= OnPackPropertyChanged;
        }

        #endregion

        #region События

        private void OnPackPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Name))
            {
                InvokePropertyChanged(nameof(Name));
            }
        }

        #endregion
    }
}

using Acly;
using DialogMaker.Core.Editor;
using DialogMaker.Editor.Menus;
using DialogMaker.Lib.Controllers;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;

namespace DialogMaker.Editor
{
    public class ProjectPack : ProjectResourcesItem
    {
        public ProjectPack(ProjectController project, DialogProjectPack pack) : base(project, pack)
        {
            Original = pack;

            _dialogsConverter = new(this);
            _dialogsSync = new(pack.Dialogs, _dialogs, _dialogsConverter);

            pack.PropertyChanged += OnPackPropertyChanged;
        }

        public DialogProjectPack Original { get; }
        public override ProjectResources Resources
        {
            get
            {
                _resources ??= new(Project, Original.Resources, Project.Resources);
                return _resources;
            }
        }
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
        public override IEnumerable<ActionButton>? Actions => null;

        private readonly ProjectDialogConverter _dialogsConverter;
        private readonly CollectionSynchronizer<DialogProjectDialog, ProjectDialog> _dialogsSync;
        private readonly ObservableCollection<ProjectDialog> _dialogs = [];
        private ProjectResources? _resources;

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            _dialogsSync.Dispose();
            _resources?.Dispose();

            _resources = null;

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

using Acly;
using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Editor.Menus;
using DialogMaker.Lib;
using DialogMaker.ViewModels;
using System.ComponentModel;
using System.Windows.Input;

namespace DialogMaker.Editor
{
    public partial class ProjectController : ObservableObject, IDisposable
    {
        public ProjectController(DialogProject project)
        {
            _project = project;
            _structure = [];
            Structure = new(_structure);
            CreatePackCommand = new RelayCommand(ExecuteCreatePack);

            project.PropertyChanged += OnProjectPropertyChanged;
            project.PacksChanged += OnProjectPacksChanged;

            foreach (var pack in project.Packs)
            {
                pack.DialogsChanged += OnPackDialogsChanged;
            }

            UpdateStructure();
        }

        ~ProjectController()
        {
            Dispose();
        }

        public bool IsDisposed
        {
            get => _isDisposed;
            private set
            {
                if (_isDisposed != value)
                {
                    _isDisposed = value;
                    InvokePropertyChanged(nameof(IsDisposed));
                }
            }
        }
        public ReferenceReadOnlyList<ProjectItem> Structure { get; }
        public string Name
        {
            get => _project.Name;
            set => _project.Name = value;
        }
        public ICommand CreatePackCommand { get; }

        private readonly DialogProject _project;
        private readonly ObservableList<ProjectItem> _structure;
        private bool _isDisposed;

        #region Управление

        public void Save()
        {
            try
            {
                _project.Save();
            }
            catch (Exception error)
            {
                error.Alert();
            }
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;

            _project.PropertyChanged -= OnProjectPropertyChanged;
            _project.PacksChanged -= OnProjectPacksChanged;

            foreach (var pack in _project.Packs)
            {
                pack.DialogsChanged -= OnPackDialogsChanged;
            }
        }
        public void UpdateStructure()
        {
            _structure.Clear();

            foreach (var pack in _project.Packs)
            {
                DialogPackContextMenu menu = new(pack);
                ProjectItem packItem = new()
                {
                    Name = pack.Name,
                    Value = pack,
                    ContextMenu = menu
                };

                foreach (var dialog in pack.Dialogs)
                {
                    packItem.Children.Add(new()
                    {
                        Icon = Icons.Message,
                        Name = dialog.Name,
                        Value = dialog,
                        ContextMenu = new DialogContextMenu(dialog)
                    });
                }

                _structure.Add(packItem);
            }
        }

        #endregion

        #region Команды

        private void ExecuteCreatePack(object? parameter)
        {
            string? name = Alerts.RequestText("Введите название набора диалогов");

            if (name == null)
            {
                return;
            }

            Try(() => _project.CreatePack(name, name));

            UpdateStructure();
            Save();
        }

        #endregion

        #region События

        private void OnProjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Save();
            InvokePropertyChanged(e.PropertyName != null ? e.PropertyName : string.Empty);
        }

        private void OnProjectPacksChanged(object? sender, ItemActionEventArgs<DialogProjectPack> e)
        {
            if (e.Action == ItemAction.Add)
            {
                e.Item.DialogsChanged += OnPackDialogsChanged;
            }
            else
            {
                e.Item.DialogsChanged -= OnPackDialogsChanged;
            }

            UpdateStructure();
            Save();
        }
        private void OnPackDialogsChanged(object? sender, ItemActionEventArgs<DialogProjectDialog> e)
        {
            UpdateStructure();
            Save();
        }

        #endregion

        #region Статика

        private static bool Try(Action method)
        {
            try
            {
                method();
            }
            catch (Exception error)
            {
                error.Alert();
                return false;
            }

            return true;
        }

        #endregion
    }
}

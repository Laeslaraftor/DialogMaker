using Acly;
using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Editor.Menus;
using DialogMaker.Lib;
using DialogMaker.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Specialized;

namespace DialogMaker.Editor
{
    public partial class ProjectController : ObservableObject, IDisposable
    {
        public ProjectController(DialogProject project)
        {
            Project = project;
            _structure = [];
            Structure = new(_structure);
            CreatePackCommand = new RelayCommand(ExecuteCreatePack);
            CreateLanguageCommand = new RelayCommand(p => project.CreateLanguage());

            _languageConverter = new(this);
            _languages = new(project.Languages, Languages, _languageConverter);

            project.PropertyChanged += OnProjectPropertyChanged;
            project.PacksChanged += OnProjectPacksChanged;
            Languages.CollectionChanged += OnLanguagesCollectionChanged;

            foreach (var pack in project.Packs)
            {
                pack.DialogsChanged += OnPackDialogsChanged;
            }

            UpdateStructure();
            UpdateDefaultLanguage();
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
        public DialogProject Project { get; }
        public ReferenceReadOnlyList<ProjectItem> Structure { get; }
        public ObservableCollection<ProjectLanguage> Languages { get; } = [];
        public string Name
        {
            get => Project.Name;
            set => Project.Name = value;
        }
        public ProjectLanguage? DefaultLanguage
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    IsDefaultLanguageSetted = value != null;

                    if (Project.DefaultLanguage != value?.Language)
                    {
                        Project.DefaultLanguage = value?.Language;
                    }

                    InvokePropertyChanged(nameof(DefaultLanguage));
                }
            }
        }
        public bool IsDefaultLanguageSetted
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    InvokePropertyChanged(nameof(IsDefaultLanguageSetted));
                }
            }
        }
        public ICommand CreatePackCommand { get; }
        public ICommand CreateLanguageCommand { get; }

        private readonly ProjectLanguageConverter _languageConverter;
        private readonly CollectionSynchronizer<DialogProjectLanguage, ProjectLanguage> _languages;
        private readonly ObservableList<ProjectItem> _structure;
        private bool _isDisposed;

        #region Управление

        public void Save()
        {
            try
            {
                Project.Save();
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

            Project.PropertyChanged -= OnProjectPropertyChanged;
            Project.PacksChanged -= OnProjectPacksChanged;
            Languages.CollectionChanged -= OnLanguagesCollectionChanged;

            foreach (var pack in Project.Packs)
            {
                pack.DialogsChanged -= OnPackDialogsChanged;
            }

            _languages.Dispose();
            _languageConverter.Dispose();

            GC.SuppressFinalize(this);
        }
        public void UpdateStructure()
        {
            _structure.Clear();

            foreach (var pack in Project.Packs)
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

        private void UpdateDefaultLanguage()
        {
            foreach (var language in Languages)
            {
                if (Project.DefaultLanguage == language.Language)
                {
                    DefaultLanguage = language;
                    return;
                }
            }

            DefaultLanguage = null;
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

            Try(() => Project.CreatePack(name, name));

            UpdateStructure();
            Save();
        }

        #endregion

        #region События

        private void OnProjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Save();
            InvokePropertyChanged(e.PropertyName != null ? e.PropertyName : string.Empty);

            if (e.PropertyName == nameof(DefaultLanguage))
            {
                UpdateDefaultLanguage();
            }
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
        private void OnLanguagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
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

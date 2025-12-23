using Acly;
using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Lib;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Specialized;
using DialogMaker.Editor.Nodes;
using System.Diagnostics.CodeAnalysis;
using DialogMaker.Editor.Filters;

namespace DialogMaker.Editor
{
    public partial class ProjectController : Disposable
    {
        public ProjectController(DialogProject project)
        {
            _controllers.Add(this);

            Project = project;
            CreatePackCommand = new RelayCommand(ExecuteCreatePack);
            CreateLanguageCommand = new RelayCommand(p => project.CreateLanguage());
            SaveCommand = new RelayCommand(ExecuteSave);
            StringConverter = new(this);
            Languages = [];
            Packs = [];

            _languageConverter = new(this);
            _languageNameConverter = new(this);
            _languages = new(project.Languages, Languages, _languageConverter);
            _languagesName = new(Languages, new ObservableList<string>(), _languageNameConverter);
            LanguagesName = new((ObservableList<string>)_languagesName.SecondCollection);

            _packsConverter = new(this);
            _packs = new(project.Packs, Packs, _packsConverter);

            project.PropertyChanged += OnProjectPropertyChanged;
            project.Languages.ItemChanged += OnProjectLanguagesItemChanged;
            Languages.CollectionChanged += OnLanguagesCollectionChanged;

            Resources = new(this, project.Resources);

            UpdateDefaultLanguage();
        }

        public DialogProject Project { get; }
        public ObservableCollection<ProjectLanguage> Languages { get; }
        public ObservableCollection<ProjectPack> Packs { get; }
        public ReferenceReadOnlyList<string> LanguagesName { get; }
        public ProjectResources Resources { get; }
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
                    InvokePropertyChanging(nameof(DefaultLanguage));
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
                    InvokePropertyChanging(nameof(IsDefaultLanguageSetted));
                    field = value;
                    InvokePropertyChanged(nameof(IsDefaultLanguageSetted));
                }
            }
        }
        public ProjectStringConverter StringConverter { get; }
        public ProjectNodesFabric NodesViewFabric { get; } = new();
        public ProjectResourcesFilter ResourcesFilter { get; } = new();
        public ICommand CreatePackCommand { get; }
        public ICommand CreateLanguageCommand { get; }
        public ICommand SaveCommand { get; }

        private readonly ProjectPackConverter _packsConverter;
        private readonly ProjectLanguageConverter _languageConverter;
        private readonly ProjectLanguageNameConverter _languageNameConverter;
        private readonly CollectionSynchronizer<DialogProjectPack, ProjectPack> _packs;
        private readonly CollectionSynchronizer<DialogProjectLanguage, ProjectLanguage> _languages;
        private readonly CollectionSynchronizer<ProjectLanguage, string> _languagesName;

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

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Project.PropertyChanged -= OnProjectPropertyChanged;
            Languages.CollectionChanged -= OnLanguagesCollectionChanged;
            Project.Languages.ItemChanged -= OnProjectLanguagesItemChanged;

            Resources.Dispose();
            _languages.Dispose();
            _languagesName.Dispose();
            _languageConverter.Dispose();
            _packs.Dispose();

            _controllers.Remove(this);
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
            Save();
        }

        private void ExecuteSave(object? parameter)
        {
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

        private void OnProjectLanguagesItemChanged(object? sender, CollectionItemEventArgs<DialogProjectLanguage> e)
        {
            if (e.Action == CollectionItemAction.Add)
            {
                e.Item.PropertyChanged -= OnLanguagePropertyChanged;
                e.Item.PropertyChanged += OnLanguagePropertyChanged;
            }
            else if (e.Action == CollectionItemAction.Remove)
            {
                e.Item.PropertyChanged -= OnLanguagePropertyChanged;
            }
        }
        private void OnLanguagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Save();
        }
        private void OnLanguagePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            try
            {
                _languagesName.SyncFirstToSecond();
            }
            catch (Exception error)
            {
                error.Alert();
            }
        }

        #endregion

        #region Статика

        private static readonly List<ProjectController> _controllers = [];

        public static bool TryFindController(DialogProject? project, [NotNullWhen(true)] out ProjectController? result)
        {
            result = null;

            if (project == null)
            {
                return false;
            }

            foreach (var controller in _controllers)
            {
                if (controller.Project == project)
                {
                    result = controller;
                    return true;
                }
            }

            return false;
        }

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

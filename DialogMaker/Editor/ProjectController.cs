using DialogMaker.Core.Editor;
using DialogMaker.Editor.Data;
using DialogMaker.Editor.Filters;
using DialogMaker.Editor.Nodes;
using DialogMaker.Lib;
using DialogMaker.Lib.Controllers;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogMaker.Editor
{
    public partial class ProjectController : ProjectResourcesItem
    {
        public ProjectController(DialogProject project) : base(ProjectConverter, project)
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

            UpdateDefaultLanguage();
        }

        public new DialogProject Project { get; }
        public ObservableCollection<ProjectLanguage> Languages { get; }
        public ObservableCollection<ProjectPack> Packs { get; }
        public ReferenceReadOnlyList<string> LanguagesName { get; }
        public override string Name
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
                    OnPropertyChanging(nameof(DefaultLanguage));
                    field = value;
                    IsDefaultLanguageSetted = value != null;

                    if (Project.DefaultLanguage != value?.Language)
                    {
                        Project.DefaultLanguage = value?.Language;
                    }

                    OnPropertyChanged(nameof(DefaultLanguage));
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
                    OnPropertyChanging(nameof(IsDefaultLanguageSetted));
                    field = value;
                    OnPropertyChanged(nameof(IsDefaultLanguageSetted));
                }
            }
        }
        public ProjectStringConverter StringConverter { get; }
        public ProjectNodesFabric NodesViewFabric { get; } = new();
        public ProjectResourcesFilter ResourcesFilter { get; } = new();
        public ICommand CreatePackCommand { get; }
        public ICommand CreateLanguageCommand { get; }
        public ICommand SaveCommand { get; }
        public override string Icon => Icons.Unknown;
        public override ContextMenu? ContextMenu => null;
        public override IEnumerable? Children => Packs;
        public ProjectResourcesItem? LastShowedTabItem
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(LastShowedTabItem));
                    field = value;
                    OnPropertyChanged(nameof(LastShowedTabItem));
                }
            }
        }
        public override bool CanClose => false;
        public override IEnumerable<ActionButton>? Actions => null;

        private readonly ProjectPackConverter _packsConverter;
        private readonly ProjectLanguageConverter _languageConverter;
        private readonly ProjectLanguageNameConverter _languageNameConverter;
        private readonly CollectionSynchronizer<DialogProjectPack, ProjectPack> _packs;
        private readonly CollectionSynchronizer<DialogProjectLanguage, ProjectLanguage> _languages;
        private readonly CollectionSynchronizer<ProjectLanguage, string> _languagesName;

        #region Управление

        public void Save()
        {
            if (IsDisposed)
            {
                return;
            }

            try
            {
                Project.Save();
            }
            catch (Exception error)
            {
                error.Log();
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
            Project.Dispose();
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

        private async void ExecuteCreatePack(object? parameter)
        {
            var info = await ProjectItemCreationInfo.Create("Создать набор", "набор");

            if (info != null)
            {
                TryExecute(() => Project.CreatePack(info.Id, info.Name));
                Save();
            }
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
            OnPropertyChanged(e.PropertyName != null ? e.PropertyName : string.Empty);

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
                error.Log();
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

        private static ProjectController ProjectConverter(ProjectResourcesOwner owner)
        {
            if (owner is ProjectController project)
            {
                return project;
            }

            throw new InvalidCastException();
        }

        #endregion
    }
}

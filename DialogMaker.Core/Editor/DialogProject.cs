using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Editor
{
    public class DialogProject : Disposable, IProjectResourcesOwner
    {
        public DialogProject(string projectPath, string id)
            : this(projectPath, id, true)
        {
        }
        public DialogProject(string projectPath, DialogProjectSavedState savedState)
            : this(projectPath, savedState.Id, false)
        {
            Name = savedState.Name;

            foreach (var pack in savedState.Packs)
            {
                string packFolder = Path.Combine(projectPath, pack);

                try
                {
                    var projectPack = DialogProjectPack.Open(this, packFolder);
                    Packs.Add(projectPack);
                }
                catch (Exception error)
                {
                    Logger.Log(error);
                }
            }

            foreach (var language in savedState.Languages)
            {
                Languages.Add(new(this, language));
            }

            if (savedState.DefaultLanguage != null &&
                Guid.TryParse(savedState.DefaultLanguage, out var defaultLanguageId) &&
                TryGetLanguage(defaultLanguageId, out var defaultLanguage))
            {
                DefaultLanguage = defaultLanguage;
            }

            Resources = DialogProjectResources.OpenOrCreate(this, DialogResourcesFlags.Root);
        }
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Рассмотрите возможность добавления модификатора "required" или объявления значения, допускающего значение NULL.
        private DialogProject(string projectPath, string id, bool createResources)
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Рассмотрите возможность добавления модификатора "required" или объявления значения, допускающего значение NULL.
        {
            Id = id;
            ProjectPath = projectPath;

            if (createResources)
            {
                Resources = new(this, DialogResourcesFlags.Root);
            }

            Languages = [];

            Languages.ItemChanged += OnLanguagesItemChanged;
        }

        public event EventHandler<ItemActionEventArgs<DialogProjectPack>>? PacksChanged;
        public event EventHandler<ResourceItemPathChangedEventArgs>? ResourceItemPathChanged;

        public string ProjectPath { get; }
        public string Id { get; }
        public string Name
        {
            get => field ?? string.Empty;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(Name));
                    field = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        public DialogProjectLanguage? DefaultLanguage
        {
            get => field;
            set
            {
                if (field != value)
                {
                    if (value != null && value.Project != this)
                    {
                        throw new ArgumentException($"Невозможно задать язык по умолчанию, так как его владельцем является другой проект.", nameof(value));
                    }

                    OnPropertyChanging(nameof(DefaultLanguage));
                    field = value;
                    OnPropertyChanged(nameof(DefaultLanguage));
                }
            }
        }
        public EditableCollection<DialogProjectPack> Packs { get; } = [];
        public EditableCollection<DialogProjectLanguage> Languages { get; }
        public DialogProjectResources Resources { get; }

        IProjectResourcesOwner? IProjectResourcesOwner.Parent => null;
        IResourcesOwner? IResourcesOwner.Parent => null;
        DialogProject IProjectResourcesOwner.Project => this;
        string IProjectResourcesOwner.Folder => ProjectPath;
        IResourcesOwner IResourcesOwner.Root => this;

        IResourcesContainer IResourcesOwner.Resources => Resources;

        private readonly List<DialogProjectResourceObject> _registeredResources = [];

        #region Управление

        public void Save()
        {
            CheckHelper.CheckIsDisposed(this);
            Resources.Save();

            foreach (var pack in Packs)
            {
                pack.Save();
            }

            DialogProjectSavedState savedState = new()
            {
                Id = Id,
                Name = Name,
                DefaultLanguage = DefaultLanguage?.ProjectId.ToString(),
                Packs = [.. Packs.Select(p => p.Id)],
                Languages = [.. Languages.Select(l => (DialogProjectLanguageSavedState)l.Save())],
            };

            string filePath = Path.Combine(ProjectPath, $"{Id}.{FileExtension}");

            savedState.Save(filePath);
        }

        public bool TryGetPack(string id, [NotNullWhen(true)] out DialogProjectPack? result)
        {
            return Packs.TryGetValue(p => p.Id == id, out result);
        }
        public bool TryGetLanguage(string id, [NotNullWhen(true)] out DialogProjectLanguage? result)
        {
            return Languages.TryGetValue(l => l.Id == id, out result);
        }
        public bool TryGetLanguage(Guid id, [NotNullWhen(true)] out DialogProjectLanguage? result)
        {
            return Languages.TryGetValue(l => l.ProjectId == id, out result);
        }
        bool IResourcesOwner.TryFindChild(string id, [NotNullWhen(true)] out IResourcesOwner? result)
        {
            return Packs.TryGetValue(p => p.Id == id, out result);
        }

        public DialogProjectPack CreatePack(string id, string name)
        {
            if (TryGetPack(id, out _))
            {
                throw new ArgumentException($"Набор диалогов с идентификатором {id} уже существует.", nameof(id));
            }

            DialogProjectPack pack = new(this, id)
            {
                Name = name
            };

            if (!Directory.Exists(pack.Folder))
            {
                Directory.CreateDirectory(pack.Folder);
            }

            Packs.Add(pack);

            PacksChanged?.Invoke(this, new(ItemAction.Add, pack));

            return pack;
        }
        public bool RemovePack(DialogProjectPack pack)
        {
            if (Packs.Remove(pack))
            {
                PacksChanged?.Invoke(this, new(ItemAction.Remove, pack));
                return true;
            }

            return false;
        }

        public DialogProjectLanguage CreateLanguage()
        {
            return CreateLanguage("Идентификатор языка", "Название языка");
        }
        public DialogProjectLanguage CreateLanguage(string id, string name)
        {
            DialogProjectLanguage language = new(this)
            {
                Id = id,
                Name = name
            };

            Languages.Add(language);

            return language;
        }
        public bool RemoveLanguage(DialogProjectLanguage language)
        {
            return Languages.Remove(language);
        }

        public override string ToString()
        {
            return $"[{Id}] {Name}";
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Languages.ItemChanged -= OnLanguagesItemChanged;

            foreach (var pack in Packs)
            {
                pack.Dispose();
            }

            Packs.Clear();
            Resources.Dispose();
        }

        #endregion

        #region Ресурсы

        internal bool Register(DialogProjectResourceObject obj)
        {
            if (_registeredResources.Contains(obj))
            {
                return false;
            }

            obj.PathChanged += OnResourceItemPathChanged;

            _registeredResources.Add(obj);

            return true;
        }
        internal bool Unregister(DialogProjectResourceObject obj)
        {
            if (_registeredResources.Remove(obj))
            {
                obj.PathChanged -= OnResourceItemPathChanged;
                return true;
            }

            return false;
        }

        #endregion

        #region События

        private void OnResourceItemPathChanged(object sender, ResourceItemPathChangedEventArgs e)
        {
            ResourceItemPathChanged?.Invoke(this, e);
        }
        private void OnLanguagesItemChanged(object sender, CollectionItemEventArgs<DialogProjectLanguage> e)
        {
            if (e.Action == CollectionItemAction.Remove && e.Item == DefaultLanguage)
            {
                DefaultLanguage = null;
            }
        }

        #endregion

        #region Статика

        public const string FileExtension = "dproj";
        public const string FileFilter = $"Dialog projects (.{FileExtension})|*.{FileExtension}";

        public static DialogProject Open(string projectFilePath)
        {
            return SavedState.Restore<DialogProject, DialogProjectSavedState>(projectFilePath);
        }
        public static DialogProject Create(string id, string projectFolder)
        {
            return new(projectFolder, id);
        }

        #endregion
    }
}

using Acly;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace DialogMaker.Core.Editor
{
    public class DialogProject : ObservableObject, IProjectResourcesOwner, IDisposable
    {
        public DialogProject(string projectPath, string id)
            : this(projectPath, id, true)
        {
        }
        public DialogProject(string projectPath, DialogProjectSavedState savedState) 
            : this(projectPath, savedState.Id, false)
        {
            foreach (var pack in savedState.Packs)
            {
                string packFolder = Path.Combine(projectPath, pack);

                try
                {
                    var projectPack = DialogProjectPack.Open(this, packFolder);
                    _packs.Add(projectPack);
                }
                catch
                {
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
                _defaultLanguage = defaultLanguage;
            }
        }
        private DialogProject(string projectPath, string id, bool createResources)
        {
            Id = id;
            ProjectPath = projectPath;

            if (createResources)
            {
                Resources = new(this);
            }
            else
            {
                Resources = DialogProjectResources.OpenOrCreate(this);
            }

            _packs = new();
            _languages = new();
            Packs = new(_packs);

            _languages.ItemChanged += OnLanguagesItemChanged;
        }
        ~DialogProject()
        {
            Dispose();
        }

        public event EventHandler<ItemActionEventArgs<DialogProjectPack>>? PacksChanged;

        public string ProjectPath { get; }
        public string Id { get; }
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    InvokePropertyChanged(nameof(Name));
                }
            }
        }
        public DialogProjectLanguage? DefaultLanguage
        {
            get => _defaultLanguage;
            set
            {
                if (_defaultLanguage != value)
                {
                    if (value != null && value.Project != this)
                    {
                        throw new ArgumentException($"Невозможно задать язык по умолчанию, так как его владельцем является другой проект.", nameof(value));
                    }

                    _defaultLanguage = value;
                    InvokePropertyChanged(nameof(DefaultLanguage));
                }
            }
        }
        public ReferenceReadOnlyList<DialogProjectPack> Packs { get; }
        public ObservableList<DialogProjectLanguage> Languages => _languages;
        public DialogProjectResources Resources { get; }

        DialogProject IProjectResourcesOwner.Project => this;
        string IProjectResourcesOwner.Folder => ProjectPath;

        private readonly ObservableList<DialogProjectPack> _packs;
        private readonly EditableCollection<DialogProjectLanguage> _languages;
        private string _name = string.Empty;
        private DialogProjectLanguage? _defaultLanguage;

        #region Управление

        public void Save()
        {
            Resources.Save();

            foreach (var pack in _packs)
            {
                pack.Save();
            }

            DialogProjectSavedState savedState = new()
            {
                Id = Id,
                Name = Name,
                DefaultLanguage = _defaultLanguage?.ProjectId.ToString(),
                Packs = _packs.Select(p => p.Id).ToArray(),
                Languages = Languages.Select(l => (DialogProjectLanguageSavedState)l.Save()).ToArray(),
            };

            string filePath = Path.Combine(ProjectPath, $"{Id}.{FileExtension}");

            savedState.Save(filePath);
        }

        public bool TryGetPack(string id, [NotNullWhen(true)] out DialogProjectPack? result)
        {
            return _packs.TryGetValue(p => p.Id == id, out result);
        }
        public bool TryGetLanguage(string id, [NotNullWhen(true)] out DialogProjectLanguage? result)
        {
            return Languages.TryGetValue(l => l.Id == id, out result);
        }
        public bool TryGetLanguage(Guid id, [NotNullWhen(true)] out DialogProjectLanguage? result)
        {
            return Languages.TryGetValue(l => l.ProjectId == id, out result);
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

            _packs.Add(pack);

            PacksChanged?.Invoke(this, new(ItemAction.Add, pack));

            return pack;
        }
        public bool RemovePack(DialogProjectPack pack)
        {
            if (_packs.Remove(pack))
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

        public void Dispose()
        {
            _languages.ItemChanged -= OnLanguagesItemChanged;
            GC.SuppressFinalize(this);
        }

        #endregion

        #region События

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

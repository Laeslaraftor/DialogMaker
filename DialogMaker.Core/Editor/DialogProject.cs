using Acly;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace DialogMaker.Core.Editor
{
    public class DialogProject : ObservableObject
    {
        public DialogProject(string projectPath, string id)
        {
            Id = id;
            ProjectPath = projectPath;
            Languages = new EditableCollection<DialogProjectLanguage>();

            _packs = new();
            Packs = new(_packs);
        }
        public DialogProject(string projectPath, DialogProjectSavedState savedState) 
            : this(projectPath, savedState.Id)
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
        public ObservableList<DialogProjectLanguage> Languages { get; }

        private readonly ObservableList<DialogProjectPack> _packs;
        private string _name = string.Empty;
        private DialogProjectLanguage? _defaultLanguage;

        #region Управление

        public void Save()
        {
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

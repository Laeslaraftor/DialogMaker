using DialogMaker.Core.Editor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DialogMaker.Core.Common
{
    public class DialogPackage : Disposable, IDialogResourcesContainer
    {
        public DialogPackage(DialogProject project, string folder)
        {
            var languages = project.Languages.Select(l =>
            {
                DialogLanguage lang = new(this, l);

                if (l.Id == project.DefaultLanguage?.Id)
                {
                    CurrentLanguage = lang;
                }

                return lang;
            }).ToDictionary(v => v.Id);

            Folder = folder;
            Id = project.Id;
            Name = project.Name;
            Languages = new(languages);

            foreach (var pack in project.Packs)
            {
                _folders.Add(pack.Id, null);
            }

            Folders = new([.. _folders.Keys]);
            Resources = new(this, project.Resources);
        }

        public string Id { get; }
        public string Name { get; }
        public string Folder { get; }
        public ReadOnlyDictionary<string, DialogLanguage> Languages { get; }
        public DialogLanguage? CurrentLanguage
        {
            get => field;
            set
            {
                if (field != value)
                {
                    if (value != null && value.Package != this)
                    {
                        throw new ArgumentException("Невозможно задать язык из другого пакета диалогов!", nameof(value));
                    }

                    InvokePropertyChanging(nameof(CurrentLanguage));
                    field = value;
                    InvokePropertyChanged(nameof(CurrentLanguage));
                }
            }
        }
        public IDialogResourcesContainer? Parent => null;
        public DialogResources Resources { get; }
        public ReadOnlyCollection<string> Folders { get; }
        public DialogFolder this[string id]
        {
            get
            {
                if (!TryGetFolder(id, out var folder))
                {
                    throw new ArgumentException($"Папка диалогов с идентификатором \"{id}\" не найдена!", nameof(id));
                }

                return folder;
            }
        }

        DialogPackage IDialogResourcesContainer.Package => this;
        IResourcesOwner IResourcesOwner.Root => this;
        IResourcesOwner? IResourcesOwner.Parent => null;
        IResourcesContainer IResourcesOwner.Resources => Resources;

        private readonly Dictionary<string, DialogFolder?> _folders = [];

        #region Управление

        public bool TryGetFolder(string id, [NotNullWhen(true)] out DialogFolder? result)
        {
            if (!_folders.TryGetValue(id, out result))
            {
                return false;
            }
            if (result == null)
            {
                result = DialogFolder.Open(this, id);
                _folders[id] = result;
            }

            return true;
        }
        bool IResourcesOwner.TryFindChild(string id, [NotNullWhen(true)] out IResourcesOwner? result)
        {
            result = null;

            if (TryGetFolder(id, out var folder))
            {
                result = folder;
                return true;
            }

            return false;
        }

        internal T GetResource<T>(ResourcePath path) where T : DialogResourceObject
        {
            var resource = IResourcesOwner.FindResource(this, path);

            if (resource is not T obj)
            {
                throw new InvalidProgramException($"Получен ресурс неподдерживаемого типа: {resource.GetType().FullName}");
            }

            return obj;
        }
        internal DialogResourceObject GetResource(ResourcePath path)
        {
            return GetResource<DialogResourceObject>(path);
        }

        public override string ToString()
        {
            return $"[{nameof(DialogPackage)}: {Id}] {Name}";
        }

        #endregion

        #region Константы

        public const string CurrentLanguageProperty = nameof(CurrentLanguage);

        #endregion
    }
}

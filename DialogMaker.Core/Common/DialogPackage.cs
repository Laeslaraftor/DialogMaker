using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace DialogMaker.Core.Common
{
    public class DialogPackage : ResourcesContainer, IDialogResourcesContainer
    {
        public DialogPackage(DialogPackageSavedState savedState, string folder)
        {
            Folder = folder;
            Id = savedState.Id;
            Name = savedState.Name;

            Dictionary<string, DialogLanguage> languages = new(savedState.Languages.Count);

            foreach (var info in savedState.Languages)
            {
                languages.Add(info.Key, new(this, info.Value));
            }

            Languages = new(languages);
            Resources = DialogResources.Open(this);

            if (savedState.CurrentLanguage != null 
                && languages.TryGetValue(savedState.CurrentLanguage, out var currentLanguage))
            {
                CurrentLanguage = currentLanguage;
            }

            _folders = [];
            Folders = new(savedState.Folders);

            foreach (var dialogsFolder in savedState.Folders)
            {
                _folders.Add(dialogsFolder, null);
            }
        }
        private DialogPackage(DialogProject project, string folder, Dictionary<string, DialogFolder?> folders)
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

            _folders = folders;
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
        public override DialogResources Resources { get; }
        public ReadOnlyCollection<string> Folders { get; }
        public DialogFolder this[string id]
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException($"Не возможно получить папку диалогов с идентификатором {id}, так как набор диалогов очищен");
                }
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

        private readonly Dictionary<string, DialogFolder?> _folders;

        #region Управление

        public void Save()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("Невозможно сохранить очищенный набор диалогов");
            }

            string filePath = Path.Combine(Folder, $"{Id}.{FileExtension}");
            Dictionary<string, DialogLanguageSavedState> languages = new(Languages.Count);

            foreach (var info in Languages)
            {
                languages.Add(info.Key, info.Value.Save());
            }
            foreach (var folder in _folders.Values)
            {
                folder?.Save();
            }

            DialogPackageSavedState savedState = new()
            {
                Id = Id,
                Name = Name,
                Languages = languages,
                CurrentLanguage = CurrentLanguage?.Id,
                Folders = [.. Folders]
            };

            FileExtensions.CreateDirectory(Folder);

            Resources.Save();
            savedState.Save(filePath);
        }

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
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool TryFindChild(string id, [NotNullWhen(true)] out IResourcesOwner? result)
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

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            foreach (var folder in _folders.Values)
            {
                folder?.Dispose();
            }

            _folders.Clear();
            Resources.Dispose();
        }
        protected override IEnumerable<DialogResources> GetChildResources()
        {
            foreach (var folderId in Folders)
            {
                yield return this[folderId].Resources;
            }
        }

        #endregion

        #region Константы

        public const string CurrentLanguageProperty = nameof(CurrentLanguage);
        public const string FileExtension = "dpack";

        #endregion

        #region Статика

        public static DialogPackage Create(DialogProject project, string outputFolder)
        {
            Dictionary<string, DialogFolder?> folders = project.Packs.ToDictionary<DialogProjectPack, string, DialogFolder?>(i => i.Id, i => null);
            DialogPackage result = new(project, outputFolder, folders);

            foreach (var pack in project.Packs)
            {
                var folder = DialogFolder.Create(result, pack);
                folders[pack.Id] = folder;
            }

            return new(project, outputFolder, folders);
        }
        public static IEnumerable<ProgressResult<DialogPackage>> CreateWithProgress(DialogProject project, string outputFolder)
        {
            Dictionary<string, DialogFolder?> folders = project.Packs.ToDictionary<DialogProjectPack, string, DialogFolder?>(i => i.Id, i => null);
            DialogPackage result = new(project, outputFolder, folders);
            ProgressResult<DialogPackage> progressResult = new()
            {
                Value = result,
                Extra = project,
            };
            int packsCount = project.Packs.Count;
            float progressPerFolder = 1f / packsCount;
            float count = 0;

            yield return progressResult;

            foreach (var pack in project.Packs)
            {
                float baseProgress = progressResult.Progress;
                DialogFolder? folder = null;
                //progressResult.Extra = pack;

                yield return new()
                {
                    Value = result,
                    Extra = pack
                };

                yield return progressResult;

                foreach (var progress in DialogFolder.CreateWithProgress(result, pack))
                {
                    folder = progress.Value;
                    progressResult.Progress = baseProgress + progressPerFolder * progress.Progress;
                    progressResult.LocalProgress = progressResult.Progress;

                    yield return new()
                    {
                        Value = result,
                        LocalProgress = progress.Progress,
                        Progress = progress.Progress,
                        Extra = progress.Extra,
                    };

                    yield return progressResult;
                }

                if (folder == null)
                {
                    throw new InvalidDataException($"Не удалось получить папку диалогов для набора: {pack}");
                }

                folders[pack.Id] = folder;
                count++;
                progressResult.Progress = count / packsCount;
                progressResult.LocalProgress = progressResult.Progress;

                yield return progressResult;
            }

            progressResult.Value = result;
            progressResult.IsCompleted = true;

            yield return progressResult;
        }
        public static DialogPackage Open(string filePath)
        {
            var folder = filePath.GetFileDirectory();
            var data = File.ReadAllBytes(filePath);
            var savedState = MessagePackSerializer.Deserialize<DialogPackageSavedState>(data);

            return new(savedState, folder);
        }

        #endregion
    }
}

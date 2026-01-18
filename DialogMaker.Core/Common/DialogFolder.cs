using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace DialogMaker.Core.Common
{
    public class DialogFolder : ResourcesContainer, IDialogResourcesContainer
    {
        public DialogFolder(DialogPackage package, DialogFolderSavedState savedState)
        {
            Package = package;
            Id = savedState.Id;
            Name = savedState.Name;
            Folder = Path.Combine(package.Folder, savedState.Id);
            Dialogs = new(savedState.Dialogs);
            Resources = DialogResources.Open(this);

            _dialogs = new(savedState.Dialogs.Count);

            foreach (var dialogId in savedState.Dialogs)
            {
                _dialogs.Add(dialogId, null);
            }
        }
        private DialogFolder(DialogPackage package, DialogProjectPack pack, Dictionary<string, Dialog?> dialogs)
        {
            Package = package;  
            Id = pack.Id;
            Name = pack.Name;
            Folder = Path.Combine(package.Folder, pack.Id);

            _dialogs = dialogs;
            Dialogs = new([.. _dialogs.Keys]);
            Resources = new(this, pack.Resources);
        }

        public DialogPackage Package { get; }
        public string Id { get; }
        public string Name { get; }
        public string Folder { get; }
        public override DialogResources Resources { get; }
        public ReadOnlyCollection<string> Dialogs { get; }
        public Dialog this[string id]
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException($"Невозможно получить диалог с идентификатором {id}, так как папка диалогов была очищена");
                }
                if (TryGetDialog(id, out var dialog))
                {
                    return dialog;
                }

                throw new ArgumentException($"Неизвестный идентификатор диалога: {id}");
            }
        }

        IResourcesOwner IResourcesOwner.Root => Package;
        IResourcesOwner? IResourcesOwner.Parent => Package;
        IResourcesContainer IResourcesOwner.Resources => Resources;

        private readonly Dictionary<string, Dialog?> _dialogs;

        #region Управление

        public void Save()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("Невозможно сохранить очищенную папку диалогов");
            }

            foreach (var dialog in _dialogs.Values)
            {
                dialog?.Save();
            }

            var filePath = Path.Combine(Folder, $"{Id}.{FileExtension}");
            DialogFolderSavedState savedState = new()
            {
                Id = Id,
                Name = Name,
                Dialogs = [.. Dialogs]
            };

            FileExtensions.CreateDirectory(Folder);

            Resources.Save();
            savedState.Save(filePath);
        }

        public bool TryGetDialog(string id, [NotNullWhen(true)] out Dialog? result)
        {
            if (!_dialogs.TryGetValue(id, out result))
            {
                return false;
            }
            if (result == null)
            {
                result = Dialog.Open(this, id);
                _dialogs[id] = result;
            }

            return true;
        }

        public override bool TryFindChild(string id, [NotNullWhen(true)] out IResourcesOwner? result)
        {
            result = null;

            if (TryGetDialog(id, out var dialog))
            {
                result = dialog;
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return $"[{nameof(DialogFolder)}: {Id}] {Name}";
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            foreach (var dialog in _dialogs.Values)
            {
                dialog?.Dispose();
            }

            _dialogs.Clear();
            Resources.Dispose();
        }
        protected override IEnumerable<DialogResources> GetChildResources()
        {
            foreach (var dialogId in Dialogs)
            {
                yield return this[dialogId].Resources;
            }
        }

        #endregion

        #region Константы

        public const string FileExtension = "dfolder";

        #endregion

        #region Статика

        public static DialogFolder Create(DialogPackage package, DialogProjectPack pack)
        {
            Dictionary<string, Dialog?> dialogs = pack.Dialogs.ToDictionary<DialogProjectDialog, string, Dialog?>(i => i.Id, i => null);
            DialogFolder folder = new(package, pack, dialogs);

            foreach (var dialogProject in pack.Dialogs)
            {
                var dialog = Dialog.Create(folder, dialogProject);
                dialogs[dialogProject.Id] = dialog;
            }

            return folder;
        }
        public static IEnumerable<ProgressResult<DialogFolder>> CreateWithProgress(DialogPackage package, DialogProjectPack pack)
        {
            int totalDialogsCount = pack.Dialogs.Count;
            Dictionary<string, Dialog?> dialogs = pack.Dialogs.ToDictionary<DialogProjectDialog, string, Dialog?>(i => i.Id, i => null);
            DialogFolder folder = new(package, pack, dialogs);
            ProgressResult<DialogFolder> progressResult = new()
            {
                Value = folder
            };
            float count = 0;

            foreach (var dialogProject in pack.Dialogs)
            {
                var dialog = Dialog.Create(folder, dialogProject);
                dialogs[dialogProject.Id] = dialog;
                count++;
                progressResult.Progress = count / totalDialogsCount;

                yield return progressResult;
            }

            progressResult.Progress = 1;
            progressResult.IsCompleted = true;

            yield return progressResult;
        }
        public static DialogFolder Open(DialogPackage package, string id)
        {
            var filePath = Path.Combine(package.Folder, id, $"{id}.{FileExtension}");
            var data = File.ReadAllBytes(filePath);
            var savedState = MessagePackSerializer.Deserialize<DialogFolderSavedState>(data);

            return new(package, savedState);
        }

        #endregion
    }
}

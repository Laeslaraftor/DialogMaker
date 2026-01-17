using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Executioning;
using System;
using MessagePack;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace DialogMaker.Core.Common
{
    public class Dialog : Disposable, IDialogResourcesContainer
    {
        public Dialog(DialogFolder folder, DialogSavedState savedState)
        {
            Parent = folder;
            Id = savedState.Id;
            Name = savedState.Name;
            Folder = Path.Combine(folder.Folder, savedState.Id);
            Resources = DialogResources.Open(this);

            Dictionary<int, DialogItemReference> resourcesIndex = new(savedState.ResourcesIndex.Count);

            foreach (var info in savedState.ResourcesIndex)
            {
                resourcesIndex.Add(info.Key, DialogItemReference.Parse(info.Value));
            }

            _resourcesIndex = new(resourcesIndex);
            _bytecode = savedState.Bytecode;
        }
        private Dialog(DialogFolder folder, DialogProjectDialog dialog, DialogCompilerOutput compiledDialog)
        {
            Parent = folder;
            Id = dialog.Id;
            Name = dialog.Name;
            Folder = Path.Combine(folder.Folder, dialog.Id);
            Resources = new(this, dialog.Resources);

            _resourcesIndex = new(compiledDialog.Context.GetGlobalValues());
            _bytecode = compiledDialog.Code;
        }

        public DialogPackage Package => Parent.Package;
        public DialogFolder Parent { get; }
        public string Id { get; }
        public string Name { get; }
        public string Folder { get; }
        public DialogResources Resources { get; }

        IResourcesOwner IResourcesOwner.Root => Package;
        IResourcesOwner? IResourcesOwner.Parent => Parent;
        IResourcesContainer IResourcesOwner.Resources => Resources;

        private readonly ReadOnlyDictionary<int, DialogItemReference> _resourcesIndex;
        private readonly byte[] _bytecode;

        #region Управление

        public void Save()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("Невозможно сохранить очищенный диалог");
            } 

            var filePath = Path.Combine(Folder, $"{Id}.{FileExtension}");
            Dictionary<int, string> resourcesIndexes = new(_resourcesIndex.Count);

            foreach (var info in _resourcesIndex)
            {
                resourcesIndexes.Add(info.Key, info.Value.ToString());
            }

            DialogSavedState savedState = new()
            {
                Id = Id,
                Name = Name,
                Bytecode = _bytecode,
                ResourcesIndex = resourcesIndexes
            };

            FileExtensions.CreateDirectory(Folder);

            Resources.Save();
            savedState.Save(filePath);
        }

        bool IResourcesOwner.TryFindChild(string id, [NotNullWhen(true)] out IResourcesOwner? result)
        {
            result = null;
            return false;
        }

        public override string ToString()
        {
            return $"[{nameof(Dialog)}: {Id}] {Name}";
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Resources.Dispose();
        }

        #endregion

        #region Константы

        public const string FileExtension = "dialog";

        #endregion

        #region Статика

        public static Dialog Create(DialogFolder folder, DialogProjectDialog dialog)
        {
            var compiler = DialogCompiler.Create(dialog);
            var compiledDialog = compiler.Compile();

            return new(folder, dialog, compiledDialog);
        }
        public static Dialog Open(DialogFolder folder, string id)
        {
            var filePath = Path.Combine(folder.Folder, id, $"{id}.{FileExtension}");
            var data = File.ReadAllBytes(filePath);
            var savedState = MessagePackSerializer.Deserialize<DialogSavedState>(data);

            return new(folder, savedState);
        }

        #endregion
    }
}

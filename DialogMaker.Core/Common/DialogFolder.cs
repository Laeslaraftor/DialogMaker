using DialogMaker.Core.Editor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace DialogMaker.Core.Common
{
    public class DialogFolder : Disposable, IDialogResourcesContainer
    {
        public DialogFolder(DialogPackage package, DialogProjectPack pack)
        {
            Package = package;  
            Id = pack.Id;
            Name = pack.Name;
            Folder = Path.Combine(package.Folder, pack.Id);

            foreach (var dialog in pack.Dialogs)
            {
                _dialogs.Add(dialog.Id, new(this, dialog));
            }

            Dialogs = new([.. _dialogs.Keys]);
            Resources = new(this, pack.Resources);
        }

        public DialogPackage Package { get; }
        public string Id { get; }
        public string Name { get; }
        public string Folder { get; }
        public DialogResources Resources { get; }
        public ReadOnlyCollection<string> Dialogs { get; }
        public Dialog this[string id]
        {
            get
            {
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


        private readonly Dictionary<string, Dialog?> _dialogs = [];

        #region Управление

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

        bool IResourcesOwner.TryFindChild(string id, [NotNullWhen(true)] out IResourcesOwner? result)
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

        #endregion

        #region Статика

        public static DialogFolder Open(DialogPackage package, string id)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

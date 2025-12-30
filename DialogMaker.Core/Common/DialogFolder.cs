using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Common
{
    public class DialogFolder : Disposable, IDialogResourcesContainer
    {
        public DialogPackage Package { get; }
        public string Id { get; }
        public string Name { get; }
        public int DialogsCount { get; }
        public DialogResources Resources => throw new NotImplementedException();
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

        private bool TryGetDialog(string id, [NotNullWhen(true)] out Dialog? result)
        {
            result = null;

            if (_dialogs.TryGetValue(id, out var dialog))
            {
                dialog ??= Dialog.Open(this, id);
                _dialogs[id] = dialog;
            }
            else
            {
                return false;
            }

            result = dialog;

            return true;
        }
        

        #endregion
    }
}

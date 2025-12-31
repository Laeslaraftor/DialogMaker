using DialogMaker.Core.Editor;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace DialogMaker.Core.Common
{
    public class Dialog : Disposable, IDialogResourcesContainer
    {
        public Dialog(DialogFolder folder, DialogProjectDialog dialog)
        {
            Parent = folder;
            Id = dialog.Id;
            Name = dialog.Name;
            Folder = Path.Combine(folder.Folder, dialog.Id);
            Resources = new(this, dialog.Resources);
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


        #region Управление

        bool IResourcesOwner.TryFindChild(string id, [NotNullWhen(true)] out IResourcesOwner? result)
        {
            result = null;
            return false;
        }

        public override string ToString()
        {
            return $"[{nameof(Dialog)}: {Id}] {Name}";
        }

        #endregion

        #region Статика

        internal static Dialog Open(DialogFolder folder, string id)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

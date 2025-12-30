using System;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Common
{
    public class Dialog : Disposable, IDialogResourcesContainer
    {
        public DialogPackage Package => Folder.Package;
        public DialogFolder Folder { get; }
        public DialogResources Resources => throw new NotImplementedException();
        public string Name { get; }

        public IResourcesOwner Root => throw new NotImplementedException();

        public IResourcesOwner? Parent => throw new NotImplementedException();

        public string Id => throw new NotImplementedException();

        IResourcesContainer IResourcesOwner.Resources => Resources;

        #region Управление

        bool IResourcesOwner.TryFindChild(string id, [NotNullWhen(true)] out IResourcesOwner? result)
        {
            result = null;
            return false;
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

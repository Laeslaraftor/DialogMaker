using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Executioning;
using System;

namespace DialogMaker.Core.Common
{
    public abstract class DialogResourceObject : Disposable, IResource
    {
        public DialogResourceObject(DialogResources resources, DialogProjectResourceObject resourceObject)
        {
            Id = resourceObject.Id;
            Resources = resources;
            IsSeparated = resourceObject.IsSeparated;
        }
        public DialogResourceObject(DialogResources resources, DialogResourceObjectSavedState savedState)
        {
            Id = savedState.Id;
            Resources = resources;
            IsSeparated = savedState.IsSeparated;
        }

        public abstract DialogResourceType ResourceType { get; }
        public string Id { get; }
        public DialogResources Resources { get; }
        public bool IsSeparated { get; }

        IResourcesContainer IResource.Container => Resources;

        #region Управление

        public DialogItemReference CreateReference()
        {
            return DialogItemReference.CreateUnknown(this);
        }
        public ResourcePath GetPath()
        {
            if (IsSeparated)
            {
                throw new InvalidOperationException(IResourceItem.GetPathExceptionMessage);
            }

            return ResourcePath.CreatePath(this);
        }
        public IVariable ToVariable()
        {
            throw new System.NotImplementedException();
        }

        public DialogResourceObjectSavedState Save()
        {
            DialogResourceObjectSavedState result = CreateSavedState();
            result.Id = Id;
            result.IsSeparated = IsSeparated;

            return result;
        }

        protected abstract DialogResourceObjectSavedState CreateSavedState();

        #endregion
    }
}

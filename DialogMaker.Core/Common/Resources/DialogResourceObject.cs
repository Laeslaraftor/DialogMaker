using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Executioning;

namespace DialogMaker.Core.Common
{
    public abstract class DialogResourceObject : Disposable, IResource
    {
        public DialogResourceObject(DialogResources resources, DialogProjectResourceObject resourceObject)
        {
            Id = resourceObject.Id;
            Resources = resources;
        }
        public DialogResourceObject(DialogResources resources, DialogResourceObjectSavedState savedState)
        {
            Id = savedState.Id;
            Resources = resources;
        }

        public abstract DialogResourceType ResourceType { get; }
        public string Id { get; }
        public DialogResources Resources { get; }
        public bool IsSeparated => false;

        IResourcesContainer IResource.Container => Resources;

        #region Управление

        public DialogItemReference CreateReference()
        {
            return DialogItemReference.CreateUnknown(this);
        }
        public ResourcePath GetPath()
        {
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

            return result;
        }

        protected abstract DialogResourceObjectSavedState CreateSavedState();

        #endregion
    }
}

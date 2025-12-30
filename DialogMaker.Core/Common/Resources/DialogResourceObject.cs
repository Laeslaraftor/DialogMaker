using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;

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

        IResourcesContainer IResource.Container => Resources;

        #region Управление

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

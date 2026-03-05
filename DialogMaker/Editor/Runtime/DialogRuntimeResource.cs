using DialogMaker.Core;
using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning;

namespace DialogMaker.Editor.Runtime
{
    public class DialogRuntimeResource(int index, IResourcesOwner resourcesOwner, DialogItemReference reference) : Disposable
    {
        public int Index { get; } = index;
        public IResourcesOwner ResourcesOwner { get; } = resourcesOwner;
        public DialogItemReference Reference { get; } = reference;
        public IResourceItem? ResourceItem
        {
            get
            {
                if (field == null)
                {
                    try
                    {
                        field = Reference.GetItem(ResourcesOwner);
                    }
                    catch (Exception error)
                    {
                        error.Log();
                    }
                }

                return field;
            }
        }

        #region Управление

        public override string ToString()
        {
            return Reference.ToString();
        }

        #endregion
    }
}

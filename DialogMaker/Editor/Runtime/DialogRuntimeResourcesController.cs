using DialogMaker.Core;
using DialogMaker.Core.Executioning;
using System.Collections.ObjectModel;

namespace DialogMaker.Editor.Runtime
{
    public class DialogRuntimeResourcesController : Disposable
    {
        public DialogRuntimeResourcesController(DialogRuntimeResources resources)
        {
            Resources = resources;
            Items = new([.. resources.Items.Select(i => new DialogRuntimeResource(i.Key, resources.ResourcesOwner, i.Value))]);
        }

        public DialogRuntimeResources Resources { get; }
        public ReadOnlyCollection<DialogRuntimeResource> Items { get; }

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            foreach (var item in Items)
            {
                item.Dispose();
            }
        }

        #endregion
    }
}

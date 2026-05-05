using DialogMaker.Core;
using DialogMaker.Core.Executioning.Builders;
using System.Collections.ObjectModel;

namespace DialogMaker.Editor.Runtime
{
    public class DialogRuntimeResourcesController : Disposable
    {
        public DialogRuntimeResourcesController(IResourcesOwner resourcesOwner, DialogExecutionContextBuilder contextBuilder)
        {
            SortedDictionary<int, DialogRuntimeResource> items = [];

            foreach (var info in contextBuilder.GetGlobalValues())
            {
                items.Add(info.Key, new(info.Key, resourcesOwner, info.Value));
            }
            foreach (var info in contextBuilder.GetLocalValues())
            {
                items.Add(info.Key, new(info.Key, resourcesOwner, info.Value));
            }

            Items = new([.. items.Values]);
        }

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

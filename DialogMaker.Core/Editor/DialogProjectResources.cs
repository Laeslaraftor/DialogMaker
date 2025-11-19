using System.Collections.ObjectModel;

namespace DialogMaker.Core
{
    public class DialogProjectResources
    {
        public ObservableCollection<DialogProjectReplica> Replicas { get; } = new();
        public ObservableCollection<DialogProjectResourceItem> Items { get; } = new();
    }
}

using DialogMaker.Core.Common;
using System.Collections.ObjectModel;

namespace DialogMaker.Core.Executioning
{
    public interface IJoinOperationInfo : IResourceItem
    {
        public ReadOnlyCollection<DialogPosition> Outputs { get; }
        public ReadOnlyCollection<int> InputSections { get; }
    }
}

using System.Collections.ObjectModel;

namespace DialogMaker.Core.Common
{
    public interface IStringCollection : IResourceItem
    {
        public ReadOnlyCollection<IResourceString> Strings { get; }
    }
}

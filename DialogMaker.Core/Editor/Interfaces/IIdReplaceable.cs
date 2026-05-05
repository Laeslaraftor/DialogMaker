namespace DialogMaker.Core.Editor
{
    public interface IIdReplaceable
    {
        public bool ContainsId(Guid id);
        public void ReplaceIds(IDictionary<Guid, Guid> identifiersMap);
    }
}

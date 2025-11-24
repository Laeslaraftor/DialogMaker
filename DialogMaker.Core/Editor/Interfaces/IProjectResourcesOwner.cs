namespace DialogMaker.Core.Editor
{
    public interface IProjectResourcesOwner
    {
        public DialogProject Project { get; }
        public string Folder { get; }
        public DialogProjectResources Resources { get; }
    }
}

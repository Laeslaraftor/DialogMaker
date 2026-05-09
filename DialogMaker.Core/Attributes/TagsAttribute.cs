namespace DialogMaker.Core
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class TagsAttribute(params string[] tags) : Attribute
    {
        public TagsAttribute(string tags)
            : this(tags.Split(','))
        {
        }

        public string[] Tags { get; } = tags;
    }
}

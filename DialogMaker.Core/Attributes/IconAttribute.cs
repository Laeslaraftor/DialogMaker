namespace DialogMaker.Core
{
    public sealed class IconAttribute(string icon) : Attribute
    {
        public string Icon { get; } = icon;
    }
}

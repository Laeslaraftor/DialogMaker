namespace DialogMaker.Core
{
    public sealed class AllowedTypesAttribute(AllowedObjectValues allowedTypes) : Attribute
    {
        public AllowedObjectValues AllowedTypes { get; } = allowedTypes;
        public string? SelectedTypePropertyName { get; set; }
    }
}

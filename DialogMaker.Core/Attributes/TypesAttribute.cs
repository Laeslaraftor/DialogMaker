namespace DialogMaker.Core
{
    public sealed class TypesAttribute(params Type[] types) : Attribute
    {
        public Type[] Types { get; } = types;
    }
}

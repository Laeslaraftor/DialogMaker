namespace DialogMaker.Core.Scripting.Runtime
{
    public static class DSharpMetadataExtensions
    {
        extension(IDSharpType type)
        {
            public IDSharpFieldInfo? GetFieldOrDefault(string name)
            {
                return type.GetFields().FirstOrDefault(f => f.Name == name);
            }
            public IDSharpFieldInfo GetField(string name)
            {
                return type.GetFieldOrDefault(name) ?? throw new ArgumentException($"Unable to find field {name} at {type}");
            }
            public IDSharpPropertyInfo? GetPropertyOrDefault(string name)
            {
                return type.GetProperties().FirstOrDefault(f => f.Name == name);
            }
            public IDSharpPropertyInfo GetProperty(string name)
            {
                return type.GetPropertyOrDefault(name) ?? throw new ArgumentException($"Unable to find property {name} at {type}");
            }
            public IDSharpMethodInfo? GetMethodOrDefault(string name)
            {
                return type.GetMethods().FirstOrDefault(f => f.Name == name);
            }
            public IDSharpMethodInfo GetMethod(string name)
            {
                return type.GetMethodOrDefault(name) ?? throw new ArgumentException($"Unable to find method {name} at {type}");
            }
        }
    }
}

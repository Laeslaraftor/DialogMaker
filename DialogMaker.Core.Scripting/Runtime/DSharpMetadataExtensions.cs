using DialogMaker.Core.Scripting.Runtime.Builders;

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

            /// <summary>
            /// Check is current type available to assign variable with destination type
            /// </summary>
            /// <param name="destination">Destination type</param>
            /// <returns>Is type assignable to destination type</returns>
            public bool IsAssignableTo(IDSharpType destination)
            {
                if (type == destination ||
                    destination.FullName == DSharpAssemblyBuilder.ObjectTypeFullName)
                {
                    return true;
                }

                bool ContainsInBaseType(IDSharpType currentType)
                {
                    foreach (var baseType in currentType.GetBaseTypes())
                    {
                        if (baseType == destination ||
                            ContainsInBaseType(baseType))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                return ContainsInBaseType(type);
            }
        }
    }
}

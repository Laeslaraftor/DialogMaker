using DialogMaker.Core.Scripting.Runtime.Builders;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Runtime
{
    public static class DSharpMetadataExtensions
    {
        extension(IDSharpType type)
        {
            public IDSharpFieldInfo? GetFieldOrDefault(string name)
            {
                return type.GetMemberOrDefault(t => t.GetFields(), name);
            }
            public IDSharpFieldInfo GetField(string name)
            {
                return type.GetFieldOrDefault(name) ?? throw new ArgumentException($"Unable to find field {name} at {type}");
            }
            public IDSharpPropertyInfo? GetPropertyOrDefault(string name)
            {
                return type.GetMemberOrDefault(t => t.GetProperties(), name);
            }
            public IDSharpPropertyInfo GetProperty(string name)
            {
                return type.GetPropertyOrDefault(name) ?? throw new ArgumentException($"Unable to find property {name} at {type}");
            }
            public IDSharpPropertyInfo? GetIndexerOrDefault(IEnumerable<IDSharpParameterInfo> parameters)
            {
                return type.GetMemberOrDefault(t => t.GetIndexers(), i => i.GetParameters().SequenceEqual(parameters, IDSharpParameterInfo.Comparer.Instance));
            }
            public IDSharpPropertyInfo GetIndexer(IEnumerable<IDSharpParameterInfo> parameters)
            {
                return type.GetIndexerOrDefault(parameters) 
                    ?? throw new ArgumentException($"Unable to find indexer with {parameters.Count()} parameters at {type}");
            }
            public IDSharpMethodInfo? GetMethodOrDefault(string name)
            {
                return type.GetMemberOrDefault(t => t.GetMethods(), name);
            }
            public IDSharpMethodInfo GetMethod(string name)
            {
                return type.GetMethodOrDefault(name) ?? throw new ArgumentException($"Unable to find method {name} at {type}");
            }
            public T? GetMemberOrDefault<T>(Func<IDSharpType, IEnumerable<T>> selector, string name)
                where T : IDSharpMemberInfo
            {
                return type.GetMemberOrDefault<T>(selector, m => m.Name == name);
            }
            public T? GetMemberOrDefault<T>(Func<IDSharpType, IEnumerable<T>> selector, Predicate<T> predicate)
                where T : IDSharpMemberInfo
            {
                T? Find(IDSharpType type)
                {
                    return selector(type).FirstOrDefault(m => predicate(m));
                }
                T? FindInBaseTypes(IDSharpType type)
                {
                    foreach (var baseType in type.GetBaseTypes())
                    {
                        var member = Find(baseType);
                        member ??= FindInBaseTypes(baseType);

                        if (member != null)
                        {
                            return member;
                        }
                    }

                    return default;
                }

                var result = Find(type);
                result ??= FindInBaseTypes(type);

                if (result == null && type.Assembly.ObjectType != type)
                {
                    result = type.Assembly.ObjectType.GetMemberOrDefault(selector, predicate);
                }

                return result;
            }

            /// <summary>
            /// Check is current type available to assign variable with destination type
            /// </summary>
            /// <param name="destination">Destination type</param>
            /// <returns>Is type assignable to destination type</returns>
            public bool IsAssignableTo(IDSharpType destination)
            {
                if (type == null)
                {
                    if (destination.ObjectType == DSharpObjectType.Struct ||
                        destination.ObjectType == DSharpObjectType.Enum)
                    {
                        return false;
                    }

                    return true;
                }
                if (type == destination ||
                    (type.GenericTemplate != null && type.GenericTemplate == destination) ||
                    destination.FullName == DSharpAssemblyBuilder.ObjectTypeFullName)
                {
                    return true;
                }

                bool ContainsInBaseType(IDSharpType currentType)
                {
                    foreach (var baseType in currentType.GetBaseTypes())
                    {
                        if (baseType == destination ||
                            baseType.GenericTemplate == destination ||
                            baseType == destination.GenericTemplate ||
                            baseType.GenericTemplate == destination.GenericTemplate ||
                            ContainsInBaseType(baseType))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                if (ContainsInBaseType(type))
                {
                    return true;
                }
                if (type.GenericTemplate != null)
                {
                    return ContainsInBaseType(type.GenericTemplate);
                }

                return false;
            }
            public bool ContainsBaseType(IDSharpType baseType)
            {
                foreach (var bType in type.GetBaseTypes())
                {
                    if (bType == baseType || bType.ContainsBaseType(baseType))
                    {
                        return true;
                    }
                }

                return false;
            }
            public bool CanReplaceGenericType(IDSharpType normalType)
            {
                if (!type.IsGeneric)
                {
                    throw new ArgumentException($"Current type must be a generic type");
                }

                var baseTypes = type.GetBaseTypes();

                if (baseTypes.Length == 0)
                {
                    return true;
                }
                foreach (var baseType in baseTypes)
                {
                    if (!normalType.ContainsBaseType(baseType))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
        extension(IDSharpAssembly assembly)
        {
            /// <summary>
            /// Try to find type that implementing provided generic type with given types
            /// </summary>
            /// <param name="genericType">Generic type that used by base type</param>
            /// <param name="result">Type that implementing generic type</param>
            /// <param name="parameters">Type parameters of generic type</param>
            /// <returns>Is type found</returns>
            public bool TryFindGenericImplementationType(IDSharpType genericType, [NotNullWhen(true)] out IDSharpType? result, params IEnumerable<IDSharpType> parameters)
            {
                if (genericType.GetGenericTypes().SequenceEqual(parameters))
                {
                    result = genericType;
                    return true;
                }

                result = null;
                var types = assembly.GetTypes(genericType.Namespace, genericType.Name);

                foreach (var type in types)
                {
                    var genericParameters = type.GetGenericParameters();

                    if (type == genericType ||
                        type.GenericTemplate != genericType)
                    {
                        continue;
                    }
                    if (genericParameters.SequenceEqual(parameters))
                    {
                        result = type;
                        return true;
                    }
                }

                return false;
            }
            public List<IDSharpType> FindTypeBasedOn(IDSharpType baseType)
            {
                List<IDSharpType> result = [];

                foreach (var type in assembly.Types)
                {
                    if (type.ContainsBaseType(baseType))
                    {
                        result.Add(type);
                    }
                }

                return result;
            }
        }
    }
}

using DialogMaker.Core.Scripting.Compiler.Ast;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Runtime
{
    public static class DSharpMetadataExtensions
    {
        extension(IDSharpType type)
        {
            public IDSharpFieldInfo[] GetFields() => type.GetFields(f => true);
            public IDSharpPropertyInfo[] GetProperties() => type.GetProperties(p => true);
            public IDSharpMethodInfo[] GetMethods() => type.GetMethods(p => true);
            public IDSharpMethodInfo[] GetConstructors() => type.GetConstructors(p => true);
            public IDSharpIndexerInfo[] GetIndexers() => type.GetIndexers(p => true);
            /// <summary>
            /// Get all custom binary operators for specified operator
            /// </summary>
            /// <param name="binaryOperator">Binary operator</param>
            /// <returns>Array of custom binary operators</returns>
            public IDSharpOperatorInfo[] GetOperators(DSharpBinaryOperator binaryOperator)
            {
                return [.. type.GetOperators().Where(o => o.BinaryOperator == binaryOperator)];
            }
            /// <summary>
            /// Get all custom unary operators for specified operator
            /// </summary>
            /// <param name="unaryOperator">Unary operator</param>
            /// <returns>Array of custom unary operators</returns>
            public IDSharpOperatorInfo[] GetOperators(DSharpUnaryOperator unaryOperator)
            {
                return [.. type.GetOperators().Where(o => o.UnaryOperator == unaryOperator)];
            }

            /// <summary>
            /// Check name existence in current type includes declaring types
            /// </summary>
            /// <param name="name">Name to check</param>
            /// <returns>Is name exists in current type</returns>
            public bool IsNameExists(string name)
            {
                IDSharpType? declaringType = type;

                bool ExistsIsBaseTypes(IDSharpType type)
                {
                    foreach (var baseType in type.GetBaseTypes().Where(t => t.ObjectType == DSharpObjectType.Class))
                    {
                        if (Exists(baseType) ||
                            ExistsIsBaseTypes(baseType))
                        {
                            return true;
                        }
                    }

                    return false;
                }
                bool Exists(IDSharpType type)
                {
                    return type.GetGenericTypes().Any(t => t.Name == name) ||
                           type.GetChildrenTypes().Any(t => t.Name == name) ||
                           type.GetFields().Any(f => f.Name == name) ||
                           type.GetProperties().Any(p => p.Name == name) ||
                           type.GetMethods().Any(m => m.Name == name);
                }

                do
                {
                    if (Exists(declaringType) ||
                        ExistsIsBaseTypes(declaringType))
                    {
                        return true;
                    }

                    declaringType = type.DeclaringType;
                }
                while (declaringType != null);

                return false;
            }
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
            public IDSharpIndexerInfo? GetIndexerOrDefault(params IEnumerable<IDSharpType> parametersType)
            {
                int paramsCount = parametersType.Count();

                return type.GetMemberOrDefault(t => t.GetIndexers(), indexer =>
                {
                    var parameters = indexer.GetParameters();

                    if (parameters.Length != paramsCount)
                    {
                        return false;
                    }

                    int i = 0;

                    foreach (var type in parametersType)
                    {
                        if (type != parameters[i].Type)
                        {
                            return false;
                        }

                        i++;
                    }

                    return true;
                });
            }
            public IDSharpIndexerInfo? GetIndexerOrDefault(params IEnumerable<IDSharpParameterInfo> parameters)
            {
                return type.GetMemberOrDefault(t => t.GetIndexers(), i => i.GetParameters().SequenceEqual(parameters, IDSharpParameterInfo.Comparer.Instance));
            }
            public IDSharpIndexerInfo GetIndexer(params IEnumerable<IDSharpType> parametersType)
            {
                return type.GetIndexerOrDefault(parametersType)
                    ?? throw new ArgumentException($"Unable to find indexer with {parametersType.Count()} parameters at {type}");
            }
            public IDSharpIndexerInfo GetIndexer(params IEnumerable<IDSharpParameterInfo> parameters)
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
                return type.CanCastTo(destination) != DSharpCastAvailability.No;
            }
            /// <summary>
            /// Check is type can be null
            /// </summary>
            /// <returns>Is type can be null</returns>
            public bool CanBeNull()
            {
                if (type.IsGeneric && type.GenericAttributes.HasFlag(DSharpGenericTypeAttributes.NotNull) ||
                                      type.GenericAttributes.HasFlag(DSharpGenericTypeAttributes.Struct))
                {
                    return false;
                }

                return type.ObjectType == DSharpObjectType.Class ||
                       type.ObjectType == DSharpObjectType.Interface;
            }
            /// <summary>
            /// Check is current type can be casted to destination type
            /// </summary>
            /// <param name="destination">Cast destination type</param>
            /// <returns>Is current type can be casted to destination type</returns>
            public DSharpCastAvailability CanCastTo(IDSharpType destination)
            {
                return type.CanCastTo(destination, out _);
            }
            /// <summary>
            /// Check is current type can be casted to destination type.
            /// If cast requires custom operator and it was found it will be provided and returned <c>true</c>.
            /// If cast requires custom operator and it was NOT found will be returned <c>false</c>
            /// Also some build in types can be casted without any operators
            /// </summary>
            /// <param name="destination">Cast destination type</param>
            /// <param name="castOperator">Custom operator that allows to cast types</param>
            /// <returns>Types cast availability</returns>
            public DSharpCastAvailability CanCastTo(IDSharpType destination, out IDSharpOperatorInfo? castOperator)
            {
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

                if (type == null)
                {
                    castOperator = null;
                    return destination.CanBeNull() ? DSharpCastAvailability.Implicit : DSharpCastAvailability.No;
                }
                if (type == destination)
                {
                    castOperator = null;
                    return DSharpCastAvailability.Implicit;
                }
                if (destination.IsGeneric)
                {
                    castOperator = null;

                    if (type.IsGeneric)
                    {
                        if (type.GenericAttributes != destination.GenericAttributes)
                        {
                            return DSharpCastAvailability.No;
                        }
                    }
                    else
                    {
                        if (destination.GenericAttributes.HasFlag(DSharpGenericTypeAttributes.EmptyConstructor) &&
                            type.GetConstructors(m => m.Access == DSharpAccessModifier.Public &&
                                                      m.GetParameters().Length == 0).Length == 0)
                        {
                            return DSharpCastAvailability.No;
                        }
                        if (destination.GenericAttributes.HasFlag(DSharpGenericTypeAttributes.Struct) &&
                            type.ObjectType != DSharpObjectType.Struct &&
                            type.ObjectType != DSharpObjectType.Enum)
                        {
                            return DSharpCastAvailability.No;
                        }
                    }

                    return DSharpCastAvailability.Implicit;
                }
                if (type == destination ||
                    destination == type.Assembly.ObjectType)
                {
                    castOperator = null;
                    return DSharpCastAvailability.Implicit;
                }
                if (type == type.Assembly.ObjectType)
                {
                    castOperator = null;
                    return DSharpCastAvailability.Explicit;
                }

                foreach (var @operator in type.GetCastOperators().Union(destination.GetCastOperators()))
                {
                    if (@operator.ReturnType != destination)
                    {
                        continue;
                    }

                    var parameters = @operator.GetParameters();

                    if (parameters.Length == 1 && parameters[0].Type == type)
                    {
                        castOperator = @operator;

                        if (@operator.Type == DSharpOperatorType.Implicit)
                        {
                            return DSharpCastAvailability.Implicit;
                        }

                        return DSharpCastAvailability.Explicit;
                    }
                }

                castOperator = null;

                if (DSharpBuildInTypes.TryGetInfo(type, out var targetTypeInfo) &&
                    DSharpBuildInTypes.TryGetInfo(destination, out var destinationTypeInfo))
                {
                    return DSharpBuildInTypes.CanCast(targetTypeInfo, destinationTypeInfo);
                }

                if (ContainsInBaseType(type) ||
                    (type.GenericTemplate != null && ContainsInBaseType(type.GenericTemplate)))
                {
                    return DSharpCastAvailability.Implicit;
                }

                return DSharpCastAvailability.No;
            }
            /// <summary>
            /// Check current and destination type can perform specified binary operator
            /// </summary>
            /// <param name="destination">Type to perform binary operation</param>
            /// <param name="operator">Binary operator</param>
            /// <returns>Is operation can be performed</returns>
            public bool CanPerformBinaryOperation(IDSharpType destination, DSharpBinaryOperator @operator)
            {
                return type.CanPerformBinaryOperation(destination, @operator, out _);
            }
            /// <summary>
            /// Check current and destination type can perform specified binary operator.
            /// If operation requires custom operator and it was found it will be provided and returned <c>true</c>.
            /// If operation requires custom operator and it was NOT found will be returned <c>false</c>
            /// Also some build in types can be perform some binary operations without any custom operators
            /// </summary>
            /// <param name="destination">Type to perform binary operation</param>
            /// <param name="operator">Binary operator</param>
            /// <param name="binaryOperator">Custom operator that was found to perform operation</param>
            /// <returns>Is operation can be performed</returns>
            public bool CanPerformBinaryOperation(IDSharpType destination, DSharpBinaryOperator @operator, out IDSharpOperatorInfo? binaryOperator)
            {
                binaryOperator = null;

                if (DSharpBuildInTypes.TryGetInfo(type, out var leftTypeInfo) &&
                    DSharpBuildInTypes.TryGetInfo(destination, out var rightTypeInfo) &&
                    leftTypeInfo.Size > 0 && rightTypeInfo.Size > 0)
                {
                    return true;
                }

                var customOperators = type.GetOperators(@operator).Union(destination.GetOperators(@operator));

                foreach (var customOperator in customOperators)
                {
                    var parameters = customOperator.GetParameters();

                    if (parameters.Length == 2 &&
                        type.IsAssignableTo(parameters[0].Type) &&
                        destination.IsAssignableTo(parameters[1].Type))
                    {
                        binaryOperator = customOperator;
                        return true;
                    }
                }

                return @operator == DSharpBinaryOperator.LogicalEquals ||
                       @operator == DSharpBinaryOperator.LogicalNotEquals;
            }
            /// <summary>
            /// Check current type can perform specified unary operator.
            /// If operation requires custom operator and it was found it will be provided and returned <c>true</c>.
            /// If operation requires custom operator and it was NOT found will be returned <c>false</c>
            /// Also some build in types can be perform some binary operations without any custom operators
            /// </summary>
            /// <param name="destination">Type to perform unary operation</param>
            /// <param name="operator">Unary operator</param>
            /// <param name="unaryOperator">Custom operator that was found to perform operation</param>
            /// <param name="outputType">Type of object that will be returned by specified unary operation</param>
            /// <returns>Is operation can be performed</returns>
            public bool CanPerformUnaryOperation(DSharpUnaryOperator @operator, out IDSharpOperatorInfo? unaryOperator, [NotNullWhen(true)] out IDSharpType? outputType)
            {
                unaryOperator = null;

                if (DSharpBuildInTypes.TryGetInfo(type, out var typeInfo) &&
                    (@operator == DSharpUnaryOperator.Not && typeInfo == DSharpBuildInTypes.Boolean ||
                    @operator != DSharpUnaryOperator.Not && typeInfo.IsNumber()))
                {
                    outputType = type.Assembly.GetType(typeInfo.FullName);
                    return true;
                }

                var customOperators = type.GetOperators(@operator);

                foreach (var customOperator in customOperators)
                {
                    var parameters = customOperator.GetParameters();

                    if (parameters.Length == 1 &&
                        parameters[0].Type == type)
                    {
                        unaryOperator = customOperator;
                        outputType = customOperator.ReturnType;
                        return true;
                    }
                }

                outputType = null;
                return false;
            }

            /// <summary>
            /// Check specified type on containing to base types of current type
            /// </summary>
            /// <param name="baseType">Type to check on containing</param>
            /// <returns>Is specified type contains in current type</returns>
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
            /// <summary>
            /// Can specified type replace current generic type
            /// </summary>
            /// <param name="normalType">Type to check on ability to replace</param>
            /// <returns>Is replace available</returns>
            /// <exception cref="ArgumentException">Current type must be a generic type</exception>
            public bool CanReplaceGenericType(IDSharpType normalType)
            {
                if (!type.IsGeneric)
                {
                    throw new ArgumentException($"Current type must be a generic type: {type}");
                }

                if (type.GenericAttributes.HasFlag(DSharpGenericTypeAttributes.Struct) &&
                    normalType.ObjectType != DSharpObjectType.Struct &&
                    normalType.ObjectType != DSharpObjectType.Enum)
                {
                    return false;
                }
                if (type.GenericAttributes.HasFlag(DSharpGenericTypeAttributes.EmptyConstructor))
                {
                    var constructors = normalType.GetConstructors();

                    if (constructors.Length > 0 &&
                        !constructors.Any(c => c.GetParameters().Length == 0))
                    {
                        return false;
                    }
                }
                
                foreach (var genericBaseType in type.GetBaseTypes())
                {
                    if (genericBaseType != normalType &&
                        !normalType.ContainsBaseType(genericBaseType))
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
            /// <summary>
            /// Find all types in assembly that inherits by specified type
            /// </summary>
            /// <param name="baseType">Base type to search</param>
            /// <returns>List of types that inherits from specified type</returns>
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
        extension(IDSharpMethodInfo method)
        {
            public string ToString(IDictionary<IDSharpType, IDSharpType>? replacedTypes)
            {
                string result = " ";

                if (method.DeclaringType != null)
                {
                    result += method.DeclaringType.FullName + ".";
                }

                result += method.Name;

                IDSharpType ReplaceType(IDSharpType type)
                {
                    if (replacedTypes != null &&
                        replacedTypes.TryGetValue(type, out var newType))
                    {
                        return newType;
                    }

                    return type;
                }

                if (method.ReturnType == null)
                {
                    result = DSharpBuildInTypes.Void.ShortName + result;
                }
                else
                {
                    result = ReplaceType(method.ReturnType) + result;
                }

                var parameters = method.GetParameters();
                var genericParameters = method.GetGenericParameters();

                if (genericParameters.Length > 0)
                {
                    result += '<';
                    bool isFirst = true;

                    foreach (var genericParameter in genericParameters)
                    {
                        if (!isFirst)
                        {
                            result += ", ";
                        }

                        result += ReplaceType(genericParameter);
                        isFirst = false;
                    }

                    result += '>';
                }

                result += '(';

                if (parameters.Length > 0)
                {
                    bool isFirst = true;

                    foreach (var parameter in parameters)
                    {
                        if (!isFirst)
                        {
                            result += ", ";
                        }

                        result += parameter.ToString(replacedTypes);
                        isFirst = false;
                    }
                }

                result += ')';

                return result;
            }
        }
        extension(IDSharpParameterInfo parameter)
        {
            public string ToString(IDictionary<IDSharpType, IDSharpType>? replacedTypes)
            {
                string result = string.Empty;

                if (parameter.Mode != DSharpMethodParameterMode.Default)
                {
                    result = parameter.Mode.ToString().ToLower() + " ";
                }

                if (replacedTypes != null && replacedTypes.TryGetValue(parameter.Type, out var newType))
                {
                    result += newType;
                }
                else
                {
                    result += parameter.Type;
                }

                result += " " + parameter.Name;

                return result;
            }
        }
    }
}

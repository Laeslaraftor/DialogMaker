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
                        parameters[0].Type == type &&
                        parameters[1].Type == destination)
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

using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Runtime;
using System.Collections.ObjectModel;
using System.Reflection;

namespace DialogMaker.Core.Scripting.Compiler
{
    /// <summary>
    /// Information about calling a generic method
    /// </summary>
    public class DSharpMethodCallingInfo(IDSharpMethodInfo method, IList<IDSharpType?> parameters, IDictionary<IDSharpType, IDSharpType> genericParameters)
    {
        public DSharpMethodCallingInfo(IDSharpMethodInfo method)
            : this(method, [.. method.GetParameters().Select(p => p.Type)], _emptyGenericParameters)
        {
        }

        /// <summary>
        /// Generic method that calling
        /// </summary>
        public IDSharpMethodInfo Method { get; } = method;
        /// <summary>
        /// Invocation parameters
        /// </summary>
        public ReadOnlyCollection<IDSharpType?> Parameters { get; } = new(parameters);
        /// <summary>
        /// Replaced generic parameters. Keys - method generic types, Values - invocation generic parameters.
        /// </summary>
        public ReadOnlyDictionary<IDSharpType, IDSharpType> GenericParameters { get; } = new(genericParameters);

        #region Controls

        /// <summary>
        /// Replace types and create new method calling information.
        /// If no types replaced then it return current instance
        /// </summary>
        /// <param name="replacedMembers">Replaced members</param>
        /// <returns>New method calling information with replaced types or current instance</returns>
        public DSharpMethodCallingInfo ReplaceTypes(IReadOnlyDictionary<IDSharpMemberInfo, IDSharpMemberInfo> replacedMembers)
        {
            List<IDSharpType?> parameters = [.. Parameters];
            Dictionary<IDSharpType, IDSharpType> genericParameters = new(GenericParameters);
            bool isAnyTypeReplaced = false;

            for (int i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];

                if (parameter != null && replacedMembers.TryGetValue(parameter, out var replacedMember) &&
                    replacedMember is IDSharpType typeMember)
                {
                    parameters[i] = typeMember;
                    isAnyTypeReplaced = true;
                }
            }

            foreach (var info in genericParameters)
            {
                if (replacedMembers.TryGetValue(info.Value, out var replacedMember) &&
                    replacedMember is IDSharpType typeMember)
                {
                    genericParameters[info.Key] = typeMember;
                    isAnyTypeReplaced = true;
                }
            }

            if (!isAnyTypeReplaced)
            {
                return this;
            }

            return new(Method, parameters, genericParameters);
        }

        /// <summary>
        /// Get parameter types with replacing generics
        /// </summary>
        /// <returns>Array of parameter types with replaced generics</returns>
        public IDSharpType[] GetCallingParameterTypes()
        {
            IDSharpType[] parameters = [.. Method.GetParameters().Select(p => p.Type)];

            for (int i = 0; i < parameters.Length; i++)
            {
                if (GenericParameters.TryGetValue(parameters[i], out var replacedType))
                {
                    parameters[i] = replacedType;
                }
            }

            return parameters;
        }
        /// <summary>
        /// Get type of method returning value
        /// </summary>
        /// <param name="assemblyBuilder">Assembly builder for filling generics</param>
        /// <returns>Type of method returning value</returns>
        public IDSharpType? GetReturnType(DSharpAssemblyBuilder assemblyBuilder)
        {
            var returnType = Method.ReturnType;

            if (returnType == null)
            {
                return null;
            }
            if (GenericParameters.TryGetValue(returnType, out var replacedType))
            {
                return replacedType;
            }
            if (returnType.TryFillRecursive(assemblyBuilder, GenericParameters, out var filledType))
            {
                return filledType;
            }

            return returnType;
        }

        public override string ToString()
        {
            return Method.ToString() ?? string.Empty;
        }

        #endregion

        #region Resolving

        private static readonly ReadOnlyDictionary<IDSharpType, IDSharpType> _emptyGenericParameters = new(new Dictionary<IDSharpType, IDSharpType>());

        /// <summary>
        /// Create method calling information. It automatically detects generic parameter if it possible
        /// </summary>
        /// <param name="method">Calling method</param>
        /// <param name="parameters">Invocation parameters</param>
        /// <param name="genericParameters">Replaced generic parameters</param>
        /// <returns>Method calling information</returns>
        /// <exception cref="InvalidOperationException">Generic parameters amount not matching</exception>
        /// <exception cref="InvalidOperationException">Type can not replace generic</exception>
        public static DSharpMethodCallingInfo Create(IDSharpMethodInfo method, IDSharpType?[] parameters, IDSharpType[]? genericParameters)
        {
            var methodParameters = method.GetParameters();
            var genericTypes = method.GetGenericParameters();
            Dictionary<IDSharpParameterInfo, IDSharpType> parametersType = [];
            Dictionary<IDSharpType, IDSharpType> replacedTypes = [];

            foreach (var parameter in methodParameters)
            {
                parametersType.Add(parameter, parameter.Type);
            }

            if (genericParameters != null)
            {
                if (genericParameters.Length != genericTypes.Length)
                {
                    throw new InvalidOperationException($"Generic parameters amount not matching. Method \"{method}\" have {genericTypes.Length} generic parameters, but got {genericParameters.Length}");
                }

                for (int i = 0; i < genericTypes.Length; i++)
                {
                    var genericType = genericTypes[i];
                    var genericParameter = genericParameters[i];

                    if (!genericType.CanReplaceGenericType(genericParameter))
                    {
                        throw new InvalidOperationException($"Type \"{genericParameter}\" can not replace generic \"{genericType}\" (index: {i}) at \"{method}\"");
                    }

                    replacedTypes.Add(genericType, genericParameter);
                }

                foreach (var parameter in methodParameters)
                {
                    if (replacedTypes.TryGetValue(parameter.Type, out var replacedType))
                    {
                        parametersType[parameter] = replacedType;
                    }
                }
            }
            else if (genericParameters == null && genericTypes.Length > 0)
            {
                replacedTypes = DetectReplacedGenerics(method, parameters, parametersType);

                if (genericTypes.Length != replacedTypes.Count)
                {
                    throw new InvalidOperationException($"Unable to automatically detect types for replacing generic types at \"{method}\"");
                }
            }

            int index = 0;

            foreach (var parameterInfo in parametersType)
            {
                IDSharpType typeForAssignment = parameterInfo.Value;

                if (parameterInfo.Key.Mode == DSharpMethodParameterMode.Params)
                {
                    if (index >= parameters.Length)
                    {
                        break;
                    }

                    var parameterTypeGenericParameters = parameterInfo.Value.GetGenericParameters();

                    if (parameterTypeGenericParameters.Length == 1)
                    {
                        typeForAssignment = parameterTypeGenericParameters[0];
                    }
                }

                var parameter = parameters[index];

                if (!parameter!.IsAssignableTo(typeForAssignment))
                {
                    throw new InvalidOperationException($"Invalid parameter for \"{parameterInfo.Key.Name}\". Required value with \"{parameterInfo.Value}\", got \"{parameter}\" at \"{method}\"");
                }

                index++;
            }

            return new(method, parameters, replacedTypes);
        }
        /// <summary>
        /// Find generic types replaces in calling parameter types
        /// </summary>
        /// <param name="method">Calling method</param>
        /// <param name="parameters">Calling parameters</param>
        /// <param name="parameterTypes"></param>
        /// <returns>Detected generic replaces</returns>
        /// <exception cref="InvalidOperationException">Unable to detect parameter type for replacing generic</exception>
        public static Dictionary<IDSharpType, IDSharpType> DetectReplacedGenerics(IDSharpMethodInfo method, IDSharpType?[] parameters, Dictionary<IDSharpParameterInfo, IDSharpType>? parameterTypes = null)
        {
            Dictionary<IDSharpType, IDSharpType> result = [];
            var methodParameters = method.GetParameters();

            if (methodParameters.Length == 0)
            {
                return result;
            }

            var methodGenerics = method.GetGenericParameters();

            if (methodGenerics.Length == 0)
            {
                return result;
            }

            int minParametersLength = Math.Min(parameters.Length, methodParameters.Length);
            int genericIndex = 0;

            bool TryAdd(IDSharpParameterInfo parameter, IDSharpType genericType, IDSharpType replacedType, IDSharpType newParameterType)
            {
                if (!result.TryAdd(genericType, replacedType))
                {
                    return false;
                }
                if (parameterTypes != null)
                {
                    if (!parameterTypes.TryAdd(parameter, newParameterType))
                    {
                        parameterTypes[parameter] = newParameterType;
                    }
                }

                return true;
            }

            foreach (var generic in methodGenerics)
            {
                for (int i = 0; i < minParametersLength; i++)
                {
                    var methodParameter = methodParameters[i];
                    var parameter = parameters[i]
                            ?? throw new InvalidOperationException($"Unable to detect \"{methodParameter.Name}\" parameter type for replacing generic \"{generic}\" at \"{method}\"");

                    if (!generic.CanReplaceGenericType(parameter))
                    {
                        continue;
                    }

                    if (methodParameter.Type == generic)
                    {
                        if (!TryAdd(methodParameter, generic, parameter, parameter))
                        {
                            goto End;
                        }

                        continue;
                    }
                    if (methodParameter.Type.TryFindTypeWithGeneric(generic, out var typeWithGenericInMethodParameter, out var methodParameterGenericIndex) &&
                        parameter.TryFindTypeWithGeneric(generic, methodParameterGenericIndex, false, out var parameterTypeWithGeneric, out var replacedGeneric, out _))
                    {
                        if (!TryAdd(methodParameter, generic, replacedGeneric, parameter))
                        {
                            goto End;
                        }
                    }
                }

                genericIndex++;
            }

        End:
            return result;
        }
        /// <summary>
        /// Get most suitable method calling 
        /// </summary>
        /// <param name="callingInfos">List of method calling infos for selecting most suitable calling among them</param>
        /// <param name="parameters">Calling parameters</param>
        /// <returns>Most suitable calling info</returns>
        public static DSharpMethodCallingInfo GetMostSuitable(Dictionary<DSharpMethodCallingInfo, IDSharpType?[]> callingInfos)
        {
            if (callingInfos.Count == 0)
            {
                throw new ArgumentException($"Empty calling infos", nameof(callingInfos));
            }
            if (callingInfos.Count == 1)
            {
                return callingInfos.First().Key;
            }

            KeyValuePair<DSharpMethodCallingInfo?, int> minMethodCallingCasts = new(null, int.MaxValue);

            foreach (var info in callingInfos)
            {
                var callingInfo = info.Key;
                var parameters = info.Value;
                var callingTypes = callingInfo.GetCallingParameterTypes();
                var hasParams = info.Key.Method.HasParams;

                if ((!hasParams && callingTypes.Length != parameters.Length) ||
                    (hasParams && parameters.Length - 1 > callingTypes.Length))
                {
                    throw new InvalidOperationException($"Calling parameter length don't match with provided parameters length: {callingInfo}");
                }

                int castsCount = 0;
                IDSharpType?[]? callingParams = hasParams ? GetCallingParams(info.Key.Method, parameters) : null;

                for (int i = 0; i < callingTypes.Length; i++)
                {
                    var callingType = callingTypes[i];
                    var parameter = parameters[i];

                    if (callingType == parameter)
                    {
                        continue;
                    }
                    else if (i == callingTypes.Length - 1 && callingParams != null)
                    {
                        var commonType = callingParams!.GetNearestCommonType();
                        var parameterGenericParameters = callingType.GetGenericParameters();

                        if (parameterGenericParameters.Length == 1)
                        {
                            var genericParameter = parameterGenericParameters[0];

                            if (genericParameter == commonType)
                            {
                                continue;
                            }
                            else if (commonType.IsAssignableTo(genericParameter, true))
                            {
                                castsCount++;
                                continue;
                            }
                        }

                        parameter = commonType;
                    }
                    if (parameter == null)
                    {
                        castsCount++;
                        continue;
                    }

                    var canCast = parameter.CanCastTo(callingType);

                    if (canCast == DSharpCastAvailability.Implicit)
                    {
                        castsCount++;
                        continue;
                    }

                    castsCount += 2;
                }

                if (castsCount == 0)
                {
                    return callingInfo;
                }

                if (minMethodCallingCasts.Value > castsCount)
                {
                    minMethodCallingCasts = new(callingInfo, castsCount);
                }
            }

            if (minMethodCallingCasts.Key == null)
            {
                throw new InvalidOperationException("Unable to find most suitable calling");
            }

            return minMethodCallingCasts.Key;
        }
        public static IDSharpType?[]? GetCallingParams(IDSharpMethodInfo method, IEnumerable<IDSharpType?> callingParameters)
        {
            var methodParameters = method.GetParameters();

            if (methodParameters.Length == 0)
            {
                return null;
            }

            int normalParameters = methodParameters.Length - 1;
            int delta = callingParameters.Count() - normalParameters;

            if (0 > delta)
            {
                throw new ArgumentException("Not enough calling parameters", nameof(callingParameters));
            }
            if (delta == 0)
            {
                return null;
            }
            else if (delta == 1)
            {
                var lastCallingParameter = callingParameters.LastOrDefault();

                if (lastCallingParameter == methodParameters[^1].Type)
                {
                    return [];
                }
            }

            IDSharpType?[] result = new IDSharpType[delta];

            int i = 0;
            int index = 0;

            foreach (var callParameter in callingParameters)
            {
                if (i >= normalParameters)
                {
                    if (index >= delta)
                    {
                        break;
                    }

                    result[index] = callParameter;
                    index++;
                }

                i++;
            }

            return result;
        }

        #endregion
    }
}

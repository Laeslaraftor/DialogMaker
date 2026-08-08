using DialogMaker.Core.Scripting.Runtime;
using System.Collections.ObjectModel;

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
                int detectedGenericParameters = 0;
                int genericIndex = 0;

                foreach (var genericType in genericTypes)
                {
                    for (int i = 0; i < methodParameters.Length; i++)
                    {
                        var methodParameter = methodParameters[i];

                        if (methodParameter.Type != genericType)
                        {
                            continue;
                        }

                        var parameter = parameters[i]
                            ?? throw new InvalidOperationException($"Unable to detect \"{methodParameter.Name}\" parameter type for replacing generic \"{genericType}\" at \"{method}\"");

                        if (!methodParameter.Type.CanReplaceGenericType(parameter))
                        {
                            throw new InvalidOperationException($"Type \"{parameter}\" in specified parameter (index: {i}) can not replace generic \"{genericType}\" (index: {genericIndex}) at \"{method}\"");
                        }

                        detectedGenericParameters++;
                        parametersType[methodParameter] = parameter;
                        replacedTypes.Add(methodParameter.Type, parameter);
                    }

                    genericIndex++;
                }

                if (genericTypes.Length != detectedGenericParameters)
                {
                    throw new InvalidOperationException($"Unable to automatically detect types for replacing generic types at \"{method}\"");
                }
            }

            int index = 0;

            foreach (var parameterInfo in parametersType)
            {
                var parameter = parameters[index];

                if (!parameter!.IsAssignableTo(parameterInfo.Value))
                {
                    throw new InvalidOperationException($"Invalid parameter for \"{parameterInfo.Key.Name}\". Required value with \"{parameterInfo.Value}\", got \"{parameter}\" at \"{method}\"");
                }

                index++;
            }

            return new(method, parameters, replacedTypes);
        }
        /// <summary>
        /// Get most suitable method calling 
        /// </summary>
        /// <param name="callingInfos">List of method calling infos for selecting most suitable calling among them</param>
        /// <param name="parameters">Calling parameters</param>
        /// <returns>Most suitable calling info</returns>
        public static DSharpMethodCallingInfo GetMostSuitable(List<DSharpMethodCallingInfo> callingInfos, IDSharpType?[] parameters)
        {
            if (callingInfos.Count == 0)
            {
                throw new ArgumentException($"Empty calling infos", nameof(callingInfos));
            }
            if (callingInfos.Count == 1)
            {
                return callingInfos[0];
            }

            KeyValuePair<DSharpMethodCallingInfo?, int> minMethodCallingCasts = new(null, int.MaxValue);

            foreach (var callingInfo in callingInfos)
            {
                var callingTypes = callingInfo.GetCallingParameterTypes();

                if (callingTypes.Length != parameters.Length)
                {
                    throw new InvalidOperationException($"Calling parameter length don't match with provided parameters length: {callingInfo}");
                }

                int castsCount = 0;

                for (int i = 0; i < callingTypes.Length; i++)
                {
                    var callingType = callingTypes[i];
                    var parameter = parameters[i];

                    if (callingType == parameter)
                    {
                        continue;
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

        #endregion
    }
}

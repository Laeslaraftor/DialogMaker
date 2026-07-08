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

        private static readonly ReadOnlyDictionary<IDSharpType, IDSharpType> _emptyGenericParameters = new(new Dictionary<IDSharpType, IDSharpType>());

        #region Resolving

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

        #endregion
    }
}

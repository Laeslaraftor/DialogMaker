using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler
{
    /// <summary>
    /// Information about iteration method
    /// </summary>
    public class DSharpIterationMethodInfo(AstNode node,
                                           DSharpMethodBuilder method,
                                           DSharpTypeBuilder? enumerable,
                                           DSharpMethodBuilder? enumerableConstructor,
                                           DSharpTypeBuilder enumerator,
                                           IDSharpType enumeratingType,
                                           DSharpMethodBuilder? enumeratorConstructor,
                                           DSharpFieldBuilder stateField,
                                           DSharpFieldBuilder currentValueField,
                                           Dictionary<DSharpTypeBuilder, DSharpTypeBuilder>? methodToEnumerableGenericTypes,
                                           Dictionary<DSharpTypeBuilder, DSharpTypeBuilder>? methodToEnumeratorGenericTypes)
    {
        /// <summary>
        /// Node that represents method
        /// </summary>
        public AstNode Node { get; } = node;
        /// <summary>
        /// Iteration method to which proposed this info
        /// </summary>
        public DSharpMethodBuilder Method { get; } = method;
        /// <summary>
        /// Generated enumerable that just returns enumerator.
        /// It can be null when methods returns enumerator instead of enumerable
        /// </summary>
        public DSharpTypeBuilder? Enumerable { get; } = enumerable;
        /// <summary>
        /// Constructor for creating enumerable instance.
        /// It can be null when enumerable not created or if method have no parameters
        /// </summary>
        public DSharpMethodBuilder? EnumerableConstructor { get; } = enumerableConstructor;
        /// <summary>
        /// Generated values enumerator.
        /// This types contains all local variables and parameters as fields
        /// </summary>
        public DSharpTypeBuilder Enumerator { get; } = enumerator;
        /// <summary>
        /// Values type that enumerator was returns
        /// </summary>
        public IDSharpType EnumeratingType { get; } = enumeratingType;
        /// <summary>
        /// Constructor for creating enumerator.
        /// It can be null when method have no parameters
        /// </summary>
        public DSharpMethodBuilder? EnumeratorConstructor { get; } = enumeratorConstructor;
        /// <summary>
        /// Information about enumerator
        /// </summary>
        public DSharpIEnumeratorType EnumeratorInfo { get; } = DSharpIEnumeratorType.Create(enumerator);
        /// <summary>
        /// Property that returns current enumerating value
        /// </summary>
        public DSharpPropertyBuilder CurrentValueProperty => (DSharpPropertyBuilder)EnumeratorInfo.CurrentProperty;
        /// <summary>
        /// Method for resetting enumerator
        /// </summary>
        public DSharpMethodBuilder ResetMethod => (DSharpMethodBuilder)EnumeratorInfo.ResetMethod;
        /// <summary>
        /// Method for enumerating. This method contains main implementation
        /// </summary>
        public DSharpMethodBuilder MoveNextMethod => (DSharpMethodBuilder)EnumeratorInfo.MoveNextMethod;
        /// <summary>
        /// Method for disposing all usings.
        /// It can be null when no one variable uses "using"
        /// </summary>
        public DSharpMethodBuilder? DisposeMethod { get; private set; }
        /// <summary>
        /// Field that stores current state. Field type is <see cref="DSharpBuildInTypes.Int"/>
        /// </summary>
        public DSharpFieldBuilder StateField { get; } = stateField;
        /// <summary>
        /// Field that stores current value from <see cref="MoveNextMethod"/>.Field type is <see cref="EnumeratingType"/>
        /// </summary>
        public DSharpFieldBuilder CurrentValueField { get; } = currentValueField;
        /// <summary>
        /// Yield statements that represents enumerator states
        /// </summary>
        public HashSet<YieldStatementNode> YieldStatements { get; } = [];
        /// <summary>
        /// Method generic parameters with corresponding enumerable generic types
        /// </summary>
        public Dictionary<DSharpTypeBuilder, DSharpTypeBuilder>? MethodToEnumerableGenericTypes { get; } = methodToEnumerableGenericTypes;
        /// <summary>
        /// Method generic parameters with corresponding enumerator generic types
        /// </summary>
        public Dictionary<DSharpTypeBuilder, DSharpTypeBuilder>? MethodToEnumeratorGenericTypes { get; } = methodToEnumeratorGenericTypes;

        #region Controls

        /// <summary>
        /// Make enumerable disposable. 
        /// This method implements <see cref="DSharpBuildInTypes.Extra.IDisposable"/> in <see cref="Enumerator"/> if it not implemented yet
        /// </summary>
        /// <returns>Dispose method</returns>
        public DSharpMethodBuilder MakeDisposable()
        {
            if (DisposeMethod != null)
            {
                return DisposeMethod;
            }

            var implementedMembers = Enumerator.ImplementInterface(Method.Assembly.IDisposableType);
            var disposeMethod = (implementedMembers.FirstOrDefault(m => m.Name == DSharpIDisposableType.DisposeMethodName &&
                                                                       m is DSharpMethodBuilder) as DSharpMethodBuilder)
                ?? throw new DSharpCompilerException($"Failed to implement \"{Method.Assembly.IDisposableType}\" at \"{Enumerator}\": \"{DSharpIDisposableType.DisposeMethodName}\" method not found", Node);
            DisposeMethod = disposeMethod;

            return disposeMethod;
        }

        #endregion

        #region Constants

        /// <summary>
        /// Name of field that stores current enumerable state
        /// </summary>
        public const string StateFieldName = "<>_states";
        /// <summary>
        /// Name of field that stores current value
        /// </summary>
        public const string CurrentValueFieldName = "<>_currentValue";

        #endregion

        #region Static

        /// <summary>
        /// Create information for iteration method.
        /// It also prepares necessary types
        /// </summary>
        /// <param name="method">Method for that need to create information</param>
        /// <param name="methodNode">Node that represents method</param>
        /// <returns>Information about iteration method</returns>
        /// <exception cref="DSharpCompilerException"></exception>
        public static DSharpIterationMethodInfo Create(DSharpMethodBuilder method, AstNode methodNode)
        {
            if (method.ReturnType == null)
            {
                throw new DSharpCompilerException($"Iteration methods should return enumerable or enumerator", methodNode);
            }

            foreach (var parameter in method.Parameters)
            {
                if (parameter.Mode == DSharpMethodParameterMode.Ref ||
                    parameter.Mode == DSharpMethodParameterMode.Out)
                {
                    throw new DSharpCompilerException($"Unavailable parameter for iteration method: {parameter}: iteration method can not contains \"{parameter.Mode}\" parameters", methodNode);
                }
            }

            var assembly = method.Assembly;
            IDSharpType returnType = (IDSharpType)assembly.GetType(method.ReturnType);
            IDSharpType enumeratingType;
            IDSharpType enumeratorInterface;

            if (returnType == assembly.IEnumerableType ||
                returnType == assembly.IEnumeratorType)
            {
                enumeratorInterface = assembly.IEnumeratorType;
                enumeratingType = assembly.ObjectType;
            }
            else if (returnType.GenericTemplate == assembly.TypedIEnumerableType ||
                     returnType.GenericTemplate == assembly.TypedIEnumeratorType)
            {
                enumeratingType = returnType.GetGenericParameters().FirstOrDefault()
                    ?? throw new DSharpCompilerException("Iteration method return type not contains generic parameter that representing value type", methodNode);
                enumeratorInterface = assembly.FillGeneric(assembly.TypedIEnumeratorType, enumeratingType);
            }
            else
            {
                throw new DSharpCompilerException($"Iteration method should return \"{DSharpBuildInTypes.Extra.IEnumerable}\", \"{DSharpBuildInTypes.Extra.TypedIEnumerable}\", \"{DSharpBuildInTypes.Extra.IEnumerator}\" or \"{DSharpBuildInTypes.Extra.TypedIEnumerator}\"", methodNode);
            }

            DSharpTypeBuilder? enumerableType = null;
            DSharpMethodBuilder? enumerableConstructor = null;
            Dictionary<DSharpTypeBuilder, DSharpTypeBuilder>? methodToEnumerableGenericTypes = null;
            Dictionary<DSharpTypeBuilder, DSharpTypeBuilder>? methodToEnumeratorGenericTypes = null;
            DSharpMethodBuilder? getEnumeratorMethod = null;

            if (returnType == assembly.IEnumerableType ||
                returnType.GenericTemplate == assembly.TypedIEnumerableType)
            {
                string typeName = $"<>Enumerable_{method.Name}_{methodNode.Line}_{methodNode.Column}";

                if (method.DeclaringType is DSharpTypeBuilder declaringTypeBuilder)
                {
                    enumerableType = declaringTypeBuilder.CreateChildType(typeName);
                    enumerableType.Access = DSharpAccessModifier.Private;
                }
                else
                {
                    enumerableType = assembly.CreateType(typeName);
                    enumerableType.Access = DSharpAccessModifier.Internal;
                }

                methodToEnumerableGenericTypes = CopyGenericTypes(method, enumerableType);
                var newReturnType = returnType;

                if (methodToEnumerableGenericTypes != null)
                {
                    newReturnType = assembly.ReplaceTypes(returnType, method.DeclaringType, methodToEnumerableGenericTypes);
                }
                if (method.Parameters.Count > 0)
                {
                    enumerableConstructor = CreateConstructor(method, enumerableType, methodToEnumerableGenericTypes);
                }

                var implementedMembers = enumerableType.ImplementInterface(newReturnType, true);
                getEnumeratorMethod = implementedMembers.FirstOrDefault(m => m.Name == "GetEnumerator" && m is DSharpMethodBuilder) as DSharpMethodBuilder;
            }

            string enumeratorTypeName = $"<>Enumerator_{method.Name}_{methodNode.Line}_{methodNode.Column}";
            DSharpTypeBuilder enumeratorType;
            DSharpMethodBuilder? enumeratorConstructor = null;

            if (enumerableType != null)
            {
                enumeratorType = enumerableType.CreateChildType(enumeratorTypeName);
                enumeratorType.Access = DSharpAccessModifier.Private;
            }
            else
            {
                enumeratorType = assembly.CreateType(enumeratorTypeName);
                enumeratorType.Access = DSharpAccessModifier.Internal;
            }

            methodToEnumeratorGenericTypes = CopyGenericTypes(method, enumeratorType);

            if (methodToEnumeratorGenericTypes != null)
            {
                enumeratorInterface = assembly.ReplaceTypes(enumeratorInterface, null, methodToEnumeratorGenericTypes);
            }
            if (method.Parameters.Count > 0)
            {
                enumeratorConstructor = CreateConstructor(method, enumeratorType, methodToEnumerableGenericTypes);
            }

            var implementedEnumeratorMembers = enumeratorType.ImplementInterface(enumeratorInterface);
            var stateField = enumeratorType.CreateField(StateFieldName);
            var currentValueField = enumeratorType.CreateField(CurrentValueFieldName);

            stateField.Access = DSharpAccessModifier.Private;
            stateField.FieldType = assembly.Int32Token;
            currentValueField.Access = DSharpAccessModifier.Private;
            currentValueField.FieldType = assembly.GetTypeToken(enumeratingType);

            var methodCode = method.GetBytecodeBuilder();

            foreach (var methodParameter in method.Parameters)
            {
                methodCode.LoadLocal(methodParameter);
            }
            if (enumerableType != null)
            {
                if (getEnumeratorMethod == null)
                {
                    throw new DSharpCompilerException($"\"GetEnumerator\" method not found at \"{enumerableType}\"", methodNode);
                }

                var getEnumeratorCode = getEnumeratorMethod.GetBytecodeBuilder();

                if (enumerableConstructor != null)
                {
                    var code = enumerableConstructor.GetBytecodeBuilder();
                    int parameterIndex = 0;
                    code.LoadInstance();
                    getEnumeratorCode.LoadInstance();

                    foreach (var parameter in enumerableConstructor.Parameters)
                    {
                        var field = enumerableType.CreateField(parameter.Name ?? $"_f{parameterIndex}");
                        field.FieldType = parameter.Type;
                        field.Access = DSharpAccessModifier.Private;

                        code.LoadLocal(parameter);
                        code.StorePropertyOrField(field);
                        code.Pop();

                        getEnumeratorCode.LoadPropertyOrField(field);

                        parameterIndex++;
                    }

                    if (enumeratorConstructor == null)
                    {
                        throw new DSharpCompilerException("Enumerator constructor not created when method have parameters", methodNode);
                    }

                    methodCode.New(enumerableConstructor);
                    getEnumeratorCode.New(enumeratorConstructor);
                }
                else
                {
                    methodCode.New(enumerableType);
                    getEnumeratorCode.New(enumeratorType);
                }
            }
            else
            {
                if (enumeratorConstructor != null)
                {
                    methodCode.New(enumeratorConstructor);
                }
                else
                {
                    methodCode.New(enumeratorType);
                }
            }
            if (enumeratorConstructor != null)
            {
                int parameterIndex = 0;
                var code = enumeratorConstructor.GetBytecodeBuilder();
                code.LoadInstance();

                foreach (var parameter in enumeratorConstructor.Parameters)
                {
                    var field = enumeratorType.CreateField(parameter.Name ?? $"_f{parameterIndex}");
                    field.FieldType = parameter.Type;
                    field.Access = DSharpAccessModifier.Private;

                    code.LoadLocal(parameter);
                    code.StorePropertyOrField(field);
                    code.Pop();

                    parameterIndex++;
                }
            }

            DSharpIterationMethodInfo resultInfo = new(methodNode,
                                                       method,
                                                       enumerableType,
                                                       enumerableConstructor,
                                                       enumeratorType,
                                                       enumeratingType,
                                                       enumeratorConstructor,
                                                       stateField,
                                                       currentValueField,
                                                       methodToEnumerableGenericTypes,
                                                       methodToEnumeratorGenericTypes);

            var resetMethodCode = resultInfo.ResetMethod.GetBytecodeBuilder();
            resetMethodCode.LoadInstance();
            resetMethodCode.Push(0);
            resetMethodCode.StorePropertyOrField(stateField);
            resetMethodCode.Pop();
            resetMethodCode.Push(null);
            resetMethodCode.StorePropertyOrField(currentValueField);

            var currentValueProperty = resultInfo.CurrentValueProperty;
            var currentValuePropertyGetter = currentValueProperty.CreateGetter();
            currentValueProperty.Access = DSharpAccessModifier.Public;
            var currentValuePropertyCode = currentValuePropertyGetter.GetBytecodeBuilder();
            currentValuePropertyCode.LoadInstance();
            currentValuePropertyCode.LoadPropertyOrField(currentValueField);
            currentValuePropertyCode.Return();

            return resultInfo;
        }

        private static Dictionary<DSharpTypeBuilder, DSharpTypeBuilder>? CopyGenericTypes(DSharpMethodBuilder method, DSharpTypeBuilder type)
        {
            Dictionary<DSharpTypeBuilder, DSharpTypeBuilder>? replacedTypes = null;

            foreach (var genericType in method.GenericParameters)
            {
                var newGenericType = type.CreateGenericType(genericType.Name);
                newGenericType.GenericAttributes = genericType.GenericAttributes;

                replacedTypes ??= [];
                replacedTypes.Add(genericType, newGenericType);
            }

            return replacedTypes;
        }
        private static DSharpMethodBuilder CreateConstructor(DSharpMethodBuilder method, DSharpTypeBuilder type, Dictionary<DSharpTypeBuilder, DSharpTypeBuilder>? replacedTypes)
        {
            var constructor = type.CreateConstructor();
            constructor.Access = DSharpAccessModifier.Public;
            method.CopyParametersTo(method.Assembly, constructor.Parameters, false, replacedTypes);

            return constructor;
        }

        #endregion
    }
}

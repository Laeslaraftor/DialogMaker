using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Compiler.Scopes;
using DialogMaker.Core.Scripting.Runtime;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Compiler
{
    /// <summary>
    /// Information about iteration method
    /// </summary>
    public class DSharpIterationMethodInfo(AstNode node,
                                           DSharpMethodBuilder method,
                                           DSharpCompilerMethodScope.CaptureInfo? closure,
                                           DSharpTypeBuilder? enumerable,
                                           DSharpMethodBuilder? enumerableConstructor,
                                           DSharpTypeBuilder enumerator,
                                           IDSharpType enumeratingType,
                                           DSharpMethodBuilder? enumeratorConstructor,
                                           DSharpFieldBuilder stateField,
                                           DSharpFieldBuilder currentValueField,
                                           DSharpFieldBuilder? closureField,
                                           Dictionary<DSharpTypeBuilder, DSharpTypeBuilder>? methodToEnumerableGenericTypes,
                                           Dictionary<DSharpTypeBuilder, DSharpTypeBuilder>? methodToEnumeratorGenericTypes,
                                           Dictionary<DSharpMethodBuilderParameter, DSharpFieldBuilder>? methodParametersToEnumeratorFields)
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
        /// Closure with captured variables and local functions
        /// </summary>
        public DSharpCompilerMethodScope.CaptureInfo? Closure { get; } = closure;
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
        /// Field that stores index of next instruction after last yield returning. Field type is <see cref="DSharpBuildInTypes.Int"/>.
        /// Value -1 means that enumerator was force stopped (yield break) or ended. 
        /// Value 0 means default state
        /// </summary>
        public DSharpFieldBuilder InstructionField { get; } = stateField;
        /// <summary>
        /// Field that stores current value from <see cref="MoveNextMethod"/>. Field type is <see cref="EnumeratingType"/>.
        /// </summary>
        public DSharpFieldBuilder ValueField { get; } = currentValueField;
        /// <summary>
        /// Field that stores closure instance
        /// </summary>
        public DSharpFieldBuilder? ClosureField { get; } = closureField;
        /// <summary>
        /// Fields that created instead of local variables
        /// </summary>
        public Dictionary<AstNode, DSharpFieldBuilder> VariableFields { get; } = [];
        /// <summary>
        /// Variables list for scopes
        /// </summary>
        public Dictionary<DSharpCompilerScope, List<DSharpFieldBuilder>> ScopedVariableFields { get; } = [];
        /// <summary>
        /// Method generic parameters with corresponding enumerable generic types
        /// </summary>
        public Dictionary<DSharpTypeBuilder, DSharpTypeBuilder>? MethodToEnumerableGenericTypes { get; } = methodToEnumerableGenericTypes;
        /// <summary>
        /// Method generic parameters with corresponding enumerator generic types
        /// </summary>
        public Dictionary<DSharpTypeBuilder, DSharpTypeBuilder>? MethodToEnumeratorGenericTypes { get; } = methodToEnumeratorGenericTypes;
        /// <summary>
        /// Enumerator fields that represents method parameters
        /// </summary>
        public Dictionary<DSharpMethodBuilderParameter, DSharpFieldBuilder>? MethodParametersToEnumeratorFields { get; } = methodParametersToEnumeratorFields;
        /// <summary>
        /// Finalization methods collection
        /// </summary>
        public ReadOnlyCollection<FinallyMethodInfo> FinalizationMethods
        {
            get 
            {
                field ??= new(_finalizationMethods);
                return field;
            }
        }

        private readonly List<FinallyMethodInfo> _finalizationMethods = [];

        #region Controls

        /// <summary>
        /// Add scoped variable to list
        /// </summary>
        /// <param name="scope">Scope that contains adding variable</param>
        /// <param name="variable">Variable to add</param>
        public void AddScopedVariable(DSharpCompilerScope scope, DSharpFieldBuilder variable)
        {
            if (!ScopedVariableFields.TryGetValue(scope, out var variables))
            {
                variables = [];
                ScopedVariableFields.Add(scope, variables);
            }
            if (variables.Any(v => v.Name == variable.Name))
            {
                throw new ArgumentException($"Variable with same name \"{variable.Name}\" already exists in current scope");
            }

            variables.Add(variable);
        }
        /// <summary>
        /// Try get scoped variable by name
        /// </summary>
        /// <param name="scope">Scope that contains variable</param>
        /// <param name="name">Name of searching variable</param>
        /// <param name="result">Variable that was found</param>
        /// <returns>Is variable successfully found</returns>
        public bool TryGetScopedVariable(DSharpCompilerScope scope, string name, [NotNullWhen(true)] out DSharpFieldBuilder? result)
        {
            result = null;

            if (ScopedVariableFields.TryGetValue(scope, out var variables))
            {
                result = variables.FirstOrDefault(v => v.Name == name);
            }

            return result != null;
        }

        /// <summary>
        /// Try to get variable field by node that references to this variable
        /// </summary>
        /// <param name="referenceToVariable">Node that references to variable</param>
        /// <param name="result">Field that represents variable that was found</param>
        /// <returns>Is variable field was successfully found</returns>
        public bool TryGetVariableByReferenceNode(AstNode referenceToVariable, [NotNullWhen(true)] out DSharpFieldBuilder? result)
        {
            result = null;

            if (Closure == null)
            {
                return false;
            }

            foreach (var variable in Closure.Capture.Variables.Values)
            {
                if (variable.References.Contains(referenceToVariable) &&
                    VariableFields.TryGetValue(variable.Declaration.Node, out result))
                {
                    return true;
                }
            }

            return false;
        }

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

            var resetMethodCode = ResetMethod.GetBytecodeBuilder();
            resetMethodCode.LoadInstance();
            resetMethodCode.Call(disposeMethod);

            var finalizer = Enumerator.CreateFinalizer();
            var finalizerCode = finalizer.GetBytecodeBuilder();
            finalizerCode.LoadInstance();
            finalizerCode.Call(disposeMethod);

            return disposeMethod;
        }
        /// <summary>
        /// Create finalization method which will be called on enumeration disposing if it enabled
        /// </summary>
        /// <param name="codeBuilderHandler">Finalization method bytecode builder handler</param>
        /// <returns>Information about finalization method</returns>
        public FinallyMethodInfo CreateFinalizationMethod(Action<DSharpBytecodeBuilder>? codeBuilderHandler = null)
        {
            string methodName = $"<>Finally_{_finalizationMethods.Count}";
            string methodFlagName = $"<>Finally_{_finalizationMethods.Count}_IsEnabled";
            var method = Enumerator.CreateMethod(methodName);
            var field = Enumerator.CreateField(methodFlagName);
            field.Access = DSharpAccessModifier.Private;
            field.FieldType = Method.Assembly.BoolToken;
            method.Access = DSharpAccessModifier.Private;

            var disposeMethod = MakeDisposable();
            var disposeCode = disposeMethod.GetBytecodeBuilder();
            disposeCode.LoadInstance();
            disposeCode.Call(method);
            disposeCode.Pop();

            var methodCode = method.GetBytecodeBuilder();
            methodCode.LoadInstance();
            methodCode.LoadPropertyOrField(field);
            methodCode.Push(true);
            methodCode.Equals();
            var skipExiting = methodCode.JumpIfTrue();
            methodCode.Return();
            skipExiting.ReferencedInstruction = methodCode.PopRepeat(3);
            methodCode.Push(false);
            methodCode.StorePropertyOrField(field);
            methodCode.PopRepeat(2);

            codeBuilderHandler?.Invoke(methodCode);

            return new(method, field);
        }

        #endregion

        #region Constants

        /// <summary>
        /// Name of field that stores index of next instruction after last yield returning
        /// </summary>
        public const string InstructionFieldName = "<>_instruction";
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
        /// <param name="methodClosure">
        /// Closure that contains captured variables and local functions.
        /// It will be given in first constructor argument and stored as field.
        /// </param>
        /// <returns>Information about iteration method</returns>
        /// <exception cref="DSharpCompilerException"></exception>
        public static DSharpIterationMethodInfo Create(DSharpMethodBuilder method, AstNode methodNode, DSharpCompilerMethodScope.CaptureInfo? methodClosure = null)
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
            Dictionary<DSharpMethodBuilderParameter, DSharpFieldBuilder>? methodParametersToEnumeratorFields = null;
            DSharpMethodBuilder? getEnumeratorMethod = null;
            DSharpMethodBuilderParameter? enumerableClosureParameter = null;
            DSharpMethodBuilderParameter? enumeratorClosureParameter = null;
            DSharpFieldBuilder? closureContainerField = null;

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
                if (method.Parameters.Count > 0 || methodClosure != null)
                {
                    enumerableConstructor = CreateConstructor(method, enumerableType, methodToEnumerableGenericTypes);

                    if (methodClosure != null)
                    {
                        enumerableClosureParameter = InsertClosureParameter(enumerableConstructor, methodClosure);
                    }
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
            if (method.Parameters.Count > 0 || methodClosure != null)
            {
                enumeratorConstructor = CreateConstructor(method, enumeratorType, methodToEnumerableGenericTypes);

                if (methodClosure != null)
                {
                    enumeratorClosureParameter = InsertClosureParameter(enumeratorConstructor, methodClosure);
                }
            }

            var implementedEnumeratorMembers = enumeratorType.ImplementInterface(enumeratorInterface);
            var instructionField = enumeratorType.CreateField(InstructionFieldName);
            var currentValueField = enumeratorType.CreateField(CurrentValueFieldName);

            instructionField.Access = DSharpAccessModifier.Private;
            instructionField.FieldType = assembly.Int32Token;
            currentValueField.Access = DSharpAccessModifier.Private;
            currentValueField.FieldType = assembly.GetTypeToken(enumeratingType);

            var methodCode = method.GetBytecodeBuilder();

            methodClosure?.LoadClosureContainer(methodCode);

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

                    foreach (var parameter in enumerableConstructor.Parameters)
                    {
                        var field = enumerableType.CreateField(parameter.Name ?? $"_f{parameterIndex}");
                        field.FieldType = parameter.Type;
                        field.Access = DSharpAccessModifier.Private;

                        code.LoadLocal(parameter);
                        code.StorePropertyOrField(field);
                        code.Pop();

                        getEnumeratorCode.LoadInstance();
                        getEnumeratorCode.LoadPropertyOrField(field);
                        getEnumeratorCode.PopOffset(1);

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

                getEnumeratorCode.Return();
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

            methodCode.Return();

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

                    if (parameter != enumeratorClosureParameter)
                    {
                        methodParametersToEnumeratorFields ??= [];
                        methodParametersToEnumeratorFields.Add(method.Parameters[parameterIndex], field);

                        parameterIndex++;
                    }
                    else
                    {
                        closureContainerField = field;
                    }
                }
            }

            DSharpIterationMethodInfo resultInfo = new(methodNode,
                                                       method,
                                                       methodClosure,
                                                       enumerableType,
                                                       enumerableConstructor,
                                                       enumeratorType,
                                                       enumeratingType,
                                                       enumeratorConstructor,
                                                       instructionField,
                                                       currentValueField,
                                                       closureContainerField,
                                                       methodToEnumerableGenericTypes,
                                                       methodToEnumeratorGenericTypes,
                                                       methodParametersToEnumeratorFields);

            var resetMethodCode = resultInfo.ResetMethod.GetBytecodeBuilder();
            resetMethodCode.LoadInstance();
            resetMethodCode.Push(0);
            resetMethodCode.StorePropertyOrField(instructionField);
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

            if (methodClosure != null && closureContainerField != null)
            {
                methodClosure.CustomClosureContainers.Add(resultInfo.MoveNextMethod, closureContainerField);
            }

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
        private static DSharpMethodBuilderParameter InsertClosureParameter(DSharpMethodBuilder method, DSharpCompilerMethodScope.CaptureInfo methodClosure)
        {
            DSharpMethodBuilderParameter closureParameter = new(method.Assembly)
            {
                Name = methodClosure.ClosureContainer?.Name ?? methodClosure.ClosureFieldContainer?.Name ?? "closureInstance",
                Type = methodClosure.Type
            };

            method.Parameters.Insert(0, closureParameter);

            return closureParameter;
        }
        private static DSharpMethodBuilder CreateConstructor(DSharpMethodBuilder method, DSharpTypeBuilder type, Dictionary<DSharpTypeBuilder, DSharpTypeBuilder>? replacedTypes)
        {
            var constructor = type.CreateConstructor();
            constructor.Access = DSharpAccessModifier.Public;
            method.CopyParametersTo(method.Assembly, constructor.Parameters, false, replacedTypes);

            return constructor;
        }

        #endregion

        #region Records

        /// <summary>
        /// Information about finally method
        /// </summary>
        /// <param name="Method">Method that contains finalization code</param>
        /// <param name="IsEnabledField">
        /// Field that contains boolean value that represents finalization active status.
        /// If value stores <c>true</c> - method can be called on enumerator disposing
        /// </param>
        public record struct FinallyMethodInfo(DSharpMethodBuilder Method, DSharpFieldBuilder IsEnabledField);

        #endregion
    }
}

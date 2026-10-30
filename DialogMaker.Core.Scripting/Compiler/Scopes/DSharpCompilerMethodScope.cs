using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Scopes
{
    /// <summary>
    /// Method scope
    /// </summary>
    /// <remarks>
    /// Create new instance of method scope
    /// </remarks>
    /// <param name="method">Method that contains current scope</param>
    /// <param name="parent">Parent scope</param>
    public class DSharpCompilerMethodScope(DSharpMethodBuilder method, DSharpCompilerScope? parent) : DSharpCompilerScope(method.Assembly, parent)
    {

        /// <summary>
        /// Method that contains current scope
        /// </summary>
        public DSharpMethodBuilder Method { get; } = method;
        /// <summary>
        /// Current scope variables without arguments
        /// </summary>
        public List<DSharpMethodBuilderParameter> Variables { get; } = [];
        /// <summary>
        /// Current scope local functions
        /// </summary>
        public Dictionary<string, DSharpMethodBuilder> LocalFunctions { get; } = [];
        /// <summary>
        /// Type that purposed to used as containers for captured values
        /// </summary>
        public CaptureInfo? Closure { get; private set; }
        /// <summary>
        /// Iteration information for current method
        /// </summary>
        public DSharpIterationMethodInfo? IterationMethodInfo { get; set; }
        /// <summary>
        /// Is root scope. Root scope contains method parameters as variables
        /// </summary>
        public bool IsRoot { get; set; }
        public override bool IsUnsafe => RecursiveCheck<bool?>(scope =>
        {
            if (scope is DSharpCompilerMethodScope methodScope &&
                methodScope.IsRoot)
            {
                return methodScope.Method.IsUnsafeRecursive();
            }

            return null;
        }) ?? false;

        public override void Clear(bool recursive = true, int bytecodeStartIndex = 0)
        {
            base.Clear(recursive);

            Variables.Clear();
            DSharpBytecodeBuilder? code = null;

            if (!Method.IsDeclaration)
            {
                code = Method.GetBytecodeBuilder();
                code.Clear();
            }
            if (Closure?.ClosureContainer != null)
            {
                Variables.Add(Closure.ClosureContainer);
                code?.LocalVariables.Add(Closure.ClosureContainer);
            }
        }

        /// <summary>
        /// Create captures container type
        /// </summary>
        /// <param name="capture">Information of capturing for that need to create capture</param>
        /// <param name="isIterationMethod">
        /// Is method for that creating capturing is iteration. 
        /// Closure container creates as field for iteration methods,
        /// for default methods creates variable
        /// </param>
        /// <returns>Type that purposed to use as container for captured values</returns>
        public CaptureInfo GetOrCreateCapture(DSharpCaptureInfo.Capture capture, bool isIterationMethod)
        {
            DSharpCompilerScope? parent = this;

            while (parent != null)
            {
                if (parent is DSharpCompilerMethodScope methodScope &&
                    methodScope.Closure?.Capture == capture)
                {
                    return methodScope.Closure;
                }

                parent = parent.Parent;
            }

            if (Closure != null)
            {
                throw new DSharpCompilerException($"Unable to create second container for captured variables for current context at \"{Method}\"", capture.Scope);
            }

            string name = $"<>_Capture{capture.Scope.Line}_{capture.Scope.Column}";
            string instanceVariableName = $"<>_captureInstance_{capture.Scope.Line}_{capture.Scope.Column}";
            var type = Assembly.CreateType(name, Method.DeclaringType);
            DSharpMethodBuilderParameter? variableContainer = null;
            DSharpFieldBuilder? fieldContainer = null;

            if (isIterationMethod)
            {
                if (Method.DeclaringType is not DSharpTypeBuilder iterationMethodDeclaringType)
                {
                    throw new DSharpCompilerException("Iteration method should be declared in builder for creating closure", capture.Scope);
                }

                fieldContainer = iterationMethodDeclaringType.CreateField(instanceVariableName);
                fieldContainer.Access = DSharpAccessModifier.Private;
                fieldContainer.FieldType = type;
            }
            else
            {
                if (CreateVariable(instanceVariableName, type) is not DSharpMethodBuilderParameter variable)
                {
                    throw new DSharpCompilerException($"Unable to variable for storing closure in \"{Method}\"", capture.Scope);
                }

                variableContainer = variable;
            }

            Closure = new(this, Method.DeclaringType, type, capture, variableContainer, fieldContainer);

            if (type.DeclaringType != null)
            {
                type.Access = DSharpAccessModifier.Private;
            }
            foreach (var capturedVariable in capture.Variables.Values)
            {
                var field = type.CreateField(capturedVariable.Name);
                field.Access = DSharpAccessModifier.Public;
                Closure.CapturedDeclaration.Add(capturedVariable.Declaration.Node, field);
            }
            if (capture.HasInstance)
            {
                Closure.GetOrCreateInstanceField();
            }

            return Closure;
        }

        protected override IEnumerable<IDSharpType> GetTypes()
        {
            return Method.GetGenericParameters();
        }
        protected override IEnumerable<IDSharpType> GetTypes(string name)
        {
            foreach (var genericType in Method.GetGenericParameters().Where(t => t.Name == name))
            {
                yield return genericType;
            }
        }
        protected override IEnumerable<IDSharpMemberInfo> GetMembers(string name)
        {
            if (IterationMethodInfo != null &&
                IterationMethodInfo.TryGetScopedVariable(this, name, out var variable))
            {
                yield return variable;
            }

            foreach (var type in GetTypes(name))
            {
                yield return type;
            }
        }
        protected override IEnumerable<IDSharpParameterInfo> GetVariables()
        {
            if (IsRoot)
            {
                foreach (var parameter in Method.Parameters)
                {
                    yield return parameter;
                }
            }

            foreach (var variable in Variables)
            {
                yield return variable;
            }
        }
        protected override IDSharpParameterInfo? CreateLocalVariable(string name, IDSharpType type)
        {
            if (Method.IsAbstract || Method.IsExtern)
            {
                return null;
            }

            var code = Method.GetBytecodeBuilder();

            if (Method.Parameters.Any(p => p.Name == name) ||
                Variables.Any(v => v.Name == name))
            {
                return null;
            }

            DSharpMethodBuilderParameter variable = new(Method.Assembly)
            {
                Name = name,
                Type = Method.Assembly.GetTypeToken(type)
            };
            code.LocalVariables.Add(variable);
            Variables.Add(variable);

            return variable;
        }

        #region Classes

        /// <summary>
        /// Information about captured variables
        /// </summary>
        /// <param name="scope">Scope that created this capture information</param>
        /// <param name="originalType">Type that contains original method</param>
        /// <param name="type">Type that contains fields as containers for variables and method that used as local variables</param>
        /// <param name="capture">Information about captured variables</param>
        /// <param name="instanceVariable">Variable that contains instance of type instance</param>
        /// <param name="instanceField">Field that contains instance of type instance</param>
        public class CaptureInfo(DSharpCompilerMethodScope scope, IDSharpType? originalType, DSharpTypeBuilder type, DSharpCaptureInfo.Capture capture, DSharpMethodBuilderParameter? instanceVariable, DSharpFieldBuilder? instanceField)
        {
            public DSharpCompilerMethodScope Scope { get; } = scope;
            /// <summary>
            /// Type that contains original method
            /// </summary>
            public IDSharpType? OriginalType { get; } = originalType;
            /// <summary>
            /// Type that contains fields as containers for variables and method that used as local variables
            /// </summary>
            public DSharpTypeBuilder Type { get; } = type;
            /// <summary>
            /// Field that contains current instance
            /// </summary>
            public DSharpFieldBuilder? InstanceField { get; private set; }
            /// <summary>
            /// Information about captured variables
            /// </summary>
            public DSharpCaptureInfo.Capture Capture { get; } = capture;
            /// <summary>
            /// Variable that stores instance of closure type
            /// </summary>
            public DSharpMethodBuilderParameter? ClosureContainer { get; } = instanceVariable;
            /// <summary>
            /// Field that stores instance of closure type
            /// </summary>
            public DSharpFieldBuilder? ClosureFieldContainer { get; } = instanceField;
            /// <summary>
            /// Captured parameters as fields
            /// </summary>
            public Dictionary<DSharpMethodBuilderParameter, DSharpFieldBuilder> CapturedParameters { get; } = [];
            /// <summary>
            /// Variables definitions as fields
            /// </summary>
            public Dictionary<AstNode, DSharpFieldBuilder> CapturedDeclaration { get; } = [];
            /// <summary>
            /// Custom fields that stores closure
            /// </summary>
            public Dictionary<DSharpMethodBuilder, DSharpFieldBuilder> CustomClosureContainers { get; } = [];
            /// <summary>
            /// Compiled delegates
            /// </summary>
            public Dictionary<DelegateExpressionNode, DSharpMethodBuilder> Delegates { get; } = [];
            /// <summary>
            /// Information about compiled delegates
            /// </summary>
            public Dictionary<DelegateExpressionNode, DSharpCompiledDelegateInfo> CompiledDelegatesInfo { get; } = [];

            /// <summary>
            /// Create field for containing calling object instance.
            /// If this field already created then it just return that field
            /// </summary>
            /// <returns>Field for containing calling object instance</returns>
            /// <exception cref="InvalidOperationException">Unable to create field for containing instance for capturing without provided original type</exception>
            public DSharpFieldBuilder GetOrCreateInstanceField()
            {
                if (InstanceField == null)
                {
                    if (OriginalType == null)
                    {
                        throw new DSharpCompilerException("Unable to create field for containing instance for capturing without provided original type", Capture.Scope);
                    }

                    DSharpFieldBuilder? parentInstanceField = null;

                    if (Scope.Parent != null && Scope.Parent.TryGetClosure(out var parentClosure))
                    {
                        parentInstanceField = parentClosure.GetOrCreateInstanceField();
                    }
                    if (OriginalType.IsStatic)
                    {
                        throw new DSharpCompilerException($"Unable to capture instance of static type \"{OriginalType}\"", Capture.Scope);
                    }

                    var instanceField = Type.CreateField(InstanceFieldName);
                    instanceField.FieldType = parentInstanceField?.FieldType ?? Type.Assembly.GetTypeToken(OriginalType);
                    instanceField.Access = DSharpAccessModifier.Public;
                    InstanceField = instanceField;
                }

                return InstanceField;
            }
            /// <summary>
            /// Generate code for loading closure instance from it's container
            /// </summary>
            /// <param name="code">Bytecode builder for generating code</param>
            /// <exception cref="InvalidOperationException">Closure container not found</exception>
            public void LoadClosureContainer(DSharpBytecodeBuilder code)
            {
                static void LoadFromField(DSharpBytecodeBuilder code, DSharpFieldBuilder field)
                {
                    code.LoadInstance();
                    code.LoadPropertyOrField(field);
                    code.PopOffset(1);
                }

                if (CustomClosureContainers.TryGetValue(code.Method, out var containerField))
                {
                    LoadFromField(code, containerField);
                }
                else if (ClosureFieldContainer != null)
                {
                    LoadFromField(code, ClosureFieldContainer);
                }
                else if (ClosureContainer != null)
                {
                    if (code.Method.DeclaringType == Type)
                    {
                        code.LoadInstance();
                    }
                    else
                    {
                        code.LoadLocal(ClosureContainer);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Closure container not found");
                }
            }
            /// <summary>
            /// Generate code for storing closure instance from it's container
            /// </summary>
            /// <param name="code">Bytecode builder for generating code</param>
            /// <param name="removeValueAndInstance">
            /// true: remove storing value and instance if loaded,
            /// false: remove only instance if it loaded,
            /// null: no nothing
            /// </param>
            /// <exception cref="InvalidOperationException">Closure container not found</exception>
            public void StoreClosureContainer(DSharpBytecodeBuilder code, bool? removeValueAndInstance = false)
            {
                static void StoreFromField(DSharpBytecodeBuilder code, DSharpFieldBuilder field, bool? removeValueAndInstance)
                {
                    code.LoadInstance();
                    code.StackMove(0, 1);
                    code.StorePropertyOrField(field);

                    if (removeValueAndInstance == true)
                    {
                        code.PopRepeat(2);
                    }
                    else if (removeValueAndInstance == false)
                    {
                        code.PopOffset(1);
                    }
                }

                if (CustomClosureContainers.TryGetValue(code.Method, out var containerField))
                {
                    StoreFromField(code, containerField, removeValueAndInstance);
                }
                else if (ClosureFieldContainer != null)
                {
                    StoreFromField(code, ClosureFieldContainer, removeValueAndInstance);
                }
                else if (ClosureContainer != null)
                {
                    code.StoreLocal(ClosureContainer);

                    if (removeValueAndInstance == true)
                    {
                        code.Pop();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Closure container not found");
                }
            }

            /// <summary>
            /// Name of field that contains captured instance
            /// </summary>
            public const string InstanceFieldName = "<>_instance";
        }

        #endregion
    }
}

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
        /// Is root scope. Root scope contains method parameters as variables
        /// </summary>
        public bool IsRoot { get; set; }

        public override void Clear(bool recursive = true)
        {
            base.Clear(recursive);

            Variables.Clear();
            DSharpBytecodeBuilder? code = null;

            if (!Method.IsDeclaration)
            {
                code = Method.GetBytecodeBuilder();
                code.Clear();
            }
            if (Closure != null)
            {
                Variables.Add(Closure.ClosureContainer);
                code?.LocalVariables.Add(Closure.ClosureContainer);
            }
        }

        /// <summary>
        /// Create captures container type
        /// </summary>
        /// <param name="owner">Node that contains captured variables</param>
        /// <returns>Type that purposed to use as container for captured values</returns>
        public CaptureInfo GetOrCreateCapture(DSharpCaptureInfo.Capture capture)
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

            if (CreateVariable(instanceVariableName, type) is not DSharpMethodBuilderParameter variable)
            {
                throw new DSharpCompilerException($"Unable to variable for storing closure in \"{Method}\"", capture.Scope);
            }

            if (type.DeclaringType != null)
            {
                type.Access = DSharpAccessModifier.Private;
            }
            foreach (var capturedVariable in capture.Variables.Keys)
            {
                var field = type.CreateField(capturedVariable);
                field.Access = DSharpAccessModifier.Public;
            }

            Closure = new(Method.DeclaringType, type, capture, variable);

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
            return GetTypes(name);
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
        /// <param name="originalType">Type that contains original method</param>
        /// <param name="type">Type that contains fields as containers for variables and method that used as local variables</param>
        /// <param name="capture">Information about captured variables</param>
        /// <param name="instanceVariable">Variable that contains instance of type instance</param>
        public class CaptureInfo(IDSharpType? originalType, DSharpTypeBuilder type, DSharpCaptureInfo.Capture capture, DSharpMethodBuilderParameter instanceVariable)
        {
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
            /// Variable that contains instance of type instance
            /// </summary>
            public DSharpMethodBuilderParameter ClosureContainer { get; } = instanceVariable;

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
                    if (OriginalType.IsStatic)
                    {
                        throw new DSharpCompilerException($"Unable to capture instance of static type \"{OriginalType}\"", Capture.Scope);
                    }

                    var instanceField = Type.CreateField(InstanceFieldName);
                    instanceField.FieldType = Type.Assembly.GetTypeToken(OriginalType);
                    instanceField.Access = DSharpAccessModifier.Public;
                    InstanceField = instanceField;
                }

                return InstanceField;
            }

            /// <summary>
            /// Name of field that contains captured instance
            /// </summary>
            public const string InstanceFieldName = "<>_instance";
        }

        #endregion
    }
}

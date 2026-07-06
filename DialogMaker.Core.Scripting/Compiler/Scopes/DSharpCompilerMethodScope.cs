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
        /// Is root scope. Root scope contains method parameters as variables
        /// </summary>
        public bool IsRoot { get; set; }

        protected override IEnumerable<IDSharpType> GetTypes(string name)
        {
            foreach (var genericType in Method.GetGenericParameters().Where(t => t.Name == name))
            {
                yield return genericType;
            }
        }
        protected override IEnumerable<IDSharpMemberInfo> GetMembers()
        {
            yield break;
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

        
    }
}

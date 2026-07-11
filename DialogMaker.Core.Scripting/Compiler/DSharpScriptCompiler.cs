using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Compiler.Scopes;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler
{
    /// <summary>
    /// D# script compiler
    /// </summary>
    /// <param name="assembly">Assembly to compile script</param>
    public partial class DSharpScriptCompiler(DSharpAssemblyBuilder assembly)
    {
        /// <summary>
        /// Assembly to compile script
        /// </summary>
        public DSharpAssemblyBuilder Assembly { get; } = assembly;
        /// <summary>
        /// Root scope for this script.
        /// It contains all usings and namespaces declared in script
        /// </summary>
        public DSharpCompilerFileScope Scope { get; } = new(assembly, null);
        /// <summary>
        /// Current script
        /// </summary>
        public DSharpScript? Script { get; set; }

        private DSharpCompilerContext Context => new()
        {
            Assembly = Assembly,
            Compiler = this,
            Usings = Scope.Namespaces,
            Scope = Scope
        };

        private readonly Dictionary<IDSharpType, DSharpCompilerTypeScope> _typeScopes = [];
        private readonly Dictionary<DSharpMethodBuilder, DSharpCompilerMethodScope> _methodScopes = [];

        #region Controls

        /// <summary>
        /// Reset script and remove all compiled types, global variables and functions from assembly
        /// </summary>
        public void Reset()
        {
            foreach (var type in _createdTypes.Keys)
            {
                Assembly.RemoveType(type);
            }
            foreach (var globalVariable in _createdGlobalVariables.Keys)
            {
                Assembly.RemoveGlobalVariable(globalVariable);
            }
            foreach (var globalFunction in _createdMethods.Keys.Where(m => m.DeclaringType == null))
            {
                Assembly.RemoveGlobalFunction(globalFunction);
            }

            Scope.Namespaces.Clear();
            _types.Clear();
            _createdGlobalVariables.Clear();
            _createdTypes.Clear();
            _createdFields.Clear();
            _createdProperties.Clear();
            _createdMethods.Clear();
            _enumTypes.Clear();
            _enumValues.Clear();
            _createdFinalizers.Clear();
            _createdIndexers.Clear();
            _createdOperators.Clear();
            _createdConstructors.Clear();
            _createdGlobalVariablesRawValueExpressions.Clear();
            _propertyFields.Clear();
            _propertiesWithCustomAccessors.Clear();
            _currentNamespace = null;
            _typesToSetupBases = null;
        }
        /// <summary>
        /// Parse script and create types that declared in it
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to declare types when tree root not specified</exception>
        public void DeclareTypes()
        {
            if (Script == null)
            {
                throw new InvalidOperationException("Unable to declare types when tree root not specified");
            }

            ParseStatements(Script.Statements);
        }
        /// <summary>
        /// Resolve types on created types and members
        /// </summary>
        public void ResolveTypes()
        {
            ResolveCreatedTypes();
        }
        /// <summary>
        /// Validate all types that created by script.
        /// It checks interfaces and abstract classes implementations
        /// </summary>
        public partial void ValidateTypes();
        /// <summary>
        /// Compile code inside script methods
        /// </summary>
        public void CompileCode()
        {
            foreach (var info in _createdProperties)
            {
                CompileProperty(info.Key, info.Value);
            }
            foreach (var info in _createdIndexers)
            {
                CompileProperty(info.Key, info.Value);
            }
            foreach (var info in _createdMethods)
            {
                CompileMethod(info.Key, info.Value);
            }
            foreach (var info in _createdConstructors)
            {
                CompileMethod(info.Key, info.Value);
            }
            foreach (var info in _createdFinalizers)
            {
                CompileMethod(info.Key, info.Value);
            }
        }
        /// <summary>
        /// Final resolving types and creating all required generics
        /// </summary>
        public void ApplyTypes()
        {
            int i = 0;
            var types = Assembly.Types;

            while (i < types.Count)
            {
                var type = types[i];
                type.Update();
                i++;
            }
        }

        public override string ToString()
        {
            return Script?.ToString() ?? base.ToString();
        }

        #endregion

        #region Scopes

        /// <summary>
        /// Get scope for member based on current script with specified parent
        /// </summary>
        /// <param name="member">Member to creating scope</param>
        /// <param name="parent">Parent of new scope</param>
        /// <returns>Member scope</returns>
        public DSharpCompilerScope GetScope(IDSharpMemberInfo member, DSharpCompilerScope? parent)
        {
            if (parent == null)
            {
                return GetScope(member);
            }
            if (member is DSharpMethodBuilder method)
            {
                return new DSharpCompilerMethodScope(method, parent);
            }
            else if (member is IDSharpType typeMember)
            {
                return new DSharpCompilerTypeScope(Assembly, typeMember, parent);
            }
            else if (member.DeclaringType != null)
            {
                return new DSharpCompilerTypeScope(Assembly, member.DeclaringType, parent);
            }

            return Scope;
        }

        /// <summary>
        /// Get scope for member based on current script
        /// </summary>
        /// <param name="member">Member to creating scope</param>
        /// <returns>Member scope</returns>
        public DSharpCompilerScope GetScope(IDSharpMemberInfo member)
        {
            if (member is DSharpMethodBuilder method)
            {
                return GetScope(method);
            }
            else if (member is IDSharpType typeMember)
            {
                return GetScope(typeMember);
            }
            else if (member.DeclaringType != null)
            {
                return GetScope(member.DeclaringType);
            }

            return Scope;
        }
        /// <summary>
        /// Get scope for type based on current script
        /// </summary>
        /// <param name="type">Type to creating scope</param>
        /// <returns>Type scope</returns>
        public DSharpCompilerTypeScope GetScope(IDSharpType type)
        {
            if (!_typeScopes.TryGetValue(type, out var scope))
            {
                scope = new(Assembly, type, Scope);
                _typeScopes.Add(type, scope);
            }

            return scope;
        }
        /// <summary>
        /// Get scope for method based on current script
        /// </summary>
        /// <param name="method">Method to creating scope</param>
        /// <returns>Method scope</returns>
        public DSharpCompilerMethodScope GetScope(DSharpMethodBuilder method)
        {
            if (!_methodScopes.TryGetValue(method, out var scope))
            {
                DSharpCompilerScope parentScope = Scope;

                if (method.DeclaringType != null)
                {
                    parentScope = GetScope(method.DeclaringType);
                }

                scope = new(method, parentScope)
                {
                    IsRoot = true
                };
                _methodScopes.Add(method, scope);
            }

            return scope;
        }

        #endregion

        #region Types resolving

        private DSharpTypeToken ResolveType(DSharpMemberInfoBuilder member, TypeInfoNode typeInfo)
        {
            var context = Context;
            context.CurrentMember = member;
            context.Scope = GetScope(member);

            if (typeInfo.Name == DSharpAssemblyBuilder.VarName &&
                member.DeclaringType == null &&
                member is DSharpFieldBuilder field &&
                _createdGlobalVariablesRawValueExpressions.TryGetValue(field, out var valueExpression))
            {
                context.ParentExpression = valueExpression;
            }

            var typeToken = context.ResolveType(typeInfo);
            var type = Assembly.GetType(typeToken);

            if (!context.CanAccessTo(type))
            {
                context.ThrowCanNotAccessException(type);
            }

            return typeToken;
        }

        #endregion
    }
}

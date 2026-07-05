using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Scopes;
using DialogMaker.Core.Scripting.Runtime;
using DialogMaker.Core.Scripting.Runtime.Builders;

namespace DialogMaker.Core.Scripting.Compiler
{
    public partial class DSharpScriptCompiler(DSharpAssemblyBuilder assembly)
    {
        public DSharpAssemblyBuilder Assembly { get; } = assembly;
        public DSharpCompilerFileScope Scope { get; } = new(assembly, null);
        public DSharpTreeRoot? TreeRoot { get; set; }

        private DSharpCompilerContext Context => new()
        {
            Assembly = Assembly,
            Usings = Scope.Namespaces,
            Scope = Scope
        };

        private readonly Dictionary<IDSharpType, DSharpCompilerTypeScope> _typeScopes = [];
        private readonly Dictionary<DSharpMethodBuilder, DSharpCompilerMethodScope> _methodScopes = [];

        #region Controls

        public void Reset()
        {
            Scope.Namespaces.Clear();
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
        }
        public void DeclareTypes()
        {
            if (TreeRoot == null)
            {
                throw new InvalidOperationException($"Unable to declare types when tree root not specified");
            }

            ParseStatements(TreeRoot.Statements);
        }
        public void ResolveTypes()
        {
            ResolveCreatedTypes();
        }
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

        #endregion

        #region Scopes

        public DSharpCompilerTypeScope GetScope(IDSharpType type)
        {
            if (!_typeScopes.TryGetValue(type, out var scope))
            {
                scope = new(Assembly, type, Scope);
                _typeScopes.Add(type, scope);
            }

            return scope;
        }
        public DSharpCompilerMethodScope GetScope(DSharpMethodBuilder method)
        {
            if (!_methodScopes.TryGetValue(method, out var scope))
            {
                DSharpCompilerScope parentScope = Scope;

                if (method.DeclaringType != null)
                {
                    parentScope = GetScope(method.DeclaringType);
                }

                scope = new(method, parentScope);
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

using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Runtime.Compilers;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    public struct DSharpCompilerContext
    {
        public DSharpCompilerContext()
        {
        }
        public DSharpCompilerContext(DSharpCompilerContext context)
            : this(context, context.CurrentMember)
        {
        }
        public DSharpCompilerContext(DSharpCompilerContext context, IDSharpMemberInfo? currentMember)
        {
            Assembly = context.Assembly;
            Usings = context.Usings;
            TypeResolver = context.TypeResolver;
            CurrentMember = currentMember;
            ResolvedTypes = context.ResolvedTypes;
            ParentExpression = context.ParentExpression;
        }

        public DSharpAssemblyBuilder? Assembly { get; set; }
        public IEnumerable<string>? Usings { get; set; }
        public Func<ExpressionNode, IDSharpType?>? TypeResolver { get; set; }
        public IDSharpMemberInfo? CurrentMember { get; set; }
        public Dictionary<string, DSharpTypeToken>? ResolvedTypes { get; set; }
        public ExpressionNode? ParentExpression { get; set; }

        #region Управление

        /// <summary>
        /// Try to resolve type that referenced by identifier. 
        /// It finds element (method/function parameter/local variable, property, field, method) 
        /// that represents by expression and return it's type
        /// </summary>
        /// <param name="expression">Expression that contains identifier of element</param>
        /// <param name="result">Resolved type</param>
        /// <returns>Resolve status</returns>
        public readonly bool TryResolveTypeOfExpression(ExpressionNode expression, [NotNullWhen(true)] out IDSharpType? result)
        {
            result = null;

            if (CurrentMember == null && TypeResolver == null && Assembly == null)
            {
                return false;
            }
            if (expression is IdentifierExpressionNode identifier)
            {
                if (CurrentMember is IDSharpMethodInfo method)
                {
                    var parameters = method.GetParameters();
                    var parameter = parameters.FirstOrDefault(p => p.Name == identifier.Name);

                    if (parameter != null)
                    {
                        result = parameter.Type;
                        return true;
                    }
                }
                if (Assembly != null)
                {
                    var variables = Assembly.GetGlobalVariables();
                    var variable = variables.FirstOrDefault(v => v.Name == identifier.Name);

                    if (variable != null)
                    {
                        result = variable.FieldType;
                        return true;
                    }
                }
                if (CurrentMember?.DeclaringType != null)
                {
                    var property = CurrentMember.DeclaringType.GetProperty(identifier.Name);

                    if (property != null)
                    {
                        result = property.PropertyType;
                        return true;
                    }

                    var field = CurrentMember.DeclaringType.GetField(identifier.Name);

                    if (field != null)
                    {
                        result = field.FieldType;
                        return true;
                    }
                }
            }
            else if (expression is MemberAccessExpressionNode &&
                     TryResolveMember(expression, out var resolvedMember) &&
                     resolvedMember.TryGetReturnType(out result))
            {
                return true;
            }
            if (TypeResolver != null)
            {
                result = TypeResolver(expression);

                if (result != null)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Try to resolve type by information
        /// </summary>
        /// <param name="typeInfo">Information about type</param>
        /// <param name="result">Resolved type</param>
        /// <returns>Resolve status</returns>
        public readonly bool TryResolveMember(ExpressionNode expression, [NotNullWhen(true)] out IDSharpMemberInfo? result)
        {
            result = null;

            if (expression is IdentifierExpressionNode identifier)
            {
                var name = identifier.GetName(true);

                if (CurrentMember != null)
                {
                    result = FindAnyAvailableMember(name);
                }
                if (result == null && Assembly != null)
                {
                    if (ParentExpression is not MemberAccessExpressionNode &&
                        TryResolveType(name, out var type))
                    {
                        result = Assembly.GetType(type);
                    }
                    else
                    {
                        result = Assembly.GlobalVariables.FirstOrDefault(v => v.Name == name);
                        result ??= Assembly.GlobalFunctions.FirstOrDefault(f => f.Name == name);
                    }
                }
            }
            else if (expression is CallExpressionNode callExpression)
            {
                if (callExpression.Callee == null)
                {
                    throw new ArgumentException($"Method or function identifier not provided: {expression}", nameof(expression));
                }
                if (callExpression.Callee is not IdentifierExpressionNode methodIdentifier)
                {
                    throw new ArgumentException($"Method or function name must be identifier, got: {callExpression.Callee}", nameof(expression));
                }

                var name = methodIdentifier.GetName(false);
                DSharpMetadataToken[] args = new DSharpMetadataToken[callExpression.Arguments.Count];
                result = FindMethod(name, true, args);
            }
            else if (expression is MemberAccessExpressionNode memberAccess)
            {
                while (memberAccess.Target is MemberAccessExpressionNode targetMemberAccess)
                {
                    memberAccess = targetMemberAccess;
                }

                if (Assembly == null)
                {
                    throw new InvalidOperationException($"Unable to get expression type because compiler context does not contains assembly builder: {memberAccess}");
                }

                IDSharpMemberInfo? targetType = null;

                ExpressionNode ParseMemberAccess(MemberAccessExpressionNode memberAccess, DSharpCompilerContext context)
                {
                    if (memberAccess.Target == null ||
                        memberAccess.Member == null)
                    {
                        throw new ArgumentException($"Incomplete expression: {memberAccess}");
                    }

                    targetType = memberAccess.Target.GetExpressionType(context.Assembly!, context)
                        ?? throw new ArgumentException($"Expression should return some value: {memberAccess.Target}");

                    if (memberAccess.Member is MemberAccessExpressionNode nextMemberAccess)
                    {
                        context = new(context, targetType)
                        {
                            ParentExpression = memberAccess
                        };
                        return ParseMemberAccess(nextMemberAccess, context);
                    }

                    return memberAccess.Member;
                }

                var memberExpression = ParseMemberAccess(memberAccess, this);

                if (targetType == null)
                {
                    throw new InvalidOperationException($"Unable to find type of expression: {expression}");
                }

                DSharpCompilerContext context = new(this, targetType)
                {
                    ParentExpression = memberAccess
                };

                context.TryResolveMember(memberExpression, out result);
            }

            if (result == null && TypeResolver != null)
            {
                result = TypeResolver(expression);
            }

            return result != null;
        }
        public readonly bool TryResolveType(TypeInfoNode typeInfo, [NotNullWhen(true)] out DSharpTypeToken? result)
        {
            result = null;
            string typeName = typeInfo.GetFullName(true, false);
            string? @namespace = CurrentMember?.DeclaringType?.Namespace;
            result = InternalTryResolveType(@namespace, typeName);

            if (result != null)
            {
                return true;
            }

            if (Usings != null)
            {
                foreach (var @using in Usings)
                {
                    result = InternalTryResolveType(@using, typeName);

                    if (result != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        public readonly bool TryResolveType(string name, [NotNullWhen(true)] out DSharpTypeToken? result)
        {
            result = null;

            if (Assembly != null && Assembly.TryGetStandardType(name, out result))
            {
                return true;
            }

            result = InternalTryResolveType(null, name);

            if (result != null)
            {
                return true;
            }
            if (CurrentMember?.DeclaringType?.Namespace != null)
            {
                result = InternalTryResolveType(CurrentMember.DeclaringType.Namespace, name);

                if (result != null)
                {
                    return true;
                }
            }
            if (Usings != null)
            {
                foreach (var @namespace in Usings)
                {
                    result = InternalTryResolveType(@namespace, name);

                    if (result != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        public readonly DSharpTypeToken ResolveType(TypeInfoNode typeInfo)
        {
            if (TryResolveType(typeInfo, out var result))
            {
                return result;
            }

            throw new ArgumentException($"Unable to resolve type: {typeInfo.GetFullName(false, false)}", nameof(typeInfo));
        }
        public readonly IDSharpMemberInfo FindAnyAvailableMember(string name)
        {
            return FindAnyAvailableMember<IDSharpMemberInfo>(name);
        }
        public readonly T FindAnyAvailableMember<T>(string name)
            where T : IDSharpMemberInfo
        {
            var members = FindAvailableMembers<T>(name);

            if (members.Count == 1)
            {
                return members[0];
            }
            else if (members.Count > 1)
            {
                throw new ArgumentException($"Found multiple members with the same name {name} in {CurrentMember}", nameof(name));
            }

            throw new ArgumentException($"Unknown member: {name}", nameof(name));
        }
        public readonly List<T> FindAvailableMembers<T>(string name)
            where T : IDSharpMemberInfo
        {
            List<T> result = [];

            bool Add(IDSharpMemberInfo member)
            {
                if (member is T typedMember)
                {
                    result.Add(typedMember);
                    return true;
                }

                return false;
            }
            int AddRange(IEnumerable<IDSharpMemberInfo> members)
            {
                int count = 0;

                foreach (var member in members)
                {
                    if (Add(member))
                    {
                        count++;
                    }
                }

                return count;
            }

            if (Assembly != null)
            {
                if (Assembly.TryGetStandardType(name, out var typeToken))
                {
                    Add(Assembly.GetType(typeToken));
                    return result;
                
                }
                var variable = Assembly.GetGlobalVariables().FirstOrDefault(f => f.Name == name);
                var functions = Assembly.GetGlobalFunctions().Where(f => f.Name == name);

                Add(variable);
                AddRange(functions);
            }
            if (CurrentMember == null)
            {
                return result;
            }

            IDSharpType? type = CurrentMember as IDSharpType ?? CurrentMember.DeclaringType;

            if (type == null)
            {
                return result;
            }

            int FindMember(IDSharpType type, Predicate<IDSharpMemberInfo> predicate)
            {
                var properties = type.GetProperties(predicate);
                var fields = type.GetFields(predicate);
                var methods = type.GetMethods(predicate);
                var constructors = type.GetConstructors(predicate);

                return AddRange(properties) + AddRange(fields) + AddRange(methods);
            }
            int FindBaseTypeMember(IDSharpType type, Predicate<IDSharpMemberInfo> predicate)
            {
                int count = 0;

                foreach (var baseType in type.GetBaseTypes())
                {
                    count += FindMember(baseType, predicate);
                    count += FindBaseTypeMember(baseType, predicate);
                }

                return count;
            }
            int FindInDeclaringType(IDSharpType type)
            {
                if (type.DeclaringType == null)
                {
                    return 0;
                }

                type = type.DeclaringType;
                int count = FindMember(type, m => m.Name == name && m.IsStatic);

                return count + FindInDeclaringType(type);
            }

            FindMember(type, m => m.Name == name);
            FindBaseTypeMember(type, m => m.Name == name && m.Access == DSharpAccessModifier.Public ||
                                                            m.Access == DSharpAccessModifier.Protected);
            FindInDeclaringType(type);

            return result;
        }
        public readonly IDSharpMethodInfo? FindConstructor(bool matchByParametersCount, params IEnumerable<DSharpMetadataToken>? parameters)
        {
            var members = FindAvailableMembers<IDSharpMethodInfo>(DSharpTypeBuilder.ConstructorName);

            if (members.Count == 0)
            {
                if (parameters == null)
                {
                    throw new ArgumentException($"Unable to find any constructor at {CurrentMember}");
                }

                return null;
            }

            try
            {
                return FindMethod(members, matchByParametersCount, parameters);
            }
            catch (Exception error)
            {
                throw new ArgumentException($"Unable to find constructor", error);
            }
        }
        public readonly IDSharpMethodInfo FindMethod(string name, bool matchByParametersCount, params IEnumerable<DSharpMetadataToken>? parameters)
        {
            var members = FindAvailableMembers<IDSharpMethodInfo>(name);

            if (members.Count == 0)
            {
                throw new ArgumentException($"Unable to find any method with name {name} at {CurrentMember}", nameof(name));
            }

            try
            {
                return FindMethod(members, matchByParametersCount, parameters);
            }
            catch (Exception error)
            {
                throw new ArgumentException($"Unable to find method {name}", error);
            }
        }
        public readonly IDSharpMethodInfo FindMethod(IEnumerable<IDSharpMethodInfo> members, bool matchByParametersCount, params IEnumerable<DSharpMetadataToken>? parameters)
        {
            if (!members.Any())
            {
                throw new ArgumentException($"Members enumeration is empty", nameof(members));
            }
            if (Assembly == null)
            {
                throw new InvalidOperationException("Assembly not provided");
            }
            if (parameters == null || !parameters.Any())
            {
                return members.First();
            }

            int parameterIndex = 0;
            List<IDSharpMethodInfo> methods = [.. members];

            if (matchByParametersCount)
            {
                int parametersCount = parameters.Count();
                methods.RemoveAll(m => m.GetParameters().Length != parametersCount);
            }
            else
            {
                Dictionary<IDSharpMethodInfo, IDSharpParameterInfo[]> methodParameters = [];

                foreach (var parameter in parameters)
                {
                    if (Assembly.GetType(parameter) is not IDSharpType parameterType)
                    {
                        throw new ArgumentException($"Parameter type must be a type on parameter with index {parameterIndex}", nameof(parameters));
                    }

                    methods.RemoveAll(method =>
                    {
                        if (!methodParameters.TryGetValue(method, out var p))
                        {
                            p = method.GetParameters();
                            methodParameters.Add(method, p);
                        }

                        if (parameterIndex >= p.Length)
                        {
                            return true;
                        }

                        return parameterType != p[parameterIndex];
                    });

                    parameterIndex++;
                }
            }

            if (methods.Count == 1)
            {
                return methods[0];
            }
            else if (methods.Count > 1)
            {
                throw new ArgumentException($"Multiple methods with same parameters type was found at {CurrentMember}", nameof(members));
            }

            throw new ArgumentException($"Unable to find overload for method at {CurrentMember}", nameof(members));
        }


        private readonly DSharpTypeToken? InternalTryResolveType(string? @namespace, string typeName)
        {
            var fullName = $"{@namespace}.{typeName}";

            if (ResolvedTypes != null &&
                (ResolvedTypes.TryGetValue(typeName, out var token) ||
                ResolvedTypes.TryGetValue(fullName, out token)))
            {
                return token;
            }
            if (Assembly != null &&
                (Assembly.TryGetStandardType(typeName, out token) ||
                Assembly.TryGetTypeToken(typeName, out token)))
            {
                ResolvedTypes?.Add(typeName, token);
                return token;
            }
            if (Assembly != null && CurrentMember?.DeclaringType != null)
            {
                var rootType = CurrentMember.DeclaringType;

                while (rootType != null)
                {
                    if (rootType.Name == typeName)
                    {
                        return Assembly.GetTypeToken(rootType);
                    }

                    rootType = rootType.DeclaringType;
                }
            }
            if (@namespace == null || Assembly == null)
            {
                return null;
            }
            if (Assembly.TryGetTypeToken(fullName, out token))
            {
                ResolvedTypes?.Add(fullName, token);
                return token;
            }

            return null;
        }

        #endregion
    }
}

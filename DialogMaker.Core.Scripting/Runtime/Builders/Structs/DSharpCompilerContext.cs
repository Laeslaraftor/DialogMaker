using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Runtime.Compilers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

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
            MemberResolver = context.MemberResolver;
            CurrentMember = currentMember;
            ResolvedTypes = context.ResolvedTypes;
            ParentExpression = context.ParentExpression;
        }

        public DSharpAssemblyBuilder? Assembly { get; set; }
        public IEnumerable<string>? Usings { get; set; }
        public Func<object, IDSharpType?>? TypeResolver { get; set; }
        public Func<DSharpCompilerContext, object, IDSharpMemberInfo?>? MemberResolver { get; set; }
        public IDSharpMemberInfo? CurrentMember { get; set; }
        public Dictionary<string, DSharpTypeToken>? ResolvedTypes { get; set; }
        public ExpressionNode? ParentExpression { get; set; }
        public DSharpBytecodeBuilder.Instruction? CurrentLoopStartInstruction { get; set; }
        public DSharpBytecodeBuilder.Instruction? CurrentLoopEndInstruction { get; set; }

        #region Доступ

        public readonly bool CanAccessTo(IDSharpMemberInfo member)
        {
            if (member.DeclaringType == null &&
                member.Access == DSharpAccessModifier.Public)
            {
                return true;
            }
            if (CurrentMember != null)
            {
                IDSharpType? memberType = CurrentMember as IDSharpType ?? CurrentMember.DeclaringType;
                IDSharpType? rootType = memberType;

                while (rootType != null)
                {
                    if (member.DeclaringType == rootType)
                    {
                        return true;
                    }

                    rootType = rootType.DeclaringType;
                }

                bool CheckInBaseTypes(DSharpAssemblyBuilder assembly, IDSharpType type)
                {
                    foreach (var baseMember in type.GetAllMembers(m => m.Access == DSharpAccessModifier.Public ||
                                                                       m.Access == DSharpAccessModifier.Protected ||
                                                                       m.Access == DSharpAccessModifier.Internal && m.Assembly == assembly))
                    {
                        if (baseMember == member)
                        {
                            return true;
                        }
                    }

                    foreach (var baseType in type.GetBaseTypes())
                    {
                        if (CheckInBaseTypes(assembly, baseType))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                if (Assembly != null &&
                    memberType != null &&
                    CheckInBaseTypes(Assembly, memberType))
                {
                    return true;
                }
            }

            var currentType = CurrentMember as IDSharpType ?? CurrentMember?.DeclaringType;

            while (currentType != null)
            {
                if (currentType.Access != DSharpAccessModifier.Public)
                {
                    return false;
                }

                currentType = currentType.DeclaringType;
            }

            return member.Access == DSharpAccessModifier.Public;
        }
        [DoesNotReturn]
        public readonly void ThrowCanNotAccessException(IDSharpMemberInfo member)
        {
            string message = $"Can not access to \"{member}\"";

            if (CurrentMember != null)
            {
                message += $" from \"{CurrentMember}\"";
            }

            throw new InvalidOperationException(message);
        }
        [DoesNotReturn]
        public readonly void ThrowThisOrBaseIsUnavailable(ExpressionNode expression)
        {
            if (expression is ThisExpressionNode)
            {
                ThrowThisIsUnavailable(expression);
            }
            else if (expression is BaseExpressionNode)
            {
                ThrowBaseIsUnavailable(expression);
            }

            throw new InvalidOperationException($"Expression is unavailable in current context: {expression}");
        }
        [DoesNotReturn]
        public readonly void ThrowThisIsUnavailable(ExpressionNode expression)
        {
            throw new InvalidOperationException($"\"this\" is unavailable in current context: {expression}");
        }
        [DoesNotReturn]
        public readonly void ThrowBaseIsUnavailable(ExpressionNode expression)
        {
            throw new InvalidOperationException($"\"base\" is unavailable in current context: {expression}");
        }

        #endregion

        #region Поиск типов

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
                var member = TypeResolver(expression);

                if (member == null)
                {
                    return false;
                }
                if (member is IDSharpType typeMember)
                {
                    result = typeMember;
                    return true;
                }
                if (member.TryGetReturnType(out result))
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
                if (memberAccess.Target == null || memberAccess.Member == null)
                {
                    throw new ArgumentException($"Incomplete expression: {expression}", nameof(expression));
                }
                if (!TryResolveMember(memberAccess.Target, out var member))
                {
                    throw new InvalidOperationException($"Unable to resolve member for expression: {memberAccess.Target}");
                }
                if (member is not IDSharpType)
                {
                    if (member.TryGetReturnType(out var memberType))
                    {
                        member = memberType;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unable to get returning type of \"{member}\"");
                    }
                }

                DSharpCompilerContext context = new(this, member);
                return context.TryResolveMember(memberAccess.Member, out result);

                //while (memberAccess.Target is MemberAccessExpressionNode targetMemberAccess)
                //{
                //    memberAccess = targetMemberAccess;
                //}

                //if (Assembly == null)
                //{
                //    throw new InvalidOperationException($"Unable to get expression type because compiler context does not contains assembly builder: {memberAccess}");
                //}

                //IDSharpMemberInfo? targetType = null;

                //ExpressionNode ParseMemberAccess(MemberAccessExpressionNode memberAccess, DSharpCompilerContext context)
                //{
                //    if (memberAccess.Target == null ||
                //        memberAccess.Member == null)
                //    {
                //        throw new ArgumentException($"Incomplete expression: {memberAccess}");
                //    }

                //    targetType = memberAccess.Target.GetExpressionType(context.Assembly!, context)
                //        ?? throw new ArgumentException($"Expression should return some value: {memberAccess.Target}");

                //    if (memberAccess.Member is MemberAccessExpressionNode nextMemberAccess)
                //    {
                //        context = new(context, targetType)
                //        {
                //            ParentExpression = memberAccess
                //        };
                //        return ParseMemberAccess(nextMemberAccess, context);
                //    }

                //    return memberAccess.Member;
                //}

                //var memberExpression = ParseMemberAccess(memberAccess, this);

                //if (targetType == null)
                //{
                //    throw new InvalidOperationException($"Unable to find type of expression: {expression}");
                //}

                //DSharpCompilerContext context = new(this, targetType)
                //{
                //    ParentExpression = memberAccess
                //};

                //context.TryResolveMember(memberExpression, out result);
            }

            if (result == null && TypeResolver != null)
            {
                result = TypeResolver(expression);
            }

            return result != null;
        }
        public readonly bool TryResolveMember(string name, [NotNullWhen(true)] out IDSharpMemberInfo? result)
        {
            return TryResolveMember(name, false, out result);
        }
        public readonly bool TryResolveType(TypeInfoNode typeInfo, [NotNullWhen(true)] out DSharpTypeToken? result)
        {
            result = null;
            string typeName = typeInfo.GetFullName(true, false);

            if (typeName == DSharpAssemblyBuilder.VarName)
            {
                if (ParentExpression != null)
                {
                    if (Assembly == null)
                    {
                        throw new InvalidOperationException($"Unable to resolve type because assembly builder not specified");
                    }

                    var member = ParentExpression.GetExpressionType(Assembly, this);

                    if (member != null)
                    {
                        result = Assembly.GetTypeToken(member);
                    }
                }

                return result != null;
            }
            if (CurrentMember != null && Assembly != null)
            {
                IDSharpType? member;

                if (CurrentMember is IDSharpType type)
                {
                    member = type;
                }
                else
                {
                    member = CurrentMember.DeclaringType;
                }

                while (member != null)
                {
                    var genericType = member.GetGenericTypes().FirstOrDefault(t => t.Name == typeName);

                    if (genericType != null)
                    {
                        result = Assembly.GetTypeToken(genericType);
                        break;
                    }

                    member = member.DeclaringType;
                }
            }

            if (result == null)
            {
                var declaringType = CurrentMember?.DeclaringType;

                result = InternalTryResolveType(declaringType?.FullName, typeName);
                result ??= InternalTryResolveType(declaringType?.Namespace, typeName);

                if (result == null)
                {
                    if (Usings != null)
                    {
                        foreach (var @using in Usings)
                        {
                            result = InternalTryResolveType(@using, typeName);

                            if (result != null)
                            {
                                if (typeInfo.ArrayDimensions > 0)
                                {
                                    var arrayType = CreateArray(result, typeInfo.ArrayDimensions);
                                    result = Assembly!.GetTypeToken(arrayType);
                                }

                                break;
                            }
                        }
                    }
                }
            }
            if (result != null)
            {
                if (Assembly == null)
                {
                    throw new InvalidOperationException($"Unable to resolve type because assembly builder not provided: {typeInfo}");
                }

                if (typeInfo.ArrayDimensions > 0)
                {
                    var arrayType = CreateArray(result, typeInfo.ArrayDimensions);
                    result = Assembly.GetTypeToken(arrayType);
                }
                else if (typeInfo.GenericParameters.Count > 0)
                {
                    IDSharpType[] genericParameters = new IDSharpType[typeInfo.GenericParameters.Count];

                    for (int i = 0; i < typeInfo.GenericParameters.Count; i++)
                    {
                        genericParameters[i] = (IDSharpType)Assembly.GetType(ResolveType(typeInfo.GenericParameters[i]));
                    }

                    var genericType = (IDSharpType)Assembly.GetType(result);

                    if (genericType.GenericTemplate != null)
                    {
                        genericType = genericType.GenericTemplate;
                    }

                    DSharpTypeBuilder? parent = null;

                    if (genericType.DeclaringType is DSharpTypeBuilder declaringTypeBuilder)
                    {
                        parent = declaringTypeBuilder;
                    }

                    genericType = Assembly.FillGeneric(genericType, parent, genericParameters);
                    result = Assembly.GetTypeToken(genericType);
                }

                return true;
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
            if (CurrentMember is DSharpMethodBuilder method)
            {
                var parameter = method.Parameters.FirstOrDefault(p => p.Name == name);

                if (parameter?.Type != null)
                {
                    result = parameter.Type;
                    return true;
                }
            }
            if (Assembly != null && TypeResolver != null)
            {
                var type = TypeResolver(name);

                if (type != null)
                {
                    result = Assembly.GetTypeToken(type);

                    if (result != null)
                    {
                        return true;
                    }
                }
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

            throw new ArgumentException($"Unable to resolve type \"{typeInfo.GetFullName(false, false)}\": {typeInfo}", nameof(typeInfo));
        }
        public readonly DSharpTypeToken ResolveType(string typeName)
        {
            if (TryResolveType(typeName, out var result))
            {
                return result;
            }

            throw new ArgumentException($"Unable to resolve type \"{typeName}\"", nameof(typeName));
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
                return members.First();
            }
            else if (members.Count > 1)
            {
                StringBuilder builder = new();
                builder.AppendLine($"Found multiple members with the same name \"{name}\" in {CurrentMember}");
                int i = 1;

                foreach (var member in members)
                {
                    builder.AppendLine($"{i}: {member}");
                    i++;
                }

                throw new ArgumentException(builder.ToString().TrimEnd(), nameof(name));
            }

            throw new ArgumentException($"Unknown member \"{name}\" at \"{CurrentMember}\"", nameof(name));
        }
        public readonly HashSet<T> FindAvailableMembers<T>(string name)
            where T : IDSharpMemberInfo
        {
            HashSet<T> result = [];

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
                if (TryResolveMember(name, true, out var typeToken))
                {
                    Add(typeToken);
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
                //var constructors = type.GetConstructors(predicate);

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

                count += FindMember(type.Assembly.ObjectType, predicate);

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

            AddRange(type.GetChildrenTypes().Where(t => t.Name == name));
            FindMember(type, m => m.Name == name);
            FindBaseTypeMember(type, m => m.Name == name && (m.Access == DSharpAccessModifier.Public ||
                                                            m.Access == DSharpAccessModifier.Protected));

            if (ParentExpression == null)
            {
                FindInDeclaringType(type);
            }

            return result;
        }
        public readonly IDSharpMethodInfo? FindConstructor(int parametersCount)
        {
            DSharpMetadataToken[] tokens = new DSharpMetadataToken[parametersCount];
            return FindConstructor(true, tokens);
        }
        public readonly IDSharpMethodInfo? FindConstructor(bool matchByParametersCount, params IEnumerable<DSharpMetadataToken>? parameters)
        {
            if (CurrentMember == null)
            {
                throw new InvalidOperationException("Unable to find constructor with no provided current member");
            }

            var type = CurrentMember as IDSharpType ?? CurrentMember.DeclaringType;

            if (type == null)
            {
                throw new InvalidOperationException($"Unable to find constructor using member: {CurrentMember}");
            }

            var constructors = type.GetConstructors();

            if (constructors.Length == 0)
            {
                if (parameters != null)
                {
                    throw new ArgumentException($"Unable to find any constructor at {CurrentMember}");
                }

                return null;
            }

            int parametersCount = 0;

            if (parameters != null)
            {
                parametersCount = parameters.Count();
            }

            static bool SequenceEqual(IDSharpParameterInfo[] parameters, IEnumerable<DSharpMetadataToken> tokens)
            {
                if (parameters.Length == 0)
                {
                    return false;
                }

                int i = 0;

                foreach (var token in tokens)
                {
                    if (i + 1 >= parameters.Length ||
                        parameters[i].Type.MetadataToken != token)
                    {
                        return false;
                    }

                    i++;
                }

                if (i + 1 != parameters.Length)
                {
                    return false;
                }

                return true;
            }

            if (matchByParametersCount)
            {
                return constructors.FirstOrDefault(c => c.GetParameters().Length == parametersCount)
                    ?? throw new ArgumentException($"Unable to find constructor with parameters count {parametersCount} at \"{type}\"");
            }
            else
            {
                foreach (var constructor in constructors)
                {
                    var constructorParameters = constructor.GetParameters();

                    if (parameters == null || parametersCount == 0)
                    {
                        if (constructorParameters.Length == 0)
                        {
                            return constructor;
                        }
                    }
                    else if (SequenceEqual(constructorParameters, parameters))
                    {
                        return constructor;
                    }
                }

                throw new ArgumentException($"Unable to find constructor with parameters count {parametersCount} at \"{type}\"");
            }
        }
        public readonly IDSharpMethodInfo FindMethod(CallExpressionNode callExpression)
        {
            if (callExpression.Callee == null)
            {
                throw new ArgumentException($"Incomplete expression: {callExpression}", nameof(callExpression));
            }

            string name;
            ExpressionNode callee = callExpression.Callee;
            var context = this;

            if (callExpression.Callee is MemberAccessExpressionNode memberAccess)
            {
                if (Assembly == null)
                {
                    throw new InvalidOperationException("Unable to find method with member access without specified assembly builder");
                }

                while (memberAccess.Member is MemberAccessExpressionNode accessMember)
                {
                    var type = memberAccess.GetExpressionType(Assembly, context);
                    context.CurrentMember = type;

                    memberAccess = accessMember;
                }

                if (memberAccess.Member == null)
                {
                    throw new ArgumentException($"Incomplete expression: {callExpression}", nameof(callExpression));
                }

                if (memberAccess.Member is CallExpressionNode memberAccessCallExpression)
                {
                    if (memberAccessCallExpression.Callee == null)
                    {
                        throw new ArgumentException($"Incomplete expression: {memberAccessCallExpression}", nameof(callExpression));
                    }

                    callee = memberAccessCallExpression.Callee;
                }
                else
                {
                    callee = memberAccess.Member;
                }
            }
            if (callee is IdentifierExpressionNode identifier)
            {
                name = identifier.GetName(true);
            }
            else
            {
                throw new ArgumentException($"Invalid method or function identifier: {callee}", nameof(callExpression));
            }

            DSharpMetadataToken[] parameters = new DSharpMetadataToken[callExpression.Arguments.Count];

            return context.FindMethod(name, true, parameters);
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

        private readonly bool TryResolveMember(string name, bool recursive, [NotNullWhen(true)] out IDSharpMemberInfo? result)
        {
            result = null;

            if (Assembly == null)
            {
                return false;
            }

            if (CurrentMember != null && !recursive)
            {
                try
                {
                    result = FindAnyAvailableMember(name);
                    return true;
                }
                catch
                {
                }
            }
            if (MemberResolver != null)
            {
                result = MemberResolver(this, name);

                if (result != null)
                {
                    return true;
                }
            }
            if (TypeResolver != null)
            {
                result = TypeResolver(name);

                if (result != null)
                {
                    return true;
                }
            }

            if (TryResolveType(name, out var resolvedType))
            {
                result = Assembly.GetType(resolvedType);
            }

            return result != null;
        }
        private readonly DSharpTypeToken? InternalTryResolveType(string? @namespace, string typeName)
        {
            var fullName = typeName;

            if (@namespace != null)
            {
                fullName = $"{@namespace}.{fullName}";
            }

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
            if (Assembly != null && CurrentMember != null)
            {
                if (CurrentMember is IDSharpType typeCurrentMember &&
                    typeCurrentMember.FullName == fullName)
                {
                    return Assembly.GetTypeToken(typeCurrentMember);
                }

                IDSharpType? rootType;

                if (CurrentMember is IDSharpType memberAsType)
                {
                    rootType = memberAsType;
                }
                else
                {
                    rootType = CurrentMember.DeclaringType;
                }

                while (rootType != null)
                {
                    if (rootType.FullName == fullName)
                    {
                        return Assembly.GetTypeToken(rootType);
                    }
                    foreach (var genericType in rootType.GetGenericTypes())
                    {
                        if (genericType.Name == typeName)
                        {
                            return Assembly.GetTypeToken(genericType);
                        }
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
        private readonly IDSharpType CreateArray(DSharpTypeToken typeToken, int arrayDimension)
        {
            if (Assembly == null)
            {
                throw new InvalidOperationException($"Unable to create array because assembly builder not provided");
            }

            var type = (IDSharpType)Assembly.GetType(typeToken);

            for (int i = 0; i < arrayDimension; i++)
            {
                type = Assembly.CreateArray(type);
            }

            return type;
        }

        #endregion
    }
}

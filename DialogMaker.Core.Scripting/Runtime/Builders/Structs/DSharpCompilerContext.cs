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
            CurrentLoopStartInstruction = context.CurrentLoopStartInstruction;
            CurrentLoopEndInstruction = context.CurrentLoopEndInstruction;
            NowInCatchBlock = context.NowInCatchBlock;
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
        public bool NowInCatchBlock { get; set; }

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
            string message = $"Can not access to {member.Access.ToString().ToLower()} \"{member}\"";

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
            else if (expression is ThisExpressionNode thisExpression)
            {
                if (CurrentMember != null && CurrentMember.IsStatic)
                {
                    ThrowThisIsUnavailable(expression);
                }

                result = CurrentMember;
                return result != null;
            }
            else if (expression is BaseExpressionNode baseExpression)
            {
                if (CurrentMember == null || Assembly == null)
                {
                    return false;
                }
                if (CurrentMember.IsStatic)
                {
                    ThrowBaseIsUnavailable(expression);
                }

                IDSharpType? type = CurrentMember as IDSharpType ?? CurrentMember.DeclaringType;

                if (type == null)
                {
                    return false;
                }

                if (type == Assembly.ObjectType)
                {
                    ThrowBaseIsUnavailable(expression);
                }

                result = type.GetBaseTypes().FirstOrDefault(t => t.ObjectType != DSharpObjectType.Interface) ?? Assembly.ObjectType;

                return true;
            }
            else if (expression is AwaitExpressionNode awaitExpression)
            {
                if (awaitExpression.Expression == null)
                {
                    throw new ArgumentException($"Invalid expression: {awaitExpression}", nameof(expression));
                }

                return TryResolveMember(awaitExpression.Expression, out result);
            }
            else if (expression is CallExpressionNode callExpression)
            {
                result = FindMethod(callExpression);
            }
            else if (expression is ArrayAccessExpressionNode arrayExpression)
            {
                result = FindIndexer(arrayExpression);
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
                        throw new InvalidOperationException($"Unable to get returning type of \"{member}\": {memberAccess.Target}");
                    }
                }

                DSharpCompilerContext context = new(this, member);

                return context.TryResolveMember(memberAccess.Member, out result);
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
        public readonly bool TryResolveType(ExpressionNode typeExpression, [NotNullWhen(true)] out DSharpTypeToken? result)
        {
            return TryResolveType(typeExpression, out result, out _);
        }
        public readonly bool TryResolveType(ExpressionNode typeExpression, [NotNullWhen(true)] out DSharpTypeToken? result, [NotNullWhen(true)] out string? name)
        {
            name = null;

            if (typeExpression is IdentifierExpressionNode identifier)
            {
                if (TryResolveType(identifier.Name, identifier.GenericParameters, 0, false, out result))
                {
                    name = identifier.GetName(false);
                    return true;
                }
            }
            else if (typeExpression is MemberAccessExpressionNode memberAccess)
            {
                IdentifierExpressionNode? endPointIdentifier = null;
                var memberAccessFull = memberAccess.GetName(false, false);
                var memberAccessName = memberAccess.GetName(false, true);

                do
                {
                    if (memberAccess.Member is IdentifierExpressionNode endIdentifier)
                    {
                        endPointIdentifier = endIdentifier;
                        break;
                    }
                    if (memberAccess.Member is MemberAccessExpressionNode nextMemberAccess)
                    {
                        memberAccess = nextMemberAccess;
                    }
                    else
                    {
                        break;
                    }
                }
                while(true);

                if (endPointIdentifier != null && TryResolveType(memberAccessName, endPointIdentifier.GenericParameters, 0, false, out result))
                {
                    name = memberAccessFull;
                    return true;
                }
            }

            result = null;
            return false;
        }
        public readonly bool TryResolveType(TypeInfoNode typeInfo, [NotNullWhen(true)] out DSharpTypeToken? result)
        {
            if (TryResolveType(typeInfo, false, out result))
            {
                return true;
            }

            return TryResolveType(typeInfo, true, out result);
        }
        public readonly bool TryResolveType(string name, [NotNullWhen(true)] out DSharpTypeToken? result)
        {
            result = null;

            if (Assembly != null && Assembly.TryGetStandardType(name, out result))
            {
                return true;
            }

            result = InternalTryResolveType(null, name, null);

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
                result = InternalTryResolveType(CurrentMember.DeclaringType.Namespace, name, null);

                if (result != null)
                {
                    return true;
                }
            }
            if (Usings != null)
            {
                foreach (var @namespace in Usings)
                {
                    result = InternalTryResolveType(@namespace, name, null);

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
        public readonly IDSharpMemberInfo FindAnyAvailableMember(IdentifierExpressionNode identifier)
        {
            try
            {
                return FindAnyAvailableMember<IDSharpMemberInfo>(identifier.GetName(true));
            }
            catch (Exception error)
            {
                if (Assembly != null && TryResolveType(identifier.Name, identifier.GenericParameters, 0, true, out var result))
                {
                    return Assembly.GetType(result);
                }

                throw new InvalidOperationException($"Unable to find any available member for: {identifier}", error);
            }
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
        public readonly IDSharpMethodInfo? FindConstructor(IEnumerable<ExpressionNode> parameters)
        {
            var arguments = GetArgumentTypes(parameters);
            return FindConstructor(arguments);
        }
        public readonly IDSharpMethodInfo? FindConstructor(params IDSharpType?[]? parameters)
        {
            if (CurrentMember == null)
            {
                throw new InvalidOperationException("Unable to find constructor with no provided current member");
            }

            var type = (CurrentMember as IDSharpType ?? CurrentMember.DeclaringType)
                ?? throw new InvalidOperationException($"Unable to find constructor using member: {CurrentMember}");
            var constructors = type.GetConstructors();

            if (constructors.Length == 0)
            {
                if (parameters != null)
                {
                    throw new ArgumentException($"Unable to find any constructor at {type}");
                }

                return null;
            }

            int parametersCount = 0;

            if (parameters != null)
            {
                parametersCount = parameters.Length;
            }

            bool SequenceEqual(IDSharpParameterInfo[] args)
            {
                if (args.Length != parametersCount ||
                    parameters == null && args.Length > 0)
                {
                    return false;
                }

                for (int i = 0; i < args.Length; i++)
                {
                    if (!parameters![i]!.IsAssignableTo(args[i].Type))
                    {
                        return false;
                    }
                }

                return true;
            }

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
                else if (SequenceEqual(constructorParameters))
                {
                    return constructor;
                }
            }

            throw new ArgumentException($"Unable to find constructor with parameters count {parametersCount} at \"{type}\"");
        }
        public readonly IDSharpMethodInfo FindMethod(CallExpressionNode callExpression, IDSharpMemberInfo? argumentsMember = null)
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

            IDSharpType?[] parameters;

            if (argumentsMember == null)
            {
                parameters = GetArgumentTypes(callExpression.Arguments);
            }
            else
            {
                DSharpCompilerContext argumentsContext = new(this, argumentsMember);
                parameters = argumentsContext.GetArgumentTypes(callExpression.Arguments);
            }

            try
            {
                return context.FindMethod(name, parameters);
            }
            catch (Exception error)
            {
                throw new InvalidOperationException($"Unable to find method \"{name}\" with {parameters.Length} parameters", error);
            }
        }
        public readonly IDSharpIndexerInfo FindIndexer(ArrayAccessExpressionNode arrayAccessExpression, IDSharpMemberInfo? argumentsMember = null)
        {
            if (arrayAccessExpression.Array == null)
            {
                throw new ArgumentException($"Invalid expression: {arrayAccessExpression}", nameof(arrayAccessExpression));
            }
            if (Assembly == null)
            {
                throw new InvalidOperationException($"Unable to find indexer when assembly not provided: {arrayAccessExpression}");
            }
            if (arrayAccessExpression.Array.GetExpressionType(Assembly, this) is not IDSharpType arrayType)
            {
                throw new InvalidOperationException($"Unable to get array type: {arrayAccessExpression.Array}");
            }

            IDSharpType?[] parameters;
            DSharpCompilerContext arrayContext = new(this, arrayType);

            if (argumentsMember == null)
            {
                parameters = GetArgumentTypes(arrayAccessExpression.Arguments);
            }
            else
            {
                DSharpCompilerContext argumentsContext = new(this, argumentsMember);
                parameters = argumentsContext.GetArgumentTypes(arrayAccessExpression.Arguments);
            }

            try
            {
                return arrayContext.FindIndexer(parameters);
            }
            catch (Exception error)
            {
                throw new InvalidOperationException($"Unable to find indexer for expression: {arrayAccessExpression}", error);
            }
        }
        public readonly IDSharpIndexerInfo FindIndexer(params IDSharpType?[] parameters)
        {
            IDSharpType? currentType = (CurrentMember as IDSharpType ?? CurrentMember?.DeclaringType)
                ?? throw new InvalidOperationException($"Unable to find indexer when current member not provided");

            bool IsValid(IDSharpParameterInfo[] indexerParameters)
            {
                for (int i = 0; i < indexerParameters.Length; i++)
                {
                    if (!parameters[i]!.IsAssignableTo(indexerParameters[i].Type))
                    {
                        return false;
                    }
                }

                return true;
            }

            foreach (var member in currentType.GetAllMembers(m => m is IDSharpIndexerInfo))
            {
                if (member is not IDSharpIndexerInfo indexer)
                {
                    continue;
                }

                var indexerParameters = indexer.GetParameters();

                if (indexerParameters.Length != parameters.Length)
                {
                    continue;
                }
                if (IsValid(indexerParameters))
                {
                    return indexer;
                }
            }

            throw new InvalidOperationException($"Can not find indexer in \"{currentType}\" with {parameters.Length} parameters");
        }
        public readonly IDSharpMethodInfo FindMethod(string name, params IDSharpType?[] parameters)
        {
            IDSharpType? currentType = CurrentMember as IDSharpType ?? CurrentMember?.DeclaringType;
            IEnumerable<IDSharpMemberInfo> members;
            object membersSource;

            if (currentType == null)
            {
                if (Assembly == null)
                {
                    throw new InvalidOperationException($"Unable to find method when current member and assembly not provided");
                }

                members = Assembly.GetGlobalFunctions().Where(m => m.Name == name);
                membersSource = Assembly;
            }
            else
            {
                membersSource = currentType;
                members = currentType.GetAllMembers(m => m.Name == name &&
                                                         m is IDSharpMethodInfo method &&
                                                         m.DeclaringType?.ObjectType != DSharpObjectType.Interface &&
                                                         method.MethodType == DSharpMethodType.Default);
            }

            bool IsValid(IDSharpParameterInfo[] methodParameters)
            {
                for (int i = 0; i < methodParameters.Length; i++)
                {
                    if (!parameters[i]!.IsAssignableTo(methodParameters[i].Type))
                    {
                        return false;
                    }
                }

                return true;
            }

            IDSharpMethodInfo? result = null;

            foreach (var member in members)
            {
                if (member is not IDSharpMethodInfo method)
                {
                    continue;
                }

                var methodParameters = method.GetParameters();

                if (methodParameters.Length != parameters.Length)
                {
                    continue;
                }
                if (IsValid(methodParameters))
                {
                    if (result != null)
                    {
                        if (result.OverrideMethod != null)
                        {
                            var currentOverride = result.OverrideMethod;
                            bool skipMethod = false;

                            while (currentOverride != null)
                            {
                                if (currentOverride == method)
                                {
                                    skipMethod = true;
                                    break;
                                }

                                currentOverride = currentOverride.OverrideMethod;
                            }

                            if (skipMethod)
                            {
                                continue;
                            }
                        }

                        throw new InvalidOperationException($"Multiple methods \"{name}\" with same parameters was found at \"{membersSource}\"");
                    }

                    result = method;
                }
            }

            if (result != null)
            {
                return result;
            }

            string message = $"Can not find method \"{name}\" in \"{currentType}\" with {parameters.Length} parameters";

            if (parameters.Length > 0)
            {
                message += ":";
                StringBuilder builder = new();
                builder.AppendLine(message);

                for (int i = 0; i < parameters.Length; i++)
                {
                    builder.AppendLine($"{i}: {parameters[i]}");
                }

                message = builder.ToString().TrimEnd();
            }

            throw new InvalidOperationException(message);
        }
        public readonly IDSharpType?[] GetArgumentTypes(IEnumerable<ExpressionNode> arguments)
        {
            if (Assembly == null)
            {
                throw new InvalidOperationException("Unable to resolve argument types when assembly not specified");
            }

            IDSharpType?[] parameters = new IDSharpType[arguments.Count()];
            int i = 0;

            foreach (var arg in arguments)
            {
                if (arg.IsNullExpression())
                {
                    parameters[i] = null;
                    i++;
                    continue;
                }
                if (arg.GetExpressionType(Assembly, this) is not IDSharpType argumentType)
                {
                    throw new InvalidOperationException($"Unable to get argument type: {arg}");
                }

                parameters[i] = argumentType;
                i++;
            }

            return parameters;
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
        private readonly bool TryResolveType(TypeInfoNode typeInfo, bool simplifyGenerics, [NotNullWhen(true)] out DSharpTypeToken? result)
        {
            return TryResolveType(typeInfo.GetSimpleFullName(), typeInfo.GenericParameters, typeInfo.ArrayDimensions, simplifyGenerics, out result);
        }
        private readonly bool TryResolveType(string typeName, List<TypeInfoNode> generics, int arrayDimension, bool simplifyGenerics, [NotNullWhen(true)] out DSharpTypeToken? result)
        {
            result = null;
            List<DSharpTypeToken> genericParameters = [];
            

            foreach (var generic in generics)
            {
                if (!TryResolveType(generic, out var genericTypeToken))
                {
                    throw new InvalidOperationException($"Unable to resolve generic parameter: {generic}");
                }

                genericParameters.Add(genericTypeToken);

            }

            List<IDSharpType> genericParametersTypes;

            if (Assembly != null)
            {
                var context = this;
                genericParametersTypes = [.. genericParameters.Select(p => (IDSharpType)context.Assembly.GetType(p))];
            }
            else
            {
                genericParametersTypes = [];
            }

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
                    var childType = member.GetChildrenTypes().FirstOrDefault(t => t.Name == typeName &&
                                                                                  (t.GetGenericParameters().SequenceEqual(genericParametersTypes) ||
                                                                                   t.GetGenericTypes().SequenceEqual(genericParametersTypes)));

                    if (childType != null)
                    {
                        result = Assembly.GetTypeToken(childType);
                        break;
                    }

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

                result = InternalTryResolveType(declaringType?.FullName, typeName, genericParameters);
                result ??= InternalTryResolveType(declaringType?.Namespace, typeName, genericParameters);

                if (result == null)
                {
                    if (Usings != null)
                    {
                        foreach (var @using in Usings)
                        {
                            result = InternalTryResolveType(@using, typeName, genericParameters);

                            if (result != null)
                            {
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
                    throw new InvalidOperationException($"Unable to resolve type because assembly builder not provided: {typeName}");
                }

                if (arrayDimension > 0)
                {
                    var arrayType = CreateArray(result, arrayDimension);
                    result = Assembly.GetTypeToken(arrayType);
                }

                return true;
            }

            return false;
        }
        private readonly DSharpTypeToken? InternalTryResolveType(string? @namespace, string typeName, List<DSharpTypeToken>? genericTypes)
        {
            var fullName = typeName;

            if (@namespace != null)
            {
                fullName = $"{@namespace}.{fullName}";
            }
            if (Assembly != null &&
                (Assembly.TryGetStandardType(typeName, out DSharpTypeToken? token) ||
                Assembly.TryGetTypeToken(@namespace, typeName, genericTypes, out token)))
            {
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

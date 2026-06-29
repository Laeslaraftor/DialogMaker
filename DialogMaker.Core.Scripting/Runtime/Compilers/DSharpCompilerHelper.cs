using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Runtime.Builders;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    /// <summary>
    /// Class with extension methods for expressions
    /// </summary>
    public static class DSharpCompilerExpressionExtensions
    {
        extension(DSharpTokenType token)
        {
            /// <summary>
            /// Get parameter mode from token
            /// </summary>
            /// <returns>Parameter mode</returns>
            public DSharpMethodParameterMode ToParameterType()
            {
                if (token == DSharpTokenType.Ref)
                {
                    return DSharpMethodParameterMode.Ref;
                }
                else if (token == DSharpTokenType.Out)
                {
                    return DSharpMethodParameterMode.Out;
                }

                return DSharpMethodParameterMode.Default;
            }
        }
        extension(IDSharpMemberInfo member)
        {
            /// <summary>
            /// Try get type that returns by member. If specified member is type will be returned false or method return type is null
            /// </summary>
            /// <param name="result">Type that returns by member</param>
            /// <returns>Type is not null</returns>
            public bool TryGetReturnType([NotNullWhen(true)] out IDSharpType? result)
            {
                result = null;

                if (member is IDSharpType)
                {
                    return false;
                }
                else if (member is IDSharpFieldInfo field)
                {
                    result = field.FieldType;
                }
                else if (member is IDSharpPropertyInfo property)
                {
                    result = property.PropertyType;
                }
                else if (member is IDSharpMethodInfo method)
                {
                    result = method.ReturnType;
                }

                return result != null;
            }
            /// <summary>
            /// Try get base of current member. 
            /// If current member is type then will be returned base type that is not interface.
            /// If current member is method or property then will be returned override member,
            /// and if override member not found then will be returned base type of declaring type
            /// </summary>
            /// <param name="result">Base of current member</param>
            /// <returns>Is base member found</returns>
            public bool TryGetBase([NotNullWhen(true)] out IDSharpMemberInfo? result)
            {
                result = null;

                if (member == member.Assembly.ObjectType)
                {
                    return false;
                }
                if (member is IDSharpType typeMember)
                {
                    result = typeMember.GetBaseTypes().FirstOrDefault(t => t.ObjectType != DSharpObjectType.Interface) 
                        ?? member.Assembly.ObjectType;
                    return true;
                }
                if (member is IDSharpMethodInfo method)
                {
                    result = method.OverrideMethod;
                }
                else if (member is IDSharpPropertyInfo property)
                {
                    result = property.OverrideProperty;
                }
                if (result == null && member.DeclaringType is IDSharpType declaringType)
                {
                    result = declaringType.GetBaseTypes().FirstOrDefault(t => t.ObjectType != DSharpObjectType.Interface);
                }

                return result != null;
            }
            public bool SameSignatureTo(IDSharpMemberInfo otherMember, bool strict = false)
            {
                if (member.Name != otherMember.Name ||
                    member.Access != otherMember.Access)
                {
                    return false;
                }

                if (member is IDSharpType currentType && otherMember is IDSharpType otherType)
                {
                    return currentType.ObjectType == otherType.ObjectType;
                }
                else if (member is IDSharpFieldInfo currentField && otherMember is IDSharpFieldInfo otherField)
                {
                    return currentField.FieldType.IsAssignableTo(otherField.FieldType);
                }
                else if (member is IDSharpPropertyInfo currentProperty && otherMember is IDSharpPropertyInfo otherProperty)
                {
                    if (strict)
                    {
                        if (currentProperty.CanRead != otherProperty.CanRead ||
                            currentProperty.CanWrite != otherProperty.CanWrite ||
                            currentProperty.Getter?.Access != otherProperty.Getter?.Access ||
                            currentProperty.Setter?.Access != otherProperty.Setter?.Access)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if ((otherProperty.CanWrite && !currentProperty.CanWrite) ||
                            (otherProperty.CanRead && !currentProperty.CanRead) ||
                            (otherProperty.CanWrite && currentProperty.Setter?.Access != otherProperty.Setter?.Access) ||
                            (otherProperty.CanRead && currentProperty.Getter?.Access != otherProperty.Getter?.Access))
                        {
                            return false;
                        }
                    }

                    return currentProperty.PropertyType.IsAssignableTo(otherProperty.PropertyType);
                }
                else if (member is IDSharpIndexerInfo currentIndexer && otherMember is IDSharpIndexerInfo otherIndexer)
                {
                    return currentIndexer.PropertyType.IsAssignableTo(otherIndexer.PropertyType) &&
                           currentIndexer.GetParameters().SequenceEqual(otherIndexer.GetParameters(), IDSharpParameterInfo.Comparer.Instance);
                }
                else if (member is IDSharpMethodInfo currentMethod && otherMember is IDSharpMethodInfo otherMethod)
                {
                    if ((currentMethod.ReturnType == null && otherMethod.ReturnType != null) ||
                        (currentMethod.ReturnType != null && otherMethod.ReturnType == null))
                    {
                        return false;
                    }

                    return ((currentMethod.ReturnType == null && otherMethod.ReturnType == null) ||
                           currentMethod.ReturnType!.IsAssignableTo(otherMethod.ReturnType!)) &&
                           currentMethod.GetParameters().SequenceEqual(otherMethod.GetParameters(), IDSharpParameterInfo.Comparer.Instance) &&
                           currentMethod.GetGenericParameters().SequenceEqual(otherMethod.GetGenericParameters());
                }

                return false;
            }
        }
        extension(IDSharpType type)
        {
            /// <summary>
            /// Get all members of interfaces that must be implemented
            /// </summary>
            /// <returns>Members to implement</returns>
            public IEnumerable<IDSharpMemberInfo> GetInterfaceMembersToImplement()
            {
                if (type.ObjectType != DSharpObjectType.Interface)
                {
                    yield break;
                }

                foreach (var indexer in type.GetIndexers())
                {
                    if (indexer.IsDeclaration)
                    {
                        yield return indexer;
                    }
                }
                foreach (var property in type.GetProperties())
                {
                    if (property.IsDeclaration)
                    {
                        yield return property;
                    }
                }
                foreach (var method in type.GetMethods())
                {
                    if (method.IsDeclaration)
                    {
                        yield return method;
                    }
                }

                foreach (var baseType in type.GetBaseTypes().Where(t => t.ObjectType == DSharpObjectType.Interface))
                {
                    foreach (var baseMember in baseType.GetInterfaceMembersToImplement())
                    {
                        yield return baseMember;
                    }
                }
            }
            /// <summary>
            /// Get all members of type include inherited members
            /// </summary>
            /// <param name="predicate">Predicate for members</param>
            /// <returns>All members of specified type</returns>
            public IEnumerable<IDSharpMemberInfo> GetAllMembers(Predicate<IDSharpMemberInfo>? predicate = null)
            {
                return type.GetAllMembers(true, predicate);
            }
            /// <summary>
            /// Get all members of type include inherited members
            /// </summary>
            /// <param name="includeDeclaringType">Include all members in all declaring types</param>
            /// <param name="predicate">Predicate for members</param>
            /// <returns>All members of specified type</returns>
            public IEnumerable<IDSharpMemberInfo> GetAllMembers(bool includeDeclaringType, Predicate<IDSharpMemberInfo>? predicate = null)
            {
                foreach (var member in type.GetAllLocalMembers(predicate))
                {
                    yield return member;
                }

                foreach (var baseType in type.GetBaseTypes())
                {
                    foreach (var member in baseType.GetAllLocalMembers(predicate))
                    {
                        yield return member;
                    }
                }

                if (type != type.Assembly.ObjectType)
                {
                    foreach (var member in type.Assembly.ObjectType.GetAllLocalMembers(predicate, false, true))
                    {
                        yield return member;
                    }
                }

                if (includeDeclaringType)
                {
                    var declaringType = type.DeclaringType;

                    while (declaringType != null)
                    {
                        foreach (var declaringTypeMember in declaringType.GetAllLocalMembers(predicate, false, false))
                        {
                            yield return declaringTypeMember;
                        }

                        declaringType = type.DeclaringType;
                    }
                }
            }

            private IEnumerable<IDSharpMemberInfo> GetAllLocalMembers(Predicate<IDSharpMemberInfo>? predicate = null, bool constructors = true, bool indexers = true)
            {
                bool IsValid(IDSharpMemberInfo member)
                {
                    return predicate == null || predicate(member);
                }

                if (constructors)
                {
                    foreach (var member in type.GetConstructors())
                    {
                        if (IsValid(member))
                        {
                            yield return member;
                        }
                    }
                }

                foreach (var member in type.GetFields())
                {
                    if (IsValid(member))
                    {
                        yield return member;
                    }
                }
                foreach (var member in type.GetProperties())
                {
                    if (IsValid(member))
                    {
                        yield return member;
                    }
                }
                foreach (var member in type.GetMethods())
                {
                    if (IsValid(member))
                    {
                        yield return member;
                    }
                }
                
                if (indexers)
                {
                    foreach (var member in type.GetIndexers())
                    {
                        if (IsValid(member))
                        {
                            yield return member;
                        }
                    }
                }
            }
        }
        extension(DSharpBinaryOperator @operator)
        {
            /// <summary>
            /// Check is operator logical
            /// </summary>
            /// <returns>Is operator logical</returns>
            public bool IsLogical()
            {
                return @operator == DSharpBinaryOperator.LogicalAnd ||
                       @operator == DSharpBinaryOperator.LogicalOr ||
                       @operator == DSharpBinaryOperator.LogicalEquals ||
                       @operator == DSharpBinaryOperator.LogicalNotEquals ||
                       @operator == DSharpBinaryOperator.LogicalLess ||
                       @operator == DSharpBinaryOperator.LogicalLessOrEquals ||
                       @operator == DSharpBinaryOperator.LogicalGreater ||
                       @operator == DSharpBinaryOperator.LogicalGreaterOrEquals;
            }
        }
        extension(ExpressionNode expression)
        {
            /// <summary>
            /// Get is current expression null
            /// </summary>
            /// <returns>Is current expression null</returns>
            public bool IsNullExpression()
            {
                if (expression is LiteralExpressionNode literalExpression)
                {
                    return literalExpression.Type == DSharpLiteralType.Null;
                }

                return false;
            }
            /// <summary>
            /// Get result type of expression
            /// </summary>
            /// <param name="assembly">Assembly builder for finding types</param>
            /// <returns>Result type of expression</returns>
            /// <exception cref="InvalidOperationException"></exception>
            /// <exception cref="ArgumentException"></exception>
            public IDSharpMemberInfo? GetExpressionType(DSharpAssemblyBuilder assembly, DSharpCompilerContext context = default)
            {
                if (expression is LiteralExpressionNode literalExpression)
                {
                    return assembly.GetType(literalExpression.Type);
                }
                else if (expression is NameOfExpressionNode)
                {
                    return assembly.StringType;
                }
                else if (expression is TypeOfExpressionNode)
                {
                    return assembly.TypeType;
                }
                else if (expression is SizeOfExpressionNode)
                {
                    return assembly.Int32Type;
                }
                else if (expression is CallExpressionNode callExpression)
                {
                    return context.FindMethod(callExpression).ReturnType;
                }
                else if (expression is ThisExpressionNode thisExpression)
                {
                    if (context.CurrentMember == null)
                    {
                        throw new ArgumentException($"Unable to get type of \"this\" because current member not provided: {expression}", nameof(context));
                    }
                    if (context.CurrentMember is IDSharpType type)
                    {
                        return type;
                    }
                    else if (context.CurrentMember.DeclaringType == null)
                    {
                        throw new InvalidOperationException($"Unable to get current instance inside global member: {expression}");
                    }
                    if (context.CurrentMember.IsStatic)
                    {
                        throw new InvalidOperationException($"Unable to get current instance inside static member: {expression}");
                    }

                    return context.CurrentMember.DeclaringType;
                }
                else if (expression is BaseExpressionNode baseExpression)
                {
                    if (context.CurrentMember == null)
                    {
                        throw new ArgumentException($"Unable to get type of \"base\" because current member not provided: {expression}", nameof(context));
                    }
                    if (context.CurrentMember.IsStatic)
                    {
                        throw new InvalidOperationException($"Unable to access to base inside static member: {expression}");
                    }

                    IDSharpType currentType;

                    if (context.CurrentMember is IDSharpType typeMember)
                    {
                        currentType = typeMember;
                    }
                    else
                    {
                        if (context.CurrentMember.DeclaringType == null)
                        {
                            throw new ArgumentException($"Unable to get type of \"base\" from member without declaring type: {expression}", nameof(context));
                        }

                        currentType = context.CurrentMember.DeclaringType;
                    }

                    var baseTypes = currentType.GetBaseTypes().Where(t => t.ObjectType != DSharpObjectType.Interface);

                    foreach (var baseType in baseTypes)
                    {
                        return baseType;
                    }

                    if (context.Assembly != null && currentType != context.Assembly.ObjectType)
                    {
                        return context.Assembly.ObjectType;
                    }

                    throw new ArgumentException($"Unable to get type of \"base\" from type that do not have base types \"{currentType}\"", nameof(context));
                }
                if (expression is IdentifierExpressionNode identifierExpression &&
                    context.CurrentMember is IDSharpMethodInfo method &&
                    identifierExpression.TryGetLocalMember(method, out var localMemberInfo))
                {
                    return localMemberInfo.Value.Type;
                }

                if (context.TryResolveMember(expression, out var result))
                {
                    if (result.TryGetReturnType(out var returnType))
                    {
                        return returnType;
                    }

                    return result;
                }
                else if (expression.TrySimplifyToLiteral(out var literal))
                {
                    return assembly.GetType(literal.Type);
                }
                else if (expression is UnaryExpressionNode unaryExpression)
                {
                    if (unaryExpression.Operand == null)
                    {
                        throw new ArgumentException($"Unable to get type of expression because operand of expression is null: {expression}", nameof(expression));
                    }

                    return unaryExpression.Operand.GetExpressionType(assembly, context);
                }
                else if (expression is BinaryExpressionNode binaryExpression)
                {
                    if (binaryExpression.Operator == DSharpBinaryOperator.Mod)
                    {
                        return assembly.Int32Type;
                    }
                    else if (binaryExpression.Operator == DSharpBinaryOperator.LogicalOr ||
                             binaryExpression.Operator == DSharpBinaryOperator.LogicalAnd ||
                             binaryExpression.Operator == DSharpBinaryOperator.LogicalEquals ||
                             binaryExpression.Operator == DSharpBinaryOperator.LogicalNotEquals ||
                             binaryExpression.Operator == DSharpBinaryOperator.LogicalLess ||
                             binaryExpression.Operator == DSharpBinaryOperator.LogicalLessOrEquals ||
                             binaryExpression.Operator == DSharpBinaryOperator.LogicalGreater ||
                             binaryExpression.Operator == DSharpBinaryOperator.LogicalGreaterOrEquals)
                    {
                        return assembly.BoolType;
                    }
                    if (binaryExpression.Left == null ||
                        binaryExpression.Right == null)
                    {
                        throw new ArgumentException($"Incomplete operator expression: {expression}", nameof(expression));
                    }

                    return binaryExpression.Left.VerifyAndUniteType(assembly, binaryExpression.Right, context);
                }
                else if (expression is DecrementExpressionNode decrementExpression)
                {
                    if (decrementExpression.Expression == null)
                    {
                        throw new ArgumentException($"Unable to get type of expression because expression is null: {expression}", nameof(expression));
                    }

                    return decrementExpression.Expression.GetExpressionType(assembly, context);
                }
                else if (expression is IncrementExpressionNode incrementExpression)
                {
                    if (incrementExpression.Expression == null)
                    {
                        throw new ArgumentException($"Unable to get type of expression because expression is null: {expression}", nameof(expression));
                    }

                    return incrementExpression.Expression.GetExpressionType(assembly, context);
                }
                else if (expression is NewExpressionNode newExpression)
                {
                    IDSharpType? type = null;

                    if (newExpression.Type == null)
                    {
                        if (!context.TryResolveTypeOfExpression(expression, out type))
                        {
                            throw new InvalidOperationException($"Unable to resolve type for: {expression}");
                        }
                    }
                    if (type == null && newExpression.Type != null)
                    {
                        if (context.Assembly != null && context.TryResolveType(newExpression.Type, out var resolvedTypeToken))
                        {
                            type = context.Assembly.GetType(resolvedTypeToken) as IDSharpType;
                        }
                        else
                        {
                            if (context.Assembly != null)
                            {
                                type = context.Assembly.GetType(context.ResolveType(newExpression.Type)) as IDSharpType;
                            }
                            else
                            {
                                type = assembly.GetType(newExpression.Type);
                            }
                        }

                    }
                    if (newExpression.Type == null)
                    {
                        throw new ArgumentException($"Unknown type: {newExpression.Type}", nameof(expression));
                    }
                    if (type == null)
                    {
                        return null;
                    }

                    if (expression is NewArrayExpressionNode)
                    {
                        return assembly.CreateArray(type);
                    }

                    return type;
                }
                else if (expression is ArrayAccessExpressionNode arrayExpression)
                {
                    var indexer = context.FindIndexer(arrayExpression);
                    return indexer.PropertyType;
                }

                throw new ArgumentException($"Unable to get type of expression: {expression}", nameof(expression));
            }
            public IDSharpType? VerifyAndUniteType(DSharpAssemblyBuilder assembly, ExpressionNode otherExpression, DSharpCompilerContext context = default)
            {
                var leftTypeMember = expression.GetExpressionType(assembly, context);
                var rightTypeMember = otherExpression.GetExpressionType(assembly, context);

                if (leftTypeMember == null || rightTypeMember == null)
                {
                    throw new InvalidOperationException($"Unable to find one of expressions type. Left: {leftTypeMember}, right: {rightTypeMember}");
                }

                IDSharpType? leftType = leftTypeMember as IDSharpType;
                IDSharpType? rightType = rightTypeMember as IDSharpType;

                if ((leftType == null || rightType == null) &&
                    (!leftTypeMember.TryGetReturnType(out leftType) ||
                    !rightTypeMember.TryGetReturnType(out rightType)))
                {
                    throw new InvalidOperationException($"Expression must return value. Left: {leftTypeMember}, right: {rightTypeMember}");
                }
                if (leftType == rightType)
                {
                    return leftType;
                }

                var stringType = (IDSharpType)assembly.GetType(assembly.StringToken);

                if (leftType == stringType ||
                    rightType == stringType)
                {
                    return stringType;
                }

                var charType = (IDSharpType)assembly.GetType(assembly.CharToken);

                if (leftType == charType ||
                    rightType == charType)
                {
                    return stringType;
                }

                throw new InvalidOperationException($"Can not get type for expressions: {expression}, {otherExpression}");
            }
            public bool TrySimplifyToLiteral([NotNullWhen(true)] out DSharpLiteralValue result)
            {
                result = default;
                bool expressionFailed = false;
                DSharpLiteralValue lastExpressionValue = default;
                Dictionary<DSharpMathOperationValues, DSharpLiteralValue>? expressionValues = null;

                bool TryGetValue(ExpressionNode expression, [NotNullWhen(true)] out DSharpLiteralValue result)
                {
                    result = default;

                    if (expressionValues == null)
                    {
                        return false;
                    }
                    foreach (var info in expressionValues)
                    {
                        if (info.Key.Contains(expression))
                        {
                            result = info.Value;
                            return true;
                        }
                    }

                    return false;
                }
                void AddExpressionValue(ExpressionNode left, ExpressionNode right, DSharpLiteralValue value)
                {
                    DSharpMathOperationValues operation = new(left, right);

                    if (expressionValues == null)
                    {
                        expressionValues = [];
                        expressionValues.Add(operation, value);
                        return;
                    }

                    foreach (var info in expressionValues)
                    {
                        if (info.Key.Contains(left) || info.Key.Contains(right))
                        {
                            expressionValues[info.Key] = value;
                        }
                    }

                    if (!expressionValues.TryAdd(operation, value))
                    {
                        expressionValues[operation] = value;
                    }
                }
                void HandleExpression(ExpressionNode left, ExpressionNode right, DSharpBinaryOperator @operator)
                {
                    if (expressionFailed ||
                        left is not LiteralExpressionNode leftLiteral ||
                        right is not LiteralExpressionNode rightLiteral)
                    {
                        expressionFailed = true;
                        return;
                    }

                    DSharpLiteralValue value = DSharpLiteralValue.Null;
                    DSharpLiteralValue leftValue = leftLiteral.Value;
                    DSharpLiteralValue rightValue = rightLiteral.Value;

                    if (expressionValues != null)
                    {
                        if (TryGetValue(left, out var savedLeftValue))
                        {
                            leftValue = savedLeftValue;
                        }
                        if (TryGetValue(right, out var savedRightValue))
                        {
                            rightValue = savedRightValue;
                        }
                    }

                    if (@operator.IsLogical())
                    {
                        if (@operator == DSharpBinaryOperator.LogicalAnd ||
                            @operator == DSharpBinaryOperator.LogicalOr)
                        {
                            if (!leftValue.IsBool || !rightValue.IsBool)
                            {
                                throw new ArgumentException($"Logical operators requires boolean values. Left: {leftValue}, right: {rightValue}, operator: {(DSharpTokenType)@operator}");
                            }
                            if (@operator == DSharpBinaryOperator.LogicalAnd)
                            {
                                value = leftValue.AsBool() && rightValue.AsBool();
                            }
                            else if (@operator == DSharpBinaryOperator.LogicalOr)
                            {
                                value = leftValue.AsBool() || rightValue.AsBool();
                            }
                        }
                        else if (@operator == DSharpBinaryOperator.LogicalEquals)
                        {
                            value = leftValue == rightValue;
                        }
                        else if (@operator == DSharpBinaryOperator.LogicalNotEquals)
                        {
                            value = leftValue != rightValue;
                        }
                        else
                        {
                            if (!leftValue.IsNumber || !rightValue.IsNumber)
                            {
                                throw new ArgumentException($"Operator {(DSharpTokenType)@operator} requires numbers for comparison. Left: {leftValue}, right: {rightValue}");
                            }
                            if (@operator == DSharpBinaryOperator.LogicalLess)
                            {
                                value = leftValue.AsNumber<double>() < rightValue.AsNumber<double>();
                            }
                            else if (@operator == DSharpBinaryOperator.LogicalLessOrEquals)
                            {
                                value = leftValue.AsNumber<double>() <= rightValue.AsNumber<double>();
                            }
                            if (@operator == DSharpBinaryOperator.LogicalGreater)
                            {
                                value = leftValue.AsNumber<double>() > rightValue.AsNumber<double>();
                            }
                            else if (@operator == DSharpBinaryOperator.LogicalGreaterOrEquals)
                            {
                                value = leftValue.AsNumber<double>() >= rightValue.AsNumber<double>();
                            }
                        }
                    }
                    else if (@operator == DSharpBinaryOperator.Plus)
                    {
                        if (leftValue.IsString || rightValue.IsString ||
                            (leftValue.IsChar && rightValue.IsChar))
                        {
                            if (leftValue.IsNull)
                            {
                                value = rightValue;
                            }
                            else if (rightValue.IsNull)
                            {
                                value = leftValue;
                            }
                            else
                            {
                                value = leftValue.ToString() + rightValue.ToString();
                            }
                        }
                        else if (leftValue.IsNumber && rightValue.IsNumber)
                        {
                            value = leftValue.AsNumber<double>() + rightValue.AsNumber<double>();
                        }
                        else if (leftValue.IsNumber && rightValue.IsChar)
                        {
                            value = leftValue.AsNumber<double>() + rightValue.AsChar();
                        }
                        else if (leftValue.IsChar && rightValue.IsNumber)
                        {
                            value = leftValue.AsChar() + rightValue.AsNumber<double>();
                        }
                        else
                        {
                            throw new ArgumentException($"Operator {(DSharpTokenType)@operator} got invalid values. Left: {leftValue}, right: {rightValue}");
                        }
                    }
                    else
                    {
                        if (!leftValue.IsNumber || !rightValue.IsNumber)
                        {
                            throw new ArgumentException($"Operator {(DSharpTokenType)@operator} requires numbers. Left: {leftValue}, right: {rightValue}");
                        }
                        if (@operator == DSharpBinaryOperator.Minus)
                        {
                            value = leftValue.AsNumber<double>() - rightValue.AsNumber<double>();
                        }
                        else if (@operator == DSharpBinaryOperator.Multiply)
                        {
                            value = leftValue.AsNumber<double>() * rightValue.AsNumber<double>();
                        }
                        else if (@operator == DSharpBinaryOperator.Divide)
                        {
                            value = leftValue.AsNumber<double>() / rightValue.AsNumber<double>();
                        }
                        else if (@operator == DSharpBinaryOperator.Mod)
                        {
                            value = leftValue.AsNumber<double>() % rightValue.AsNumber<double>();
                        }
                    }

                    expressionValues ??= [];
                    lastExpressionValue = value;

                    AddExpressionValue(left, right, value);
                }

                if (expression is LiteralExpressionNode literalExpression)
                {
                    result = literalExpression.Value;
                    return true;
                }
                else if (expression is UnaryExpressionNode unaryExpression)
                {
                    if (unaryExpression.Operand?.TrySimplifyToLiteral(out var literal) != true)
                    {
                        return false;
                    }

                    if (unaryExpression.Operator == DSharpUnaryOperator.Not)
                    {
                        if (!literal.IsBool)
                        {
                            throw new ArgumentException($"Not operator (!) available only to boolean value");
                        }

                        result = !literal.AsBool();
                        return true;
                    }
                    else if (unaryExpression.Operator == DSharpUnaryOperator.Increment)
                    {
                        if (!literal.IsNumber)
                        {
                            throw new ArgumentException($"Increment operator available only to number value");
                        }

                        result = literal.AsNumber() + 1;
                        return true;
                    }
                    else if (unaryExpression.Operator == DSharpUnaryOperator.Decrement)
                    {
                        if (!literal.IsNumber)
                        {
                            throw new ArgumentException($"Decrement operator available only to number value");
                        }

                        result = literal.AsNumber() - 1;
                        return true;
                    }
                    else if (unaryExpression.Operator == DSharpUnaryOperator.Minus)
                    {
                        if (!literal.IsNumber)
                        {
                            throw new ArgumentException($"Minus operator available only to number value");
                        }

                        result = -literal.AsNumber();
                        return true;
                    }
                }
                else if (expression is BinaryExpressionNode binaryExpression)
                {
                    binaryExpression.CompileExpression(HandleExpression);

                    if (!expressionFailed)
                    {
                        result = lastExpressionValue;
                        return true;
                    }

                    return false;
                }
                else if (expression is ParenContainedExpressionNode parenContainedExpression)
                {
                    return parenContainedExpression.Expression?.TrySimplifyToLiteral(out result) == true;
                }

                return false;
            }
        }
        extension(BinaryExpressionNode expression)
        {
            public void CompileExpression(BinaryExpressionCompileHandler handler)
            {
                expression.CompileExpression(handler, []);
            }

            private void CompileExpression(BinaryExpressionCompileHandler handler, HashSet<BinaryExpressionNode> compiledExpressions)
            {
                if (expression.Left == null || expression.Right == null)
                {
                    throw new ArgumentException($"Incomplete expression: {expression}", nameof(expression));
                }
                if (!compiledExpressions.Add(expression))
                {
                    return;
                }

                ExpressionNode leftExpression = expression.Left;
                ExpressionNode rightExpression = expression.Right;

                ExpressionNode GetTargetExpression(ExpressionNode expression, Func<BinaryExpressionNode, ExpressionNode?> selector)
                {
                    if (expression is BinaryExpressionNode binary)
                    {
                        var sideExpression = selector(binary);

                        return sideExpression == null
                            ? throw new ArgumentException($"Incomplete expression: {expression}", nameof(expression))
                            : GetTargetExpression(sideExpression, selector);
                    }
                    else if (expression is ParenContainedExpressionNode parenContained)
                    {
                        if (parenContained.Expression == null)
                        {
                            throw new ArgumentException($"Empty expression: {parenContained}", nameof(expression));
                        }

                        return GetTargetExpression(parenContained.Expression, selector);
                    }
                    else if (expression is UnaryExpressionNode unary)
                    {
                        if (unary.Operand == null)
                        {
                            throw new ArgumentException($"Incomplete expression: {expression}", nameof(expression));
                        }

                        return unary.Operand;
                    }

                    return expression;
                }

                if (expression.Left is ParenContainedExpressionNode leftParenContained)
                {
                    if (leftParenContained.Expression == null)
                    {
                        throw new ArgumentException($"Empty left side of expression: {expression}", nameof(expression));
                    }

                    if (leftParenContained.Expression is BinaryExpressionNode leftBinaryExpression)
                    {
                        leftBinaryExpression.CompileExpression(handler, compiledExpressions);
                        leftExpression = GetTargetExpression(leftBinaryExpression, b => b.Right);
                    }
                    else
                    {
                        leftExpression = leftParenContained.Expression;
                    }
                }
                else if (expression.Left is BinaryExpressionNode leftBinaryExpression)
                {
                    leftBinaryExpression.CompileExpression(handler, compiledExpressions);
                    leftExpression = GetTargetExpression(leftBinaryExpression, b => b.Right);
                }
                if (expression.Right is ParenContainedExpressionNode rightParenContained)
                {
                    if (rightParenContained.Expression == null)
                    {
                        throw new ArgumentException($"Empty right side of expression: {expression}", nameof(expression));
                    }

                    if (rightParenContained.Expression is BinaryExpressionNode rightBinaryExpression)
                    {
                        rightBinaryExpression.CompileExpression(handler, compiledExpressions);
                        rightExpression = GetTargetExpression(rightBinaryExpression, b => b.Left);
                    }
                    else
                    {
                        rightExpression = rightParenContained.Expression;
                    }
                }
                else if (expression.Right is BinaryExpressionNode rightBinaryExpression)
                {
                    if (rightBinaryExpression.Operator == DSharpBinaryOperator.Multiply ||
                        rightBinaryExpression.Operator == DSharpBinaryOperator.Divide ||
                        expression.Operator.IsLogical())
                    {
                        rightBinaryExpression.CompileExpression(handler, compiledExpressions);
                    }
                    if (rightBinaryExpression.Left == null)
                    {
                        throw new ArgumentException($"Incomplete expression: {rightBinaryExpression}", nameof(expression));
                    }

                    rightExpression = GetTargetExpression(rightBinaryExpression, b => b.Left);
                }

                handler(leftExpression, rightExpression, expression.Operator);
            }
        }
        extension(IdentifierExpressionNode identifierExpression)
        {
            /// <summary>
            /// Try to get local member of method or function (parameter, local variable)
            /// </summary>
            /// <param name="method">Method that must contains local member</param>
            /// <param name="result">Result of searching</param>
            /// <returns>Is local member found</returns>
            public bool TryGetLocalMember(IDSharpMethodInfo method, [NotNullWhen(true)] out LocalMemberInfo result)
            {
                result = default;
                var identifier = identifierExpression.GetName(false);
                var parameter = method.GetParameters().FirstOrDefault(p => p.Name == identifier);

                if (parameter != null)
                {
                    result = new(LocalMemberType.Parameter, parameter);
                    return true;
                }
                if (method is DSharpMethodBuilder builder)
                {
                    var code = builder.GetBytecodeBuilder();
                    var variable = code.LocalVariables.FirstOrDefault(v => v.Name == identifier);

                    if (variable != null)
                    {
                        result = new(LocalMemberType.Variable, variable);
                        return true;
                    }
                }

                return false;
            }
        }
        extension(NameOfExpressionNode nameofExpression)
        {
            /// <summary>
            /// Get value of nameof expression
            /// </summary>
            /// <param name="method">Method that contains this expression</param>
            /// <param name="context">Compiler context</param>
            /// <returns>Resolved name of expression</returns>
            /// <exception cref="ArgumentException">Invalid expression</exception>
            /// <exception cref="InvalidOperationException">Unable to resolve name of expression</exception>
            public string GetValue(DSharpMethodBuilder method, DSharpCompilerContext context)
            {
                if (nameofExpression.Value == null)
                {
                    throw new ArgumentException($"Invalid expression: {nameofExpression}", nameof(nameofExpression));
                }
                if (nameofExpression.Value is IdentifierExpressionNode identifier)
                {
                    var parameter = method.Parameters.FirstOrDefault(p => p.Name == identifier.Name);

                    if (parameter != null)
                    {
                        return parameter.Name ?? string.Empty;
                    }

                    var code = method.GetBytecodeBuilder();
                    parameter = code.LocalVariables.FirstOrDefault(p => p.Name == identifier.Name);

                    if (parameter != null)
                    {
                        return parameter.Name ?? string.Empty;
                    }
                }
                try
                {
                    if (context.TryResolveMember(nameofExpression.Value, out var member))
                    {
                        return member.Name;
                    }
                }
                catch
                {
                }
                if (context.TryResolveType(nameofExpression.Value, out _, out var name))
                {
                    return name;
                }

                throw new InvalidOperationException($"Unable to resolve name of expression: {nameofExpression.Value}");
            }
        }
    }
}

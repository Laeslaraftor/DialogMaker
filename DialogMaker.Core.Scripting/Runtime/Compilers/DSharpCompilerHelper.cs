using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Runtime.Builders;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;

namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    /// <summary>
    /// Class with extension methods for expressions
    /// </summary>
    public static class DSharpCompilerExpressionExtensions
    {
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
            public bool SameSignatureTo(IDSharpMemberInfo otherMember)
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
                    return currentField.FieldType == otherField.FieldType;
                }
                else if (member is IDSharpPropertyInfo currentProperty && otherMember is IDSharpPropertyInfo otherProperty)
                {
                    return currentProperty.PropertyType == otherProperty.PropertyType;
                }
                else if (member is IDSharpMethodInfo currentMethod && otherMember is IDSharpMethodInfo otherMethod)
                {
                    return currentMethod.ReturnType == otherMethod.ReturnType &&
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

                foreach (var baseType in type.GetBaseTypes())
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
                bool IsValid(IDSharpMemberInfo member)
                {
                    return predicate == null || predicate(member);
                }

                foreach (var member in type.GetConstructors())
                {
                    if (IsValid(member))
                    {
                        yield return member;
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

                foreach (var baseType in type.GetBaseTypes())
                {
                    foreach (var member in baseType.GetAllMembers(predicate))
                    {
                        yield return member;
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
                if (expression is IdentifierExpressionNode identifierExpression &&
                         context.CurrentMember is IDSharpMethodInfo method)
                {
                    var identifier = identifierExpression.GetName(false);
                    var parameter = method.GetParameters().FirstOrDefault(p => p.Name == identifier);

                    if (parameter != null)
                    {
                        return parameter.Type;
                    }
                    if (method is DSharpMethodBuilder builder)
                    {
                        var code = builder.GetBytecodeBuilder();
                        var variable = code.LocalVariables.FirstOrDefault(v => v.Name == identifier);

                        if (variable?.Type != null)
                        {
                            return builder.Assembly.GetType(variable.Type);
                        }
                    }
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
                    return assembly.GetType(literal.GetValueType());
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
                        return assembly.GetType(DSharpLiteralType.Number);
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
                        return assembly.GetType(DSharpLiteralType.Bool);
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

                    return type;
                }
                else if (expression is NewArrayExpressionNode newArrayExpression)
                {
                    throw new NotImplementedException("Array types not implemented");
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
                if (!leftTypeMember.TryGetReturnType(out var leftType) ||
                    !rightTypeMember.TryGetReturnType(out var rightType))
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
                                value = leftValue.AsNumber() < rightValue.AsNumber();
                            }
                            else if (@operator == DSharpBinaryOperator.LogicalLessOrEquals)
                            {
                                value = leftValue.AsNumber() <= rightValue.AsNumber();
                            }
                            if (@operator == DSharpBinaryOperator.LogicalGreater)
                            {
                                value = leftValue.AsNumber() > rightValue.AsNumber();
                            }
                            else if (@operator == DSharpBinaryOperator.LogicalGreaterOrEquals)
                            {
                                value = leftValue.AsNumber() >= rightValue.AsNumber();
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
                            value = leftValue.AsNumber() + rightValue.AsNumber();
                        }
                        else if (leftValue.IsNumber && rightValue.IsChar)
                        {
                            value = leftValue.AsNumber() + rightValue.AsChar();
                        }
                        else if (leftValue.IsChar && rightValue.IsNumber)
                        {
                            value = leftValue.AsChar() + rightValue.AsNumber();
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
                            value = leftValue.AsNumber() - rightValue.AsNumber();
                        }
                        else if (@operator == DSharpBinaryOperator.Multiply)
                        {
                            value = leftValue.AsNumber() * rightValue.AsNumber();
                        }
                        else if (@operator == DSharpBinaryOperator.Divide)
                        {
                            value = leftValue.AsNumber() / rightValue.AsNumber();
                        }
                        else if (@operator == DSharpBinaryOperator.Mod)
                        {
                            value = leftValue.AsNumber() % rightValue.AsNumber();
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
    }
}

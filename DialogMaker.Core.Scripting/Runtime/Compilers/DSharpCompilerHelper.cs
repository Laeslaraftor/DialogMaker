using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Runtime.Builders;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    public static class DSharpCompilerExpressionExtensions
    {
        extension(ExpressionNode expression)
        {
            public IDSharpType? GetExpressionType(DSharpAssemblyBuilder assembly, Func<IdentifierExpressionNode, IDSharpType?>? identifierResolver = null)
            {
                if (expression is LiteralExpressionNode literalExpression)
                {
                    return assembly.GetType(literalExpression.Type);
                }
                else if (expression is IdentifierExpressionNode identifierExpression)
                {
                    var identifierMember = identifierExpression.ResolveIdentifierMember(assembly, identifierResolver);

                    if (identifierMember is IDSharpType type)
                    {
                        return type;
                    }
                    else if (identifierMember is IDSharpFieldInfo field)
                    {
                        return field.FieldType;
                    }
                    else if (identifierMember is IDSharpPropertyInfo property)
                    {
                        return property.PropertyType;
                    }
                    else if (identifierMember is IDSharpMethodInfo method)
                    {
                        return method.ReturnType;
                    }

                    throw new InvalidOperationException($"Invalid member type: {identifierMember}");
                }
                else if (expression is UnaryExpressionNode unaryExpression)
                {
                    if (unaryExpression.Operand == null)
                    {
                        throw new ArgumentException($"Unable to get type of expression because operand of expression is null: {expression}", nameof(expression));
                    }

                    return unaryExpression.Operand.GetExpressionType(assembly, identifierResolver);
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

                    return binaryExpression.Left.VerifyAndUniteType(assembly, binaryExpression.Right, identifierResolver);
                }
                else if (expression is DecrementExpressionNode decrementExpression)
                {
                    if (decrementExpression.Expression == null)
                    {
                        throw new ArgumentException($"Unable to get type of expression because expression is null: {expression}", nameof(expression));
                    }

                    return decrementExpression.Expression.GetExpressionType(assembly, identifierResolver);
                }
                else if (expression is IncrementExpressionNode incrementExpression)
                {
                    if (incrementExpression.Expression == null)
                    {
                        throw new ArgumentException($"Unable to get type of expression because expression is null: {expression}", nameof(expression));
                    }

                    return incrementExpression.Expression.GetExpressionType(assembly, identifierResolver);
                }

                throw new ArgumentException($"Unable to get type of expression: {expression}", nameof(expression));
            }
            public IDSharpType VerifyAndUniteType(DSharpAssemblyBuilder assembly, ExpressionNode otherExpression, Func<IdentifierExpressionNode, IDSharpType?>? identifierResolver = null)
            {

            }
            public bool TrySimplifyToLiteral([NotNullWhen(true)] out DSharpLiteralValue result)
            {
                result = default;

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

                if (expression.Left is ParenContainedExpressionNode leftParenContained)
                {
                    if (leftParenContained.Expression == null)
                    {
                        throw new ArgumentException($"Empty left side of expression: {expression}", nameof(expression));
                    }

                    if (leftParenContained.Expression is BinaryExpressionNode leftBinaryExpression)
                    {
                        leftBinaryExpression.CompileExpression(handler, compiledExpressions);
                    }
                    else
                    {
                        leftExpression = leftParenContained.Expression;
                    }
                }
                else if (expression.Left is BinaryExpressionNode leftBinaryExpression)
                {
                    leftBinaryExpression.CompileExpression(handler, compiledExpressions);
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
                    }
                    else
                    {
                        rightExpression = rightParenContained.Expression;
                    }
                }
                else if (expression.Right is BinaryExpressionNode rightBinaryExpression)
                {
                    if (rightBinaryExpression.Operator == DSharpBinaryOperator.Multiply ||
                        rightBinaryExpression.Operator == DSharpBinaryOperator.Divide)
                    {
                        rightBinaryExpression.CompileExpression(handler, compiledExpressions);
                    }
                    if (rightBinaryExpression.Left == null)
                    {
                        throw new ArgumentException($"Incomplete expression: {rightBinaryExpression}", nameof(expression));
                    }

                    handler(leftExpression, rightBinaryExpression.Left, expression.Operator);
                    return;
                }

                handler(leftExpression, rightExpression, expression.Operator);
            }
        }
        extension(IdentifierExpressionNode identifierExpression)
        {
            public IDSharpMemberInfo ResolveIdentifierMember(DSharpAssemblyBuilder assembly, IDSharpMemberInfo? currentMember = null, Func<IdentifierExpressionNode, IDSharpMemberInfo?>? identifierResolver = null)
            {
                if (identifierResolver != null)
                {
                    var member = identifierResolver(identifierExpression);

                    if (member != null)
                    {
                        return member;
                    }
                }

                var identifier = identifierExpression.Name;
                IDSharpType? currentType = null;

                if (currentMember is IDSharpType typeMember)
                {
                    currentType = typeMember;
                }
                else if (currentMember != null)
                {
                    currentType = currentMember.DeclaringType;
                }
                if (currentType != null)
                {
                    var field = currentType.GetFieldOrDefault(identifier);

                    if (field != null)
                    {
                        return field;
                    }

                    var property = currentType.GetPropertyOrDefault(identifier);

                    if (property != null) 
                    {
                        return property;
                    }

                    var method = currentType.GetMethodOrDefault(identifier);

                    if (method != null)
                    {
                        return method;
                    }
                }

                var globalVariable = assembly.GlobalVariables.FirstOrDefault(v => v.Name == identifier);

                if (globalVariable != null)
                {
                    return globalVariable;
                }
                
                var globalFunction = assembly.GlobalFunctions.FirstOrDefault(f => f.Name == identifier);

                if (globalFunction != null)
                {
                    return globalFunction;
                }

                throw new ArgumentException($"Unable to find any member with identifier {identifierExpression}", nameof(identifierExpression));
            }
        }
    }
}

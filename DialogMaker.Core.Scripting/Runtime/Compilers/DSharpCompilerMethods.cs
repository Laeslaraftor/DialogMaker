using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Runtime.Builders;

namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    public partial class DSharpCompiler
    {
        private void CompileMethod(DSharpMethodBuilder method, InvokableNode invokableNode, DSharpMethodCompileSettings settings = default)
        {
            if (invokableNode.Body == null)
            {
                if (method.IsExtern || method.DeclaringType?.ObjectType == DSharpObjectType.Interface)
                {
                    return;
                }

                throw new ArgumentException($"Invokable node must contains body: {invokableNode}", nameof(invokableNode));
            }

            CompileMethod(method, invokableNode.Body);
        }
        private void CompileMethod(DSharpMethodBuilder method, BlockStatementNode body, DSharpMethodCompileSettings settings = default)
        {
            var code = method.GetBytecodeBuilder();
            settings.LocalVariables ??= [];
            settings.AlwaysReturnMethods ??= [];
            settings.BannedExpressions ??= [];
            DSharpCompilerContext context = new(_context, method)
            {
                TypeResolver = code.ExpressionTypeResolver,
                MemberResolver = code.ExpressionMemberResolver,
            };

            CompileStatement(method, body, 0, code, ref settings, context);

            bool alwaysReturns = settings.AlwaysReturn(method);

            if (method.ReturnType != null &&
                !alwaysReturns)
            {
                throw new ArgumentException($"Not all path returns value in \"{method}\": {body}");
            }
            if (alwaysReturns &&
                body.Token.Type == DSharpTokenType.Lambda &&
                code.Instructions.Count > 0 &&
                code.Instructions[^1].Operation != DSharpBytecodeOperation.Return)
            {
                code.Return();
            }
        }

        #region Statements

        private void CompileStatement(DSharpMethodBuilder method, BlockStatementNode blockStatement, int depth, DSharpBytecodeBuilder code, ref DSharpMethodCompileSettings settings, DSharpCompilerContext context = default)
        {
            foreach (var statement in blockStatement.Statements)
            {
                CompileStatement(method, statement, depth, code, ref settings, context);
            }
        }
        private void CompileStatement(DSharpMethodBuilder method, StatementNode statement, int depth, DSharpBytecodeBuilder code, ref DSharpMethodCompileSettings settings, DSharpCompilerContext context = default)
        {
            DSharpMethodBuilderParameter GetCustomVariable(string name, object type, ref DSharpMethodCompileSettings settings, ExpressionNode? initializer = null)
            {
                return this.GetVariable(method, name, type, initializer, ref settings, context);
            }
            DSharpMethodBuilderParameter GetVariable(VariableNode? node, ref DSharpMethodCompileSettings settings)
            {
                if (node == null)
                {
                    throw new ArgumentNullException("Variable node must be provided", nameof(node));
                }
                if (node.Type == null)
                {
                    throw new ArgumentException($"Unknown variable type: {node}", nameof(node));
                }

                return this.GetVariable(method, node.Name, node.Type, node.Initializer, ref settings, context);
            }

            if (statement is BlockStatementNode blockStatement)
            {
                CompileStatement(method, blockStatement, depth + 1, code, ref settings, context);
            }
            else if (statement is VariableStatementNode variableStatement)
            {
                if (variableStatement.Variable == null)
                {
                    throw new ArgumentException($"Invalid statement: {variableStatement}");
                }

                var variableName = variableStatement.Variable.Name;

                if (method.Parameters.FirstOrDefault(p => p.Name == variableName) != null)
                {
                    throw new ArgumentException($"Unable to declare local variable because current scope contains parameter with such name ({variableName}): {variableStatement}");
                }
                if (settings.TryGetVariable(variableName, out _))
                {
                    throw new ArgumentException($"Unable to declare local variable because variable with such name already declared ({variableName}): {variableStatement}");
                }

                var variable = GetVariable(variableStatement.Variable, ref settings);
                var originalTypeResolver = context.TypeResolver;
                context.TypeResolver = obj =>
                {
                    if (obj == null && variable.Type != null)
                    {
                        return _assemblyBuilder.GetType(variable.Type) as IDSharpType;
                    }

                    return originalTypeResolver?.Invoke(obj!);
                };

                if (variableStatement.Variable!.Initializer != null)
                {
                    CompileValueExpression(method, variableStatement.Variable.Initializer, ref settings, null, context);
                    code.StoreLocal(variable);
                    code.Pop();
                }
            }
            else if (statement is ExpressionStatementNode expressionStatement)
            {
                if (expressionStatement.Expression == null)
                {
                    throw new ArgumentException($"Invalid statement: {expressionStatement}");
                }

                CompileExpression(method, expressionStatement.Expression, ref settings, null, context);

                if (depth == 0)
                {
                    if (expressionStatement.Expression.IsNullExpression())
                    {
                        settings.AddReturnMethod(method);
                        return;
                    }

                    try
                    {
                        var expressionType = expressionStatement.Expression.GetExpressionType(_assemblyBuilder, context);

                        if (expressionType != null &&
                            (expressionType is IDSharpType || expressionType.TryGetReturnType(out _)))
                        {
                            settings.AddReturnMethod(method);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            else if (statement is IfStatementNode ifStatement)
            {
                if (ifStatement.Condition == null)
                {
                    throw new ArgumentException($"If statement should contains condition: {statement}", nameof(statement));
                }

                CompileValueExpression(method, ifStatement.Condition, ref settings, null, context);
                var jumpOperation = code.JumpIfFalse();
                code.Pop();

                if (ifStatement.ThenBranch == null)
                {
                    throw new ArgumentException($"If statement should contains then branch: {statement}", nameof(statement));
                }

                CompileStatement(method, ifStatement.ThenBranch, depth + 1, code, ref settings, context);

                DSharpBytecodeBuilder.ReferenceInstruction? skipOtherOperation = null;

                if (ifStatement.ElseBranch != null)
                {
                    skipOtherOperation = code.Jump();
                }

                jumpOperation.ReferencedInstruction = code.Empty();

                if (ifStatement.ElseBranch != null)
                {
                    if (ifStatement.ElseBranch is not BlockStatementNode &&
                        ifStatement.ElseBranch is not IfStatementNode)
                    {
                        throw new ArgumentException($"Invalid else branch: {ifStatement.ElseBranch}", nameof(statement));
                    }
                    if (depth == 0 && ifStatement.ElseBranch is BlockStatementNode elseBlock)
                    {
                        var containsReturn = elseBlock.Statements.Any(s => s is ReturnStatementNode);

                        if (containsReturn)
                        {
                            settings.AddReturnMethod(method);
                        }
                    }

                    CompileStatement(method, ifStatement.ElseBranch, depth + 1, code, ref settings, context);
                }

                skipOtherOperation?.ReferencedInstruction = code.Empty();
            }
            else if (statement is WhileStatementNode whileStatement)
            {
                if (whileStatement.Condition == null)
                {
                    throw new ArgumentException($"While statement should contains condition: {statement}", nameof(statement));
                }

                var expressionStartOperation = code.Empty(true);
                CompileValueExpression(method, whileStatement.Condition, ref settings, null, context);

                int startOperationIndex = code.Instructions.IndexOf(expressionStartOperation);
                code.Instructions.Remove(expressionStartOperation);
                expressionStartOperation = code.Instructions[startOperationIndex];
                context.CurrentLoopStartInstruction = expressionStartOperation;

                var skipLoop = code.JumpIfFalse();
                code.Pop();

                context.CurrentLoopEndInstruction = code.Empty();
                skipLoop.ReferencedInstruction = context.CurrentLoopEndInstruction;
                code.Instructions.Remove(context.CurrentLoopEndInstruction);

                if (whileStatement.Body != null)
                {
                    CompileStatement(method, whileStatement.Body, depth + 1, code, ref settings, context);
                }

                code.Jump(expressionStartOperation);
                code.Instructions.Add(context.CurrentLoopEndInstruction);
            }
            else if (statement is ForStatementNode forStatement)
            {
                if (forStatement.Initializer == null)
                {
                    throw new ArgumentException($"For statement should contains initializer: {statement}", nameof(statement));
                }
                if (forStatement.Condition == null)
                {
                    throw new ArgumentException($"For statement should contains condition: {statement}", nameof(statement));
                }
                if (forStatement.Increment == null)
                {
                    throw new ArgumentException($"For statement should contains increment: {statement}", nameof(statement));
                }
                if (forStatement.Body == null)
                {
                    throw new ArgumentException($"For statement should contains body: {statement}", nameof(statement));
                }

                var conditionType = forStatement.Condition.GetExpressionType(_assemblyBuilder, context) as IDSharpType
                    ?? throw new ArgumentException($"Unable to get condition return type: {statement}", nameof(statement));

                if (!conditionType.IsAssignableTo(_assemblyBuilder.BoolType))
                {
                    throw new ArgumentException($"Condition should return boolean value: {statement}", nameof(statement));
                }

                CompileStatement(method, forStatement.Initializer, depth + 1, code, ref settings, context);
                var startExpressionOperation = code.Empty(true);
                CompileValueExpression(method, forStatement.Condition, ref settings, null, context);
                var skipOperation = code.JumpIfFalse();
                code.Pop();

                int startExpressionIndex = code.Instructions.IndexOf(startExpressionOperation);
                code.Instructions.Remove(startExpressionOperation);
                startExpressionOperation = code.Instructions[startExpressionIndex];

                context.CurrentLoopStartInstruction = startExpressionOperation;
                context.CurrentLoopEndInstruction = code.Empty(true);
                skipOperation.ReferencedInstruction = context.CurrentLoopEndInstruction;
                code.Instructions.Remove(context.CurrentLoopEndInstruction);

                CompileStatement(method, forStatement.Body, depth + 1, code, ref settings, context);
                CompileExpression(method, forStatement.Increment, ref settings, null, context);
                code.Jump(startExpressionOperation);
                code.Instructions.Add(context.CurrentLoopEndInstruction);
            }
            else if (statement is ForeachStatementNode foreachStatement)
            {
                if (foreachStatement.Variable == null ||
                    foreachStatement.EnumeratorExpression == null ||
                    foreachStatement.Body == null)
                {
                    throw new ArgumentException($"Incomplete foreach statement: {statement}", nameof(statement));
                }

                var expressionResult = CompileValueExpression(method, foreachStatement.EnumeratorExpression, ref settings, null, context)
                    ?? throw new ArgumentException($"Invalid expression in foreach statement: {statement}", nameof(statement));

                if (!expressionResult.TryGetReturnType(out var expressionReturnType))
                {
                    throw new ArgumentException($"Unable to get return type of expression in foreach statement: {statement}", nameof(statement));
                }

                var returnTypeMethods = expressionReturnType.GetAllMembers(m => m is IDSharpMethodInfo &&
                                                                                m.DeclaringType?.ObjectType != DSharpObjectType.Interface &&
                                                                                m.Access == DSharpAccessModifier.Public)
                                                            .Cast<IDSharpMethodInfo>();
                var getEnumeratorMethod = returnTypeMethods.FirstOrDefault(m => m.Name == "GetEnumerator" &&
                                                                                m.ReturnType != null &&
                                                                                m.ReturnType.IsAssignableTo(_assemblyBuilder.IEnumeratorType.Type));

                if (getEnumeratorMethod == null)
                {
                    throw new ArgumentException($"Unable to find public GetEnumerator method with \"{_assemblyBuilder.IEnumeratorType}\" as return value in expression of foreach statement", nameof(statement));
                }

                var enumerator = DSharpIEnumeratorType.Create(getEnumeratorMethod.ReturnType!);
                DSharpMethodBuilderParameter variable;

                if (foreachStatement.Variable.Type?.Token.Type == DSharpTokenType.Var)
                {
                    variable = GetCustomVariable(foreachStatement.Variable.Name, enumerator.CurrentProperty.PropertyType, ref settings);
                }
                else
                {
                    variable = GetVariable(foreachStatement.Variable, ref settings);

                    if (variable.Type == null)
                    {
                        variable.Type = _assemblyBuilder.ObjectToken;
                    }
                    else
                    {
                        var variableType = (IDSharpType)_assemblyBuilder.GetType(variable.Type);

                        if (!enumerator.CurrentProperty.PropertyType.IsAssignableTo(variableType))
                        {
                            throw new ArgumentException($"Can not assign enumeration value to variable in foreach statement: {statement}", nameof(statement));
                        }
                    }
                }

                var enumeratorVariable = GetCustomVariable($"foreachEnumerator_{statement.Line}_{statement.Column}", getEnumeratorMethod.ReturnType!, ref settings);

                code.CallInstance(getEnumeratorMethod);
                code.PopOffset(1);
                code.StoreLocal(enumeratorVariable);
                code.CallInstance(enumerator.ResetMethod);
                var moveNextOperation = code.CallInstance(enumerator.MoveNextMethod);
                var skipIteration = code.JumpIfFalse();
                code.Pop();
                code.StartStackBlock();
                code.LoadInstanceProperty(enumerator.CurrentProperty);
                code.StoreLocal(variable);
                code.Pop();

                context.CurrentLoopStartInstruction = code.EndStackBlock();
                context.CurrentLoopEndInstruction = code.Pop();

                code.Instructions.Remove(context.CurrentLoopStartInstruction);
                code.Instructions.Remove(context.CurrentLoopEndInstruction);
                CompileStatement(method, foreachStatement.Body, depth + 1, code, ref settings, context);

                code.Instructions.Add(context.CurrentLoopStartInstruction);

                code.Jump(moveNextOperation);

                skipIteration.ReferencedInstruction = context.CurrentLoopEndInstruction;
                code.Instructions.Add(context.CurrentLoopEndInstruction);
            }
            else if (statement is ContinueStatementNode continueStatement)
            {
                if (context.CurrentLoopStartInstruction == null)
                {
                    throw new ArgumentException($"Continue statement is unavailable in current context: {statement}", nameof(statement));
                }

                code.Jump(context.CurrentLoopStartInstruction);
            }
            else if (statement is BreakStatementNode breakStatement)
            {
                if (context.CurrentLoopEndInstruction == null)
                {
                    throw new ArgumentException($"Break statement is unavailable in current context: {statement}", nameof(statement));
                }

                code.Jump(context.CurrentLoopEndInstruction);
            }
            else if (statement is ReturnStatementNode returnStatement)
            {
                IDSharpType? methodReturnType = null;

                if (method.ReturnType != null)
                {
                    methodReturnType = _assemblyBuilder.GetType(method.ReturnType) as IDSharpType;
                }
                if (returnStatement.Value != null)
                {
                    if (methodReturnType == null)
                    {
                        throw new InvalidOperationException($"Can not return value because method is not returning values: {statement}");
                    }

                    IDSharpType? expressionType = null;
                    bool isNullValue = false;

                    if (returnStatement.Value.IsNullExpression())
                    {
                        isNullValue = true;
                    }
                    else
                    {
                        expressionType = returnStatement.Value.GetExpressionType(_assemblyBuilder, context) as IDSharpType
                            ?? throw new ArgumentException($"Unable to get expression type: {returnStatement.Value}", nameof(statement));
                    }

                    if (!isNullValue && expressionType?.IsAssignableTo(methodReturnType) != true)
                    {
                        throw new ArgumentException($"Returning value must be with the same type with current method or function \"{method}\". Type that returns: {expressionType}, but required: {methodReturnType}.{Environment.NewLine}Expression: {statement}", nameof(statement));
                    }

                    var originalResolver = context.TypeResolver;

                    context.TypeResolver = obj =>
                    {
                        if (obj == null && method.ReturnType != null)
                        {
                            return _assemblyBuilder.GetType(method.ReturnType) as IDSharpType;
                        }

                        return originalResolver?.Invoke(obj!);
                    };

                    CompileValueExpression(method, returnStatement.Value, ref settings, null, context);
                }
                else if (method.ReturnType != null)
                {
                    throw new InvalidOperationException($"Return statement should returns some value in method that returning value: {statement}");
                }
                if (depth == 0)
                {
                    settings.AddReturnMethod(method);
                }

                code.Return();
            }
            else
            {
                throw new ArgumentException($"Invalid statement in current context: {statement}", nameof(statement));
            }
        }

        #endregion

        #region Expressions

        private void CompileExpression(DSharpMethodBuilder method, ExpressionNode expression, ref DSharpMethodCompileSettings settings, ExpressionNode? parentExpression = null, DSharpCompilerContext context = default)
        {
            var code = method.GetBytecodeBuilder();

            if (expression is AssignmentExpressionNode assignExpression)
            {
                if (assignExpression.Left == null || assignExpression.Right == null)
                {
                    throw new ArgumentException($"Incomplete expression: {expression}", nameof(expression));
                }

                void AddExtraAssignOperation()
                {
                    if (assignExpression.Operator == DSharpAssignmentOperator.Assign)
                    {
                        return;
                    }
                    if (assignExpression.Operator == DSharpAssignmentOperator.PlusAssign)
                    {
                        code.Add();
                    }
                    else if (assignExpression.Operator == DSharpAssignmentOperator.MinusAssign)
                    {
                        code.Subtract();
                    }
                    else if (assignExpression.Operator == DSharpAssignmentOperator.MultiplyAssign)
                    {
                        code.Multiply();
                    }
                    else if (assignExpression.Operator == DSharpAssignmentOperator.DivideAssign)
                    {
                        code.Divide();
                    }

                    code.PopOffset(1);
                    code.PopOffset(1);
                }

                if (assignExpression.Left is IdentifierExpressionNode identifier)
                {

                    var localName = identifier.GetName(false);
                    var parameter = method.Parameters.FirstOrDefault(p => p.Name == localName);

                    if (parameter != null)
                    {
                        if (assignExpression.Operator != DSharpAssignmentOperator.Assign)
                        {
                            code.LoadArgument(parameter);
                        }

                        CompileValueExpression(method, assignExpression.Right, ref settings, expression, context);
                        AddExtraAssignOperation();

                        code.StoreArgument(parameter);
                        code.Pop();
                        return;
                    }
                    if (settings.TryGetVariable(localName, out var variable))
                    {
                        if (assignExpression.Operator != DSharpAssignmentOperator.Assign)
                        {
                            code.LoadLocal(variable);
                        }

                        CompileValueExpression(method, assignExpression.Right, ref settings, expression, context);
                        AddExtraAssignOperation();

                        code.StoreLocal(variable);
                        code.Pop();
                        return;
                    }
                }

                settings.DoNotCompileEndPointMember = true;

                if (assignExpression.Left is ArrayAccessExpressionNode arrayAccess)
                {
                    if (arrayAccess.Array == null ||
                        arrayAccess.Index == null)
                    {
                        throw new ArgumentException($"Incomplete expression: {arrayAccess}");
                    }

                    CompileValueExpression(method, arrayAccess.Array, ref settings, arrayAccess, context);
                    CompileValueExpression(method, arrayAccess.Index, ref settings, arrayAccess, context);
                    CompileValueExpression(method, assignExpression.Right, ref settings, expression, context);

                    if (assignExpression.Operator != DSharpAssignmentOperator.Assign)
                    {
                        code.LoadArrayItem();
                        AddExtraAssignOperation();
                    }

                    code.StoreArrayItem();
                    code.Pop();
                    code.Pop();
                    code.Pop();
                }
                else
                {
                    IDSharpMemberInfo? member;

                    if (assignExpression.Left is MemberAccessExpressionNode leftMemberAccess)
                    {
                        member = CompileMemberAccessExpression(method, leftMemberAccess, (p, e, ref s, c) =>
                        {
                            c.ParentExpression = p;

                            if (p is ThisExpressionNode ||
                                p is BaseExpressionNode)
                            {
                                code.LoadInstance();
                            }
                            if (c.TryResolveMember(e, out var resolvedMember))
                            {
                                return resolvedMember;
                            }

                            throw new InvalidOperationException($"Unable to resolve member: {e}");
                        }, ref settings, context);
                    }
                    else
                    {
                        member = CompileValueExpression(method, assignExpression.Left, ref settings, expression, context);
                    }

                    settings.DoNotCompileEndPointMember = false;

                    if (member == null)
                    {
                        throw new ArgumentException($"Unable to find member: {assignExpression.Left}", nameof(expression));
                    }

                    CompileValueExpression(method, assignExpression.Right, ref settings, expression, context);

                    if (assignExpression.Operator != DSharpAssignmentOperator.Assign)
                    {
                        code.LoadPropertyOrField(member, settings.NextNonVirtualizedAccess);
                        AddExtraAssignOperation();
                    }

                    if (method.MethodType == DSharpMethodType.Constructor &&
                        member is IDSharpPropertyInfo property && !property.CanWrite)
                    {
                        if (property.DeclaringType == null)
                        {
                            throw new InvalidOperationException($"Property must contains declaring type {property}: {expression}");
                        }

                        var propertyField = property.DeclaringType.GetFieldOrDefault($"{property.Name}{ValueFieldNameSuffix}");

                        if (propertyField == null)
                        {
                            throw new ArgumentException($"Unable to write value to property \"{property}\" because it have not setter: {expression}", nameof(expression));
                        }

                        member = propertyField;
                    }

                    code.StorePropertyOrField(member, settings.NextNonVirtualizedAccess);
                    settings.NextNonVirtualizedAccess = false;

                    if (!member.IsStatic)
                    {
                        code.PopRepeat(2);
                    }
                    else
                    {
                        code.Pop();
                    }
                }
            }
            else if (expression is IncrementExpressionNode incrementExpression)
            {
                if (incrementExpression.Expression == null)
                {
                    throw new ArgumentException($"Invalid expression: {expression}", nameof(expression));
                }

                CompileValueExpressionOperation(method, code, incrementExpression.Expression, () => code.Increment(), ref settings, context);
            }
            else if (expression is DecrementExpressionNode decrementExpression)
            {
                if (decrementExpression.Expression == null)
                {
                    throw new ArgumentException($"Invalid expression: {expression}", nameof(expression));
                }

                CompileValueExpressionOperation(method, code, decrementExpression.Expression, () => code.Decrement(), ref settings, context);
            }
            else if (expression is CallExpressionNode ||
                     expression is MemberAccessExpressionNode ||
                     expression is BinaryExpressionNode ||
                     expression is UnaryExpressionNode ||
                     expression is LiteralExpressionNode ||
                     expression is IdentifierExpressionNode)
            {
                CompileValueExpression(method, expression, ref settings, parentExpression, context);
            }
            else
            {
                throw new ArgumentException($"Invalid expression for current context: {expression}", nameof(context));
            }
        }

        /// <summary>
        /// Compiles expression node that contains some expression that returns value or not (only when called method that returns nothing).
        /// Compiled code contains store expression value at bottom of stack and removes all temporally created values
        /// </summary>
        /// <param name="method">Method or function that contains expression</param>
        /// <param name="expression">Expression to compile</param>
        /// <param name="settings">Settings of compiling</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        private IDSharpMemberInfo? CompileValueExpression(DSharpMethodBuilder method, ExpressionNode expression, ref DSharpMethodCompileSettings settings, ExpressionNode? parentExpression = null, DSharpCompilerContext context = default)
        {
            var code = method.GetBytecodeBuilder();

            if (expression is IdentifierExpressionNode identifierExpression)
            {
                var parameter = method.Parameters.FirstOrDefault(p => p.Name == identifierExpression.Name);

                if (parameter != null)
                {
                    if (!settings.DoNotCompileEndPointMember)
                    {
                        code.LoadArgument(parameter);
                    }

                    return null;
                }
                if (settings.TryGetVariable(identifierExpression.Name, out var variable))
                {
                    if (!settings.DoNotCompileEndPointMember)
                    {
                        code.LoadLocal(variable);
                    }

                    return null;
                }

                IDSharpMemberInfo member;

                try
                {
                    member = context.FindAnyAvailableMember(identifierExpression.GetName(true));
                }
                catch (Exception error)
                {
                    throw new InvalidOperationException($"Unable to find any available member: \"{expression}\"", error);
                }

                if (member is IDSharpFieldInfo || member is IDSharpPropertyInfo)
                {
                    bool instanceLoaded = false;

                    if (!member.IsStatic &&
                        parentExpression is not MemberAccessExpressionNode &&
                        parentExpression is not IdentifierExpressionNode &&
                        ((code.Instructions.Count > 0 &&
                        code.Instructions[^1].Operation != DSharpBytecodeOperation.LoadInstance) ||
                        code.Instructions.Count == 0))
                    {
                        code.LoadInstance();

                        instanceLoaded = true;
                    }

                    if (!settings.DoNotCompileEndPointMember)
                    {
                        code.LoadPropertyOrField(member, settings.NextNonVirtualizedAccess);
                        settings.NextNonVirtualizedAccess = false;

                        if (instanceLoaded)
                        {
                            code.PopOffset(1);
                        }
                    }
                }

                return member;
            }
            else if (expression is MemberAccessExpressionNode memberAccessExpression)
            {
                return CompileMemberAccessExpression(method, memberAccessExpression, (p, e, ref s, c) =>
                {
                    c.ParentExpression = p;

                    if (c.CurrentMember != null &&
                        c.CurrentMember is not IDSharpType &&
                        c.CurrentMember.TryGetReturnType(out var memberType))
                    {
                        c.CurrentMember = memberType;
                    }

                    var result = CompileValueExpression(method, e, ref s, p, c);

                    return result;
                }, ref settings, context);
            }
            else if (expression is CallExpressionNode callExpression)
            {
                if (callExpression.Callee == null)
                {
                    throw new ArgumentException($"Unable to call method without identifier: {callExpression}", nameof(expression));
                }

                var startCurrentMember = context.CurrentMember;
                context.CurrentMember = method;

                foreach (var arg in callExpression.Arguments)
                {
                    CompileValueExpression(method, arg, ref settings, callExpression, context);
                }

                context.CurrentMember = startCurrentMember;

                int popOffset = 0;
                bool removeInstance = false;
                IDSharpMethodInfo calledMethod;

                void CallAuto(IDSharpMethodInfo method, ref DSharpMethodCompileSettings settings)
                {
                    code.CallAuto(method, settings.Await(callExpression), ref settings);
                    settings.NextNonVirtualizedAccess = false;
                }

                if (callExpression.Callee is IdentifierExpressionNode identifier)
                {
                    calledMethod = context.FindAnyAvailableMember<IDSharpMethodInfo>(identifier.Name);

                    if (calledMethod.ReturnType != null)
                    {
                        popOffset = 1;
                    }
                    if (!calledMethod.IsStatic &&
                        (parentExpression == null ||
                        parentExpression is ThisExpressionNode ||
                        parentExpression is BaseExpressionNode))
                    {
                        code.LoadInstance();
                        removeInstance = true;
                    }

                    CallAuto(calledMethod, ref settings);
                }
                else if (callExpression.Callee is MemberAccessExpressionNode memberMethod)
                {
                    if (memberMethod.Target == null)
                    {
                        throw new ArgumentException($"Target identifier can not be empty when trying to accessing member: {memberMethod}", nameof(expression));
                    }
                    if (memberMethod.Member is not IdentifierExpressionNode methodIdentifier)
                    {
                        throw new ArgumentException($"Method name must be identifier: {memberMethod}", nameof(expression));
                    }

                    calledMethod = context.FindAnyAvailableMember<IDSharpMethodInfo>(methodIdentifier.Name);

                    if (calledMethod.ReturnType != null)
                    {
                        popOffset = 1;
                    }

                    CompileValueExpression(method, memberMethod.Target, ref settings, memberMethod, context);
                    CallAuto(calledMethod, ref settings);
                    code.PopOffset(popOffset);
                }
                else
                {
                    throw new ArgumentException($"Invalid method identifier: {callExpression.Callee}", nameof(expression));
                }

                int argsCount = callExpression.Arguments.Count;

                if (removeInstance)
                {
                    argsCount++;
                }

                for (int i = 0; i < argsCount; i++)
                {
                    if (popOffset == 0)
                    {
                        code.Pop();
                        continue;
                    }

                    code.PopOffset(popOffset);
                }

                return calledMethod;
            }
            else if (expression is ArrayAccessExpressionNode arrayExpression)
            {
                if (arrayExpression.Array == null)
                {
                    throw new ArgumentException($"Array identifier can not be empty: {arrayExpression}", nameof(expression));
                }
                if (arrayExpression.Index == null)
                {
                    throw new ArgumentException($"Array index must be specified: {arrayExpression}", nameof(expression));
                }

                var array = CompileValueExpression(method, arrayExpression.Array, ref settings, arrayExpression, context);
                CompileValueExpression(method, arrayExpression.Index, ref settings, arrayExpression, context);
                code.LoadArrayItem();
                code.PopOffsetRepeat(1, 2);

                return array;
            }
            else if (expression.TrySimplifyToLiteral(out var literal))
            {
                code.Push(literal);
                return null;
            }
            else if (expression is UnaryExpressionNode unaryExpression)
            {
                if (unaryExpression.Operand == null)
                {
                    throw new ArgumentException($"Incomplete expression: {unaryExpression}", nameof(expression));
                }

                var expressionType = unaryExpression.Operand.GetExpressionType(_assemblyBuilder, context)
                    ?? throw new InvalidOperationException($"Unable to get result type of expression: {unaryExpression.Operand}");

                bool CheckIsNumber(bool throwException = true)
                {
                    if (expressionType.MetadataToken != _assemblyBuilder.NumberToken)
                    {
                        if (throwException)
                        {
                            throw new ArgumentException($"Invert operator can be used only with number values: {unaryExpression}", nameof(expression));
                        }

                        return false;
                    }

                    return true;
                }

                CompileValueExpression(method, unaryExpression.Operand, ref settings, expression, context);

                if (unaryExpression.Operator == DSharpUnaryOperator.Not)
                {
                    if (expressionType.MetadataToken != _assemblyBuilder.BoolToken)
                    {
                        throw new ArgumentException($"NOT operator can be used only with boolean values: {unaryExpression}", nameof(expression));
                    }

                    code.Not();

                    return null;
                }
                else if (unaryExpression.Operator == DSharpUnaryOperator.Minus)
                {
                    CheckIsNumber();
                    code.Push(-1);
                    code.Multiply();
                    code.PopOffset(1);
                    code.PopOffset(1);

                    return null;
                }

                if (CheckIsNumber(false) && expressionType.MetadataToken != _assemblyBuilder.CharToken)
                {
                    throw new ArgumentException($"Increment and decrement can be used only with number or char values: {unaryExpression}", nameof(expression));
                }

                if (unaryExpression.Operator == DSharpUnaryOperator.Increment)
                {
                    code.Increment();
                }
                else if (unaryExpression.Operator == DSharpUnaryOperator.Decrement)
                {
                    code.Decrement();
                }
                else
                {
                    throw new ArgumentException($"Invalid operator {(DSharpTokenType)unaryExpression.Operator}: {expression}", nameof(expression));
                }

                return null;
            }
            else if (expression is BinaryExpressionNode binaryExpression)
            {
                var settingsValue = settings;

                void Handle(ExpressionNode left, ExpressionNode right, DSharpBinaryOperator @operator)
                {
                    CompileValueExpression(method, left, ref settingsValue, binaryExpression, context);
                    CompileValueExpression(method, right, ref settingsValue, binaryExpression, context);

                    switch (@operator)
                    {
                        case DSharpBinaryOperator.Plus:
                            code.Add();
                            break;
                        case DSharpBinaryOperator.Minus:
                            code.Subtract();
                            break;
                        case DSharpBinaryOperator.Multiply:
                            code.Multiply();
                            break;
                        case DSharpBinaryOperator.Divide:
                            code.Divide();
                            break;
                        case DSharpBinaryOperator.Mod:
                            code.Mod();
                            break;
                        case DSharpBinaryOperator.LogicalAnd:
                            code.And();
                            break;
                        case DSharpBinaryOperator.LogicalOr:
                            code.Or();
                            break;
                        case DSharpBinaryOperator.LogicalLess:
                            code.Less();
                            break;
                        case DSharpBinaryOperator.LogicalLessOrEquals:
                            code.LessOrEqual();
                            break;
                        case DSharpBinaryOperator.LogicalGreater:
                            code.Greater();
                            break;
                        case DSharpBinaryOperator.LogicalGreaterOrEquals:
                            code.GreaterOrEqual();
                            break;
                        case DSharpBinaryOperator.LogicalEquals:
                            code.Equals();
                            break;
                        case DSharpBinaryOperator.LogicalNotEquals:
                            code.NotEquals();
                            break;
                        default:
                            throw new ArgumentException($"Unknown operator \"{(DSharpTokenType)@operator}\": {binaryExpression}", nameof(@operator));
                    }

                    code.PopOffsetRepeat(1, 2);
                }

                binaryExpression.CompileExpression(Handle);

                return null;
            }
            else if (expression is NewInstanceExpressionNode newExpression)
            {
                TypeInfoNode? typeInfo = newExpression.Type;
                IDSharpType type;

                if (typeInfo == null)
                {
                    type = context.TypeResolver?.Invoke(null!) ??
                        throw new ArgumentException($"Can not create new instance when type not specified: {newExpression}", nameof(expression)); ;
                }
                else
                {
                    var typeToken = ResolveType(method, typeInfo);
                    type = (IDSharpType)_assemblyBuilder.GetType(typeToken);
                }
                if (type.ObjectType == DSharpObjectType.Interface)
                {
                    throw new ArgumentException($"Can not create new instance of interface \"{type}\": {expression}", nameof(expression));
                }
                if (type.IsAbstract)
                {
                    throw new ArgumentException($"Can not create new instance of abstract object \"{type}\": {expression}", nameof(expression));
                }

                if (newExpression.Parameters.Count == 0)
                {
                    var constructors = type.GetConstructors();

                    if (constructors.Length > 0)
                    {
                        var noParametersConstructor = constructors.FirstOrDefault(c => c.GetParameters().Length == 0)
                            ?? throw new InvalidOperationException($"Can not create new instance of object \"{type}\" because it not contains constructor with no parameters");
                    }

                    code.New(type);
                }
                else
                {
                    foreach (var parameter in newExpression.Parameters)
                    {
                        CompileValueExpression(method, parameter, ref settings, expression, context);
                    }

                    DSharpCompilerContext typeContext = new(context, type);
                    var constructor = typeContext.FindConstructor(newExpression.Parameters.Count)
                        ?? throw new ArgumentException($"Unable to find constructor with parameters {newExpression.Parameters.Count} at {type}:{Environment.NewLine} {expression}", nameof(expression));
                    code.New(constructor);
                }

                if (newExpression.Parameters.Count == 1)
                {
                    code.PopOffset(1);
                }
                else if (newExpression.Parameters.Count > 1)
                {
                    code.PopOffsetRepeat(1, newExpression.Parameters.Count);
                }

                context.CurrentMember = type;

                foreach (var initializer in newExpression.PropertiesInitializer)
                {
                    if (initializer.Operator != DSharpAssignmentOperator.Assign)
                    {
                        throw new ArgumentException($"Member initializer must be assignment, got: {initializer.Operator}: {initializer}", nameof(expression));
                    }
                    if (initializer.Left is not IdentifierExpressionNode identifier)
                    {
                        throw new ArgumentException($"Invalid member identifier: {initializer.Left}", nameof(expression));
                    }
                    if (initializer.Right == null)
                    {
                        throw new ArgumentException($"Initializer should contain value: {initializer}", nameof(initializer));
                    }

                    var name = identifier.GetName(false);
                    var property = type.GetProperty(name);

                    CompileValueExpression(method, initializer.Right, ref settings, initializer, context);

                    if (property != null)
                    {
                        code.StoreInstanceProperty(property);
                    }

                    var field = type.GetField(name);

                    if (field != null)
                    {
                        code.StoreInstanceField(field);
                    }

                    code.Pop();

                    if (property == null && field == null)
                    {
                        throw new ArgumentException($"Unknown member {name} at {type}: {initializer}", nameof(initializer));
                    }
                }

                return type;
            }
            else if (expression is NewArrayExpressionNode newArrayExpression)
            {
                if (newArrayExpression.Type == null)
                {
                    throw new ArgumentException($"Array type not specified: {expression}", nameof(expression));
                }
                if (!context.TryResolveType(newArrayExpression.Type, out var typeToken))
                {
                    throw new ArgumentException($"Unable to resolve array type: {expression}", nameof(expression));
                }
                if (newArrayExpression.SizeExpressions.Count == 0 && newArrayExpression.ItemsExpressions.Count == 0)
                {
                    throw new ArgumentException($"Array size or items not specified: {expression}", nameof(expression));
                }
                if (newArrayExpression.SizeExpressions.Count > 0 && newArrayExpression.ItemsExpressions.Count > 0)
                {
                    throw new ArgumentException($"Array items preset unavailable when size specified: {expression}", nameof(expression));
                }

                var type = (IDSharpType)_assemblyBuilder.GetType(typeToken);
                int arraySize = newArrayExpression.SizeExpressions.Count;

                if (newArrayExpression.SizeExpressions.Count > 0)
                {
                    CompileValueExpression(method, newArrayExpression.SizeExpressions[0], ref settings, expression, context);
                }
                else
                {
                    code.Push(newArrayExpression.ItemsExpressions.Count);
                }

                code.NewArray(type);
                code.PopOffset(1);

                if (newArrayExpression.ItemsExpressions.Count > 0)
                {
                    code.Push(0);
                }
                foreach (var item in newArrayExpression.ItemsExpressions)
                {
                    CompileValueExpression(method, item, ref settings, expression, context);
                    code.StoreArrayItem();
                    code.Pop();
                    code.Increment();
                }
                if (newArrayExpression.ItemsExpressions.Count > 0)
                {
                    code.Pop();
                }

                return _assemblyBuilder.CreateArray(type);
            }
            else if (expression is AwaitExpressionNode awaitExpression)
            {
                if (awaitExpression.Expression is not CallExpressionNode calling)
                {
                    throw new ArgumentException($"await appliable only for methods and functions", nameof(expression));
                }

                settings.CallingsToAwait ??= [];
                settings.CallingsToAwait.Add(calling);

                return CompileValueExpression(method, calling, ref settings, expression, context);
            }
            else if (expression is LiteralExpressionNode literalExpression)
            {
                code.Push(literalExpression.Value);
                return null;
            }
            else if (expression is ThisExpressionNode thisExpression)
            {
                if (method.IsStatic)
                {
                    throw new InvalidOperationException($"Unable to load current instance inside static member: {expression}");
                }
                if (method.DeclaringType == null)
                {
                    throw new InvalidOperationException($"Unable to load current instance inside global function");
                }
                if (parentExpression is not MemberAccessExpressionNode)
                {
                    code.LoadInstance();
                }

                return method.DeclaringType;
            }
            else if (expression is BaseExpressionNode baseExpression)
            {
                if (parentExpression is not MemberAccessExpressionNode parentMemberAccess ||
                    parentMemberAccess.Target != null &&
                    parentMemberAccess.Target is not BaseExpressionNode)
                {
                    throw new InvalidOperationException($"\"base\" is unavailable in current context: {expression}");
                }
                if (method.IsStatic)
                {
                    throw new InvalidOperationException($"Unable to load current instance inside static member: {expression}");
                }
                if (method.DeclaringType == null)
                {
                    throw new InvalidOperationException($"Unable to load current instance inside global function");
                }
                if (context.CurrentMember == _assemblyBuilder.ObjectType ||
                    context.CurrentMember?.DeclaringType == _assemblyBuilder.ObjectType)
                {
                    throw new InvalidOperationException($"Unable to access to base type of \"{_assemblyBuilder.ObjectType}\"");
                }
                if (expression.GetExpressionType(_assemblyBuilder, context) is not IDSharpType type)
                {
                    throw new ArgumentException($"Unable to get expression type: {expression}", nameof(expression));
                }

                settings.NextNonVirtualizedAccess = true;

                return type;
            }
            else if (expression is IncrementExpressionNode incrementExpression)
            {
                if (incrementExpression.Expression == null)
                {
                    throw new ArgumentException($"Invalid expression: {expression}", nameof(expression));
                }

                bool popLast = parentExpression == null;

                return CompileValueExpressionOperation(method, code, incrementExpression.Expression, () => code.Increment(), ref settings, context, popLast);
            }
            else if (expression is DecrementExpressionNode decrementExpression)
            {
                if (decrementExpression.Expression == null)
                {
                    throw new ArgumentException($"Invalid expression: {expression}", nameof(expression));
                }

                bool popLast = parentExpression == null;

                return CompileValueExpressionOperation(method, code, decrementExpression.Expression, () => code.Decrement(), ref settings, context, popLast);
            }
            else if (expression is AssignmentExpressionNode assignmentExpression)
            {
                CompileExpression(method, assignmentExpression, ref settings, parentExpression, context);
                return null;
            }

            throw new ArgumentException($"Unable to compile expression: {expression}", nameof(expression));
        }
        public IDSharpMemberInfo? CompileMemberAccessExpression(DSharpMethodBuilder method, MemberAccessExpressionNode memberAccessExpression, MemberAccessExpressionEndPointHandler endPointHandler, ref DSharpMethodCompileSettings settings, DSharpCompilerContext context = default)
        {
            var startDoNotCompileEndPointMemberValue = settings.DoNotCompileEndPointMember;

            if (context.CurrentMember == null)
            {
                context.CurrentMember = method.DeclaringType;
            }
            else if (context.CurrentMember != null &&
                     context.CurrentMember is not IDSharpType &&
                     context.CurrentMember.TryGetReturnType(out var contextMemberType))
            {
                context.CurrentMember = contextMemberType;
            }

            settings.DoNotCompileEndPointMember = false;

            var currentMemberAccess = memberAccessExpression;
            var code = method.GetBytecodeBuilder();
            ExpressionNode? previousTarget = null;
            IDSharpMemberInfo? currentMember = context.CurrentMember;
            bool previousIsThis = false;
            bool canUseThis = true;
            bool canUseBase = true;
            bool lastAccessedAsLocalMember = false;

            while (true)
            {
                IDSharpMemberInfo currentType;
                bool currentIsBase = false;
                bool currentIsThis = false;
                bool accessedAsLocalMember = false;

                if (currentMemberAccess.Target is ThisExpressionNode thisExpression)
                {
                    if (method.DeclaringType == null)
                    {
                        throw new InvalidOperationException($"Unable to use \"this\" in method that do not have declaring type: {thisExpression}");
                    }
                    if (!canUseThis)
                    {
                        context.ThrowThisIsUnavailable(thisExpression);
                    }

                    currentType = method.DeclaringType;
                    currentMember = method.DeclaringType;
                    previousIsThis = true;
                    currentIsThis = true;
                }
                else if (currentMemberAccess.Target is BaseExpressionNode baseExpression)
                {
                    if (context.CurrentMember == null || !canUseBase)
                    {
                        context.ThrowBaseIsUnavailable(baseExpression);
                    }

                    IDSharpType currentMemberType;

                    if (context.CurrentMember is IDSharpType type)
                    {
                        currentMemberType = type;
                    }
                    else
                    {
                        if (context.CurrentMember.DeclaringType == null)
                        {
                            context.ThrowBaseIsUnavailable(baseExpression);
                        }

                        currentMemberType = context.CurrentMember.DeclaringType;
                    }
                    if (currentMemberType == method.Assembly.ObjectType)
                    {
                        context.ThrowBaseIsUnavailable(baseExpression);
                    }

                    var baseType = currentMemberType.GetBaseTypes().FirstOrDefault(t => t.ObjectType != DSharpObjectType.Interface);
                    currentIsBase = true;
                    currentType = baseType ?? method.Assembly.ObjectType;
                    currentMember = currentType;
                    settings.NextNonVirtualizedAccess = true;
                }
                else if (!previousIsThis && !settings.NextNonVirtualizedAccess &&
                         currentMemberAccess.Target is IdentifierExpressionNode identifier &&
                         identifier.TryGetLocalMember(method, out var localMemberInfo))
                {
                    currentType = localMemberInfo.Value.Type;
                    lastAccessedAsLocalMember = true;
                    accessedAsLocalMember = true;

                    if (localMemberInfo.Type == LocalMemberType.Parameter)
                    {
                        code.LoadArgument(localMemberInfo.Value);
                    }
                    else
                    {
                        code.LoadLocal(localMemberInfo.Value);
                    }
                }
                else
                {
                    var expressionMember = CompileValueExpression(method, currentMemberAccess.Target!, ref settings, previousTarget, context)
                        ?? throw new InvalidOperationException($"Unable to get type of expression: {currentMemberAccess.Target}");

                    if (settings.NextNonVirtualizedAccess &&
                        expressionMember.DeclaringType != context.CurrentMember)
                    {
                        throw new InvalidOperationException($"Unable to find \"{currentMemberAccess.Target}\" at \"{context.CurrentMember}\"");
                    }

                    currentMember = expressionMember;

                    if (expressionMember is not IDSharpType && expressionMember.TryGetReturnType(out var expressionMemberType))
                    {
                        expressionMember = expressionMemberType;
                    }

                    settings.NextNonVirtualizedAccess = false;
                    currentType = expressionMember;
                }

                canUseThis = false;

                if (!currentIsThis)
                {
                    previousIsThis = false;
                }
                if (!currentIsBase)
                {
                    canUseBase = false;
                }
                if (!accessedAsLocalMember)
                {
                    lastAccessedAsLocalMember = false;
                }

                context.CurrentMember = currentType;
                previousTarget = currentMemberAccess.Target;

                if (currentMemberAccess.Member is not MemberAccessExpressionNode nextMemberAccess)
                {
                    break;
                }

                currentMemberAccess = nextMemberAccess;
            }

            if (currentMemberAccess.Member == null)
            {
                throw new InvalidOperationException($"Incomplete expression: {currentMemberAccess}");
            }

            var result = endPointHandler(previousTarget, currentMemberAccess.Member, ref settings, context);
            settings.DoNotCompileEndPointMember = startDoNotCompileEndPointMemberValue;

            if (result != null && result.IsStatic &&
                (currentMember is not IDSharpType ||
                previousTarget is ThisExpressionNode ||
                previousTarget is BaseExpressionNode))
            {
                throw new InvalidOperationException($"Unable to access to static member \"{result}\" throw instance, use full path to access it: {currentMemberAccess.Member}");
            }
            if (result != null && !result.IsStatic &&
                !lastAccessedAsLocalMember &&
                currentMember is IDSharpType &&
                previousTarget is not ThisExpressionNode &&
                previousTarget is not BaseExpressionNode)
            {
                throw new InvalidOperationException($"Unable to access to non static member \"{result}\" without object instance: {currentMemberAccess.Member}");
            }

            return result;
        }
        private IDSharpMemberInfo? CompileValueExpressionOperation(DSharpMethodBuilder method, DSharpBytecodeBuilder code, ExpressionNode expression, Action operation, ref DSharpMethodCompileSettings settings, DSharpCompilerContext context = default, bool popLast = true)
        {
            if (expression is IdentifierExpressionNode identifier)
            {
                var localName = identifier.GetName(false);
                var parameter = method.Parameters.FirstOrDefault(p => p.Name == localName);
                bool success = false;

                if (parameter != null)
                {
                    success = true;
                    code.LoadArgument(parameter);
                    operation();
                    code.StoreArgument(parameter);
                }
                else if (settings.TryGetVariable(localName, out var variable))
                {
                    success = true;
                    code.LoadLocal(variable);
                    operation();
                    code.StoreLocal(variable);
                }

                if (success)
                {
                    if (popLast)
                    {
                        code.Pop();
                    }

                    return null;
                }
            }

            settings.DoNotCompileEndPointMember = true;

            var member = CompileValueExpression(method, expression, ref settings, expression, context)
                ?? throw new ArgumentException($"Unable to find member: {expression}", nameof(expression));

            code.LoadPropertyOrField(member, settings.NextNonVirtualizedAccess);
            settings.NextNonVirtualizedAccess = false;
            operation();
            code.StorePropertyOrField(member, settings.NextNonVirtualizedAccess);
            settings.NextNonVirtualizedAccess = false;

            if (popLast)
            {
                code.Pop();
            }

            return member;
        }

        #endregion

        #region Акцессоры свойств

        private void CompileGetterMethod(DSharpMethodBuilder method, DSharpMethodCompileSettings settings = default)
        {
            if (settings.IdentifiersAsField?.TryGetValue(FieldKeyword, out var field) != true)
            {
                throw new ArgumentException($"Field for store value must be provided", nameof(settings));
            }

            var code = method.GetBytecodeBuilder();
            code.LoadField(field);
            code.Return();
        }
        private void CompileSetterMethod(DSharpMethodBuilder method, DSharpMethodCompileSettings settings = default)
        {
            if (settings.IdentifiersAsField?.TryGetValue(FieldKeyword, out var field) != true)
            {
                throw new ArgumentException($"Field for store value must be provided", nameof(settings));
            }

            var code = method.GetBytecodeBuilder();
            code.LoadArgument(method.Parameters[0]);
            code.StoreField(field);
        }

        #endregion

        #region Дополнительно

        private DSharpMethodBuilderParameter GetVariable(DSharpMethodBuilder method, string name, object type, ExpressionNode? initializerExpression, ref DSharpMethodCompileSettings settings, DSharpCompilerContext context)
        {
            if (settings.TryGetVariable(name, out var result))
            {
                return result;
            }

            settings.LocalVariables ??= [];
            context.ParentExpression = initializerExpression;

            DSharpTypeToken typeToken;

            if (type is TypeInfoNode typeInfoNode)
            {
                typeToken = context.ResolveType(typeInfoNode);
            }
            else if (type is DSharpTypeToken providedTypeToken)
            {
                typeToken = providedTypeToken;
            }
            else if (type is IDSharpType providedType)
            {
                typeToken = _assemblyBuilder.GetTypeToken(providedType);
            }
            else if (type is string typeName)
            {
                typeToken = context.ResolveType(typeName);
            }
            else
            {
                throw new ArgumentException($"Invalid object as type: {type}", nameof(type));
            }

            DSharpMethodBuilderParameter variable = new(method.Assembly)
            {
                Name = name,
                Type = typeToken
            };
            settings.LocalVariables.Add(name, variable);

            var code = method.GetBytecodeBuilder();
            code.LocalVariables.Add(variable);

            return variable;
        }

        #endregion
    }
}

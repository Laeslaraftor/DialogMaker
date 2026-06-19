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
            DSharpCompilerContext context = new(_context, method)
            {
                TypeResolver = code.ExpressionTypeResolver,
                MemberResolver = code.ExpressionMemberResolver,
            };

            CompileStatement(method, body, code, settings, context);
        }

        #region Statements

        private void CompileStatement(DSharpMethodBuilder method, BlockStatementNode blockStatement, DSharpBytecodeBuilder code, DSharpMethodCompileSettings settings = default, DSharpCompilerContext context = default)
        {
            foreach (var statement in blockStatement.Statements)
            {
                CompileStatement(method, statement, code, settings, context);
            }
        }
        private void CompileStatement(DSharpMethodBuilder method, StatementNode statement, DSharpBytecodeBuilder code, DSharpMethodCompileSettings settings = default, DSharpCompilerContext context = default)
        {
            DSharpMethodBuilderParameter GetVariable(VariableNode? node)
            {
                if (node == null)
                {
                    throw new ArgumentNullException("Variable node must be provided", nameof(node));
                }

                return this.GetVariable(method, node, settings);
            }

            if (statement is BlockStatementNode blockStatement)
            {
                CompileStatement(method, blockStatement, code, settings, context);
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

                var variable = GetVariable(variableStatement.Variable);
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
                    CompileValueExpression(method, variableStatement.Variable.Initializer, settings, null, context);
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

                CompileExpression(method, expressionStatement.Expression, settings, null, context);
            }
            else if (statement is IfStatementNode ifStatement)
            {
                if (ifStatement.Condition == null)
                {
                    throw new ArgumentException($"If statement should contains condition: {statement}", nameof(statement));
                }

                CompileValueExpression(method, ifStatement.Condition, settings, null, context);
                var jumpOperation = code.JumpIfFalse();
                code.Pop();

                if (ifStatement.ThenBranch == null)
                {
                    throw new ArgumentException($"If statement should contains then branch: {statement}", nameof(statement));
                }

                CompileStatement(method, ifStatement.ThenBranch, code, settings, context);

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

                    CompileStatement(method, ifStatement.ElseBranch, code, settings, context);
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
                CompileValueExpression(method, whileStatement.Condition, settings, null, context);

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
                    CompileStatement(method, whileStatement.Body, code, settings, context);
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

                CompileStatement(method, forStatement.Initializer, code, settings, context);
                var startExpressionOperation = code.Empty(true);
                CompileValueExpression(method, forStatement.Condition, settings, null, context);
                var skipOperation = code.JumpIfFalse();
                code.Pop();

                int startExpressionIndex = code.Instructions.IndexOf(startExpressionOperation);
                code.Instructions.Remove(startExpressionOperation);
                startExpressionOperation = code.Instructions[startExpressionIndex];

                context.CurrentLoopStartInstruction = startExpressionOperation;
                context.CurrentLoopEndInstruction = code.Empty(true);
                skipOperation.ReferencedInstruction = context.CurrentLoopEndInstruction;
                code.Instructions.Remove(context.CurrentLoopEndInstruction);

                CompileStatement(method, forStatement.Body, code, settings, context);
                CompileExpression(method, forStatement.Increment, settings, null, context);
                code.Jump(startExpressionOperation);
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

                    CompileValueExpression(method, returnStatement.Value, settings, null, context);
                }
                else if (method.ReturnType != null)
                {
                    throw new InvalidOperationException($"Return statement should returns some value in method that returning value: {statement}");
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

        private void CompileExpression(DSharpMethodBuilder method, ExpressionNode expression, DSharpMethodCompileSettings settings = default, ExpressionNode? parentExpression = null, DSharpCompilerContext context = default)
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

                        CompileValueExpression(method, assignExpression.Right, settings, expression, context);
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

                        CompileValueExpression(method, assignExpression.Right, settings, expression, context);
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

                    CompileValueExpression(method, arrayAccess.Array, settings, arrayAccess, context);
                    CompileValueExpression(method, arrayAccess.Index, settings, arrayAccess, context);
                    CompileValueExpression(method, assignExpression.Right, settings, expression, context);

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
                    var member = CompileValueExpression(method, assignExpression.Left, settings, expression, context)
                        ?? throw new ArgumentException($"Unable to find member: {assignExpression.Left}", nameof(expression));

                    CompileValueExpression(method, assignExpression.Right, settings, expression, context);

                    if (assignExpression.Operator != DSharpAssignmentOperator.Assign)
                    {
                        code.LoadPropertyOrField(member);
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

                    code.StorePropertyOrField(member);
                    code.Pop();
                }
            }
            else if (expression is IncrementExpressionNode incrementExpression)
            {
                if (incrementExpression.Expression == null)
                {
                    throw new ArgumentException($"Invalid expression: {expression}", nameof(expression));
                }

                CompileValueExpressionOperation(method, code, incrementExpression.Expression, () => code.Increment(), settings, context);
            }
            else if (expression is DecrementExpressionNode decrementExpression)
            {
                if (decrementExpression.Expression == null)
                {
                    throw new ArgumentException($"Invalid expression: {expression}", nameof(expression));
                }

                CompileValueExpressionOperation(method, code, decrementExpression.Expression, () => code.Decrement(), settings, context);
            }
            else if (expression is CallExpressionNode ||
                     expression is MemberAccessExpressionNode)
            {
                CompileValueExpression(method, expression, settings, parentExpression, context);
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
        private IDSharpMemberInfo? CompileValueExpression(DSharpMethodBuilder method, ExpressionNode expression, DSharpMethodCompileSettings settings = default, ExpressionNode? parentExpression = null, DSharpCompilerContext context = default)
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

                if (!settings.DoNotCompileEndPointMember &&
                    (member is IDSharpFieldInfo || member is IDSharpPropertyInfo))
                {
                    code.LoadPropertyOrField(member);
                }

                return member;
            }
            else if (expression is MemberAccessExpressionNode memberAccessExpression)
            {
                MemberAccessExpressionNode rootExpression = memberAccessExpression;
                var startContextMember = context.CurrentMember;
                var startContextResolver = context.TypeResolver;
                int accessCompileCount = 0;

                do
                {
                    if (rootExpression.Target == null)
                    {
                        throw new ArgumentException($"Target identifier can not be empty when trying to accessing member: {memberAccessExpression}", nameof(expression));
                    }

                    var expressionMember = CompileValueExpression(method, rootExpression.Target, settings, rootExpression, context);

                    if (expressionMember == null)
                    {
                        if (rootExpression.Target is not IdentifierExpressionNode identifier)
                        {
                            throw new InvalidOperationException($"Invalid identifier: {rootExpression.Target}");
                        }
                        if (context.TryResolveType(identifier.GetName(true), out var targetTypeToken))
                        {
                            expressionMember = _assemblyBuilder.GetType(targetTypeToken);
                            accessCompileCount++;
                        }
                        else
                        {
                            throw new InvalidOperationException($"Unable to get type for: {rootExpression.Target}");
                        }
                    }
                    else if (expressionMember.TryGetReturnType(out var returnType))
                    {
                        expressionMember = returnType;
                        accessCompileCount++;
                    }

                    context.CurrentMember = expressionMember;

                    if (rootExpression.Member is MemberAccessExpressionNode accessMember)
                    {
                        rootExpression = accessMember;
                    }
                    else
                    {
                        break;
                    }
                }
                while (true);

                if (rootExpression.Member == null)
                {
                    throw new ArgumentException($"Member must be specified: {memberAccessExpression}", nameof(expression));
                }

                DSharpCompilerContext parentContext = new(context, startContextMember);
                context.TypeResolver = obj =>
                {
                    try
                    {
                        if (parentContext.TryResolveType(obj.ToString(), out var typeToken))
                        {
                            return _assemblyBuilder.GetType(typeToken) as IDSharpType;
                        }
                    }
                    catch
                    {
                    }

                    return startContextResolver?.Invoke(obj);
                };
                var member = CompileValueExpression(method, rootExpression.Member, settings, rootExpression, context);

                if (member != null && member.TryGetReturnType(out _))
                {
                    if (accessCompileCount == 1)
                    {
                        code.PopOffset(1);
                    }
                    else if (accessCompileCount > 1)
                    {
                        code.PopOffsetRepeat(1, accessCompileCount);
                    }
                }

                return member;
            }
            else if (expression is CallExpressionNode callExpression)
            {
                if (callExpression.Callee == null)
                {
                    throw new ArgumentException($"Unable to call method without identifier: {callExpression}", nameof(expression));
                }

                foreach (var arg in callExpression.Arguments)
                {
                    CompileValueExpression(method, arg, settings, callExpression, context);
                }

                int popOffset = 0;
                IDSharpMethodInfo calledMethod;

                void CallAuto(IDSharpMethodInfo method)
                {
                    if (settings.Await(callExpression))
                    {
                        if (method.IsStatic)
                        {
                            code.AwaitCall(method);
                        }
                        else
                        {
                            code.AwaitCallInstance(method);
                        }
                    }
                    else
                    {
                        if (method.IsStatic)
                        {
                            code.Call(method);
                        }
                        else
                        {
                            code.CallInstance(method);
                        }
                    }
                }

                if (callExpression.Callee is IdentifierExpressionNode identifier)
                {
                    calledMethod = context.FindAnyAvailableMember<IDSharpMethodInfo>(identifier.Name);

                    if (calledMethod.ReturnType != null)
                    {
                        popOffset = 1;
                    }

                    CallAuto(calledMethod);
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

                    CompileValueExpression(method, memberMethod.Target, settings, memberMethod, context);
                    CallAuto(calledMethod);
                    code.PopOffset(popOffset);
                }
                else
                {
                    throw new ArgumentException($"Invalid method identifier: {callExpression.Callee}", nameof(expression));
                }

                for (int i = 0; i < callExpression.Arguments.Count; i++)
                {
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

                var array = CompileValueExpression(method, arrayExpression.Array, settings, arrayExpression, context);
                CompileValueExpression(method, arrayExpression.Index, settings, arrayExpression, context);
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

                CompileValueExpression(method, unaryExpression.Operand, settings, expression, context);

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
                void Handle(ExpressionNode left, ExpressionNode right, DSharpBinaryOperator @operator)
                {
                    CompileValueExpression(method, left, settings, binaryExpression, context);
                    CompileValueExpression(method, right, settings, binaryExpression, context);

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

                if (newExpression.Parameters.Count == 0)
                {
                    code.New(type);
                }
                else
                {
                    foreach (var parameter in newExpression.Parameters)
                    {
                        CompileValueExpression(method, parameter, settings, expression, context);
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

                    CompileValueExpression(method, initializer.Right, settings, initializer, context);

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
                    CompileValueExpression(method, newArrayExpression.SizeExpressions[0], settings, expression, context);
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
                    CompileValueExpression(method, item, settings, expression, context);
                    code.StoreArrayItem();
                    code.Pop();
                    code.Increment();
                }
                if (newArrayExpression.ItemsExpressions.Count > 0)
                {
                    code.Pop();
                }

                return type;
            }
            else if (expression is AwaitExpressionNode awaitExpression)
            {
                if (awaitExpression.Expression is not CallExpressionNode calling)
                {
                    throw new ArgumentException($"await appliable only for methods and functions", nameof(expression));
                }

                settings.CallingsToAwait ??= [];
                settings.CallingsToAwait.Add(calling);

                return CompileValueExpression(method, calling, settings, expression, context);
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
            else if (expression is IncrementExpressionNode incrementExpression)
            {
                if (incrementExpression.Expression == null)
                {
                    throw new ArgumentException($"Invalid expression: {expression}", nameof(expression));
                }

                bool popLast = parentExpression == null;

                return CompileValueExpressionOperation(method, code, incrementExpression.Expression, () => code.Increment(), settings, context, popLast);
            }
            else if (expression is DecrementExpressionNode decrementExpression)
            {
                if (decrementExpression.Expression == null)
                {
                    throw new ArgumentException($"Invalid expression: {expression}", nameof(expression));
                }

                bool popLast = parentExpression == null;

                return CompileValueExpressionOperation(method, code, decrementExpression.Expression, () => code.Decrement(), settings, context, popLast);
            }

            throw new ArgumentException($"Unable to compile expression: {expression}", nameof(expression));
        }

        private IDSharpMemberInfo? CompileValueExpressionOperation(DSharpMethodBuilder method, DSharpBytecodeBuilder code, ExpressionNode expression, Action operation, DSharpMethodCompileSettings settings = default, DSharpCompilerContext context = default, bool popLast = true)
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

            var member = CompileValueExpression(method, expression, settings, expression, context)
                ?? throw new ArgumentException($"Unable to find member: {expression}", nameof(expression));

            code.LoadPropertyOrField(member);
            operation();
            code.StorePropertyOrField(member);

            if (popLast)
            {
                code.Pop();
            }

            return member;
        }

        #endregion

        #region Поиск членов


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

        private DSharpMethodBuilderParameter GetVariable(DSharpMethodBuilder method, VariableNode node, DSharpMethodCompileSettings settings)
        {
            if (settings.LocalVariables?.TryGetValue(node, out var result) == true)
            {
                return result;
            }
            if (node.Type == null)
            {
                throw new ArgumentException($"Unknown variable type: {node}", nameof(node));
            }

            settings.LocalVariables ??= [];
            DSharpCompilerContext context = new(_context, method)
            {
                ParentExpression = node.Initializer
            };

            var type = context.ResolveType(node.Type);
            DSharpMethodBuilderParameter variable = new(method.Assembly)
            {
                Name = node.Name,
                Type = type
            };
            settings.LocalVariables.Add(node, variable);

            var code = method.GetBytecodeBuilder();
            code.LocalVariables.Add(variable);

            return variable;
        }

        #endregion
    }
}

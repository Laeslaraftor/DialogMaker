using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Compiler.Scopes;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler
{
    public partial class DSharpScriptCompiler
    {
        #region Methods

        private void CompileMethod(DSharpMethodBuilder method, InvokableNode invokableNode, DSharpMethodCompileSettings settings = default)
        {
            if (invokableNode.Body == null)
            {
                if (method.IsExtern || method.IsAbstract || method.DeclaringType?.ObjectType == DSharpObjectType.Interface)
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
            DSharpCompilerContext context = new(Context)
            {
                CurrentMember = method,
                TypeResolver = code.ExpressionTypeResolver,
                MemberResolver = code.ExpressionMemberResolver,
            };
            context.Scope = GetScope(method);

            if (method.MethodType == DSharpMethodType.Constructor &&
                _createdConstructors.TryGetValue(method, out var node))
            {
                CompileConstructor(method, node, code, context);
            }

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

        private void CompileConstructor(DSharpMethodBuilder method, ConstructorNode node, DSharpBytecodeBuilder code, DSharpCompilerContext context = default)
        {
            if (node.Type == DSharpConstructorType.Default)
            {
                return;
            }
            if (node.Type == DSharpConstructorType.BaseInvocation)
            {
                var baseType = (method.DeclaringType?.BaseTypes.FirstOrDefault(t => t.ObjectType == DSharpObjectType.Class))
                    ?? throw new InvalidOperationException($"Unable to invoke constructor in base class because current type not inherit any types: {node}");
                context = new(context, baseType);
            }

            var constructor = context.FindConstructor(node.ExtraInvokeParameters);

            if (constructor == null)
            {
                return;
            }

            code.LoadInstance();

            if (node.Type == DSharpConstructorType.BaseInvocation)
            {
                code.CallBaseInstance(constructor);
            }
            else
            {
                code.CallInstance(constructor);
            }

            code.Pop();
        }

        /// <summary>
        /// Compile fields initializer. 
        /// It creates initializer method that sets values to fields with initializers.
        /// If no one field or property contains initializer it skip type
        /// </summary>
        /// <param name="type">Type to create initializer</param>
        private void CompileFieldsInitializer(DSharpTypeBuilder type)
        {
            Dictionary<DSharpMemberInfoBuilder, ExpressionNode> initializers = [];
            Dictionary<DSharpMemberInfoBuilder, ExpressionNode> staticInitializers = [];

            void AddMember(DSharpMemberInfoBuilder member, ExpressionNode initializer)
            {
                if (member.IsStatic)
                {
                    staticInitializers.Add(member, initializer);
                    return;
                }

                initializers.Add(member, initializer);
            }

            foreach (var field in type.Fields)
            {
                if (_createdFields.TryGetValue(field, out var node) && 
                    node.Initializer != null)
                {
                    AddMember(field, node.Initializer);
                }
            }
            foreach (var property in type.Properties)
            {
                if (_createdProperties.TryGetValue(property, out var node) && 
                    node.Initializer != null)
                {
                    if (property.Setter == null)
                    {
                        if (!_propertyFields.TryGetValue(property, out var valueField))
                        {
                            throw new InvalidOperationException($"Unable to initialize \"{property}\" property without setter and custom getter: {node}");
                        }

                        AddMember(valueField, node.Initializer);
                        continue;
                    }

                    AddMember(property, node.Initializer);
                }
            }

            void CreateInitializers(Dictionary<DSharpMemberInfoBuilder, ExpressionNode> initializers, bool isStatic)
            {
                DSharpMethodBuilder? initializer = (isStatic ? type.StaticInitializer : type.Initializer) 
                    ?? throw new InvalidOperationException($"Unable to compile fields initializer because it not exists in \"{type}\"");
                var code = initializer.GetBytecodeBuilder();
                DSharpMethodCompileSettings settings = new()
                {
                    LocalVariables = [],
                    AlwaysReturnMethods = [],
                    BannedExpressions = []
                };
                DSharpCompilerContext context = new(Context, type)
                {
                    TypeResolver = code.ExpressionTypeResolver,
                    MemberResolver = code.ExpressionMemberResolver,
                };
                context = new(context, initializer);

                code.LoadInstance();

                foreach (var info in initializers)
                {
                    CompileValueExpression(initializer, info.Value, ref settings, null, context);
                    code.StorePropertyOrField(info.Key);
                    code.Pop();
                }
            }

            if (initializers.Count != 0)
            {
                CreateInitializers(initializers, false);
            }
            if (staticInitializers.Count != 0)
            {
                CreateInitializers(staticInitializers, true);
            }
        }

        #endregion

        #region Statements

        private void CompileStatement(DSharpMethodBuilder method, BlockStatementNode blockStatement, int depth, DSharpBytecodeBuilder code, ref DSharpMethodCompileSettings settings, DSharpCompilerContext context = default)
        {
            if (context.Scope?.Parent == null || depth == 0)
            {
                context.Scope = GetScope(method);
            }
            else
            {
                context.Scope = new DSharpCompilerMethodScope(method, context.Scope);
            }

            foreach (var statement in blockStatement.Statements)
            {
                CompileStatement(method, statement, blockStatement, depth, code, ref settings, context);
            }
        }
        private void CompileStatement(DSharpMethodBuilder method, StatementNode statement, StatementNode? parentStatement, int depth, DSharpBytecodeBuilder code, ref DSharpMethodCompileSettings settings, DSharpCompilerContext context = default)
        {
            IDSharpParameterInfo GetCustomVariable(string name, object type, ref DSharpMethodCompileSettings settings, ExpressionNode? initializer = null)
            {
                return CreateVariable(method, name, type, initializer, ref settings, context);
            }
            IDSharpParameterInfo GetVariable(VariableNode? node, ref DSharpMethodCompileSettings settings)
            {
                if (node == null)
                {
                    throw new ArgumentNullException("Variable node must be provided", nameof(node));
                }
                if (node.Type == null)
                {
                    throw new ArgumentException($"Unknown variable type: {node}", nameof(node));
                }

                return CreateVariable(method, node.Name, node.Type, node.Initializer, ref settings, context);
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

                IDSharpParameterInfo variable;

                try
                {
                    variable = GetVariable(variableStatement.Variable, ref settings);
                }
                catch (Exception error)
                {
                    throw new InvalidOperationException($"Unable to create variable in current scope: {statement}", error);
                }

                var originalTypeResolver = context.TypeResolver;
                var initializer = variableStatement.Variable.Initializer;
                context.TypeResolver = obj =>
                {
                    if ((obj == null || obj is NewExpressionNode newExpression && newExpression.Type == null) &&
                        variable.Type != null)
                    {
                        return variable.Type;
                    }

                    return originalTypeResolver?.Invoke(obj!);
                };

                if (initializer != null)
                {
                    CompileExpressionValueWithRequestedType(method, variable.Type, code, initializer, ref settings, null, context);
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

                settings.LastOperationIsReturnsValue = false;
                IDSharpMemberInfo? expressionMember;

                try
                {
                    expressionMember = CompileExpression(method, expressionStatement.Expression, ref settings, null, context);
                }
                catch (Exception error)
                {
                    throw new InvalidOperationException($"Failed to compile: {expressionStatement.Expression}", error);
                }

                if (depth == 0)
                {
                    if (expressionStatement.Expression is ThrowExpressionNode)
                    {
                        settings.AddReturnMethod(method);
                    }
                    if (parentStatement?.Token.Type == DSharpTokenType.Lambda)
                    {
                        if (settings.LastOperationIsReturnsValue)
                        {
                            settings.AddReturnMethod(method);
                            return;
                        }

                        try
                        {
                            var expressionType = expressionStatement.Expression.GetExpressionType(Assembly, context);

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
            }
            else if (statement is IfStatementNode ifStatement)
            {
                if (ifStatement.Condition == null)
                {
                    throw new ArgumentException($"If statement should contains condition: {statement}", nameof(statement));
                }


                var boolType = Assembly.BoolType;

                CompileExpressionValueWithRequestedType(method, boolType, code, ifStatement.Condition, ref settings, null, context);
                var jumpOperation = code.JumpIfFalse();
                code.Pop();
                code.StartScope();

                if (ifStatement.ThenBranch == null)
                {
                    throw new ArgumentException($"If statement should contains then branch: {statement}", nameof(statement));
                }

                CompileStatement(method, ifStatement.ThenBranch, ifStatement, depth + 1, code, ref settings, context);
                code.EndScope();

                DSharpBytecodeBuilder.ReferenceInstruction? skipOtherOperation = null;

                if (ifStatement.ElseBranch != null)
                {
                    skipOtherOperation = code.Jump();
                }
                else
                {
                    code.SkipNext();
                }

                jumpOperation.ReferencedInstruction = code.Pop();

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

                    code.StartScope();
                    CompileStatement(method, ifStatement.ElseBranch, ifStatement, depth + 1, code, ref settings, context);
                    code.EndScope();
                    code.SkipNext();
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
                    CompileStatement(method, whileStatement.Body, whileStatement, depth + 1, code, ref settings, context);
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

                CompileStatement(method, forStatement.Initializer, forStatement, depth + 1, code, ref settings, context);

                var conditionType = forStatement.Condition.GetExpressionType(Assembly, context) as IDSharpType
                    ?? throw new ArgumentException($"Unable to get condition return type: {statement}", nameof(statement));

                if (!conditionType.IsAssignableTo(Assembly.BoolType))
                {
                    throw new ArgumentException($"Condition should return boolean value, but returns \"{conditionType}\": {statement}", nameof(statement));
                }

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

                CompileStatement(method, forStatement.Body, forStatement, depth + 1, code, ref settings, context);
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
                                                                                m.ReturnType.IsAssignableTo(Assembly.IEnumeratorType.Type));

                if (getEnumeratorMethod == null)
                {
                    throw new ArgumentException($"Unable to find public GetEnumerator method with \"{Assembly.IEnumeratorType}\" as return value in expression of foreach statement: {statement}", nameof(statement));
                }

                var enumerator = DSharpIEnumeratorType.Create(getEnumeratorMethod.ReturnType!);
                IDSharpParameterInfo variable;

                if (foreachStatement.Variable.Type?.Token.Type == DSharpTokenType.Var)
                {
                    variable = GetCustomVariable(foreachStatement.Variable.Name, enumerator.CurrentProperty.PropertyType, ref settings);
                }
                else
                {
                    variable = GetVariable(foreachStatement.Variable, ref settings);

                    if (variable is not DSharpMethodBuilderParameter variableBuilder)
                    {
                        throw new InvalidOperationException($"Unable to create variable for foreach method because immutable variable was returned: {statement}");
                    }

                    if (variable.Type == null)
                    {
                        variableBuilder.Type = Assembly.ObjectToken;
                    }
                    else
                    {
                        if (!enumerator.CurrentProperty.PropertyType.IsAssignableTo(variable.Type))
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
                code.StartScope();
                code.LoadPropertyOrField(enumerator.CurrentProperty);
                code.StoreLocal(variable);
                code.Pop();

                context.CurrentLoopStartInstruction = code.EndScope();
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
                if (context.NowInFinallyBlock)
                {
                    throw new InvalidOperationException($"Returning is unavailable in finally block: {statement}");
                }

                IDSharpType? methodReturnType = null;

                if (method.ReturnType != null)
                {
                    methodReturnType = Assembly.GetType(method.ReturnType) as IDSharpType;
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
                        var originalTypeResolver = context.TypeResolver;
                        context.TypeResolver = obj =>
                        {
                            if (obj == returnStatement.Value &&
                                obj is NewExpressionNode newExpression &&
                                newExpression.Type == null)
                            {
                                return Assembly.GetTypeOrDefault(method.ReturnType) as IDSharpType;
                            }

                            return originalTypeResolver?.Invoke(obj);
                        };

                        expressionType = returnStatement.Value.GetExpressionType(Assembly, context) as IDSharpType
                            ?? throw new ArgumentException($"Unable to get expression type: {returnStatement.Value}", nameof(statement));
                        context.TypeResolver = originalTypeResolver;
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
                            return Assembly.GetType(method.ReturnType) as IDSharpType;
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
            else if (statement is TryStatementNode tryStatement)
            {
                if (tryStatement.TryBlock == null)
                {
                    throw new InvalidOperationException($"Try block should contains body: {statement}");
                }
                if (tryStatement.CatchBlocks.Count == 0)
                {
                    throw new InvalidOperationException($"Try block should have at least 1 catch block: {statement}");
                }
                if (tryStatement.TryBlock.Statements.Count == 0)
                {
                    return;
                }

                Dictionary<TryStatementNode.CatchBlock, DSharpBytecodeBuilder.ReferenceInstruction> catchBlockRegisterInstructions = [];
                DSharpBytecodeBuilder.ReferenceInstruction? finallyRegisterInstruction = null;
                DSharpBytecodeBuilder.Instruction endInstruction = code.StopTrying();
                HashSet<IDSharpType> catchingExceptions = [];
                bool emptyCatchBlockAlreadyCreated = false;

                code.Instructions.Remove(endInstruction);

                DSharpBytecodeBuilder.Instruction CompileBlock(ref DSharpMethodCompileSettings settings, BlockStatementNode block, DSharpCompilerContext context, Action? extra = null, bool endJump = true, int depthOffset = 1)
                {
                    int startIndex = code.Instructions.Count;
                    DSharpBytecodeBuilder.Instruction start = code.StartScope();

                    extra?.Invoke();
                    CompileStatement(method, block, depth + depthOffset, code, ref settings, context);

                    code.EndScope();

                    if (endJump)
                    {
                        if (finallyRegisterInstruction != null)
                        {
                            code.Finally();
                        }

                        code.Jump(endInstruction);
                    }

                    return start;
                }

                code.StartTrying();

                foreach (var catchBlock in tryStatement.CatchBlocks)
                {
                    if (catchBlock.Statements == null)
                    {
                        throw new InvalidOperationException($"Catch block should contains body: {catchBlock}");
                    }

                    DSharpBytecodeBuilder.ReferenceInstruction instruction;

                    if (catchBlock.ExceptionType == null)
                    {
                        if (emptyCatchBlockAlreadyCreated)
                        {
                            throw new InvalidOperationException($"Try/catch/finally can not contains multiple catch blocks without specified exception");
                        }

                        instruction = code.RegisterCatch();
                        emptyCatchBlockAlreadyCreated = true;
                    }
                    else
                    {
                        IDSharpType exceptionType;

                        try
                        {
                            var typeToken = context.ResolveType(catchBlock.ExceptionType);
                            exceptionType = (IDSharpType)Assembly.GetType(typeToken);
                        }
                        catch (Exception error)
                        {
                            throw new InvalidOperationException($"Unable to resolve exception type: {catchBlock.ExceptionType}", error);
                        }

                        if (!exceptionType.IsAssignableTo(Assembly.ExceptionType))
                        {
                            throw new InvalidOperationException($"\"{exceptionType}\" is not inherit exception type: {catchBlock}");
                        }

                        if (!catchingExceptions.Add(exceptionType))
                        {
                            throw new InvalidOperationException($"Exception \"{exceptionType}\" already handling: {catchBlock}");
                        }

                        instruction = code.RegisterTypedCatch(exceptionType);
                    }

                    catchBlockRegisterInstructions.Add(catchBlock, instruction);
                }

                if (tryStatement.FinallyBlock != null)
                {
                    finallyRegisterInstruction = code.RegisterFinally();
                }

                CompileBlock(ref settings, tryStatement.TryBlock, context);

                int catchBlockIndex = 0;

                foreach (var catchInfo in catchBlockRegisterInstructions)
                {
                    Action? extra = null;
                    var catchContext = context;
                    catchContext.NowInCatchBlock = true;
                    IDSharpType? exceptionType = null;

                    if (catchInfo.Key.ExceptionVariableIdentifier != null &&
                        catchInfo.Value is DSharpBytecodeBuilder.TypedReferenceInstruction typedReference)
                    {
                        catchContext.Scope = new DSharpCompilerMethodScope(method, catchContext.Scope);
                        exceptionType = typedReference.Type;
                        var variableName = catchInfo.Key.ExceptionVariableIdentifier.Name;
                        IDSharpParameterInfo exceptionVariable;

                        try
                        {
                            exceptionVariable = catchContext.Scope.CreateVariable(variableName, typedReference.Type);
                        }
                        catch (Exception error)
                        {
                            throw new InvalidOperationException($"Unable to create variable \"{variableName}\" for catch block: {catchInfo.Key}", error);
                        }

                        extra = () =>
                        {
                            code.StoreLocal(exceptionVariable);
                        };
                    }

                    int depthOffset = 1;

                    if (depth == 0 &&
                        (catchInfo.Key.ExceptionType == null ||
                        catchInfo.Key.ExceptionType != null && exceptionType == Assembly.ExceptionType))
                    {
                        depthOffset = 0;
                    }

                    catchInfo.Value.ReferencedInstruction = CompileBlock(ref settings,
                                                                         catchInfo.Key.Statements!,
                                                                         catchContext,
                                                                         extra,
                                                                         finallyRegisterInstruction != null || catchBlockIndex + 1 < catchBlockRegisterInstructions.Count,
                                                                         depthOffset);

                    catchBlockIndex++;
                }

                if (finallyRegisterInstruction != null && tryStatement.FinallyBlock != null)
                {
                    var finallyContext = context;
                    finallyContext.NowInFinallyBlock = true;
                    finallyRegisterInstruction.ReferencedInstruction = CompileBlock(ref settings, tryStatement.FinallyBlock, finallyContext, null, false);
                    code.Return();
                }

                code.Instructions.Add(endInstruction);
            }
            else
            {
                throw new ArgumentException($"Invalid statement in current context: {statement}", nameof(statement));
            }
        }

        #endregion

        #region Expressions

        private IDSharpMemberInfo? CompileExpression(DSharpMethodBuilder method, ExpressionNode expression, ref DSharpMethodCompileSettings settings, ExpressionNode? parentExpression = null, DSharpCompilerContext context = default)
        {
            var code = method.GetBytecodeBuilder();

            if (expression is AssignmentExpressionNode assignExpression)
            {
                if (assignExpression.Left == null || assignExpression.Right == null)
                {
                    throw new ArgumentException($"Incomplete expression: {expression}", nameof(expression));
                }

                void CompileRight(IDSharpType type, ref DSharpMethodCompileSettings settings)
                {
                    CompileExpressionValueWithRequestedType(method, type, code, assignExpression.Right!, ref settings, expression, context);
                }
                DSharpBinaryOperator GetBinaryOperator()
                {
                    return assignExpression.Operator switch
                    {
                        DSharpAssignmentOperator.PlusAssign => DSharpBinaryOperator.Plus,
                        DSharpAssignmentOperator.MinusAssign => DSharpBinaryOperator.Minus,
                        DSharpAssignmentOperator.MultiplyAssign => DSharpBinaryOperator.Multiply,
                        DSharpAssignmentOperator.DivideAssign => DSharpBinaryOperator.Divide,
                        DSharpAssignmentOperator.ModAssign => DSharpBinaryOperator.Mod,
                        DSharpAssignmentOperator.AndAssign => DSharpBinaryOperator.And,
                        DSharpAssignmentOperator.OrAssign => DSharpBinaryOperator.Or,
                        DSharpAssignmentOperator.XorAssign => DSharpBinaryOperator.LogicalXor,
                        _ => throw new InvalidOperationException()
                    };
                }
                void CompileExpression(IDSharpType type, ref DSharpMethodCompileSettings settings)
                {
                    if (assignExpression.Operator == DSharpAssignmentOperator.Assign)
                    {
                        CompileRight(type, ref settings);
                        return;
                    }

                    var binaryOperator = GetBinaryOperator();
                    var resultType = CompileBinaryExpression(method, code, binaryOperator, assignExpression.Left!, assignExpression.Right!, ref settings, assignExpression, context);

                    CastTypes(method, resultType, type, code, assignExpression, context);

                    code.PopOffsetRepeat(1, 2);
                }

                if (assignExpression.Left is IdentifierExpressionNode identifier)
                {
                    var localName = identifier.GetName(false);

                    if (context.TryResolveVariable(localName, out var variable))
                    {
                        CompileExpression(variable.Type, ref settings);

                        code.StoreLocal(variable);
                        code.Pop();
                        return null;
                    }
                }
                if (assignExpression.Left is ArrayAccessExpressionNode arrayAccess)
                {
                    if (arrayAccess.Array == null ||
                        arrayAccess.Arguments == null)
                    {
                        throw new ArgumentException($"Incomplete expression: {arrayAccess}");
                    }

                    var indexer = GetArrayIndexer(method, arrayAccess, context);

                    if (indexer.Setter == null)
                    {
                        throw new InvalidOperationException($"Unable to assign value without setter: {expression}");
                    }

                    var indexerSetterParameters = indexer.Setter.GetParameters();
                    var valueType = indexerSetterParameters[0].Type;

                    CompileValueExpression(method, arrayAccess.Array, ref settings, arrayAccess, context);
                    CompileExpression(indexer.PropertyType, ref settings);

                    for (int i = 0; i < arrayAccess.Arguments.Count; i++)
                    {
                        var requestedType = indexerSetterParameters[i + 1].Type;
                        var arg = arrayAccess.Arguments[i];
                        CompileExpressionValueWithRequestedType(method, requestedType, code, arg, ref settings, arrayAccess, context);
                    }

                    code.LoadPropertyOrField(indexer, settings.NextNonVirtualizedAccess);
                    //code.CallAuto(indexer.Setter);
                    code.PopRepeat(arrayAccess.Arguments.Count + 2);
                }
                else
                {
                    IDSharpMemberInfo? member = CompileEndPointMember(method, code, assignExpression.Left, assignExpression, ref settings, context)
                        ?? throw new ArgumentException($"Unable to find member: {assignExpression.Left}", nameof(expression));

                    if (!member.TryGetReturnType(out var returnType))
                    {
                        throw new InvalidOperationException($"Unable to get value type of \"{member}\": {expression}");
                    }


                    CompileExpression(returnType, ref settings);

                    if (method.MethodType == DSharpMethodType.Constructor &&
                        member is IDSharpPropertyInfo property && !property.CanWrite)
                    {
                        if (property.DeclaringType == null)
                        {
                            throw new InvalidOperationException($"Property must contains declaring type {property}: {expression}");
                        }

                        var propertyField = property.DeclaringType.GetFieldOrDefault($"{property.Name}{ValueFieldNameSuffix}")
                            ?? throw new ArgumentException($"Unable to write value to property \"{property}\" because it have not setter: {expression}", nameof(expression));
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

                    return member;
                }
            }
            else if (expression is IncrementExpressionNode incrementExpression)
            {
                if (incrementExpression.Expression == null)
                {
                    throw new ArgumentException($"Invalid expression: {expression}", nameof(expression));
                }

                return CompileValueExpressionOperation(method, code, incrementExpression.Expression, () => code.Increment(), ref settings, context);
            }
            else if (expression is DecrementExpressionNode decrementExpression)
            {
                if (decrementExpression.Expression == null)
                {
                    throw new ArgumentException($"Invalid expression: {expression}", nameof(expression));
                }

                return CompileValueExpressionOperation(method, code, decrementExpression.Expression, () => code.Decrement(), ref settings, context);
            }
            else if (expression is CallExpressionNode ||
                     expression is AwaitExpressionNode ||
                     expression is MemberAccessExpressionNode ||
                     expression is BinaryExpressionNode ||
                     expression is UnaryExpressionNode ||
                     expression is LiteralExpressionNode ||
                     expression is IdentifierExpressionNode ||
                     expression is ArrayAccessExpressionNode ||
                     expression is ThrowExpressionNode ||
                     expression is NameOfExpressionNode ||
                     expression is TypeOfExpressionNode ||
                     expression is SizeOfExpressionNode ||
                     expression is ThisExpressionNode)
            {
                return CompileValueExpression(method, expression, ref settings, parentExpression, context);
            }
            else
            {
                throw new ArgumentException($"Invalid expression for current context: {expression}", nameof(context));
            }

            return null;
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
                if (context.TryResolveVariable(identifierExpression.Name, out var variable))
                {
                    if (!settings.DoNotCompileEndPointMember)
                    {
                        code.LoadLocal(variable);
                        settings.LastOperationIsReturnsValue = true;
                    }

                    return null;
                }
                var parameter = method.Parameters.FirstOrDefault(p => p.Name == identifierExpression.Name);

                IDSharpMemberInfo member;

                try
                {
                    member = context.FindAnyAvailableMember(identifierExpression);
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
                        settings.LastOperationIsReturnsValue = true;

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
                DSharpMethodCallingInfo calledMethodInfo;
                IDSharpMethodInfo calledMethod;

                try
                {
                    calledMethodInfo = context.FindMethod(callExpression, method);
                    calledMethod = calledMethodInfo.Method;
                }
                catch (Exception error)
                {
                    throw new InvalidOperationException($"Unable to resolve method: {expression}", error);
                }

                context.CurrentMember = method;

                foreach (var arg in callExpression.Arguments)
                {
                    CompileValueExpression(method, arg, ref settings, callExpression, context);
                }

                context.CurrentMember = startCurrentMember;

                int popOffset = 0;
                bool removeInstance = false;

                void CallAuto(IDSharpMethodInfo method, ref DSharpMethodCompileSettings settings)
                {
                    bool awaitMethod = false;

                    if (settings.Await(method))
                    {
                        awaitMethod = true;
                        settings.CallingsToAwait?.Remove(method);
                    }

                    code.CallAuto(calledMethodInfo, awaitMethod, ref settings);
                    settings.NextNonVirtualizedAccess = false;

                    if (method.ReturnType != null)
                    {
                        settings.LastOperationIsReturnsValue = true;
                    }
                }

                if (!calledMethod.IsStatic && method.IsStatic &&
                        parentExpression is not IdentifierExpressionNode &&
                        parentExpression is not MemberAccessExpressionNode)
                {
                    throw new InvalidOperationException($"Unable to call instance method from static method: {expression}");
                }
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
                if (arrayExpression.Arguments == null)
                {
                    throw new ArgumentException($"Array index must be specified: {arrayExpression}", nameof(expression));
                }

                var array = CompileValueExpression(method, arrayExpression.Array, ref settings, arrayExpression, context);

                foreach (var arg in arrayExpression.Arguments)
                {
                    CompileValueExpression(method, arg, ref settings, arrayExpression, context);
                }

                IDSharpIndexerInfo indexer;

                try
                {
                    indexer = GetArrayIndexer(method, arrayExpression, context);
                }
                catch (Exception error)
                {
                    throw new InvalidOperationException($"Unable to find indexer: {arrayExpression}", error);
                }

                if (!indexer.CanRead)
                {
                    throw new InvalidOperationException($"Unable to get value from \"{indexer}\" because it have not getter");
                }

                code.LoadPropertyOrField(indexer, settings.NextNonVirtualizedAccess);
                //code.CallAuto(indexer.Getter);
                code.PopOffsetRepeat(1, 2);
                settings.LastOperationIsReturnsValue = true;

                return indexer.PropertyType;
            }
            else if (expression.TrySimplifyToLiteral(out var literal))
            {
                code.Push(literal);
                settings.LastOperationIsReturnsValue = true;
                return null;
            }
            else if (expression is UnaryExpressionNode unaryExpression)
            {
                if (unaryExpression.Operand == null)
                {
                    throw new ArgumentException($"Incomplete expression: {unaryExpression}", nameof(expression));
                }

                CompileUnaryExpression(method, code, unaryExpression.Operator, unaryExpression.Operand, ref settings, unaryExpression, context);
                return null;
            }
            else if (expression is BinaryExpressionNode binaryExpression)
            {
                CompileBinaryExpression(method, code, binaryExpression, ref settings, parentExpression, context);
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
                    type = (IDSharpType)Assembly.GetType(typeToken);
                }
                if (type.ObjectType == DSharpObjectType.Interface)
                {
                    throw new ArgumentException($"Can not create new instance of interface \"{type}\": {expression}", nameof(expression));
                }
                if (type.IsAbstract)
                {
                    throw new ArgumentException($"Can not create new instance of abstract object \"{type}\": {expression}", nameof(expression));
                }
                if (type.IsStatic)
                {
                    throw new ArgumentException($"Can not create new instance of static object \"{type}\": {expression}", nameof(expression));
                }
                if (type.IsGeneric && !type.GenericAttributes.HasFlag(DSharpGenericTypeAttributes.EmptyConstructor))
                {
                    throw new ArgumentException($"Can not create new instance of generic type \"{type}\" without constructor: {expression}");
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

                    var parameters = context.GetArgumentTypes(newExpression.Parameters);
                    DSharpCompilerContext typeContext = new(context, type);

                    try
                    {
                        var constructor = typeContext.FindConstructor(parameters)
                            ?? throw new ArgumentException($"Unable to find constructor with parameters {newExpression.Parameters.Count} at {type}:{Environment.NewLine} {expression}", nameof(expression));
                        code.New(constructor);
                    }
                    catch (Exception error)
                    {
                        throw new InvalidOperationException($"Unable to find constructor at \"{type}\": {expression}", error);
                    }
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
                    IDSharpMemberInfo? member = type.GetPropertyOrDefault(name);
                    member ??= type.GetFieldOrDefault(name);

                    if (member == null)
                    {
                        throw new ArgumentException($"Unknown member {name} at \"{type}\": {initializer}", nameof(initializer));
                    }

                    CompileValueExpression(method, initializer.Right, ref settings, initializer, context);

                    code.StorePropertyOrField(member);
                    code.Pop();
                }

                settings.LastOperationIsReturnsValue = true;

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

                var type = (IDSharpType)Assembly.GetType(typeToken);
                type = Assembly.CreateArray(type);
                var arrayInfo = DSharpArrayType.Create(type);
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

                for (int i = 0; i < newArrayExpression.ItemsExpressions.Count; i++)
                {
                    var item = newArrayExpression.ItemsExpressions[i];

                    CompileValueExpression(method, item, ref settings, expression, context);
                    code.Push(i);
                    code.StorePropertyOrField(arrayInfo.Indexer);
                    //code.CallAuto(arrayInfo.IndexerSetter);
                    code.PopRepeat(2);
                }

                settings.LastOperationIsReturnsValue = true;

                return Assembly.CreateArray(type);
            }
            else if (expression is AwaitExpressionNode awaitExpression)
            {
                if (awaitExpression.Expression == null)
                {
                    throw new ArgumentException($"Invalid expression: {awaitExpression}", nameof(expression));
                }
                if (!context.TryResolveMember(awaitExpression, out var awaitMember))
                {
                    throw new InvalidOperationException($"Unable to resolve member for awaiting: {awaitExpression}");
                }
                if (awaitMember is not IDSharpMethodInfo methodToAwait)
                {
                    throw new ArgumentException($"await appliable only for methods and functions", nameof(expression));
                }

                settings.CallingsToAwait ??= [];
                settings.CallingsToAwait.Add(methodToAwait);

                return CompileValueExpression(method, awaitExpression.Expression, ref settings, expression, context);
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
                    settings.LastOperationIsReturnsValue = true;
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
                if (context.CurrentMember == Assembly.ObjectType ||
                    context.CurrentMember?.DeclaringType == Assembly.ObjectType)
                {
                    throw new InvalidOperationException($"Unable to access to base type of \"{Assembly.ObjectType}\"");
                }
                if (expression.GetExpressionType(Assembly, context) is not IDSharpType type)
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
            else if (expression is ThrowExpressionNode throwExpression)
            {
                var throwValue = throwExpression.ValueExpression;
                DSharpCompilerContext throwContext = context;

                if (throwValue != null)
                {
                    if (!throwValue.IsNullExpression())
                    {
                        var originalResolver = context.TypeResolver;

                        IDSharpType? TypeResolver(object? expression)
                        {
                            if (expression == null ||
                                (expression is NewExpressionNode newExpression &&
                                newExpression.Type == null &&
                                Equals(expression, throwValue)))
                            {
                                return Assembly.ExceptionType;
                            }

                            return originalResolver?.Invoke(expression!);
                        }

                        throwContext = new(context)
                        {
                            TypeResolver = TypeResolver
                        };

                        if (throwValue.GetExpressionType(Assembly, throwContext) is not IDSharpType valueType)
                        {
                            throw new InvalidOperationException($"Unable to get type of throwing value: {throwValue}");
                        }
                        if (!valueType.IsAssignableTo(Assembly.ExceptionType))
                        {
                            throw new InvalidOperationException($"Throwing value should be \"{Assembly.ExceptionType}\" or inherit it, but got \"{valueType}\": {throwValue}");
                        }
                    }

                    CompileValueExpression(method, throwValue, ref settings, throwExpression, throwContext);
                }
                else if (!context.NowInCatchBlock)
                {
                    throw new InvalidOperationException($"Operator \"throw\" without parameters in unavailable outside of catch block");
                }

                code.Throw();
                settings.LastOperationIsReturnsValue = true;

                return null;
            }
            else if (expression is CastExpressionNode castExpression)
            {
                if (castExpression.Type == null || castExpression.Expression == null)
                {
                    throw new ArgumentException($"Invalid expression: {expression}", nameof(expression));
                }

                var typeToken = context.ResolveType(castExpression.Type);
                var type = (IDSharpType)Assembly.GetType(typeToken);

                return CompileExpressionValueWithRequestedType(method, type, code, castExpression.Expression, ref settings, castExpression, context);
            }
            else if (expression is NameOfExpressionNode nameofExpression)
            {
                var value = nameofExpression.GetValue(method, context);
                code.Push(value);
                return null;
            }
            else if (expression is TypeOfExpressionNode typeOfExpression)
            {
                if (typeOfExpression.Value == null)
                {
                    throw new InvalidOperationException($"Unable to compile typeof expression because type not specified: {expression}");
                }
                IDSharpType type;

                try
                {
                    var typeToken = context.ResolveType(typeOfExpression.Value);
                    type = (IDSharpType)Assembly.GetType(typeToken);
                }
                catch (Exception error)
                {
                    throw new InvalidOperationException($"Unable to compile typeof expression: {expression}", error);
                }

                code.LoadTypeInformation(type);
                code.Call(Assembly.RuntimeHelperType.CreateTypeMethod);
                code.PopOffset(1);

                return null;
            }
            else if (expression is SizeOfExpressionNode sizeOfExpression)
            {
                if (sizeOfExpression.Value == null)
                {
                    throw new InvalidOperationException($"Unable to compile sizeof expression because type not specified: {expression}");
                }

                IDSharpType type;

                try
                {
                    var typeToken = context.ResolveType(sizeOfExpression.Value);
                    type = (IDSharpType)Assembly.GetType(typeToken);
                }
                catch (Exception error)
                {
                    throw new InvalidOperationException($"Unable to compile sizeof expression: {expression}", error);
                }

                if (type.GetSize(true, true) == -1)
                {
                    code.LoadTypeSize(type);
                }
                else
                {
                    code.PushSize(type);
                }

                return null;
            }

            throw new ArgumentException($"Unable to compile expression: {expression}", nameof(expression));
        }
        private IDSharpIndexerInfo GetArrayIndexer(DSharpMethodBuilder method, ArrayAccessExpressionNode arrayExpression, DSharpCompilerContext context)
        {
            if (arrayExpression.Array == null)
            {
                throw new ArgumentException($"Invalid expression: {arrayExpression}");
            }

            IDSharpMemberInfo? arrayType = arrayExpression.Array.GetExpressionType(Assembly, context);

            if (arrayType != null && arrayType.TryGetReturnType(out var memberType))
            {
                arrayType = memberType;
            }
            else if (arrayType is not IDSharpType)
            {
                throw new InvalidOperationException($"Unable to get type of array expression: {arrayExpression}");
            }

            return context.FindIndexer(arrayExpression, method);
        }
        private IDSharpType CompileUnaryExpression(DSharpMethodBuilder method, DSharpBytecodeBuilder code, DSharpUnaryOperator unaryOperator, ExpressionNode expression, ref DSharpMethodCompileSettings settings, ExpressionNode? parentExpression = null, DSharpCompilerContext context = default)
        {
            if (expression.GetExpressionType(Assembly, context) is not IDSharpType expressionType)
            {
                throw new InvalidOperationException($"Unable to compile binary operation. Unable to get type of left expression: {expression}");
            }

            bool compileExpression = true;

            if (!expressionType.CanPerformUnaryOperation(unaryOperator, out var @operator, out var outputType))
            {
                bool throwException = true;

                if (!expressionType.IsNumber() && unaryOperator == DSharpUnaryOperator.Not)
                {
                    var boolType = Assembly.BoolType;

                    try
                    {
                        CompileExpressionValueWithRequestedType(method, boolType, code, expression, ref settings, parentExpression, context);
                        throwException = false;
                        compileExpression = false;
                    }
                    catch
                    {
                    }
                }

                if (throwException)
                {
                    throw new InvalidOperationException($"Unable to perform {unaryOperator} operation with \"{expressionType}\": {expression}");
                }
            }

            if (compileExpression)
            {
                CompileValueExpression(method, expression, ref settings, parentExpression, context);
            }

            if (@operator == null)
            {
                code.UnaryOperation(unaryOperator);
                return expressionType;
            }
            else
            {
                code.CallAuto(@operator.Method);
                code.PopOffset(1);
                return @operator.ReturnType;
            }
        }
        /// <summary>
        /// Compile binary expression. Firstly compiles left expression, then right expression turn.
        /// After all adds binary operator - standard or custom.
        /// This method automatically check expressions return type for logical operators.
        /// If operator was logical OR (||) or logical AND (&&) it skips right expression.
        /// Logical OR skip right expression if left value is <c>True</c>,
        /// logical AND skip right expression if left value is <c>False</c>
        /// </summary>
        /// <param name="method">Method that contains expressions</param>
        /// <param name="code">Bytecode builder for writing code</param>
        /// <param name="binaryExpression">Binary expression not compile</param>
        /// <param name="settings">Method compiling settings</param>
        /// <param name="parentExpression">Parent expression of specified binary expression</param>
        /// <param name="context">Compiling context</param>
        /// <returns>Type that returns by binary expression</returns>
        /// <exception cref="InvalidOperationException">Unable to compile binary operation. Unable to get type of left expression</exception>
        /// <exception cref="InvalidOperationException">Unable to compile binary operation. Unable to get type of right expression</exception>
        /// <exception cref="InvalidOperationException">Unable to perform operation</exception>
        private IDSharpType CompileBinaryExpression(DSharpMethodBuilder method, DSharpBytecodeBuilder code, BinaryExpressionNode binaryExpression, ref DSharpMethodCompileSettings settings, ExpressionNode? parentExpression = null, DSharpCompilerContext context = default)
        {
            var settingsValue = settings;

            Dictionary<ExpressionNode, IDSharpType> expressionTypes = [];
            IDSharpType? lastType = null;

            void Handle(DSharpBinaryExpressionSide left, DSharpBinaryExpressionSide right, DSharpBinaryOperator @operator, ref DSharpMethodCompileSettings settings)
            {
                IDSharpType type;

                if (expressionTypes.TryGetValue(left.Value, out var leftType) &&
                    expressionTypes.TryGetValue(right.Value, out var rightType))
                {
                    type = CompileBinaryExpression(method, code, @operator, leftType, rightType, ref settings, binaryExpression, context);
                }
                else
                {
                    type = CompileBinaryExpression(method, code, @operator, left.Value, right.Value, ref settings, binaryExpression, context);
                }

                expressionTypes.TryAdd(left.Value, type);
                expressionTypes.TryAdd(right.Value, type);

                if (left.Parent != null)
                {
                    expressionTypes.TryAdd(left.Parent, type);
                }
                if (right.Parent != null)
                {
                    expressionTypes.TryAdd(right.Value, type);
                }

                lastType = type;
            }

            binaryExpression.CompileExpression(Handle, ref settings);

            settings.LastOperationIsReturnsValue = true;

            return lastType!;
        }
        /// <summary>
        /// Compile binary expression. Firstly compiles left expression, then right expression turn.
        /// After all adds binary operator - standard or custom.
        /// This method automatically check expressions return type for logical operators.
        /// If operator was logical OR (||) or logical AND (&&) it skips right expression.
        /// Logical OR skip right expression if left value is <c>True</c>,
        /// logical AND skip right expression if left value is <c>False</c>
        /// </summary>
        /// <param name="method">Method that contains expressions</param>
        /// <param name="code">Bytecode builder for writing code</param>
        /// <param name="binaryOperator">Binary operator for compiling</param>
        /// <param name="left">Left expression of operation</param>
        /// <param name="right">Right expression of operation</param>
        /// <param name="settings">Method compiling settings</param>
        /// <param name="parentExpression">Parent expression of specified binary expression</param>
        /// <param name="context">Compiling context</param>
        /// <returns>Type that returns by binary expression</returns>
        /// <exception cref="InvalidOperationException">Unable to compile binary operation. Unable to get type of left expression</exception>
        /// <exception cref="InvalidOperationException">Unable to compile binary operation. Unable to get type of right expression</exception>
        /// <exception cref="InvalidOperationException">Unable to perform operation</exception>
        private IDSharpType CompileBinaryExpression(DSharpMethodBuilder method, DSharpBytecodeBuilder code, DSharpBinaryOperator binaryOperator, ExpressionNode left, ExpressionNode right, ref DSharpMethodCompileSettings settings, ExpressionNode? parentExpression = null, DSharpCompilerContext context = default)
        {
            if (left.GetExpressionType(Assembly, context) is not IDSharpType leftType)
            {
                throw new InvalidOperationException($"Unable to compile binary operation. Unable to get type of left expression: {left}");
            }

            var originalTypeResolver = context.TypeResolver;
            context.TypeResolver = obj =>
            {
                if (obj == null || obj == right)
                {
                    return leftType;
                }

                return originalTypeResolver?.Invoke(obj);
            };

            if (right.GetExpressionType(Assembly, context) is not IDSharpType rightType)
            {
                throw new InvalidOperationException($"Unable to compile binary operation. Unable to get type of right expression: {right}");
            }

            void CompileExpression(ExpressionNode expression, ref DSharpMethodCompileSettings settings)
            {
                CompileValueExpression(method, expression, ref settings, parentExpression, context);
            }

            CompileExpression(left, ref settings);

            DSharpBytecodeBuilder.ReferenceInstruction? skipInstruction = null;

            if (binaryOperator == DSharpBinaryOperator.LogicalAnd)
            {
                skipInstruction = code.JumpIfFalse();
            }
            else if (binaryOperator == DSharpBinaryOperator.LogicalOr)
            {
                skipInstruction = code.JumpIfTrue();
            }

            CompileExpression(right, ref settings);

            IDSharpType resultType;

            try
            {
                resultType = CompileBinaryExpression(method, code, binaryOperator, leftType, rightType, ref settings, parentExpression, context);
            }
            catch (Exception error)
            {
                throw new InvalidOperationException($"Unable to compile {binaryOperator} binary operation. Left: {left}. Right: {right}", error);
            }

            skipInstruction?.ReferencedInstruction = code.Empty();

            return resultType;
        }
        private IDSharpType CompileBinaryExpression(DSharpMethodBuilder method, DSharpBytecodeBuilder code, DSharpBinaryOperator binaryOperator, IDSharpType leftType, IDSharpType rightType, ref DSharpMethodCompileSettings settings, ExpressionNode? parentExpression = null, DSharpCompilerContext context = default)
        {
            if (!leftType.CanPerformBinaryOperation(rightType, binaryOperator, out var @operator))
            {
                throw new InvalidOperationException($"Unable to perform {binaryOperator} operation with \"{leftType}\" and \"{rightType}\"");
            }

            IDSharpType GetResultType(IDSharpType operatorOutputType)
            {
                if (binaryOperator.IsLogical() &&
                    operatorOutputType != Assembly.BoolType)
                {
                    CastTypes(method, operatorOutputType, Assembly.BoolType, code, parentExpression, context);
                    return Assembly.BoolType;
                }

                return operatorOutputType;
            }

            if (@operator == null)
            {
                code.BinaryOperation(binaryOperator);
                code.PopPreviousTwo();

                if (binaryOperator.IsLogical())
                {
                    return Assembly.BoolType;
                }

                return leftType;
            }
            else
            {
                code.CallAuto(@operator.Method);
                code.PopPreviousTwo();
            }

            return GetResultType(@operator.ReturnType);
        }
        private IDSharpMemberInfo? CompileExpressionValueWithRequestedType(DSharpMethodBuilder method, IDSharpType requestedType, DSharpBytecodeBuilder code, ExpressionNode expression, ref DSharpMethodCompileSettings settings, ExpressionNode? parentExpression = null, DSharpCompilerContext context = default)
        {
            var originalTypeResolver = context.TypeResolver;
            context.TypeResolver = obj =>
            {
                if (obj == null || obj == expression)
                {
                    return requestedType;
                }

                return originalTypeResolver?.Invoke(obj);
            };

            if (expression.GetExpressionType(Assembly, context) is not IDSharpType expressionType)
            {
                throw new InvalidOperationException($"Unable to get type of expression: {expression}");
            }

            var result = CompileValueExpression(method, expression, ref settings, parentExpression, context);

            try
            {
                CastTypes(method, expressionType, requestedType, code, parentExpression, context);
            }
            catch (Exception error)
            {
                throw new InvalidOperationException($"Unable to cast \"{expressionType}\" to \"{requestedType}\": {expression}", error);
            }

            return result;
        }
        private void CastTypes(DSharpMethodBuilder method, IDSharpType targetType, IDSharpType requestedType, DSharpBytecodeBuilder code, ExpressionNode? parentExpression = null, DSharpCompilerContext context = default)
        {
            if (requestedType == targetType)
            {
                return;
            }

            var castAvailability = targetType.CanCastTo(requestedType, out var @operator);

            if (castAvailability == DSharpCastAvailability.No)
            {
                throw new InvalidOperationException($"Unable to cast \"{targetType}\" to \"{requestedType}\"");
            }
            if (@operator == null)
            {
                code.Cast(requestedType);
            }
            else
            {
                if (castAvailability == DSharpCastAvailability.Explicit && parentExpression is not CastExpressionNode)
                {
                    throw new InvalidOperationException($"An explicit cast is required. Current value type is \"{targetType}\", but requires \"{requestedType}\"");
                }

                code.CallAuto(@operator.Method);
                code.PopOffset(1);
            }
        }
        private IDSharpMemberInfo? CompileMemberAccessExpression(DSharpMethodBuilder method, MemberAccessExpressionNode memberAccessExpression, MemberAccessExpressionEndPointHandler endPointHandler, ref DSharpMethodCompileSettings settings, DSharpCompilerContext context = default)
        {
            var startDoNotCompileEndPointMemberValue = settings.DoNotCompileEndPointMember;
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
                    if (!context.CurrentMember.TryGetBase(out var baseType))
                    {
                        context.ThrowBaseIsUnavailable(baseExpression);
                    }

                    currentIsBase = true;
                    currentType = baseType ?? method.Assembly.ObjectType;
                    currentMember = currentType;
                    settings.NextNonVirtualizedAccess = true;
                }
                else if (!previousIsThis && !settings.NextNonVirtualizedAccess &&
                         currentMemberAccess.Target is IdentifierExpressionNode identifier &&
                         identifier.TryGetLocalMember(context, out var localMember))
                {
                    currentType = localMember.Type;
                    lastAccessedAsLocalMember = true;
                    accessedAsLocalMember = true;

                    code.LoadLocal(localMember);
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

                context = new(context, currentType);
                //context.CurrentMember = currentType;
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
                previousTarget is not BaseExpressionNode &&
                previousTarget is not ArrayAccessExpressionNode)
            {
                throw new InvalidOperationException($"Unable to access to non static member \"{result}\" without object instance: {currentMemberAccess.Member}");
            }

            return result;
        }
        private IDSharpMemberInfo? CompileEndPointMember(DSharpMethodBuilder method, DSharpBytecodeBuilder code, ExpressionNode expression, ExpressionNode? parentExpression, ref DSharpMethodCompileSettings settings, DSharpCompilerContext context)
        {
            bool startDoNotCompileEndPointMember = settings.DoNotCompileEndPointMember;
            settings.DoNotCompileEndPointMember = true;
            IDSharpMemberInfo? member;

            if (expression is MemberAccessExpressionNode leftMemberAccess)
            {
                member = CompileMemberAccessExpression(method, leftMemberAccess, (p, e, ref s, c) =>
                {
                    c.ParentExpression = p;

                    if (p is ThisExpressionNode ||
                        p is BaseExpressionNode)
                    {
                        code.LoadInstance();
                    }

                    try
                    {
                        if (c.TryResolveMember(e, out var resolvedMember))
                        {
                            return resolvedMember;
                        }
                    }
                    catch (Exception error)
                    {
                        throw new InvalidOperationException($"Unable to resolve member: {e}", error);
                    }

                    throw new InvalidOperationException($"Unable to resolve member: {e}");
                }, ref settings, context);
            }
            else
            {
                member = CompileValueExpression(method, expression, ref settings, parentExpression, context);
            }

            settings.DoNotCompileEndPointMember = startDoNotCompileEndPointMember;

            return member;
        }
        private IDSharpMemberInfo? CompileValueExpressionOperation(DSharpMethodBuilder method, DSharpBytecodeBuilder code, ExpressionNode expression, Action operation, ref DSharpMethodCompileSettings settings, DSharpCompilerContext context = default, bool popLast = true)
        {
            if (expression is IdentifierExpressionNode identifier)
            {
                var localName = identifier.GetName(false);
                bool success = false;

                if (context.TryResolveVariable(localName, out var variable))
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
                    else
                    {
                        settings.LastOperationIsReturnsValue = true;
                    }

                    return null;
                }
            }

            var member = CompileEndPointMember(method, code, expression, null, ref settings, context)
                ?? throw new ArgumentException($"Unable to find member: {expression}", nameof(expression));

            code.LoadPropertyOrField(member, settings.NextNonVirtualizedAccess);
            settings.NextNonVirtualizedAccess = false;
            operation();
            code.StorePropertyOrField(member, settings.NextNonVirtualizedAccess);
            settings.NextNonVirtualizedAccess = false;

            if (popLast)
            {
                if (member.IsStatic)
                {
                    code.Pop();
                }
                else
                {
                    code.PopRepeat(2);
                }
            }
            else if (member.IsStatic)
            {
                code.PopOffset(1);
            }

            return member;
        }

        #endregion

        #region Accessors

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
            code.LoadLocal(method.Parameters[0]);
            code.StoreField(field);
        }

        #endregion

        #region Variables

        private IDSharpParameterInfo CreateVariable(DSharpMethodBuilder method, string name, object type, ExpressionNode? initializerExpression, ref DSharpMethodCompileSettings settings, DSharpCompilerContext context)
        {
            context.ParentExpression = initializerExpression;

            IDSharpType variableType;

            if (type is TypeInfoNode typeInfoNode)
            {
                variableType = (IDSharpType)Assembly.GetType(context.ResolveType(typeInfoNode));
            }
            else if (type is DSharpTypeToken providedTypeToken)
            {
                variableType = (IDSharpType)Assembly.GetType(providedTypeToken);
            }
            else if (type is IDSharpType providedType)
            {
                variableType = providedType;
            }
            else if (type is string typeName)
            {
                variableType = (IDSharpType)Assembly.GetType(context.ResolveType(typeName));
            }
            else
            {
                throw new ArgumentException($"Invalid object as type: {type}", nameof(type));
            }

            if (initializerExpression != null)
            {
                var originalTypeResolver = context.TypeResolver;
                context.TypeResolver = obj =>
                {
                    if (obj == initializerExpression)
                    {
                        return variableType;
                    }

                    return originalTypeResolver?.Invoke(obj);
                };
                var expressionValue = initializerExpression.GetExpressionType(Assembly, context);
                context.TypeResolver = originalTypeResolver;

                if (expressionValue == null || expressionValue is not IDSharpType valueType)
                {
                    throw new InvalidOperationException($"Unable to get type of expression: {expressionValue}");
                }
                if (!valueType.IsAssignableTo(variableType))
                {
                    throw new InvalidOperationException($"Unable to assign value with type \"{valueType}\" to variable with \"{variableType}\" type: {initializerExpression}");
                }
            }

            if (context.Scope == null)
            {
                throw new InvalidOperationException($"Unable to create variable \"{name}\" without scope at \"{method}\"");
            }
            if (context.Scope.TryCreateVariable(name, variableType, out var result))
            {
                return result;
            }

            throw new InvalidOperationException($"Unable to create variable \"{name}\" at \"{method}\"");
        }

        #endregion
    }
}

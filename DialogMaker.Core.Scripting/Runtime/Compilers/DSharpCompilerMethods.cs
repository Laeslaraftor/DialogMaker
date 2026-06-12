using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Runtime.Builders;

namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    public partial class DSharpCompiler
    {
        private void CompileMethod(DSharpMethodBuilder method, InvokableNode invokableNode, DSharpMethodCompileSettings settings = default)
        {
            if (invokableNode.Body == null)
            {
                if (method.IsExtern)
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

            DSharpMethodBuilderParameter GetVariable(VariableNode? node)
            {
                if (node == null)
                {
                    throw new ArgumentNullException("Variable node must be provided", nameof(node));
                }

                return this.GetVariable(method, node, settings);
            }

            foreach (var statement in body.Statements)
            {
                if (statement is VariableStatementNode variableStatement)
                {
                    var variable = GetVariable(variableStatement.Variable);

                    if (variableStatement.Variable!.Initializer != null)
                    {
                        CompileValueExpression(method, variableStatement.Variable.Initializer, settings);
                        code.StoreLocal(variable);
                        code.Pop();
                    }
                }
            }
        }

        #region Выражения

        private void CompileExpression(DSharpMethodBuilder method, ExpressionNode expression, DSharpMethodCompileSettings settings = default)
        {
            var code = method.GetBytecodeBuilder();

            if (expression is AssignmentExpressionNode assignExpression)
            {
                if (assignExpression.Left is not IdentifierExpressionNode identifierExpression)
                {
                    throw new ArgumentException($"Assignment available only for variables, properties or fields");
                }
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
        private void CompileValueExpression(DSharpMethodBuilder method, ExpressionNode expression, DSharpMethodCompileSettings settings = default, ExpressionNode? parentExpression = null)
        {
            var code = method.GetBytecodeBuilder();
            DSharpCompilerContext context = new()
            {
                Assembly = method.Assembly,
                Usings = _usings,
                TypeResolver = code.ExpressionTypeResolver,
                CurrentMember = method
            };

            if (expression is IdentifierExpressionNode identifierExpression)
            {
                var parameter = method.Parameters.FirstOrDefault(p => p.Name == identifierExpression.Name);

                if (parameter != null)
                {
                    code.LoadArgument(parameter);
                    return;
                }
                if (settings.TryGetVariable(identifierExpression.Name, out var variable))
                {
                    code.LoadLocal(variable);
                    return;
                }

                var member = context.FindAnyAvailableMember(identifierExpression.Name);
                // Надо как то сделать получение экземпляра при наличии и необходимости

                if (member is IDSharpMethodInfo)
                {
                    throw new ArgumentException($"Unable to use method as value, it purposed to call: {expression}", nameof(expression));
                }
                else if (member is IDSharpPropertyInfo property)
                {
                    if (!property.CanRead)
                    {
                        throw new InvalidOperationException($"Unable to read property {property.Name} because has not contains getter");
                    }

                    code.LoadProperty(property);
                    return;
                }
                else if (member is IDSharpFieldInfo field)
                {
                    code.LoadField(field);
                    return;
                }
                else
                {
                    throw new ArgumentException($"Invalid member {member} at expression: {expression}", nameof(expression));
                }
            }
            else if (expression is MemberAccessExpressionNode memberAccessExpression)
            {
                if (memberAccessExpression.Target == null)
                {
                    throw new ArgumentException($"Target identifier can not be empty when trying to accessing member: {memberAccessExpression}", nameof(expression));
                }
                if (memberAccessExpression.Member == null)
                {
                    throw new ArgumentException($"Member must be specified: {memberAccessExpression}", nameof(expression));
                }
                if (context.TryResolveMember(memberAccessExpression, out var member))
                {
                    Console.WriteLine("Надо как то сделать получение экземпляра при наличии");
                    return;
                }

                throw new ArgumentException($"Unable to handle member access: {expression}", nameof(expression));
            }
            else if (expression is CallExpressionNode callExpression)
            {
                if (callExpression.Callee == null)
                {
                    throw new ArgumentException($"Unable to call method without identifier: {callExpression}", nameof(expression));
                }

                foreach (var arg in callExpression.Arguments)
                {
                    CompileValueExpression(method, arg, settings);
                }

                int popOffset = 0;

                if (callExpression.Callee is IdentifierExpressionNode identifier)
                {
                    var callingMethod = context.FindAnyAvailableMember<IDSharpMethodInfo>(identifier.Name);

                    if (callingMethod.ReturnType != null)
                    {
                        popOffset = 1;
                    }
                    if (settings.Await(callExpression))
                    {
                        code.AwaitCall(callingMethod);
                    }
                    else
                    {
                        code.Call(callingMethod);
                    }
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

                    var callingMethod = context.FindAnyAvailableMember<IDSharpMethodInfo>(methodIdentifier.Name);

                    if (callingMethod.ReturnType != null)
                    {
                        popOffset = 1;
                    }

                    CompileExpression(method, memberMethod.Target, settings);

                    if (settings.Await(callExpression))
                    {
                        code.AwaitCallInstance(callingMethod);
                    }
                    else
                    {
                        code.CallInstance(callingMethod);
                    }

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

                CompileValueExpression(method, arrayExpression.Array, settings);
                CompileValueExpression(method, arrayExpression.Index, settings);
                code.LoadArrayItem();
                code.PopOffset(1);
                code.PopOffset(1);
            }
            else if (expression is UnaryExpressionNode unaryExpression)
            {

            }
            else if (expression is BinaryExpressionNode binaryExpression)
            {

            }
            else if (expression is NewInstanceExpressionNode newExpression)
            {
                TypeInfoNode? typeInfo = newExpression.Type;

                // надо сделать поиск необходимого типа при new() (typeInfo == null)

                if (typeInfo == null)
                {
                    throw new ArgumentException($"Can not create new instance when type not specified: {newExpression}", nameof(expression));
                }

                var type = ResolveType(method, typeInfo);

                foreach (var parameter in newExpression.Parameters)
                {
                    CompileValueExpression(method, parameter, settings, expression);
                }

                code.Push(type);
                code.New();

                for (int i = 1; i < newExpression.Parameters.Count + 2; i++)
                {
                    code.PopOffset(i);
                }

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
                }
            }
            else if (expression is NewArrayExpressionNode newArrayExpression)
            {

            }
            else if (expression is AwaitExpressionNode awaitExpression)
            {
                if (awaitExpression.Expression is not CallExpressionNode calling)
                {
                    throw new ArgumentException($"await appliable only for methods and functions", nameof(expression));
                }

                settings.CallingsToAwait ??= [];
                settings.CallingsToAwait.Add(calling);

                CompileValueExpression(method, calling, settings);
            }
            else if (expression is LiteralExpressionNode literalExpression)
            {
                code.Push(literalExpression.Value);
            }
            else
            {
                throw new ArgumentException($"Expression must be identifier or member access: {expression}", nameof(expression));
            }
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
            var type = ResolveType(method, node.Type);
            DSharpMethodBuilderParameter variable = new(method.Assembly)
            {
                Name = node.Name,
                Type = type
            };
            settings.LocalVariables.Add(node, variable);

            return variable;
        }

        #endregion
    }
}

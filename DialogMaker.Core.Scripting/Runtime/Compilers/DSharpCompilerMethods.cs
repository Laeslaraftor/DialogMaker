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
        private void CompileValueExpression(DSharpMethodBuilder method, ExpressionNode expression, DSharpMethodCompileSettings settings = default)
        {
            var code = method.GetBytecodeBuilder();

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

                var member = FindAnyAvailableMember(method.DeclaringType, identifierExpression.Name);

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
                    throw new ArgumentException($"Invalid member: {member}", nameof(expression));
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

                CompileValueExpression(method, memberAccessExpression.Target, settings);
                CompileValueExpression(method, memberAccessExpression.Member, settings);
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
                    var callingMethod = FindAnyAvailableMember<IDSharpMethodInfo>(method.DeclaringType, identifier.Name);

                    if (callingMethod.ReturnType != null)
                    {
                        popOffset = 1;
                    }
                    if (settings.Await(callExpression))
                    {
                        code.Await();
                    }

                    code.Call(callingMethod);
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

                    var callingMethod = FindAnyAvailableMember<IDSharpMethodInfo>(method.DeclaringType, methodIdentifier.Name);

                    if (callingMethod.ReturnType != null)
                    {
                        popOffset = 1;
                    }

                    CompileExpression(method, memberMethod.Target, settings);

                    if (settings.Await(callExpression))
                    {
                        code.Await();
                    }

                    code.CallInstance(callingMethod);
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

        private T FindAnyAvailableMember<T>(DSharpTypeBuilder? type, string name)
            where T : IDSharpMemberInfo
        {
            var member = FindAnyAvailableMember(type, name);

            if (member is not T typedMember)
            {
                throw new ArgumentException($"Unknown member: {name}", nameof(name));
            }

            return typedMember;
        }
        private IDSharpMemberInfo FindAnyAvailableMember(DSharpTypeBuilder? type, string name)
        {
            if (type == null)
            {
                var variable = _assemblyBuilder.GlobalVariables.FirstOrDefault(f => f.Name == name);

                if (variable != null)
                {
                    return variable;
                }

                var function = _assemblyBuilder.GlobalFunctions.FirstOrDefault(f => f.Name == name);

                if (function != null)
                {
                    return function;
                }

                throw new ArgumentException($"Unknown member: {name}", nameof(name));
            }

            IDSharpMemberInfo? FindMember(IDSharpType type, Predicate<IDSharpMemberInfo> predicate)
            {
                var property = type.GetPropertyOrDefault(predicate);

                if (property != null)
                {
                    return property;
                }

                var field = type.GetFieldOrDefault(predicate);

                if (field != null)
                {
                    return field;
                }

                var method = type.GetMethodOrDefault(predicate);

                if (method != null)
                {
                    return method;
                }

                return null;
            }
            IDSharpMemberInfo? FindBaseTypeMember(IDSharpType type, Predicate<IDSharpMemberInfo> predicate)
            {
                foreach (var baseType in type.GetBaseTypes())
                {
                    var member = FindMember(baseType, predicate);

                    if (member != null)
                    {
                        return member;
                    }

                    member = FindBaseTypeMember(baseType, predicate);

                    if (member != null)
                    {
                        return member;
                    }
                }

                return null;
            }
            IDSharpMemberInfo? FindInDeclaringType(IDSharpType type)
            {
                if (type.DeclaringType == null)
                {
                    return null;
                }

                type = type.DeclaringType;

                var member = FindMember(type, m => m.Name == name && m.IsStatic);

                if (member != null)
                {
                    return member;
                }

                return FindInDeclaringType(type);
            }

            var currentTypeMember = FindMember(type, m => m.Name == name);

            if (currentTypeMember != null)
            {
                return currentTypeMember;
            }

            var inheritMember = FindBaseTypeMember(type, m => m.Name == name && m.Access == DSharpAccessModifier.Public ||
                                                                                m.Access == DSharpAccessModifier.Protected);
            inheritMember ??= FindInDeclaringType(type);

            if (inheritMember != null)
            {
                return inheritMember;
            }

            throw new ArgumentException($"Unknown member: {name}", nameof(name));
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
            DSharpMethodBuilderParameter variable = new()
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

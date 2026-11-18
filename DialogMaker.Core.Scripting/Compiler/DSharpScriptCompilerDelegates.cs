using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler
{
    public partial class DSharpScriptCompiler
    {
        private DSharpArrayType InvocationListArrayInfo
        {
            get
            {
                field ??= DSharpArrayType.Create(Assembly.DelegateTypeInfo.InvocationListField.FieldType);
                return field;
            }
        }

        #region Creating

        private DSharpTypeBuilder? CreateDelegate(ObjectDeclarationNode declaration, DSharpTypeBuilder? parent)
        {
            if (declaration.Identifier == null)
            {
                throw new DSharpCompilerException("Empty identifier of object", declaration);
            }
            if (declaration.PrimaryConstructorParameters == null)
            {
                throw new DSharpCompilerException("Invalid delegate: parameters should exists", declaration);
            }

            var identifier = declaration.Identifier;
            var existedType = parent?.ChildrenTypes.FirstOrDefault(c => c.Name == identifier.Name)
                ?? Assembly.GetTypeOrDefault(_currentNamespace != null ? $"{_currentNamespace}.{identifier.GetName(true)}" : identifier.GetName(true));

            if (existedType?.GetGenericTypes().Length == declaration.Identifier.GenericParameters.Count)
            {
                throw new DSharpCompilerException($"Delegate with same name and generic parameters already exists in \"{GetNamespaceName()}\" namespace", declaration);
            }

            var type = Assembly.CreateType(identifier.Name, parent);
            type.IsSealed = true;
            CreateGenerics(type, identifier.GenericParameters);

            if (!_createdDelegates.TryGetValue(type, out var description))
            {
                var instanceConstructor = type.CreateConstructor();
                instanceConstructor.Access = DSharpAccessModifier.Public;
                instanceConstructor.Parameters.Add(new(Assembly)
                {
                    Name = "target",
                    TypeGetter = () => Assembly.ObjectToken
                });
                instanceConstructor.Parameters.Add(new(Assembly)
                {
                    Name = "runtimeMethodInfo",
                    TypeGetter = () => Assembly.NIntToken
                });
                var staticConstructor = type.CreateConstructor();
                staticConstructor.Access = DSharpAccessModifier.Public;
                staticConstructor.Parameters.Add(new(Assembly)
                {
                    Name = "runtimeMethodInfo",
                    TypeGetter = () => Assembly.NIntToken
                });
                var capacityConstructor = type.CreateConstructor();
                capacityConstructor.Access = DSharpAccessModifier.Protected;
                capacityConstructor.Parameters.Add(new(Assembly)
                {
                    Name = "capacity",
                    TypeGetter = () => Assembly.Int32Token
                });

                var createNewMethod = type.CreateMethod(DSharpDelegateType.CreateNewMethodName);
                createNewMethod.Access = DSharpAccessModifier.Protected;
                createNewMethod.Parameters.Add(new(Assembly)
                {
                    Name = "capacity",
                    TypeGetter = () => Assembly.Int32Token
                });

                description = new(type, instanceConstructor, staticConstructor, capacityConstructor, createNewMethod);

                _createdDelegates.Add(type, description);
            }

            var invokeMethod = type.CreateMethod(DSharpCustomDelegateType.InvokeMethodName);
            invokeMethod.Access = DSharpAccessModifier.Public;

            description.InvokeMethods.Add(invokeMethod, declaration);

            return type;
        }

        #endregion

        #region Compiling

        private void CompileDelegate(DSharpCompilerDelegateDescription description)
        {
            var methodDelegateType = Assembly.DelegateTypeInfo;

            if (!description.Type.BaseTypes.Contains(methodDelegateType.Type))
            {
                description.Type.AddBaseType(methodDelegateType.Type);
            }

            CompileDelegateStaticConstructor(description, description.StaticConstructor, methodDelegateType.StaticConstructor);
            CompileDelegateInstanceConstructor(description, description.InstanceConstructor, methodDelegateType.InstanceConstructor);
            CompileDelegateStaticConstructor(description, description.CapacityConstructor, methodDelegateType.CapacityConstructor);
            CompileDelegateCreateNewMethod(description);

            foreach (var info in description.InvokeMethods)
            {
                CompileDelegateInvokeMethod(description, info.Key);
            }
        }

        private void CompileDelegateStaticConstructor(DSharpCompilerDelegateDescription description, DSharpMethodBuilder constructor, IDSharpMethodInfo baseConstructor)
        {
            var code = constructor.GetBytecodeBuilder();
            code.LoadInstance();
            code.LoadLocal(constructor.Parameters[0]);
            code.Call(baseConstructor, DSharpMethodCallingType.Default);
        }
        private void CompileDelegateInstanceConstructor(DSharpCompilerDelegateDescription description, DSharpMethodBuilder constructor, IDSharpMethodInfo baseConstructor)
        {
            var code = constructor.GetBytecodeBuilder();
            code.LoadInstance();
            code.LoadLocal(constructor.Parameters[0]);
            code.LoadLocal(constructor.Parameters[1]);
            code.Call(baseConstructor, DSharpMethodCallingType.Default);
        }
        private void CompileDelegateCreateNewMethod(DSharpCompilerDelegateDescription description)
        {
            var method = description.CreateNewMethod;
            var baseMethod = Assembly.DelegateTypeInfo.CreateNewMethod;
            method.ReturnType = Assembly.DelegateTypeToken;
            method.OverrideMethod = baseMethod;

            var code = method.GetBytecodeBuilder();
            code.LoadLocal(method.Parameters[0]);
            code.New(description.CapacityConstructor);
            code.Return();
        }
        private void CompileDelegateInvokeMethod(DSharpCompilerDelegateDescription description, DSharpMethodBuilder invokeMethod)
        {
            var delegateInvocationInfo = Assembly.DelegateInvocationInfo;
            var delegateTypeInfo = Assembly.DelegateTypeInfo;
            var invocationArrayInfo = InvocationListArrayInfo;
            var code = invokeMethod.GetBytecodeBuilder();

            DSharpMethodBuilderParameter currentInvocationInfoVariable = new(Assembly)
            {
                Name = "currentInvocationInfo",
                Type = Assembly.DelegateInvocationInfoTypeToken
            };
            DSharpMethodBuilderParameter invocationListVariable = new(Assembly)
            {
                Name = "invocationList",
                Type = Assembly.GetTypeToken(invocationArrayInfo.Type)
            };
            DSharpMethodBuilderParameter invocationListLengthVariable = new(Assembly)
            {
                Name = "invocationListLength",
                Type = Assembly.Int32Token
            };
            DSharpMethodBuilderParameter iVariable = new(Assembly)
            {
                Name = "i",
                Type = Assembly.Int32Token
            };
            DSharpMethodBuilderParameter? lastResultVariable = null;

            if (invokeMethod.ReturnType != null)
            {
                lastResultVariable = new(Assembly)
                {
                    Name = "lastResult",
                    Type = invokeMethod.ReturnType
                };

                code.LocalVariables.Add(lastResultVariable);
            }

            code.LocalVariables.Add(currentInvocationInfoVariable);
            code.LocalVariables.Add(invocationListVariable);
            code.LocalVariables.Add(invocationListLengthVariable);
            code.LocalVariables.Add(iVariable);

            code.Push(0);
            code.StoreLocal(iVariable);
            code.Pop();

            if (lastResultVariable != null)
            {
                code.Push(null);
                code.StoreLocal(lastResultVariable);
                code.Pop();
            }

            code.LoadInstance();
            code.LoadPropertyOrField(delegateTypeInfo.InvocationListField);
            code.StoreLocal(invocationListVariable);
            code.LoadPropertyOrField(invocationArrayInfo.LengthProperty);
            code.StoreLocal(invocationListLengthVariable);
            code.PopRepeat(3);

            code.LoadLocal(iVariable);
            var loopStart = code.Empty();
            code.LoadLocal(invocationListLengthVariable);
            code.Less();
            var jumpToLoop = code.JumpIfTrue();
            var jumpToEnd = code.Jump();

            jumpToLoop.ReferencedInstruction = code.PopRepeat(3);
            code.LoadLocal(invocationListVariable);
            code.LoadLocal(iVariable);
            code.LoadIndexer(invocationArrayInfo.Indexer);
            code.StoreLocal(currentInvocationInfoVariable);
            code.LoadPropertyOrField(delegateInvocationInfo.TargetField);

            foreach (var parameter in invokeMethod.Parameters)
            {
                code.LoadLocal(parameter);
            }

            code.LoadLocal(currentInvocationInfoVariable);
            code.LoadInstanceField(delegateInvocationInfo.RuntimeMethodInfoField);
            code.PopOffset(1);
            code.DynamicCall();

            int popCount = invokeMethod.Parameters.Count + 5;

            if (lastResultVariable != null)
            {
                popCount++;
                code.StoreLocal(lastResultVariable);
            }

            code.PopRepeat(popCount);
            code.LoadLocal(iVariable);
            code.Increment();
            code.StoreLocal(iVariable);
            code.Jump(loopStart);

            jumpToEnd.ReferencedInstruction = code.Empty();

            if (lastResultVariable != null)
            {
                code.LoadLocal(lastResultVariable);
                code.Return();
            }
        }

        #endregion
    }
}

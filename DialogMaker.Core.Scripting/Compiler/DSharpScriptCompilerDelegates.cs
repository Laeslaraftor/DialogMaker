using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler
{
    public partial class DSharpScriptCompiler
    {
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

                description = new(type, instanceConstructor, staticConstructor);

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
            var methodDelegateType = Assembly.MethodDelegateTypeInfo;

            if (methodDelegateType.InstanceConstructor == null)
            {
                throw new DSharpCompilerException($"Type \"{methodDelegateType.Type}\" not contains constructor for creating instance delegate: {DSharpTypeBuilder.ConstructorName}({Assembly.NIntType}, {Assembly.ObjectType})", _createdTypes[description.Type]);
            }
            if (methodDelegateType.StaticConstructor == null)
            {
                throw new DSharpCompilerException($"Type \"{methodDelegateType.Type}\" not contains constructor for creating static delegate: {DSharpTypeBuilder.ConstructorName}({Assembly.NIntType})", _createdTypes[description.Type]);
            }
            if (!description.Type.BaseTypes.Contains(methodDelegateType.Type))
            {
                description.Type.AddBaseType(methodDelegateType.Type);
            }

            var runtimeMethodInfoField = methodDelegateType.Type.GetField(DSharpCustomDelegateType.RuntimeMethodInfoFieldName);
            var targetField = methodDelegateType.Type.GetField(DSharpCustomDelegateType.TargetFieldName);

            CompileDelegateStaticConstructor(description, description.StaticConstructor, methodDelegateType.StaticConstructor);
            CompileDelegateInstanceConstructor(description, description.InstanceConstructor, methodDelegateType.InstanceConstructor);

            foreach (var info in description.InvokeMethods)
            {
                CompileDelegateInvokeMethod(info.Key, targetField, runtimeMethodInfoField);
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
        private void CompileDelegateInvokeMethod(DSharpMethodBuilder invokeMethod, IDSharpFieldInfo targetField, IDSharpFieldInfo methodInfoField)
        {
            var code = invokeMethod.GetBytecodeBuilder();

            code.LoadInstance();
            code.LoadInstanceField(targetField);

            foreach (var parameter in invokeMethod.Parameters)
            {
                code.LoadLocal(parameter);
            }

            code.LoadInstance();
            code.LoadInstanceField(methodInfoField);
            code.PopOffset(1);
            code.DynamicCall();

            if (invokeMethod.ReturnType != null)
            {
                code.Return();
            }
        }

        #endregion
    }
}

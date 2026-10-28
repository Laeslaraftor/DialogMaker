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
                    Name = "runtimeMethodInfo",
                    TypeGetter = () => Assembly.NIntToken
                });
                instanceConstructor.Parameters.Add(new(Assembly)
                {
                    Name = "target",
                    TypeGetter = () => Assembly.ObjectToken
                });
                var staticConstructor = type.CreateConstructor();
                staticConstructor.Access = DSharpAccessModifier.Public;
                staticConstructor.Parameters.Add(new(Assembly)
                {
                    Name = "runtimeMethodInfo",
                    TypeGetter = () => Assembly.NIntToken
                });

                var runtimeMethodInfoField = type.CreateField(DSharpDelegateType.RuntimeMethodInfoFieldName);
                runtimeMethodInfoField.Access = DSharpAccessModifier.Private;
                runtimeMethodInfoField.FieldTypeResolver = () => Assembly.NIntToken;
                var targetField = type.CreateField(DSharpDelegateType.TargetFieldName);
                targetField.Access = DSharpAccessModifier.Private;
                targetField.FieldTypeResolver = () => Assembly.ObjectToken;

                description = new(type, instanceConstructor, staticConstructor, runtimeMethodInfoField, targetField);

                _createdDelegates.Add(type, description);
            }

            var invokeMethod = type.CreateMethod(DSharpDelegateType.InvokeMethodName);
            invokeMethod.Access = DSharpAccessModifier.Public;

            description.InvokeMethods.Add(invokeMethod, declaration);

            return type;
        }

        #endregion

        #region Compiling

        private void CompileDelegate(DSharpCompilerDelegateDescription description)
        {
            CompileDelegateStaticConstructor(description, description.StaticConstructor);
            CompileDelegateStaticConstructor(description, description.InstanceConstructor);
            CompileDelegateInstanceConstructor(description, description.InstanceConstructor);

            foreach (var info in description.InvokeMethods)
            {
                CompileDelegateInvokeMethod(description, info.Value, info.Key);
            }
        }

        private void CompileDelegateStaticConstructor(DSharpCompilerDelegateDescription description, DSharpMethodBuilder constructor)
        {
            var code = constructor.GetBytecodeBuilder();
            code.LoadInstance();
            code.LoadLocal(constructor.Parameters[0]);
            code.StoreInstanceField(description.RuntimeMethodInfoField);
        }
        private void CompileDelegateInstanceConstructor(DSharpCompilerDelegateDescription description, DSharpMethodBuilder constructor)
        {
            var code = constructor.GetBytecodeBuilder();
            code.LoadInstance();
            code.LoadLocal(constructor.Parameters[1]);
            code.StoreInstanceField(description.TargetField);
        }
        private void CompileDelegateInvokeMethod(DSharpCompilerDelegateDescription description, ObjectDeclarationNode declaration, DSharpMethodBuilder invokeMethod)
        {
            var code = invokeMethod.GetBytecodeBuilder();

            code.LoadInstance();
            code.LoadInstanceField(description.TargetField);

            foreach (var parameter in invokeMethod.Parameters)
            {
                code.LoadLocal(parameter);
            }

            code.LoadInstance();
            code.LoadInstanceField(description.RuntimeMethodInfoField);
            code.PopOffset(1);
            code.DynamicCall();

            if (invokeMethod.ReturnType != null)
            {
                code.Return();
            }
        }

        #endregion

        #region Detecting

        private IDSharpType GetDelegateTypeForMethod(IDSharpMethodInfo method)
        {
            List<IDSharpType> parameters = [.. method.GetParameters().Select(p => p.Type)];
            DSharpBuildInTypeInfo delegateTypeInfo;

            if (method.ReturnType != null)
            {
                delegateTypeInfo = DSharpBuildInTypes.Extra.GetFunc(parameters.Count);
                parameters.Add(method.ReturnType);
            }
            else
            {
                delegateTypeInfo = DSharpBuildInTypes.Extra.GetAction(parameters.Count);
            }

            IDSharpType delegateType = Assembly.GetType(delegateTypeInfo);

            if (method.ReturnType == null &&
                parameters.Count == 0)
            {
                return delegateType;
            }

            return Assembly.FillGeneric(delegateType, parameters);
        }

        #endregion
    }
}

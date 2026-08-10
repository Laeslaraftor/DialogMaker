using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler
{
    public partial class DSharpScriptCompiler
    {
        private void SetupEnum(DSharpCompilerEnumDescription description)
        {
            var type = description.TypeBuilder;
            type.ObjectType = DSharpObjectType.Enum;

            if (!type.BaseTypes.Contains(Assembly.EnumType))
            {
                type.AddBaseType(Assembly.EnumType);
            }

            IDSharpType valueType;

            if (description.DeclarationNode.BaseTypes.Count > 0)
            {
                if (description.DeclarationNode.BaseTypes.Count > 1)
                {
                    throw new DSharpCompilerException("Multiple value types", description.DeclarationNode.BaseTypes);
                }

                var typeInfo = description.DeclarationNode.BaseTypes[0];

                try
                {
                    var typeToken = ResolveType(type, typeInfo);
                    valueType = (IDSharpType)Assembly.GetType(typeToken);
                }
                catch (Exception error)
                {
                    throw new DSharpCompilerException("Unable to resolve type", typeInfo, error);
                }
            }
            else if (description.ValueFields != null &&
                     description.ValueFields.Count > 0)
            {
                List<IDSharpType> valueTypes = [];
                DSharpCompilerContext context = new(Context, type);
                bool allValuesHasNoInitializer = true;

                foreach (var info in description.ValueFields)
                {
                    if (info.Value.Initializer == null)
                    {
                        valueTypes.Add(Assembly.Int32Type);
                        continue;
                    }
                    if (info.Value.Initializer.IsNullExpression())
                    {
                        throw new DSharpCompilerException("Enum value can not be null", info.Value.Initializer);
                    }

                    allValuesHasNoInitializer = false;
                    IDSharpMemberInfo? fieldValueMember;

                    try
                    {
                        fieldValueMember = info.Value.Initializer.GetExpressionType(Assembly, context);
                    }
                    catch (Exception error)
                    {
                        throw new DSharpCompilerException("Unable to get value type", info.Value.Initializer, error);
                    }

                    if (fieldValueMember == null ||
                        !fieldValueMember.TryGetTypeOrReturnType(out var fieldValueType))
                    {
                        throw new DSharpCompilerException("Unable to get type of value", info.Value.Initializer);
                    }

                    valueTypes.Add(fieldValueType);
                }

                if (allValuesHasNoInitializer)
                {
                    valueType = Assembly.Int32Type;
                }
                else
                {
                    while (valueTypes.Count > 1)
                    {
                        int index = 0;

                        while (index + 1 < valueTypes.Count)
                        {
                            var first = valueTypes[index];
                            var second = valueTypes[index + 1];
                            var commonType = first.GetNearestCommonType(second);

                            valueTypes.RemoveRange(index, 2);
                            valueTypes.Insert(index, commonType);

                            index++;
                        }
                    }

                    valueType = valueTypes[0];

                    if (valueType == Assembly.ObjectType ||
                        valueType == Assembly.EnumType ||
                        valueType == Assembly.NullType ||
                        valueType.FullName == DSharpBuildInTypes.Void)
                    {
                        throw new DSharpCompilerException($"Nearest common type that detected for values is \"{valueType}\" which is not allowed. Types that not allowed: \"{Assembly.ObjectType}\", \"{Assembly.EnumType}\", \"{Assembly.NullType}\", \"{DSharpBuildInTypes.Void}\"", description.DeclarationNode);
                    }
                }
            }
            else
            {
                valueType = Assembly.Int32Type;
            }

            if (valueType == Assembly.ObjectType ||
                valueType == Assembly.EnumType ||
                valueType == Assembly.NullType ||
                valueType.FullName == DSharpBuildInTypes.Void)
            {
                throw new DSharpCompilerException($"Invalid enum value type: \"{valueType}\". Types that not allowed: \"{Assembly.ObjectType}\", \"{Assembly.EnumType}\", \"{Assembly.NullType}\", \"{DSharpBuildInTypes.Void}\"", description.DeclarationNode);
            }

            var valueTypeToken = Assembly.GetTypeToken(valueType);

            description.InstanceValueField ??= type.CreateField("_value");
            description.InstanceValueField.Access = DSharpAccessModifier.Private;
            description.InstanceValueField.IsReadOnly = true;
            description.InstanceValueField.FieldType = valueTypeToken;

            void SetupParameters(IList<DSharpMethodBuilderParameter> methodParameters, params (string name, DSharpTypeToken type)[] parameters)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    DSharpMethodBuilderParameter parameter;

                    if (i < methodParameters.Count)
                    {
                        parameter = methodParameters[i];
                    }
                    else
                    {
                        parameter = new(Assembly);
                        methodParameters.Add(parameter);
                    }

                    var parameterInfo = parameters[i];
                    parameter.Name = parameterInfo.name;
                    parameter.Type = parameterInfo.type;
                }
            }

            description.ValueConstructor ??= type.CreateConstructor();
            SetupParameters(description.ValueConstructor.Parameters, ("value", valueTypeToken));

            description.ToStringMethod ??= type.CreateMethod(nameof(ToString));
            description.ToStringMethod.Access = DSharpAccessModifier.Public;
            description.ToStringMethod.ReturnType = Assembly.StringToken;
            description.ToStringMethod.OverrideMethod = Assembly.ObjectTypeInfo.ToStringMethod;

            description.ExplicitEnumToValueOperator = type.CreateExplicitOperator();
            description.ExplicitEnumToValueOperator.ReturnType = type;
            SetupParameters(description.ExplicitEnumToValueOperator.Parameters, ("value", valueTypeToken));

            description.ExplicitValueToEnumOperator = type.CreateExplicitOperator();
            description.ExplicitValueToEnumOperator.ReturnType = valueTypeToken;
            SetupParameters(description.ExplicitValueToEnumOperator.Parameters, ("value", type));
        }
        private void CompileEnum(DSharpCompilerEnumDescription description)
        {
            CompileEnumValueConstructor(description);
            CompileEnumExplicitEnumToValueOperator(description);
            CompileEnumExplicitValueToEnumOperator(description);
            CompileEnumValues(description);
            CompileEnumToString(description);
        }

        private void CompileEnumValueConstructor(DSharpCompilerEnumDescription description)
        {
            if (description.ValueConstructor == null)
            {
                throw new DSharpCompilerException("Enum value constructor not found", description.DeclarationNode);
            }
            if (description.InstanceValueField == null)
            {
                throw new DSharpCompilerException("Enum instance value not found", description.DeclarationNode);
            }

            var code = description.ValueConstructor.GetBytecodeBuilder();
            code.LoadLocal(description.ValueConstructor.Parameters[0]);
            code.LoadInstance();
            code.StoreInstanceField(description.InstanceValueField);
        }
        private void CompileEnumExplicitEnumToValueOperator(DSharpCompilerEnumDescription description)
        {
            if (description.ExplicitEnumToValueOperator == null)
            {
                throw new DSharpCompilerException("Enum explicit operator for getting value not found", description.DeclarationNode);
            }
            if (description.InstanceValueField == null)
            {
                throw new DSharpCompilerException("Enum instance value not found", description.DeclarationNode);
            }
            if (description.ValueConstructor == null)
            {
                throw new DSharpCompilerException("Enum value constructor not found", description.DeclarationNode);
            }

            var code = description.ExplicitEnumToValueOperator.Method.GetBytecodeBuilder();

            if (description.ValueFields != null)
            {
                foreach (var valueField in description.ValueFields.Keys)
                {
                    code.LoadField(valueField);
                    code.LoadInstanceField(description.InstanceValueField);
                    code.LoadLocal(description.ExplicitEnumToValueOperator.Parameters[0]);
                    code.Call(Assembly.ObjectTypeInfo.EqualsMethod);
                    var skipInstruction = code.JumpIfFalse();
                    code.PopRepeat(3);
                    code.Return();
                    skipInstruction.ReferencedInstruction = code.PopRepeat(4);
                }
            }

            code.LoadLocal(description.ExplicitEnumToValueOperator.Parameters[0]);
            code.New(description.ValueConstructor);
            code.Return();
        }
        private void CompileEnumExplicitValueToEnumOperator(DSharpCompilerEnumDescription description)
        {
            if (description.ExplicitValueToEnumOperator == null)
            {
                throw new DSharpCompilerException("Enum explicit operator for getting value not found", description.DeclarationNode);
            }
            if (description.InstanceValueField == null)
            {
                throw new DSharpCompilerException("Enum instance value not found", description.DeclarationNode);
            }

            var code = description.ExplicitValueToEnumOperator.Method.GetBytecodeBuilder();
            code.LoadLocal(description.ExplicitValueToEnumOperator.Parameters[0]);
            code.LoadInstanceField(description.InstanceValueField);
            code.Return();
        }
        private void CompileEnumValues(DSharpCompilerEnumDescription description)
        {
            if (description.ValueFields == null)
            {
                return;
            }
            if (description.ValueConstructor == null)
            {
                throw new DSharpCompilerException("Enum value constructor not found", description.DeclarationNode);
            }
            if (description.InstanceValueField == null)
            {
                throw new DSharpCompilerException("Enum instance value not found", description.DeclarationNode);
            }
            if (description.InstanceValueField.FieldType == null)
            {
                throw new DSharpCompilerException("Enum instance value type not specified", description.DeclarationNode);
            }

            IDSharpType valueType = (IDSharpType)Assembly.GetType(description.InstanceValueField.FieldType);
            var staticInitializer = description.TypeBuilder.CreateInitializer(true);
            var code = staticInitializer.GetBytecodeBuilder();
            var context = CreateContext(staticInitializer);
            var settings = CreateSettings();
            int valueIndex = 0;

            foreach (var info in description.ValueFields)
            {
                if (info.Value.Initializer == null)
                {
                    code.Push(valueIndex);
                    CastTypes(staticInitializer, Assembly.Int32Type, valueType, code, null, context);
                }
                else
                {
                    CompileExpressionValueWithRequestedType(staticInitializer, valueType, code, info.Value.Initializer, ref settings, null, context);
                }

                code.New(description.ValueConstructor);
                code.StoreField(info.Key);

                valueIndex++;
            }
        }
        private void CompileEnumToString(DSharpCompilerEnumDescription description)
        {
            if (description.InstanceValueField == null)
            {
                throw new DSharpCompilerException("Enum instance value not found", description.DeclarationNode);
            }
            if (description.InstanceValueField.FieldType == null)
            {
                throw new DSharpCompilerException("Enum instance value type not specified", description.DeclarationNode);
            }
            if (description.ToStringMethod == null)
            {
                throw new DSharpCompilerException("Enum to string method not found", description.DeclarationNode);
            }

            var valueType = (IDSharpType)Assembly.GetType(description.InstanceValueField.FieldType);
            var code = description.ToStringMethod.GetBytecodeBuilder();
            bool isNumber = DSharpBuildInTypes.IsNumber(valueType);

            if (description.ValueFields == null)
            {
                code.LoadInstance();
                code.CallBaseInstance(Assembly.ObjectTypeInfo.ToStringMethod);
                return;
            }

            foreach (var field in description.ValueFields.Keys)
            {
                code.LoadInstance();
                code.LoadInstanceField(description.InstanceValueField);
                code.LoadField(field);
                code.LoadInstanceField(description.InstanceValueField);
                code.PopOffset(1);
                
                if (isNumber)
                {
                    code.Equals();
                }
                else
                {
                    code.Call(Assembly.ObjectTypeInfo.EqualsMethod);
                }

                var skipInstruction = code.JumpIfFalse();
                code.Push(field.Name);
                code.Return();
                skipInstruction.ReferencedInstruction = code.PopRepeat(4);
            }

            code.Push(description.TypeBuilder.FullName);
            code.Return();
        }
    }
}

using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler
{
    public partial class DSharpScriptCompiler
    {
        /// <summary>
        /// Fields that contains properties values.
        /// Fields creating only when property have not custom getter and setter
        /// </summary>
        private readonly Dictionary<DSharpPropertyBuilder, DSharpFieldBuilder> _propertyFields = [];
        private readonly HashSet<DSharpPropertyBuilder> _propertiesWithCustomAccessors = [];

        #region Properties

        private DSharpFieldBuilder GetValueFieldForProperty(DSharpPropertyBuilder property)
        {
            if (!_propertyFields.TryGetValue(property, out var field))
            {
                if (property.DeclaringType is not DSharpTypeBuilder declaringTypeBuilder)
                {
                    throw new DSharpCompilerException("Unable to create auto field for property that declared not in builder", _createdProperties[property]);
                }

                field = declaringTypeBuilder.CreateField(property.Name + ValueFieldNameSuffix);
                field.FieldType = property.PropertyType;
                field.Access = DSharpAccessModifier.Private;
                _propertyFields.Add(property, field);
            }

            return field;
        }

        private void CompileProperty(DSharpPropertyBuilder property, FieldNode node)
        {
            DSharpMethodCompileSettings settings = new();
            DSharpFieldBuilder? valueField = null;

            DSharpFieldBuilder GetValueField()
            {
                if (valueField == null)
                {
                    valueField = GetValueFieldForProperty(property);
                    settings.IdentifiersAsField ??= [];
                    settings.IdentifiersAsField.Add(FieldKeyword, valueField);
                }

                return valueField;
            }

            void CreateAccessor(Func<DSharpMethodBuilder> fabric, BlockStatementNode? customAccessor, DSharpAccessModifier access, Action<DSharpMethodBuilder, DSharpMethodCompileSettings> compiler)
            {
                if (customAccessor == null &&
                    property.DeclaringType.ObjectType == DSharpObjectType.Interface)
                {
                    return;
                }

                DSharpMethodBuilder accessorMethod = fabric();
                accessorMethod.Access = access;

                if (accessorMethod.IsAbstract)
                {
                    return;
                }
                if (customAccessor != null)
                {
                    settings.IdentifiersAsField ??= [];
                    settings.PropertyFieldProvider = GetValueField;
                    _propertiesWithCustomAccessors.Add(property);
                    CompileMethod(accessorMethod, customAccessor, settings);
                }
                else
                {
                    GetValueField();
                    compiler(accessorMethod, settings);
                }
            }

            if (node.CanRead)
            {
                property.CanRead = true;

                if (property.DeclaringType.ObjectType != DSharpObjectType.Interface ||
                    (property.DeclaringType.ObjectType == DSharpObjectType.Interface && node.Getter != null))
                {
                    CreateAccessor(property.CreateGetter, node.Getter, node.GetterAccess, CompileGetterMethod);
                }
            }
            if (node.CanWrite)
            {
                property.CanWrite = true;

                if (property.DeclaringType.ObjectType != DSharpObjectType.Interface ||
                    (property.DeclaringType.ObjectType == DSharpObjectType.Interface && node.Setter != null))
                {
                    CreateAccessor(property.CreateSetter, node.Setter, node.SetterAccess, CompileSetterMethod);
                }
            }
        }

        #endregion

        #region Accessors

        private void CompileGetterMethod(DSharpMethodBuilder method, DSharpMethodCompileSettings settings = default)
        {
            if (settings.IdentifiersAsField == null || !settings.IdentifiersAsField.TryGetValue(FieldKeyword, out var field))
            {
                throw new ArgumentException($"Field for store value must be provided", nameof(settings));
            }

            var code = method.GetBytecodeBuilder();

            if (method.IsStatic)
            {
                code.LoadField(field);
            }
            else
            {
                code.LoadInstance();
                code.LoadInstanceField(field);
            }

            code.Return();
        }
        private void CompileSetterMethod(DSharpMethodBuilder method, DSharpMethodCompileSettings settings = default)
        {
            if (settings.IdentifiersAsField == null || !settings.IdentifiersAsField.TryGetValue(FieldKeyword, out var field))
            {
                throw new ArgumentException($"Field for store value must be provided", nameof(settings));
            }

            var code = method.GetBytecodeBuilder();

            if (!method.IsStatic)
            {
                code.LoadInstance();
            }

            code.LoadLocal(method.Parameters[0]);
            code.StorePropertyOrField(field);
        }

        #endregion

        #region Constants

        private const string ValueFieldNameSuffix = "__value";
        private const string FieldKeyword = "field";

        #endregion
    }
}

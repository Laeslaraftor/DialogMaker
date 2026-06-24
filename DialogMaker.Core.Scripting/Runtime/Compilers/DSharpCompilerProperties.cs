using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Runtime.Builders;

namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    public partial class DSharpCompiler
    {
        /// <summary>
        /// Fields that contains properties values.
        /// Fields creating only when property have not custom getter and setter
        /// </summary>
        private readonly Dictionary<DSharpPropertyBuilder, DSharpFieldBuilder> _propertyFields = [];
        private readonly HashSet<DSharpPropertyBuilder> _propertiesWithCustomAccessors = [];

        private void CompileProperty(DSharpPropertyBuilder property, FieldNode node)
        {
            DSharpMethodCompileSettings settings = new();
            DSharpFieldBuilder? valueField = null;

            DSharpFieldBuilder GetValueField()
            {
                if (valueField == null)
                {
                    valueField = property.DeclaringType.CreateField(property.Name + ValueFieldNameSuffix);
                    valueField.FieldType = property.PropertyType;
                    valueField.Access = DSharpAccessModifier.Private;
                    settings.IdentifiersAsField ??= [];
                    settings.IdentifiersAsField.Add(FieldKeyword, valueField);
                    _propertyFields.Add(property, valueField);
                }

                return valueField;
            }

            void CreateAccessor(Func<DSharpMethodBuilder> fabric, BlockStatementNode? customAccessor, DSharpAccessModifier access, Action<DSharpMethodBuilder, DSharpMethodCompileSettings> compiler)
            {
                DSharpMethodBuilder? accessorMethod = fabric();

                if (node.GetterAccess == DSharpAccessModifier.Protected)
                {
                    accessorMethod.Access = DSharpAccessModifier.Protected;
                }
                else
                {
                    accessorMethod.Access = DSharpAccessModifier.Private;
                }

                if (customAccessor != null)
                {
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

        #region Константы

        private const string ValueFieldNameSuffix = "__value";
        private const string FieldKeyword = "field";

        #endregion
    }
}

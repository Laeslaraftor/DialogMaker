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
             
            if (node.CanRead)
            {
                var getterMethod = property.CreateGetter();
                getterMethod.Access = node.GetterAccess;

                if (node.Getter != null)
                {
                    CompileMethod(getterMethod, node.Getter, settings);
                }
                else
                {
                    GetValueField();
                    CompileGetterMethod(getterMethod, settings);
                }
            }
            if (node.CanWrite)
            {
                var setterMethod = property.CreateSetter();
                setterMethod.Access = node.SetterAccess;

                if (node.Setter != null)
                {
                    CompileMethod(setterMethod, node.Setter, settings);
                }
                else
                {
                    GetValueField();
                    CompileSetterMethod(setterMethod, settings);
                }
            }
        }

        #region Константы

        private const string ValueFieldNameSuffix = "__value";
        private const string FieldKeyword = "field";

        #endregion
    }
}

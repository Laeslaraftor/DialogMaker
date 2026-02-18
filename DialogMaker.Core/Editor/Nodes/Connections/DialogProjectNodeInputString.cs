using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning.Internal;
using System;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputString(INode node, int portId)
        : DialogProjectNodeInputValue<IResourceItem>(node, portId, DialogNodePortType.String)
    {
        public override AllowedObjectValues AllowedValues => AllowedObjectValues.Resource | AllowedObjectValues.String;
        public override Type ReflectionValueType
        {
            get
            {
                field ??= typeof(object);
                return field;
            }
        }

        #region Управление

        protected override object GetValueToSave()
        {
            if (Value is IResourceString resourceString && resourceString.IsSeparated)
            {
                return resourceString.Text;
            }

            return base.GetValueToSave();
        }

        protected override IResourceItem ExtractValue(object? value)
        {
            if (value == null)
            {
                return ResourceString.Empty;
            }
            if (value is IResourceString resource)
            {
                return resource;
            }
            else if (value is IVariable variable)
            {
                return new ResourceString(variable.Id, variable.Value.ToString());
            }

            return new ResourceString(Id, value.ToString());
        }

        #endregion
    }
}

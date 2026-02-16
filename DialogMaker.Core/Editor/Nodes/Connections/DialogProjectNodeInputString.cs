using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning.Internal;
using System;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputString(INode node, int portId)
        : DialogProjectNodeInputValue<IResourceString>(node, portId, DialogNodePortType.String)
    {
        public override AllowedObjectValues AllowedValues => AllowedObjectValues.Resource | AllowedObjectValues.String;
        public override DialogResourceType? ResourceType => DialogResourceType.String;
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
            if (Value.IsSeparated)
            {
                return Value.Text;
            }

            return base.GetValueToSave();
        }

        protected override IResourceString ExtractValue(object value)
        {
            if (value is IResourceString resource)
            {
                return resource;
            }

            return new ResourceString(Id, value.ToString());
        }

        #endregion
    }
}

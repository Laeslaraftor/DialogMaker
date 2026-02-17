using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning.Internal;
using System;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputCharacter(INode node, int portId)
        : DialogProjectNodeInputValue<ICharacter>(node, portId, DialogNodePortType.Object)
    {
        public override AllowedObjectValues AllowedValues => AllowedObjectValues.Resource | AllowedObjectValues.String;
        public override DialogResourceType? ResourceType => DialogResourceType.Character;
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
                return Value.Name;
            }

            return base.GetValueToSave();
        }

        protected override ICharacter ExtractValue(object? value)
        {
            if (value == null)
            {
                return LocalCharacter.Empty;
            }
            if (value is ICharacter resource)
            {
                return resource;
            }

            return new LocalCharacter(Id, value.ToString());
        }

        #endregion
    }
}

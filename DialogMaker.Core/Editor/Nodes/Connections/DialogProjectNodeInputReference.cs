using System;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodeInputReference(INode node, int portId, DialogResourceType type)
        : DialogProjectNodeInputValue<DialogProjectReference>(node, portId, DialogNodePortType.Object)
    {
        public override DialogResourceType? ResourceType { get; } = type;
        public override AllowedObjectValues AllowedValues => AllowedObjectValues.Resource;

        #region Управление

        protected override bool ValidateValue(DialogProjectReference? value)
        {
            if (value != null && value.ResourceType != ResourceType)
            {
                throw new ArgumentException($"Недопустимое значение. Требуется ресурс типа {ResourceType}, получено: {value.ResourceType}");
            }

            return base.ValidateValue(value);
        }

        #endregion
    }
}

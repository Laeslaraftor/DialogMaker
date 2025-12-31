using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;

namespace DialogMaker.Core.Common
{
    public class DialogResourceNumberVariable : DialogResourceVariableGeneric<float>
    {
        public DialogResourceNumberVariable(DialogResources resources, DialogProjectVariable variable) : base(resources, variable)
        {
        }
        public DialogResourceNumberVariable(DialogResources resources, DialogResourceVariableSavedState savedState) : base(resources, savedState)
        {
        }

        public override DialogVariableType Type => DialogVariableType.Number;

        protected override float DefaultValue => 0f;
    }
}

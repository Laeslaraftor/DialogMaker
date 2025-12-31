using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;

namespace DialogMaker.Core.Common
{
    public class DialogResourceBoolVariable : DialogResourceVariableGeneric<bool>
    {
        public DialogResourceBoolVariable(DialogResources resources, DialogProjectVariable variable) : base(resources, variable)
        {
        }
        public DialogResourceBoolVariable(DialogResources resources, DialogResourceVariableSavedState savedState) : base(resources, savedState)
        {
        }

        public override DialogVariableType Type => DialogVariableType.Bool;

        protected override bool DefaultValue => false;
    }
}

using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;

namespace DialogMaker.Core.Common
{
    public class DialogResourceStringVariable : DialogResourceVariableGeneric<string>
    {
        public DialogResourceStringVariable(DialogResources resources, DialogProjectVariable variable) : base(resources, variable)
        {
        }
        public DialogResourceStringVariable(DialogResources resources, DialogResourceVariableSavedState savedState) : base(resources, savedState)
        {
        }

        public override DialogVariableType Type => DialogVariableType.String;

        protected override string DefaultValue => string.Empty;
    }
}

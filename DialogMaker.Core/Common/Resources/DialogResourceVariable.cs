using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;
using System;

namespace DialogMaker.Core.Common
{
    public class DialogResourceVariable : DialogResourceObject
    {
        public DialogResourceVariable(DialogResources resources, DialogProjectResourceObject resourceObject) : base(resources, resourceObject)
        {
        }
        public DialogResourceVariable(DialogResources resources, DialogResourceObjectSavedState savedState) : base(resources, savedState)
        {
        }

        public override DialogResourceType ResourceType => DialogResourceType.Variable;

        #region Управление

        protected override DialogResourceObjectSavedState CreateSavedState()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

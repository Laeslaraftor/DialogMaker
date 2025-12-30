using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;
using System;

namespace DialogMaker.Core.Common
{
    public class DialogResourceCharacter : DialogResourceObject
    {
        public DialogResourceCharacter(DialogResources resources, DialogProjectResourceObject resourceObject) : base(resources, resourceObject)
        {
        }
        public DialogResourceCharacter(DialogResources resources, DialogResourceObjectSavedState savedState) : base(resources, savedState)
        {
        }

        public override DialogResourceType ResourceType => DialogResourceType.Character;

        #region Управление

        protected override DialogResourceObjectSavedState CreateSavedState()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

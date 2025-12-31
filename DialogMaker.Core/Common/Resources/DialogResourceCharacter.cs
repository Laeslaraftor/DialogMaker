using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;

namespace DialogMaker.Core.Common
{
    public class DialogResourceCharacter : DialogResourceObject
    {
        public DialogResourceCharacter(DialogResources resources, DialogProjectCharacter character) : base(resources, character)
        {
            if (character.Name != null)
            {
                var item = character.Name.Resolve();
                _namePath = new(character.Name.ResourcesPath, item.Id);
            }
        }
        public DialogResourceCharacter(DialogResources resources, DialogResourceCharacterSavedState savedState) : base(resources, savedState)
        {
            if (savedState.Name != null && 
                ResourcePath.TryParse(savedState.Name, out var namePath))
            {
                _namePath = namePath;
            }
        }

        public override DialogResourceType ResourceType => DialogResourceType.Character;
        public string Name => NameString?.Value ?? string.Empty;
        public DialogResourceString? NameString
        {
            get
            {
                if (field != null)
                {
                    return field;
                }
                if (_namePath.IsEmpty)
                {
                    return null;
                }

                field ??= Resources.Package.GetResource<DialogResourceString>(_namePath);

                return field;
            }
        }

        private readonly ResourcePath _namePath;

        #region Управление

        protected override DialogResourceObjectSavedState CreateSavedState()
        {
            DialogResourceCharacterSavedState result = new();

            if (!_namePath.IsEmpty)
            {
                result.Name = _namePath;
            }

            return result;
        }

        #endregion
    }
}

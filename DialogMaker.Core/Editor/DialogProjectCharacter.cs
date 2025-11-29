namespace DialogMaker.Core.Editor
{
    public class DialogProjectCharacter : DialogProjectResourceObject
    {
        public DialogProjectCharacter(DialogProjectResources resources) 
            : base(resources)
        {
        }
        public DialogProjectCharacter(DialogProjectResources resources, DialogProjectCharacterSavedState savedState)
            : base(resources, savedState)
        {
            if (savedState.Name != null)
            {
                _name = DialogProjectReference<DialogProjectString>.Restore(resources.Owner.Project, savedState.Name);
            }
        }

        public DialogProjectReference<DialogProjectString>? Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    InvokePropertyChanged(nameof(Name));
                }
            }
        }

        private DialogProjectReference<DialogProjectString>? _name;

        #region Управление

        protected override DialogProjectResourceObjectSavedState CreateSavedState()
        {
            return new DialogProjectCharacterSavedState()
            {
                Name = Name?.Save()
            };
        }

        #endregion
    }
}

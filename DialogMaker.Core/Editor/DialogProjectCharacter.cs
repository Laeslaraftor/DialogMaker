using System;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectCharacter : ObservableObject, ISavable
    {
        public DialogProjectCharacter()
        {
            Id = Guid.NewGuid();
        }
        public DialogProjectCharacter(DialogProjectCharacterSavedState savedState)
        {
            Id = Guid.Parse(savedState.Id);
            Name = savedState.Name;
        }

        public Guid Id { get; }
        public string Name
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

        private string _name = string.Empty;

        public DialogProjectCharacterSavedState Save()
        {
            return new()
            {
                Id = Id.ToString(),
                Name = Name
            };
        }

        ISavedState ISavable.Save() => Save();
    }
}

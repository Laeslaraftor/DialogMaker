using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning.Internal;
using System;
using System.Diagnostics;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectCharacter : DialogProjectResourceObject, ICharacter
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
                try
                {
                    _name = DialogProjectReference<DialogProjectString>.Restore(resources.Owner.Project, savedState.Name);
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error);
                }
            }
        }

        public override DialogResourceType ResourceType => DialogResourceType.Character;
        public DialogProjectReference<DialogProjectString>? Name
        {
            get => _name;
            set
            {
                if (_name != value && !IsDisposed)
                {
                    InvokePropertyChanging(nameof(Name));
                    _name = value;
                    InvokePropertyChanged(nameof(Name));
                }
            }
        }
        string ICharacter.Name
        {
            get
            {
                var nameReference = Name;

                if (nameReference != null)
                {
                    return nameReference.Resolve().Preview;
                }

                return NamelessCharacter;
            }
        }

        private DialogProjectReference<DialogProjectString>? _name;

        #region Управление

        public override IVariable ToVariable()
        {
            var name = Name;

            if (name != null)
            {
                return name.Resolve().ToVariable();
            }

            return new LocalVariable(Id, NamelessCharacter);
        }

        public override string ToString()
        {
            if (Name != null)
            {
                try
                {
                    return Name.Resolve().Preview;
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error);
                }
            }

            return NamelessCharacter;
        }

        protected override DialogProjectResourceObjectSavedState CreateSavedState()
        {
            return new DialogProjectCharacterSavedState()
            {
                Name = Name?.Save()
            };
        }

        #endregion

        #region Константы

        public const string NamelessCharacter = "Безымянный персонаж";

        #endregion
    }
}

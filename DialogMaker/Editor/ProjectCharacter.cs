using DialogMaker.Core.Common;
using DialogMaker.Core.Editor;
using DialogMaker.Editor.Menus;
using System.ComponentModel;

namespace DialogMaker.Editor
{
    public class ProjectCharacter : ProjectResourceItem<DialogProjectCharacter>, ICharacter
    {
        public ProjectCharacter(ProjectController controller, DialogProjectCharacter character)
            : base(controller, character)
        {
            if (character.Name != null)
            {
                try
                {
                    Name = new(controller, character.Name);
                }
                catch (Exception error)
                {
                    error.Log();
                }
            }
        }

        public ProjectReference<ProjectString, DialogProjectString>? Name
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    var reference = value?.Reference;

                    if (Original.Name != reference)
                    {
                        Original.Name = reference;
                    }

                    InvokePropertyChanged(nameof(Name));
                }
            }
        }

        string ICharacter.Name => Name?.Item?.PreviewVariant?.Text ?? string.Empty;

        #region Управление

        public override bool ContainsValue(string value)
        {
            if (base.ContainsValue(value))
            {
                return true;
            }

            return Name?.Item.ContainsValue(value) == true;
        }

        public override ItemContextMenu CreateContextMenu()
        {
            return new CharacterContextMenu(this);
        }

        #endregion

        #region События

        protected override void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            base.OnModelPropertyChanged(sender, e);

            if (e.PropertyName == nameof(Name) && 
                Name?.Reference != Original.Name)
            {
                Name = Original.Name == null ? null : new(Project, Original.Name);
            }
        }

        #endregion
    }
}

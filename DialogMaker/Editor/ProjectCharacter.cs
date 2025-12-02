using DialogMaker.Core.Editor;
using DialogMaker.Editor.Menus;
using System.ComponentModel;

namespace DialogMaker.Editor
{
    public class ProjectCharacter : ProjectResourceItem<DialogProjectCharacter>
    {
        public ProjectCharacter(ProjectController controller, DialogProjectCharacter character)
            : base(controller, character)
        {
            if (character.Name != null)
            {
                Name = new(controller, character.Name);
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

        #region Управление

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

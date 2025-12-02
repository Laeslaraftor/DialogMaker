using DialogMaker.Core.Editor;
using DialogMaker.Editor.Menus;

namespace DialogMaker.Editor
{
    public class ProjectCharacter : ProjectResourceItem<DialogProjectCharacter>
    {
        public ProjectCharacter(ProjectController controller, DialogProjectCharacter character)
            : base(controller, character)
        {
        }

        public ProjectReference<ProjectString, DialogProjectString>? Name
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    InvokePropertyChanged(nameof(Name));
                }
            }
        }

        #region Управление

        public override ItemContextMenu CreateContextMenu()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

using DialogMaker.Core.Editor;
using DialogMaker.Editor.Menus;

namespace DialogMaker.Editor
{
    public class ProjectResourceFile : ProjectResourceItem<DialogProjectItem>
    {
        public ProjectResourceFile(ProjectController project, DialogProjectItem original) : base(project, original)
        {
        }

        public override ItemContextMenu CreateContextMenu()
        {
            throw new NotImplementedException();
        }
    }
}

using DialogMaker.Core.Editor;
using System.Collections;
using System.Windows.Controls;

namespace DialogMaker.Editor
{
    public abstract class ProjectStructureItem(ProjectController project, IProjectResourcesOwner resourcesOwner) 
        : ProjectResourcesOwner(project, resourcesOwner)
    {
        public abstract string Icon { get; }
        public abstract string Name { get; set; }
        public abstract ContextMenu? ContextMenu { get; }
        public abstract IEnumerable? Children { get; }
    }
}

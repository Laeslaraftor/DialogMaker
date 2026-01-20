using DialogMaker.Core.Editor;
using DialogMaker.Lib.Controllers;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Editor
{
    public abstract class ProjectStructureItem : ProjectResourcesOwner, IItemTab
    {
        public ProjectStructureItem(ProjectController project, IProjectResourcesOwner resourcesOwner) 
            : base(project, resourcesOwner)
        {
        }
        protected ProjectStructureItem(Func<ProjectResourcesOwner, ProjectController> projectFabric, IProjectResourcesOwner resourcesOwner)
            : base(projectFabric, resourcesOwner)
        {
        }

        public event EventHandler? CloseRequested;

        public abstract string Icon { get; }
        public abstract string Name { get; set; }
        public abstract ContextMenu? ContextMenu { get; }
        public abstract IEnumerable? Children { get; }
        public abstract UIElement? TabContent { get; }
        public virtual bool CanRename => true;
        public virtual bool CanClose => true;

        #region События

        public virtual void OnClosed(object? sender, EventArgs e)
        {
        }
        public virtual void OnHided(object? sender, EventArgs e)
        {
        }
        public virtual void OnShowed(object? sender, EventArgs e)
        {
        }

        protected virtual void OnCloseRequested(EventArgs e)
        {
            Dispatch(() =>
            {
                CloseRequested?.Invoke(this, e);
            });
        }

        #endregion
    }
}

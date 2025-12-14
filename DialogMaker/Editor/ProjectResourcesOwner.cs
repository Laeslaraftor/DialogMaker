using DialogMaker.Core;
using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public abstract class ProjectResourcesOwner(ProjectController project, IProjectResourcesOwner resourcesOwner) : Disposable
    {
        public ProjectController Project { get; } = project;
        public ProjectResources Resources { get; } = new(project, resourcesOwner.Resources);

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Resources.Dispose();
        }

        #endregion
    }
}

using DialogMaker.Core;
using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public abstract class ProjectResourcesOwner(ProjectController project, IProjectResourcesOwner resourcesOwner) 
        : ObservableObject, IDisposable
    {
        ~ProjectResourcesOwner()
        {
            Dispose(false);
        }

        public ProjectController Project { get; } = project;
        public ProjectResources Resources { get; } = new(project, resourcesOwner.Resources);

        #region Управление

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            Resources.Dispose();
        }

        #endregion
    }
}

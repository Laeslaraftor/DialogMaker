using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public abstract class ProjectResourcesOwner : Disposable
    {
        public ProjectResourcesOwner(ProjectController project, IProjectResourcesOwner resourcesOwner)
        {
            Project = project;
            _resourcesOwner = resourcesOwner;
        }
        protected ProjectResourcesOwner(Func<ProjectResourcesOwner, ProjectController> projectFabric, IProjectResourcesOwner resourcesOwner)
        {
            Project = projectFabric(this);
            _resourcesOwner = resourcesOwner;
        }

        public ProjectController Project { get; }
        public virtual ProjectResources Resources
        {
            get
            {
                _resources ??= new(Project, _resourcesOwner.Resources);
                return _resources;
            }
        }

        private readonly IProjectResourcesOwner _resourcesOwner;
        private ProjectResources? _resources;

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            _resources?.Dispose();
            _resources = null;
        }

        #endregion
    }
}

using DialogMaker.Core;
using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public abstract class ProjectResourcesOwner(ProjectController project, IProjectResourcesOwner resourcesOwner) : Disposable
    {
        public ProjectController Project { get; } = project;
        public virtual ProjectResources Resources
        {
            get
            {
                _resources ??= new(Project, _resourcesOwner.Resources);
                return _resources;
            }
        }

        private readonly IProjectResourcesOwner _resourcesOwner = resourcesOwner;
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

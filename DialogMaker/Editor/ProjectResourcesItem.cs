using DialogMaker.Core.Editor;
using DialogMaker.Lib.Controllers;
using DialogMaker.Lib.Elements;
using System.Windows;

namespace DialogMaker.Editor
{
    public abstract class ProjectResourcesItem : ProjectStructureItem, IActionsItemTab
    {
        public ProjectResourcesItem(ProjectController project, IProjectResourcesOwner resourcesOwner)
            : base(project, resourcesOwner)
        {
        }
        protected ProjectResourcesItem(Func<ProjectResourcesOwner, ProjectController> projectFabric, IProjectResourcesOwner resourcesOwner)
            : base(projectFabric, resourcesOwner)
        {
        }

        public override UIElement TabContent
        {
            get
            {
                if (IsDisposed)
                {
                    throw new InvalidOperationException("Невозможно получить представление для вкладки для очищенного объекта");
                }
                if (_tabContent == null)
                {
                    var view = _dialogAndResourcesViewsPool.GetElement();
                    view.Item = this;
                    view.ItemResources = Resources;
                    _tabContent = view;
                }

                return _tabContent;
            }
        }
        public abstract IEnumerable<ActionButton>? Actions { get; }

        private DialogAndResourcesView? _tabContent;

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            OnCloseRequested(EventArgs.Empty);
        }

        #endregion

        #region События

        public override void OnClosed(object? sender, EventArgs e)
        {
            base.OnClosed(sender, e);
            RemoveAsLastShowedTabItem();

            if (_tabContent == null)
            {
                return;
            }

            var content = _tabContent;
            _tabContent = null;

            content.Item = null;
            content.ItemResources = null;
            _dialogAndResourcesViewsPool.Free(content);
        }
        public override void OnShowed(object? sender, EventArgs e)
        {
            base.OnShowed(sender, e);
            Project.LastShowedTabItem = this;
        }
        public override void OnHided(object? sender, EventArgs e)
        {
            base.OnHided(sender, e);
            RemoveAsLastShowedTabItem();
        }

        private void RemoveAsLastShowedTabItem()
        {
            if (Project.LastShowedTabItem == this)
            {
                Project.LastShowedTabItem = null;
            }
        }

        #endregion

        #region Статика

        private static readonly ElementsPool<DialogAndResourcesView> _dialogAndResourcesViewsPool = new();

        #endregion
    }
}

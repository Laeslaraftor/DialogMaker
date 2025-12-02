using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Editor.Menus;
using System.Windows.Controls;

namespace DialogMaker.Editor
{
    public abstract class ProjectResourceItem<T> : ProjectResourceItem
        where T : DialogProjectResourceObject
    {
        protected ProjectResourceItem(ProjectController project, T original) : base(project, original)
        {
            Original = original;
        }

        public T Original { get; }
    }
    public abstract class ProjectResourceItem : ObservableObject, IDisposable
    {
        protected ProjectResourceItem(ProjectController project, DialogProjectResourceObject model)
        {
            Project = project;
            Model = model;
        }
        ~ProjectResourceItem()
        {
            Dispose(false);
        }

        public ProjectController Project { get; }
        public DialogProjectResourceObject Model { get; }
        public DialogResourceType ResourceType => Model.ResourceType;
        public ContextMenu ContextMenu
        {
            get
            {
                field ??= CreateContextMenu();
                return field;
            }
        }

        #region Управление

        public abstract ItemContextMenu CreateContextMenu();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public override string? ToString()
        {
            return Model.ToString();
        }
        public virtual object? GetPreview()
        {
            return ToString();
        }
        public virtual void FreePreview(object? preview)
        {
        }

        protected virtual void Dispose(bool isDisposing)
        {
        }

        #endregion
    }
}

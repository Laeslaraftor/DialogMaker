using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public class ProjectReference<T, TItem> : ProjectReference
        where T : ProjectResourceItem<TItem> where TItem : DialogProjectResourceObject
    {
        public ProjectReference(ProjectController project, DialogProjectReference<TItem> reference)
            : base(project, reference)
        {
            Reference = reference;
            Item = (T)base.Item;
        }
        public ProjectReference(T item)
            : base(item)
        {
            Reference = item.Original;
            Item = item;
        }

        public new DialogProjectReference<TItem> Reference { get; }
        public new T Item { get; }

        #region Операторы

        public static implicit operator T(ProjectReference<T, TItem> reference)
        {
            return reference.Item;
        }
        public static implicit operator DialogProjectReference<TItem>(ProjectReference<T, TItem> reference)
        {
            return reference.Reference;
        }
        public static implicit operator ProjectReference<T, TItem>(T item)
        {
            return new(item);
        }

        #endregion
    }
}

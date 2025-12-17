using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public class ProjectReference : IModelContainer<DialogProjectReference>, IModelContainer<DialogProjectResourceObject>
    {
        public ProjectReference(ProjectController project, DialogProjectReference reference)
        {
            Project = project;
            Reference = reference;
            Item = ProjectResourceItem.Create(project, reference);
        }
        public ProjectReference(ProjectResourceItem item)
        {
            Project = item.Project;
            Reference = item.Model;
            Item = item;
        }

        public ProjectController Project { get; }
        public DialogProjectReference Reference { get; }
        public ProjectResourceItem Item { get; }

        DialogProjectReference IModelContainer<DialogProjectReference>.Model => Reference;
        DialogProjectResourceObject IModelContainer<DialogProjectResourceObject>.Model => Item.Model;

        #region Операторы

        public static implicit operator ProjectResourceItem(ProjectReference reference)
        {
            return reference.Item;
        }
        public static implicit operator DialogProjectReference(ProjectReference reference)
        {
            return reference.Reference;
        }
        public static implicit operator ProjectReference(ProjectResourceItem item)
        {
            return Create(item);
        }

        #endregion

        #region Статика

        public static ProjectReference Create(ProjectResourceItem item)
        {
            var openReference = typeof(ProjectReference<,>);
            var itemType = DialogProjectResourceObject.GetType(item.ResourceType, true);
            var closedType = openReference.MakeGenericType(item.GetType(), itemType);

            if (Activator.CreateInstance(closedType, item) is not ProjectReference result)
            {
                throw new InvalidOperationException($"Не удалось создать ссылку для {item}");
            }

            return result;
        }

        #endregion
    }
}

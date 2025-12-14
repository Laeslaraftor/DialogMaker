using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;
using System.ComponentModel;
using System.Reflection;

namespace DialogMaker.Editor
{
    public static class EditorExtensions
    {
        public static DialogProjectReference? ToOriginalReference(object? projectReference)
        {
            if (projectReference is DialogProjectReference originalReference)
            {
                return originalReference; 
            }
            if (projectReference is ProjectReference reference)
            {
                return reference.Reference;
            }

            return null;
        }
        public static ProjectResourceItem? ToEditorItem(object? projectItem)
        {
            if (projectItem is DialogProjectResourceObject resourceObject &&
                ProjectController.TryFindController(resourceObject.Resources.Owner.Project, out var controller))
            {
                return ProjectResourceItem.Create(controller, resourceObject);
            }
            else if (projectItem is ProjectResourceItem resourceItem)
            {
                return resourceItem;
            }
            else if (projectItem is DialogProjectReference reference &&
                ProjectController.TryFindController(reference.Project, out controller))
            {
                return ProjectResourceItem.Create(controller, reference.Resolve());
            }
            else if (projectItem is ProjectReference editorReference)
            {
                return editorReference.Item;
            }

            return null;
        }

        public static string GetName(this MemberInfo info)
        {
            var name = info.GetCustomAttribute<NameAttribute>();

            if (name == null)
            {
                return string.Empty;
            }

            return name.Name;
        }
        public static string GetDescription(this MemberInfo info)
        {
            var description = info.GetCustomAttribute<DescriptionAttribute>();

            if (description == null)
            {
                return string.Empty;
            }

            return description.Description;
        }
        public static DialogResourceType GetReferenceType(this MemberInfo info)
        {
            var reference = info.GetCustomAttribute<ReferenceAttribute>();

            if (reference == null)
            {
                return DialogResourceType.String;
            }

            return reference.Type;
        }
    }
}

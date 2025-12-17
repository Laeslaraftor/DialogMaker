using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Lib;
using DialogMaker.Lib.Elements;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml.Linq;

namespace DialogMaker.Editor
{
    public static class EditorExtensions
    {
        public static DialogProjectReference? ToOriginalReference(object? projectReference)
        {
            if (projectReference == null)
            {
                return null;
            }
            if (projectReference is DialogProjectReference originalReference)
            {
                return originalReference; 
            }
            if (projectReference is ProjectReference reference)
            {
                return reference.Reference;
            }
            if (projectReference is ProjectResourceItem editorItem)
            {
                return DialogProjectReference.Create(editorItem.Model);
            }
            if (projectReference is DialogProjectResourceObject item)
            {
                return DialogProjectReference.Create(item);
            }

            return null;
        }
        public static ProjectResourceItem? ToEditorItem(object? projectItem)
        {
            var type = projectItem?.GetType();

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
        public static DialogProjectNode? ToNode(this ISelectable selectable)
        {
            if (selectable is DialogProjectNode node)
            {
                return node;
            }
            else if (selectable is DiagramNode nodeView)
            {
                return nodeView.Node;
            }

            return null;
        }

        public static bool TryGetName(this MemberInfo info, [NotNullWhen(true)] out string? result)
        {
            result = info.GetCustomAttribute<NameAttribute>()?.Name;
            return result != null;
        }
        public static string GetName(this MemberInfo info)
        {
            if (!TryGetName(info, out var name))
            {
                name = info.Name;
            }

            return name;
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

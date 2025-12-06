using DialogMaker.Core;
using DialogMaker.Core.Editor.Nodes;
using System.ComponentModel;
using System.Reflection;

namespace DialogMaker.Editor
{
    public static class EditorExtensions
    {
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

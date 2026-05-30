namespace DialogMaker.Core.Scripting
{
    internal static class EnumExtensions
    {
        public static T? GetEnumAttribute<T>(this object? enumValue) where T : Attribute
        {
            return enumValue.GetEnumAttributes<T>().FirstOrDefault();
        }
        public static List<T> GetEnumAttributes<T>(this object? enumValue) where T : Attribute
        {
            if (enumValue == null)
            {
                return [];
            }

            var enumType = enumValue.GetType();
            var valueName = enumValue.ToString();

            if (valueName == null)
            {
                return [];
            }

            var memberInfo = enumType.GetMember(valueName);
            var enumValueMemberInfo = memberInfo.FirstOrDefault(m => m.DeclaringType == enumType);
            var valueAttributes = enumValueMemberInfo?.GetCustomAttributes(typeof(T), false);

            if (valueAttributes != null && valueAttributes.Length > 0)
            {
                return [.. valueAttributes.Cast<T>()];
            }

            return [];
        }
    }
}

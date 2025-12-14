using DialogMaker.Lib.InputFields;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DialogMaker.Lib.Controllers
{
    public readonly struct EditableTypeInfo(Predicate<Type?> comparer, Func<PropertyInfo, InputField> viewFabric, Func<object?, object?>? converter = null)
            : IEquatable<Type>, IEquatable<EditableTypeInfo>
    {
        public EditableTypeInfo(Type type, Func<PropertyInfo, InputField> viewFabric, Func<object?, object?>? converter = null)
            : this(t => TypeEquals(type, t), viewFabric, converter)
        {

        }

        public Predicate<Type?> Type { get; } = comparer;
        public Func<PropertyInfo, InputField> ViewFabric { get; } = viewFabric;
        public Func<object?, object?>? Converter { get; } = converter;

        #region Управление

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, ViewFabric, Converter);
        }
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is EditableTypeInfo info &&
                   Equals(info);
        }
        public bool Equals(EditableTypeInfo other)
        {
            return other.Type == Type &&
                   other.ViewFabric == ViewFabric &&
                   other.Converter == Converter;
        }
        public readonly bool Equals(Type? other)
        {
            return Type(other);
        }

        public static bool operator ==(EditableTypeInfo i1, EditableTypeInfo i2) => i1.Equals(i2);
        public static bool operator !=(EditableTypeInfo i1, EditableTypeInfo i2) => !i1.Equals(i2);
        public static bool operator ==(EditableTypeInfo i1, Type? i2) => i1.Equals(i2);
        public static bool operator !=(EditableTypeInfo i1, Type? i2) => !i1.Equals(i2);
        public static bool operator ==(Type? i1, EditableTypeInfo i2) => i2.Equals(i1);
        public static bool operator !=(Type? i1, EditableTypeInfo i2) => !i2.Equals(i1);

        #endregion

        #region Статика

        private static bool TypeEquals(Type type, Type? other)
        {
            return other != null &&
                   (type == other ||
                   type.IsEnum && other.IsEnum ||
                   type.Name.Contains(other.Name));
        }

        #endregion
    }
}

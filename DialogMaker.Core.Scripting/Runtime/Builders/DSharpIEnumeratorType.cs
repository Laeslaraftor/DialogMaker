using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Runtime.Compilers;

namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    public class DSharpIEnumeratorType(IDSharpType type, IDSharpPropertyInfo currentProperty, IDSharpMethodInfo moveNextMethod, IDSharpMethodInfo resetMethod)
    {
        public IDSharpType Type { get; } = type;
        public IDSharpPropertyInfo CurrentProperty { get; } = currentProperty;
        public IDSharpMethodInfo MoveNextMethod { get; } = moveNextMethod;
        public IDSharpMethodInfo ResetMethod { get; } = resetMethod;

        #region Константы

        public const string TypeFullName = "System.IEnumerator";
        public const string CurrentPropertyName = "Current";
        public const string MoveNextMethodName = "MoveNext";
        public const string ResetMethodName = "Reset";

        #endregion

        #region Эталонная реализация IEnumerator

        /*
        public interface IEnumerator
        {
            public object Current { get; }

            public bool MoveNext();
            public void Reset();
        }

        Состояние по умолчанию предполагает что Current == null и внутренний счётчик равен -1, 
        а первый вызов (или вызов после Reset) должен задавать первое значение перечисления в Current,
        либо возвращать false, если объектов для перечисления нету

        private class Enumerator : IEnumerator
        {
            public Enumerator(object[] items)
            {
                _items = items;
            }

            public object Current { get; private set; }

            private readonly object[] _items;
            private int _currentIndex = -1;

            public bool MoveNext()
            {
                if (_items.Length == 0 ||
                    _currentIndex + 1 >= _items.Length)
                {
                    return false;
                }

                _currentIndex++;
                Current = _items[_currentIndex];

                return true;
            }
            public void Reset()
            {
                _currentIndex = -1;
                Current = null;
            }
        }
        */

        #endregion

        #region Статика

        public static DSharpIEnumeratorType Create(IDSharpAssembly assembly)
        {
            var type = assembly.GetType(TypeFullName);
            return Create(type);
        }
        public static DSharpIEnumeratorType Create(IDSharpType type)
        {
            var assembly = type.Assembly;
            var currentProperty = FindMember<IDSharpPropertyInfo>(type, CurrentPropertyName)
                ?? throw new ArgumentException($"Can not find public property \"{CurrentPropertyName}\" in \"{type}\"");
            var moveNextMethod = FindMember<IDSharpMethodInfo>(type, MoveNextMethodName)
                ?? throw new ArgumentException($"Can not find public method \"{MoveNextMethodName}\" in \"{type}\"");
            var resetMethod = FindMember<IDSharpMethodInfo>(type, ResetMethodName)
                ?? throw new ArgumentException($"Can not find public method \"{ResetMethodName}\" in \"{type}\"");

            if (!currentProperty.PropertyType.IsAssignableTo(assembly.ObjectType))
            {
                throw new InvalidDataException($"\"{currentProperty}\" property of \"{type}\" should return an object");
            }
            if (moveNextMethod.ReturnType != assembly.GetType(DSharpAssemblyBuilder.BoolTypeFullName))
            {
                throw new InvalidDataException($"\"{moveNextMethod}\" method of \"{type}\" should return an boolean value");
            }
            if (resetMethod.ReturnType != null)
            {
                throw new InvalidDataException($"\"{resetMethod}\" method of \"{type}\" should not return value");
            }

            return new(type, currentProperty, moveNextMethod, resetMethod);
        }

        private static T? FindMember<T>(IDSharpType type, string name)
            where T : IDSharpMemberInfo
        {
            return type.GetAllMembers(m => m is T &&
                                           m.Name == name &&
                                           m.Access == DSharpAccessModifier.Public)
                       .Cast<T>()
                       .FirstOrDefault();
        }

        #endregion
    }
}

using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Runtime.Compilers;

namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    /// <summary>
    /// Class that provides information about enumerator
    /// </summary>
    /// <param name="type">Any enumerator type</param>
    /// <param name="currentProperty">Property that contains current item</param>
    /// <param name="moveNextMethod">Method for moving to next item</param>
    /// <param name="resetMethod">Method for resetting enumerator</param>
    public class DSharpIEnumeratorType(IDSharpType type, IDSharpPropertyInfo currentProperty, IDSharpMethodInfo moveNextMethod, IDSharpMethodInfo resetMethod)
    {
        /// <summary>
        /// Enumerator type
        /// </summary>
        public IDSharpType Type { get; } = type;
        /// <summary>
        /// Property that contains current item
        /// </summary>
        public IDSharpPropertyInfo CurrentProperty { get; } = currentProperty;
        /// <summary>
        /// Method for moving to next item
        /// </summary>
        public IDSharpMethodInfo MoveNextMethod { get; } = moveNextMethod;
        /// <summary>
        /// Method for resetting enumerator
        /// </summary>
        public IDSharpMethodInfo ResetMethod { get; } = resetMethod;

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            return Type.ToString();
        }

        #endregion

        #region Константы

        /// <summary>
        /// Name of field that contains current item
        /// </summary>
        public const string CurrentPropertyName = "Current";
        /// <summary>
        /// Name of method for moving to next item
        /// </summary>
        public const string MoveNextMethodName = "MoveNext";
        /// <summary>
        /// Name of method for resetting enumerator
        /// </summary>
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

        /// <summary>
        /// Create information about enumerator interface (<see cref="TypeFullName"/>)
        /// </summary>
        /// <param name="assembly">Assembly that will be used for searching interface</param>
        /// <returns>Information about enumerator interface</returns>
        public static DSharpIEnumeratorType Create(IDSharpAssembly assembly)
        {
            var type = assembly.GetType(DSharpBuildInTypes.Extra.IEnumerator);
            return Create(type);
        }
        /// <summary>
        /// Create information about enumerator
        /// </summary>
        /// <param name="type">Enumerator type</param>
        /// <returns>Information about enumerator</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidDataException"></exception>
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

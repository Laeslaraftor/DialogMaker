using DialogMaker.Core.Common;
using System;

namespace DialogMaker.Core.Executioning.Internal
{
    public readonly struct LocalCharacter(string id, string name) : ICharacter, IEquatable<ICharacter>
    {
        public LocalCharacter(int id, string name)
            : this(id.ToString(), name)
        {
        }
        public LocalCharacter(string name)
            : this(string.Empty, name)
        {
        }

        public string Id { get; } = id;
        public string Name { get; } = name;
        public DialogResourceType ResourceType => DialogResourceType.Character;
        public bool IsSeparated => true;

        #region Управление

        public DialogItemReference CreateReference()
        {
            return DialogItemReference.Create(this);
        }
        public ResourcePath GetPath()
        {
            throw new InvalidOperationException(IResourceItem.GetPathExceptionMessage);
        }
        public IVariable ToVariable()
        {
            return new LocalVariable(Id, Name);
        }

        public override string ToString()
        {
            return $"[{Id}] {Name}";
        }
        public override bool Equals(object obj)
        {
            return obj is ICharacter character && Equals(character);
        }
        public bool Equals(ICharacter other)
        {
            return Id == other.Id &&
                   Name == other.Name &&
                   ResourceType == other.ResourceType &&
                   IsSeparated == other.IsSeparated;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, IsSeparated, ResourceType);
        }

        #endregion

        #region Операторы

        public static bool operator ==(LocalCharacter c1, LocalCharacter c2) => c1.Equals(c2);
        public static bool operator !=(LocalCharacter c1, LocalCharacter c2) => !c1.Equals(c2);
        public static bool operator ==(ICharacter c1, LocalCharacter c2) => c2.Equals(c1);
        public static bool operator !=(ICharacter c1, LocalCharacter c2) => !c2.Equals(c1);
        public static bool operator ==(LocalCharacter c1, ICharacter c2) => c1.Equals(c2);
        public static bool operator !=(LocalCharacter c1, ICharacter c2) => !c1.Equals(c2);

        #endregion

        #region Статика

        public static readonly LocalCharacter Empty = new(string.Empty);

        #endregion
    }
}

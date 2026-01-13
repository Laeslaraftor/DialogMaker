using System;

namespace DialogMaker.Core.Executioning
{
    public readonly struct DialogPosition(int section, int operation) : IEquatable<DialogPosition>, IComparable, IComparable<DialogPosition>
    {
        public DialogPosition(int section)
            : this(section, 0)
        {
        }

        public int Section { get; } = section;
        public int Operation { get; } = operation;

        #region Управление

        public int CompareTo(DialogPosition other)
        {
            if (Section > other.Section ||
                (Section == other.Section &&
                Operation > other.Operation))
            {
                return 1;
            }
            if (Section == other.Section && 
                Operation == other.Operation)
            {
                return 0;
            }

            return -1;
        }
        public int CompareTo(object obj)
        {
            if (obj is DialogPosition other)
            {
                return CompareTo(other);
            }

            return -1;
        }

        public override bool Equals(object obj)
        {
            return obj is DialogPosition other &&
                   Equals(other);
        }
        public bool Equals(DialogPosition other)
        {
            return Section == other.Section &&
                   Operation == other.Operation;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Section, Operation);
        }
        public override string ToString()
        {
            return $"[{Section}, {Operation}]";
        }

        #endregion

        #region Управление

        public static readonly DialogPosition Empty = new(-1, -1);

        #endregion
    }
}

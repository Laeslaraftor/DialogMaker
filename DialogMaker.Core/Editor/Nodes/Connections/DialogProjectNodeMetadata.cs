using System;

namespace DialogMaker.Core.Editor.Nodes
{
    public readonly struct DialogProjectNodeMetadata(string name, string description, IComparable? sortValue) 
        : IEquatable<DialogProjectNodeMetadata>, IComparable, IComparable<DialogProjectNodeMetadata>
    {
        public DialogProjectNodeMetadata(string name, string description)
            : this(name, description, null)
        {
        }

        public string Name { get; } = name;
        public string Description { get; } = description;
        public IComparable? SortValue { get; } = sortValue;

        #region Управление

        public int CompareTo(DialogProjectNodeMetadata other)
        {
            if (SortValue != null)
            {
                if (other.SortValue == null)
                {
                    return 1;
                }

                return other.CompareTo(other.SortValue);
            }

            return Name.CompareTo(other.Name);
        }
        public int CompareTo(object obj)
        {
            if (obj is DialogProjectNodeMetadata other)
            {
                return CompareTo(other);
            }
            if (Name == null)
            {
                return -1;
            }

            return Name.CompareTo(obj);
        }
        public override bool Equals(object obj)
        {
            return obj is DialogProjectNodeMetadata metadata &&
                   Equals(metadata);
        }
        public bool Equals(DialogProjectNodeMetadata other)
        {
            return Name == other.Name && 
                   Description == other.Description &&
                   Equals(SortValue, other.SortValue);
        }
        public override string ToString()
        {
            string result = Name;

            if (!string.IsNullOrEmpty(Description))
            {
                result += $" {Description}";
            }

            return result;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Description, SortValue);
        }

        #endregion
    }
}

using System;

namespace DialogMaker.Core.Editor.Nodes
{
    public readonly struct DialogProjectNodeMetadata(string name, string description) 
        : IEquatable<DialogProjectNodeMetadata>
    {
        public string Name { get; } = name;
        public string Description { get; } = description;

        public readonly override bool Equals(object obj)
        {
            return obj is DialogProjectNodeMetadata metadata &&
                   Equals(metadata);
        }
        public readonly bool Equals(DialogProjectNodeMetadata other)
        {
            return Name == other.Name && 
                   Description == other.Description;
        }
        public readonly override string ToString()
        {
            string result = Name;

            if (!string.IsNullOrEmpty(Description))
            {
                result += $" {Description}";
            }

            return result;
        }
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Name, Description);
        }
    }
}

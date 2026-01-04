using DialogMaker.Core.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DialogMaker.Core.Executioning.Internal
{
    public readonly struct LocalStringCollection(string id, IList<IResourceString> strings) : IStringCollection
    {
        public string Id { get; } = id;
        public DialogResourceType ResourceType { get; } = DialogResourceType.String;
        public ReadOnlyCollection<IResourceString> Strings { get; } = new(strings);

        #region Управление

        public readonly override bool Equals(object obj)
        {
            return obj is IStringCollection other &&
                   Id == other.Id &&
                   Strings == other.Strings;
        }
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Id, Strings);
        }
        public readonly override string ToString()
        {
            if (Strings.Count == 0)
            {
                return $"[{Id}]";
            }

            string result = string.Empty;

            foreach (var value in Strings)
            {
                if (result != string.Empty)
                {
                    result += ", ";
                }

                result += value.ToString();
            }

            return $"[{Id}]: {result}";
        }

        #endregion
    }
}

using System.Reflection;

namespace DialogMaker.Core.Editor.Nodes
{
    public class SortAttribute : Attribute, IComparable
    {
        public SortAttribute(int sortValue)
        {
            SortValue = sortValue;
        }
        public SortAttribute(string sortValue)
        {
            SortValue = sortValue;
        }

        public IComparable SortValue { get; }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            if (obj is SortAttribute other)
            {
                return SortValue.CompareTo(other.SortValue);
            }
            if (obj is MemberInfo info)
            {
                other = info.GetCustomAttribute<SortAttribute>();

                if (other != null)
                {
                    return SortValue.CompareTo(other.SortValue);
                }

                return -1;
            }

            return SortValue.CompareTo(obj);
        }
    }
}

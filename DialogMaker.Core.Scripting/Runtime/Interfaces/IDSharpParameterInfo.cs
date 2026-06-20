namespace DialogMaker.Core.Scripting.Runtime
{
    public interface IDSharpParameterInfo
    {
        public string Name { get; }
        public IDSharpType Type { get; }

        #region Классы

        public sealed class Comparer : IEqualityComparer<IDSharpParameterInfo>
        {
            public bool Equals(IDSharpParameterInfo x, IDSharpParameterInfo y)
            {
                return x.Type == y.Type;
            }
            public int GetHashCode(IDSharpParameterInfo obj)
            {
                return obj.Type.GetHashCode();
            }

            #region Статика

            public static readonly Comparer Instance = new();

            #endregion
        }

        #endregion
    }
}

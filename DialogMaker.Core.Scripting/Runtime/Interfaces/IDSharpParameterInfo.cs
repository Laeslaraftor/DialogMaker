namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Interface of any parameter
    /// </summary>
    public interface IDSharpParameterInfo
    {
        /// <summary>
        /// Name of parameter
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Type of parameter
        /// </summary>
        public IDSharpType Type { get; }
        /// <summary>
        /// Parameter mode
        /// </summary>
        public DSharpMethodParameterMode Mode { get; }

        #region Классы

        /// <summary>
        /// Method parameter comparer implementation
        /// </summary>
        public sealed class Comparer : IEqualityComparer<IDSharpParameterInfo>
        {
            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="x"><inheritdoc/></param>
            /// <param name="y"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public bool Equals(IDSharpParameterInfo x, IDSharpParameterInfo y)
            {
                return x.Type == y.Type &&
                       x.Mode == y.Mode;
            }
            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="obj"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public int GetHashCode(IDSharpParameterInfo obj)
            {
                return HashCode.Combine(obj.Type, obj.Mode);
            }

            #region Статика

            /// <summary>
            /// Global instance of method parameter comparer
            /// </summary>
            public static readonly Comparer Instance = new();

            #endregion
        }

        #endregion
    }
}

using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    /// <summary>
    /// Class that provides information about array type
    /// </summary>
    /// <param name="type">Any array type</param>
    /// <param name="indexer">Items indexer</param>
    public class DSharpArrayType(IDSharpType type, IDSharpIndexerInfo indexer, IDSharpMethodInfo indexerSetter, IDSharpMethodInfo indexerGetter)
    {
        /// <summary>
        /// Array type
        /// </summary>
        public IDSharpType Type { get; } = type;
        /// <summary>
        /// Items indexer
        /// </summary>
        public IDSharpIndexerInfo Indexer { get; } = indexer;
        /// <summary>
        /// Indexer setter method
        /// </summary>
        public IDSharpMethodInfo IndexerSetter { get; } = indexerSetter;
        /// <summary>
        /// Indexer getter method
        /// </summary>
        public IDSharpMethodInfo IndexerGetter { get; } = indexerGetter;

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

        #region Статика

        /// <summary>
        /// Create information about default array type (<see cref="TypeFullName"/>)
        /// </summary>
        /// <param name="assembly">Assembly that will be used for searching array type</param>
        /// <returns>Information about array type</returns>
        public static DSharpArrayType Create(IDSharpAssembly assembly)
        {
            var type = assembly.GetType(DSharpBuildInTypes.Extra.Array);
            return Create(type);
        }
        /// <summary>
        /// Create information about provided array type
        /// </summary>
        /// <param name="type">Array type</param>
        /// <returns>Information about array type</returns>
        public static DSharpArrayType Create(IDSharpType type)
        {
            var indexer = GetIndexer(type);

            if (indexer.Setter == null)
            {
                throw new ArgumentException($"Unable to get indexer setter at \"{type}\"", nameof(type));
            }
            if (indexer.Getter == null)
            {
                throw new ArgumentException($"Unable to get indexer getter at \"{type}\"", nameof(type));
            }

            return new(type, indexer, indexer.Setter, indexer.Getter);
        }
        /// <summary>
        /// Find items indexer in specified array type
        /// </summary>
        /// <param name="type">Array type that will be used for searching items indexer</param>
        /// <returns>Items indexer</returns>
        public static IDSharpIndexerInfo GetIndexer(IDSharpType type)
        {
            var intType = type.Assembly.GetType(DSharpBuildInTypes.Int);
            return type.GetIndexer(intType);
        }

        #endregion
    }
}

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Information about <c>System.Enum</c> type
    /// </summary>
    public class DSharpEnumType(IDSharpType type, IDSharpPropertyInfo nameProperty, IDSharpFieldInfo idField)
    {
        /// <summary>
        /// Enum type
        /// </summary>
        public IDSharpType Type { get; } = type;
        /// <summary>
        /// Property that contains value name
        /// </summary>
        //public IDSharpPropertyInfo NameProperty { get; } = nameProperty;
        /// <summary>
        /// Field that contains internal value id
        /// </summary>
        //public IDSharpFieldInfo IdField { get; } = idField;

        #region Static

        /// <summary>
        /// Create information about <c>System.Enum</c> type
        /// </summary>
        /// <param name="assembly">Assembly for searching enum type</param>
        /// <returns>Information about <c>System.Enum</c> type</returns>
        public static DSharpEnumType Create(IDSharpAssembly assembly)
        {
            var type = assembly.GetType(DSharpBuildInTypes.Extra.Enum);
            //var nameProperty = type.GetProperty("Name");
            //var idField = type.GetField("_id");

            return new(type, null, null);
        }

        #endregion
    }
}

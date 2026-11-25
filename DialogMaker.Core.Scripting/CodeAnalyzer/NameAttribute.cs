namespace DialogMaker.Core.Scripting.CodeAnalyzer
{
    /// <summary>
    /// Attribute for setting name that will be used by code analyzer instead of declared name
    /// </summary>
    /// <param name="name">Name that will be used by code analyzer instead of declared name</param>
    [AttributeUsage(AttributeTargets.All)]
    internal sealed class NameAttribute(string name) : Attribute
    {
        /// <summary>
        /// Name that will be used by code analyzer instead of declared name
        /// </summary>
        public string Name { get; } = name;
    }
}

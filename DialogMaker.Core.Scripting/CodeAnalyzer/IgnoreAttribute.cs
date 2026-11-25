namespace DialogMaker.Core.Scripting.CodeAnalyzer
{
    /// <summary>
    /// Attribute that indicates when member should be ignored by code analyzer
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    internal sealed class IgnoreAttribute : Attribute
    {
    }
}

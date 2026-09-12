namespace DialogMaker.Core.Scripting.CodeAnalyzer
{
    /// <summary>
    /// Mark enum to generate helper type that contains it's values.
    /// That type will be named as [EnumName]Helper
    /// </summary>
    /// <param name="information">Type of information that need to be generated</param>
    [AttributeUsage(AttributeTargets.Enum)]
    internal class GenerateInformationAttribute(EnumValuesInformation information) : Attribute
    {
        /// <summary>
        /// Type of information that need to be generated
        /// </summary>
        public EnumValuesInformation Information { get; } = information;
    }
}

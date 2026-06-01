namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Type attributes
    /// </summary>
    [Flags]
    public enum DSharpTypeAttributes
    {
        /// <summary>
        /// Empty attribute
        /// </summary>
        None = 0x0000,
        /// <summary>
        /// Flag that marks type as class
        /// </summary>
        Class = 0x0001,
        /// <summary>
        /// Flag that marks type as struct
        /// </summary>
        Struct = 0x0002,
        /// <summary>
        /// Flag that marks type as enum
        /// </summary>
        Enum = 0x0004,
        /// <summary>
        /// Flag that marks type as interface
        /// </summary>
        Interface = 0x0008,

        /// <summary>
        /// Flag that set public access modifier
        /// </summary>
        Public = 0x0010,
        /// <summary>
        /// Flag that set private access modifier
        /// </summary>
        Private = 0x0020,
        /// <summary>
        /// Flag that set internal access modifier
        /// </summary>
        Internal = 0x0040,
        /// <summary>
        /// Flag that set internal access modifier
        /// </summary>
        Protected = 0x0050,

        /// <summary>
        /// Flag that set marks type as abstract
        /// </summary>
        Abstract = 0x0080,
        /// <summary>
        /// Flag that set marks type as sealed
        /// </summary>
        Sealed = 0x0100,
        /// <summary>
        /// Flag that set marks type as static
        /// </summary>
        Static = 0x0200,
    }
}

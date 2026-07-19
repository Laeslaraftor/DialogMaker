namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Type of memory block that used by D# virtual machine
    /// </summary>
    public enum DSharpMemoryBlockType
    {
        /// <summary>
        /// Free memory block
        /// </summary>
        Free,
        /// <summary>
        /// Memory block that contains instance of D# object
        /// </summary>
        Object,
        /// <summary>
        /// Memory block that used for D# stack
        /// </summary>
        Stack,
        /// <summary>
        /// Memory block that contains some D# runtime type information
        /// </summary>
        TypeInformation,
        /// <summary>
        /// Memory block that contains bytecode
        /// </summary>
        Bytecode
    }
}

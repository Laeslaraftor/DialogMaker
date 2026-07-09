using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# object
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DSharpObject
    {
        /// <summary>
        /// Object type
        /// </summary>
        public DSharpRuntimeTypeInfo* Type;
        /// <summary>
        /// Size for managed data, this not includes size of DSharpObject structure size.
        /// This contains length if type is array
        /// </summary>
        public int Size;
        /// <summary>
        /// Count of references to this object
        /// </summary>
        public uint ReferencesCount;
    }
}

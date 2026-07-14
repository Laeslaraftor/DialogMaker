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
        /// Total size of object
        /// </summary>
        public readonly int TotalSize => sizeof(DSharpObject) + Size;

        /// <summary>
        /// Object type
        /// </summary>
        public DSharpRuntimeTypeInfo* Type;
        /// <summary>
        /// Size for managed data, this not includes size of DSharpObject structure size.
        /// </summary>
        public int Size;
        /// <summary>
        /// Length if type is array
        /// </summary>
        public int Length;
        /// <summary>
        /// Count of references to this object
        /// </summary>
        public uint ReferencesCount;

        /// <summary>
        /// Copy source object to destination
        /// </summary>
        /// <param name="source">Object for copying values</param>
        /// <param name="destination">Object for writing copied values</param>
        public static void Copy(DSharpObject* source, DSharpObject* destination)
        {
            destination->Type = source->Type;
            destination->Size = source->Size;
            destination->Length = source->Length;
            destination->ReferencesCount = 0;

            byte* sourceData = (byte*)(source + sizeof(DSharpObject));
            byte* destinationData = (byte*)(destination + sizeof(DSharpObject));

            for (int i = 0; i < source->Size; i++)
            {
                destinationData[i] = sourceData[i];
            }
        }
    }
}

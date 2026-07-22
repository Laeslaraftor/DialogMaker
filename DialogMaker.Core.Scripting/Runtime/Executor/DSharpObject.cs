using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;
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
        /// Is object placed in heap 
        /// </summary>
        public readonly bool IsReferenceObject => Placement == DSharpObjectPlacement.Heap;

        /// <summary>
        /// Object type
        /// </summary>
        public DSharpRuntimeTypeInfo* Type;
        /// <summary>
        /// Object placement. Heap contains refence types and boxed structures,
        /// stack can contains only structures
        /// </summary>
        public DSharpObjectPlacement Placement;
        /// <summary>
        /// Is current object instance initialized.
        /// It sets <c>true</c> after calling initializer.
        /// If type have not initializer it always <c>true</c>
        /// </summary>
        public bool IsInitialized;
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

        public override string ToString()
        {
            if (Type != null)
            {
                return Type->ToString();
            }

            return base.ToString() ?? string.Empty;
        }

        #region Static

        /// <summary>
        /// Null pointer to D# object;
        /// </summary>
        public static readonly DSharpObject* Null = (DSharpObject*)0;

        /// <summary>
        /// Copy source object to destination
        /// </summary>
        /// <param name="source">Object for copying values</param>
        /// <param name="destination">Object for writing copied values</param>
        public static void Copy(DSharpObject* source, DSharpObject* destination)
        {
            *destination = *source;
            byte* sourceData = GetData(source);
            byte* destinationData = GetData(destination);

            Buffer.MemoryCopy(sourceData, destinationData, source->Size, destination->Size);
        }
        /// <summary>
        /// Copy source object to destination
        /// </summary>
        /// <param name="source">Object for copying values</param>
        /// <param name="destinationBuffer">Buffer for writing source object</param>
        /// <param name="copyReferenceTypeAsPointer">
        /// If this sets as <c>true</c> and source object is reference type then it address will be wrote into provided buffer,
        /// otherwise whole object will be copied
        /// </param>
        /// <returns>Is object copied successfully</returns>
        public static bool Copy(DSharpObject* source, UnmanagedArray<byte> destinationBuffer, bool copyReferenceTypeAsPointer = true)
        {
            if (source->IsReferenceObject && copyReferenceTypeAsPointer)
            {
                if (sizeof(nint) > destinationBuffer.Length)
                {
                    return false;
                }

                nint address = (nint)source;
                var addressBuffer = (byte*)&address;
                var buffer = destinationBuffer.GetItemReference(0);

                Buffer.MemoryCopy(addressBuffer, buffer, destinationBuffer.Length, sizeof(nint));

                return true;
            }
            if (sizeof(DSharpObject*) > destinationBuffer.Length)
            {
                return false;
            }

            var destination = (DSharpObject*)destinationBuffer.GetItemReference(0);
            *destination = *source;

            int dataBufferSize = destinationBuffer.Length - sizeof(DSharpObject);

            if (0 > dataBufferSize)
            {
                return true;
            }

            byte* sourceData = GetData(source);
            byte* destinationData = GetData(destination);

            Buffer.MemoryCopy(sourceData, destinationData, source->Size, dataBufferSize);

            return true;
        }
        /// <summary>
        /// Is null pointer to object or object data is null
        /// </summary>
        /// <param name="obj">Pointer to object</param>
        /// <returns>Is null or empty</returns>
        public static bool IsNullOrEmpty(DSharpObject* obj)
        {
            if (obj == null || obj->Size == 0)
            {
                return true;
            }

            var data = GetData(obj);

            for (int i = 0; i < obj->Size; i++)
            {
                if (data[i] != 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get pointer to D# object data
        /// </summary>
        /// <param name="obj">D# object instance</param>
        /// <returns>Pointer to D# object data</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte* GetData(DSharpObject* obj)
        {
            return (byte*)obj + sizeof(DSharpObject);
        }

        #endregion
    }
}

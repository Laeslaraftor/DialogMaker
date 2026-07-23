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
        /// Is object placed in heap 
        /// </summary>
        public readonly bool IsReferenceObject => Attributes.HasFlag(DSharpObjectAttributes.StoredInHeap);
        /// <summary>
        /// Object placement. Heap contains refence types and boxed structures,
        /// stack can contains only structures
        /// </summary>
        public DSharpObjectPlacement Placement
        {
            readonly get
            {
                if (Attributes.HasFlag(DSharpObjectAttributes.StoredInBuffer))
                {
                    return DSharpObjectPlacement.Buffer;
                }

                return DSharpObjectPlacement.Heap;
            }
            set
            {
                var opposite = DSharpObjectAttributes.StoredInHeap;

                if (value == DSharpObjectPlacement.Heap)
                {
                    opposite = DSharpObjectAttributes.StoredInBuffer;
                }

                Attributes = (Attributes & ~opposite) | (DSharpObjectAttributes)value;
            }
        }
        /// <summary>
        /// Is current object instance initialized.
        /// It sets <c>true</c> after calling initializer.
        /// If type have not initializer it always <c>true</c>
        /// </summary>
        public bool IsInitialized
        {
            readonly get => Attributes.HasFlag(DSharpObjectAttributes.Initialized);
            set
            {
                if (value)
                {
                    Attributes |= DSharpObjectAttributes.Initialized;
                }
                else
                {
                    Attributes &= ~DSharpObjectAttributes.Initialized;
                }
            }
        }
        /// <summary>
        /// Is current object array (<c>string</c> also count as array)
        /// </summary>
        public bool IsArray
        {
            readonly get => Attributes.HasFlag(DSharpObjectAttributes.Array);
            set
            {
                if (value)
                {
                    Attributes |= DSharpObjectAttributes.Array;
                }
                else
                {
                    Attributes &= ~DSharpObjectAttributes.Array;
                }
            }
        }
        /// <summary>
        /// Is current object <c>string</c>
        /// </summary>
        public readonly bool IsString => Attributes.HasFlag(DSharpObjectAttributes.String);

        /// <summary>
        /// Object type
        /// </summary>
        public DSharpRuntimeTypeInfo* Type;
        /// <summary>
        /// Object attributes
        /// </summary>
        public DSharpObjectAttributes Attributes;
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
            if (source->IsArray)
            {
                *(DSharpArray*)destination = *(DSharpArray*)source;
            }
            else
            {
                *destination = *source;
            }

            byte* sourceData = GetData(source);
            byte* destinationData = GetData(destination);
            var sourceSize = GetSize(source);
            var destinationSize = GetSize(destination);

            Buffer.MemoryCopy(sourceData, destinationData, sourceSize, destinationSize);
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
            var sourceSize = GetSize(source);

            Buffer.MemoryCopy(sourceData, destinationData, sourceSize, dataBufferSize);

            return true;
        }
        /// <summary>
        /// Is null pointer to object or object data is null
        /// </summary>
        /// <param name="obj">Pointer to object</param>
        /// <returns>Is null or empty</returns>
        public static bool IsNullOrEmpty(DSharpObject* obj)
        {
            if (obj == null)
            {
                return true;
            }

            var size = GetSize(obj);

            if (size == 0)
            {
                return true;
            }

            var data = GetData(obj);

            for (int i = 0; i < size; i++)
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
            if (obj->IsArray)
            {
                return (byte*)obj + sizeof(DSharpArray);
            }

            return (byte*)obj + sizeof(DSharpObject);
        }
        /// <summary>
        /// Get pointer to D# object data
        /// </summary>
        /// <param name="obj">D# object instance</param>
        /// <returns>Pointer to D# object data</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* GetData<T>(DSharpObject* obj) where T : unmanaged
        {
            return (T*)GetData(obj);
        }
        /// <summary>
        /// Get size for managed data, this not includes size of DSharpObject structure size. 
        /// </summary>
        /// <param name="obj">D# object instance for getting it's size</param>
        /// <returns>Size for managed data, this not includes size of DSharpObject structure size.</returns>
        public static int GetSize(DSharpObject* obj)
        {
            if (obj->IsString)
            {
                return sizeof(char) * DSharpArray.GetLength(obj);
            }
            if (obj->IsArray)
            {
                return ((DSharpArray*)obj)->Size;
            }

            return obj->Type->Size;
        }
        /// <summary>
        /// Get D# object total size (size for data + object structure size)
        /// </summary>
        /// <param name="obj">D# object for calculating it's total size</param>
        /// <returns>D# object total size</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetTotalSize(DSharpObject* obj)
        {
            var size = GetSize(obj);

            if (obj->IsArray)
            {
                size += sizeof(DSharpArray);
            }
            else
            {
                size += sizeof(DSharpObject);
            }

            return size;
        }

        #endregion
    }
}

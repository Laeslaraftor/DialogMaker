using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo
{
    /// <summary>
    /// Runtime information about field
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DSharpRuntimeFieldInfo
    {
        /// <summary>
        /// Field metadata token
        /// </summary>
        public DSharpMetadataToken MetadataToken;
        /// <summary>
        /// Is field static
        /// </summary>
        public bool IsStatic;
        /// <summary>
        /// Type that declares current field
        /// </summary>
        public DSharpRuntimeTypeInfo* DeclaringType;
        /// <summary>
        /// Type of value that contains in field
        /// </summary>
        public DSharpRuntimeTypeInfo* FieldType;
        /// <summary>
        /// Data offset in bytes
        /// </summary>
        public int Offset;

        #region Controls

        /// <summary>
        /// Read field value to managed byte array
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <returns>Field value as byte array</returns>
        public byte[] Read(DSharpObject* instance)
        {
            byte[] buffer = new byte[FieldType->ItemSize];
            byte* pointer = GetDataPointer(instance);

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = pointer[i];
            }

            return buffer;
        }
        /// <summary>
        /// Read field value to unmanaged byte array
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="buffer">Unmanaged array to write field value</param>
        public void Read(DSharpObject* instance, UnmanagedArray<byte> buffer)
        {
            int size = FieldType->ItemSize;
            byte* pointer = GetDataPointer(instance);

            for (int i = 0; i < Math.Min(size, buffer.Length); i++)
            {
                buffer[i] = pointer[i];
            }
        }
        /// <summary>
        /// Read field value and push it to stack
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="stack">Stack for pushing value</param>
        public void Read(DSharpObject* instance, DSharpStack stack)
        {
            byte* pointer = GetDataPointer(instance);

            if (!FieldType->IsValueType)
            {
                nint address = *(nint*)pointer;
                stack.PushReference(address);
                return;
            }

            var size = FieldType->Size;
            var frame = stack.PushStructure(FieldType, new(pointer, size));
        }

        /// <summary>
        /// Write data from managed array to field
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="data">Data for writing to field</param>
        public void Write(DSharpObject* instance, byte[] data)
        {
            var size = FieldType->Size;
            byte* pointer = GetDataPointer(instance);

            for (int i = 0; i < Math.Min(size, data.Length); i++)
            {
                pointer[i] = data[i];
            }
        }
        /// <summary>
        /// Write data from unmanaged array to field
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="data">Data for writing to field</param>
        public void Write(DSharpObject* instance, UnmanagedArray<byte> buffer)
        {
            int size = FieldType->ItemSize;
            byte* pointer = GetDataPointer(instance);

            for (int i = 0; i < Math.Min(size, buffer.Length); i++)
            {
                pointer[i] = buffer[i];
            }
        }
        /// <summary>
        /// Write current stack value to field
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="stack">Stack for writing it's current value to field</param>
        public void Write(DSharpObject* instance, DSharpStack stack)
        {
            int size = FieldType->ItemSize;
            byte* pointer = GetDataPointer(instance);
            DSharpStack.FrameInfo? frame = null;

            if (stack.Count != 0)
            {
                frame = stack.Peek();
            }

            if (frame == null || frame.Value.Size == 0)
            {
                for (int i = 0; i < size; i++)
                {
                    pointer[i] = 0;
                }

                return;
            }

            for (int i = 0; i < Math.Min(size, frame.Value.Size); i++)
            {
                pointer[i] = frame.Value[i];
            }
        }

        private byte* GetDataPointer(DSharpObject* instance)
        {
            byte* data;

            if (IsStatic)
            {
                data = DeclaringType->StaticFieldsData.AsPointer();
            }
            else
            {
                data = (byte*)instance;
            }

            return data + Offset;
        }

        #endregion
    }
}

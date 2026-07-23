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
        /// Field name
        /// </summary>
        public UnmanagedArray<char> Name;
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

        #region Controls

        /// <summary>
        /// Is field null at specified object
        /// </summary>
        /// <param name="instance">D# object instance to check field</param>
        /// <returns>Is field null</returns>
        public bool IsNull(DSharpObject* instance)
        {
            var data = GetDataPointer(instance);

            if (FieldType->IsValueType)
            {
                return RuntimeExtensions.IsEmpty(data, FieldType->ItemSize);
            }

            return *(nint*)data == 0;
        }

        /// <summary>
        /// Read field value to managed byte array
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <returns>Field value as byte array</returns>
        public DSharpObject* Read(DSharpObject* instance)
        {
            if (IsNull(instance))
            {
                return null;
            }

            return GetValuePointer(instance);
        }
        /// <summary>
        /// Read field value to unmanaged byte array
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="buffer">Unmanaged array to write field value</param>
        public void Read(DSharpObject* instance, UnmanagedArray<byte> buffer)
        {
            if (IsNull(instance))
            {
                buffer.Fill(0);
                return;
            }

            var value = GetValuePointer(instance);
            DSharpObject.Copy(value, buffer);
        }
        /// <summary>
        /// Read field value and push it to stack
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="stack">Stack for pushing value</param>
        public void Read(DSharpObject* instance, DSharpStack stack)
        {
            var value = GetValuePointer(instance);
            bool isNull = IsNull(instance);

            if (FieldType->IsValueType)
            {
                if (isNull)
                {
                    stack.PushStructure(FieldType, new(0, 0));
                }
                else
                {
                    stack.PushStructure(value, false);
                }

                return;
            }
            if (isNull)
            {
                stack.PushNull();
                return;
            }

            stack.PushReference(value);
        }

        /// <summary>
        /// Write data from managed array to field
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="data">Data for writing to field</param>
        public void Write(DSharpObjectsContainer objectsContainer, DSharpObject* instance, DSharpObject* value)
        {
            byte* pointer = GetDataPointer(instance);
            bool valueIsNull = DSharpObject.IsNullOrEmpty(value);
            bool isReferenceTypeField = !FieldType->IsValueType;

            if (isReferenceTypeField)
            {
                var currentValue = Read(instance);

                if (currentValue != null)
                {
                    currentValue->ReferencesCount--;
                }
            }
            if (valueIsNull)
            {
                if (isReferenceTypeField)
                {
                    *(nint*)pointer = 0;
                }
                else
                {
                    RuntimeExtensions.FillZero(pointer, FieldType->ItemSize);
                }
            }
            else
            {
                if (isReferenceTypeField)
                {
                    if (value->Placement == DSharpObjectPlacement.Buffer)
                    {
                        value = objectsContainer.Box(value);
                    }

                    *(nint*)pointer = (nint)value;
                    value->ReferencesCount++;
                }
                else
                {
                    DSharpObject.Copy(value, (DSharpObject*)pointer);
                }
            }
        }
        /// <summary>
        /// Write data from unmanaged array to field
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="data">Data for writing to field</param>
        public void Write(DSharpObjectsContainer objectsContainer, DSharpObject* instance, UnmanagedArray<byte> buffer)
        {
            if (buffer.Length == 0)
            {
                Write(objectsContainer, instance, DSharpObject.Null);
                return;
            }

            DSharpObject* obj = (DSharpObject*)buffer.GetItemReference(0);
            Write(objectsContainer, instance, obj);
        }
        /// <summary>
        /// Write current stack value to field
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="stack">Stack for writing it's current value to field</param>
        public void Write(DSharpObjectsContainer objectsContainer, DSharpObject* instance, DSharpStack stack)
        {
            var frame = stack.Peek(0);

            if (frame.ValueType == DSharpStackValueType.Structure)
            {
                Write(objectsContainer, instance, (DSharpObject*)frame.StackPointer);
            }
            else if (frame.ValueType == DSharpStackValueType.Reference)
            {
                Write(objectsContainer, instance, (DSharpObject*)frame.ReadReference());
            }
            else if (frame.ValueType == DSharpStackValueType.Null)
            {
                Write(objectsContainer, instance, DSharpObject.Null);
            }
            else
            {
                throw new InvalidOperationException($"Unable to write value to field from stack with type \"{frame.ValueType}\"");
            }
        }

        public readonly override string ToString()
        {
            if (Name.Length == 0)
            {
                return "Nameless field";
            }

            return new((ReadOnlySpan<char>)Name);
        }

        private int GetOffset(DSharpObject* instance)
        {
            if (IsStatic)
            {
                if (DeclaringType->TryGetFieldOffset(MetadataToken, out var offset))
                {
                    return offset;
                }

                throw new InvalidOperationException($"Unable to find data offset for static field: {MetadataToken}");
            }
            if (instance == null)
            {
                throw new ArgumentException($"Unable to get instance field offset without object instance: {MetadataToken}");
            }
            if (instance->Type->TryGetFieldOffset(MetadataToken, out var instanceOffset))
            {
                return instanceOffset;
            }

            throw new InvalidOperationException($"Unable to get instance field ({MetadataToken}) offset at {instance->Type->ToString()}");
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

            int offset = GetOffset(instance);

            return data + offset;
        }
        private DSharpObject* GetValuePointer(DSharpObject* instance)
        {
            byte* data = GetDataPointer(instance);

            if (FieldType->IsValueType)
            {
                return (DSharpObject*)data;
            }

            return *(DSharpObject**)data;
        }

        #endregion

        #region Static

        /// <summary>
        /// Get size that requires for structure with information about specified field
        /// </summary>
        /// <param name="field">Field to calculate size</param>
        /// <returns>Size of structure with information about specified field</returns>
        public static int GetSize(IDSharpFieldInfo field)
        {
            return sizeof(DSharpRuntimeFieldInfo) +
                   field.Name.Length * sizeof(char);
        }

        #endregion
    }
}

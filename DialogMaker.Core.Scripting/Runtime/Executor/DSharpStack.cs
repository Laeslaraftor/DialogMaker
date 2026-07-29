using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# stack implementation
    /// </summary>
    /// <param name="memoryManager">Memory manager for allocating memory for stack</param>
    /// <param name="stackCapacity">Stack capacity in frames</param>
    public unsafe class DSharpStack(DSharpVmMemoryManager memoryManager, DSharpRuntimeInformationProvider runtimeInformationProvider, int stackCapacity)
        : Disposable, IEnumerable<DSharpStack.FrameInfo>
    {
        /// <summary>
        /// Create new instance of D# stack
        /// </summary>
        /// <param name="memoryManager">Memory manager for allocating memory for stack</param>
        public DSharpStack(DSharpVmMemoryManager memoryManager, DSharpRuntimeInformationProvider runtimeInformationProvider)
            : this(memoryManager, runtimeInformationProvider, DSharpThread.DefaultStackCapacity)
        {
        }

        /// <summary>
        /// Capacity in frames
        /// </summary>
        public int Capacity { get; } = stackCapacity;
        /// <summary>
        /// Current amount of elements in stack
        /// </summary>
        public uint Count => (uint)(_frameIndex + 1);

        private readonly DSharpRuntimeInformationProvider _runtimeInformationProvider = runtimeInformationProvider;
        private readonly DSharpVmMemoryManager _memoryManager = memoryManager;
        private readonly FrameInfo* _frames = (FrameInfo*)memoryManager.Allocate(DSharpMemoryBlockType.Stack, sizeof(FrameInfo) * stackCapacity);
        private readonly byte* _stack = (byte*)memoryManager.Allocate(DSharpMemoryBlockType.Stack, stackCapacity * 1024);
        private int _frameIndex = -1;
        private int _allocatedStackSize;
        private int _currentStackCapacity = stackCapacity * 1024;

        #region Controls

        public FrameInfo PeekOnlyValues(uint offset = 0)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DSharpStack), "Stack has been disposed");
            }
            if (_frameIndex == -1)
            {
                throw new InvalidOperationException("Stack is empty");
            }

            int index = _frameIndex;

            while (index >= 0)
            {
                var frame = _frames[index];

                if (frame.ValueType == DSharpStackValueType.Null ||
                    frame.ValueType == DSharpStackValueType.Reference ||
                    frame.ValueType == DSharpStackValueType.Structure)
                {
                    if (offset == 0)
                    {
                        return frame;
                    }

                    offset--;
                }

                index--;
            }

            throw new IndexOutOfRangeException();
        }
        public FrameInfo Peek(uint offset = 0)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DSharpStack), "Stack has been disposed");
            }
            if (_frameIndex == -1)
            {
                throw new InvalidOperationException("Stack is empty");
            }

            int index = _frameIndex - (int)offset;

            if (0 > index)
            {
                throw new IndexOutOfRangeException();
            }

            return _frames[index];
        }

        public FrameInfo PushNull() => *AllocateSized(DSharpStackValueType.Null, 0);
        public void Push(byte value) => PushNumber(_runtimeInformationProvider.Byte, value);
        public void Push(sbyte value) => PushNumber(_runtimeInformationProvider.SByte, value);
        public void Push(short value) => PushNumber(_runtimeInformationProvider.Int16, value);
        public void Push(ushort value) => PushNumber(_runtimeInformationProvider.UInt16, value);
        public void Push(int value) => PushNumber(_runtimeInformationProvider.Int32, value);
        public void Push(uint value) => PushNumber(_runtimeInformationProvider.UInt32, value);
        public void Push(long value) => PushNumber(_runtimeInformationProvider.Int64, value);
        public void Push(ulong value) => PushNumber(_runtimeInformationProvider.UInt64, value);
        public void Push(bool value) => PushNumber(_runtimeInformationProvider.Boolean, value);
        public void Push(char value) => PushNumber(_runtimeInformationProvider.Char, value);
        public void Push(decimal value) => PushNumber(_runtimeInformationProvider.Decimal, value);
        public void Push(double value) => PushNumber(_runtimeInformationProvider.Double, value);
        public void Push(float value) => PushNumber(_runtimeInformationProvider.Single, value);
        public void Push(nint value) => PushNumber(_runtimeInformationProvider.IntPtr, value);
        public void Push(nuint value) => PushNumber(_runtimeInformationProvider.UIntPtr, value);
        public void Push(void* value) => Push((nint)value);
        public bool Push(DSharpRuntimeTypeInfo* type, decimal value)
        {
            if (type == _runtimeInformationProvider.Byte)
            {
                value = Math.Clamp(value, byte.MinValue, byte.MaxValue);
                Push(decimal.ToByte(value));
            }
            else if (type == _runtimeInformationProvider.SByte)
            {
                value = Math.Clamp(value, sbyte.MinValue, sbyte.MaxValue);
                Push(decimal.ToSByte(value));
            }
            else if (type == _runtimeInformationProvider.Char)
            {
                value = Math.Clamp(value, char.MinValue, char.MaxValue);
                Push((char)decimal.ToInt16(value));
            }
            else if (type == _runtimeInformationProvider.Int16)
            {
                value = Math.Clamp(value, short.MinValue, short.MaxValue);
                Push(decimal.ToInt16(value));
            }
            else if (type == _runtimeInformationProvider.UInt16)
            {
                value = Math.Clamp(value, ushort.MinValue, ushort.MaxValue);
                Push(decimal.ToUInt16(value));
            }
            else if (type == _runtimeInformationProvider.Int32)
            {
                value = Math.Clamp(value, int.MinValue, int.MaxValue);
                Push(decimal.ToInt32(value));
            }
            else if (type == _runtimeInformationProvider.UInt32)
            {
                value = Math.Clamp(value, uint.MinValue, uint.MaxValue);
                Push(decimal.ToUInt32(value));
            }
            else if (type == _runtimeInformationProvider.Int64)
            {
                value = Math.Clamp(value, long.MinValue, long.MaxValue);
                Push(decimal.ToInt64(value));
            }
            else if (type == _runtimeInformationProvider.UInt64)
            {
                value = Math.Clamp(value, ulong.MinValue, ulong.MaxValue);
                Push(decimal.ToUInt64(value));
            }
            else if (type == _runtimeInformationProvider.IntPtr)
            {
                if (sizeof(nint) == sizeof(long))
                {
                    value = Math.Clamp(value, long.MinValue, long.MaxValue);
                    Push((nint)decimal.ToInt64(value));
                }
                else
                {
                    value = Math.Clamp(value, int.MinValue, int.MaxValue);
                    Push((nint)decimal.ToInt32(value));
                }
            }
            else if (type == _runtimeInformationProvider.UIntPtr)
            {
                if (sizeof(nuint) == sizeof(long))
                {
                    value = Math.Clamp(value, ulong.MinValue, ulong.MaxValue);
                    Push((nuint)decimal.ToUInt64(value));
                }
                else
                {
                    value = Math.Clamp(value, uint.MinValue, uint.MaxValue);
                    Push((nuint)decimal.ToUInt32(value));
                }
            }
            else if (type == _runtimeInformationProvider.Single)
            {
                Push(decimal.ToSingle(value));
            }
            else if (type == _runtimeInformationProvider.Double)
            {
                Push(decimal.ToDouble(value));
            }
            else if (type == _runtimeInformationProvider.Decimal)
            {
                Push(value);
            }
            else if (type == _runtimeInformationProvider.Boolean)
            {
                Push(value > 0);
            }
            else
            {
                return false;
            }

            return true;
        }
        public void Push(DSharpLiteralType type, nint valuePointer)
        {
            if (type == DSharpLiteralType.Null)
            {
                PushNull();
            }
            else if (type == DSharpLiteralType.Bool)
            {
                Push(*(bool*)valuePointer);
            }
            else if (type == DSharpLiteralType.Char)
            {
                Push(*(char*)valuePointer);
            }
            else if (type == DSharpLiteralType.Byte)
            {
                Push(*(byte*)valuePointer);
            }
            else if (type == DSharpLiteralType.SByte)
            {
                Push(*(sbyte*)valuePointer);
            }
            else if (type == DSharpLiteralType.Short)
            {
                Push(*(short*)valuePointer);
            }
            else if (type == DSharpLiteralType.UShort)
            {
                Push(*(ushort*)valuePointer);
            }
            else if (type == DSharpLiteralType.Int)
            {
                Push(*(int*)valuePointer);
            }
            else if (type == DSharpLiteralType.UInt)
            {
                Push(*(uint*)valuePointer);
            }
            else if (type == DSharpLiteralType.Long)
            {
                Push(*(long*)valuePointer);
            }
            else if (type == DSharpLiteralType.ULong)
            {
                Push(*(ulong*)valuePointer);
            }
            else if (type == DSharpLiteralType.NInt)
            {
                Push(*(nint*)valuePointer);
            }
            else if (type == DSharpLiteralType.NUInt)
            {
                Push(*(nuint*)valuePointer);
            }
            else if (type == DSharpLiteralType.Double)
            {
                Push(*(double*)valuePointer);
            }
            else if (type == DSharpLiteralType.Float)
            {
                Push(*(float*)valuePointer);
            }
            else if (type == DSharpLiteralType.Decimal)
            {
                Push(*(decimal*)valuePointer);
            }
            else
            {
                throw new ArgumentException($"Unsupported literal type: {type}");
            }
        }
        public void Push(DSharpLiteralValue literalValue)
        {
            if (literalValue.IsNull)
            {
                PushNull();
            }
            else if (literalValue.IsBool)
            {
                Push(literalValue.AsBool());
            }
            else if (literalValue.IsChar)
            {
                Push(literalValue.AsChar());
            }
            else if (literalValue.IsNumber)
            {
                byte* buffer = stackalloc byte[64];
                nint bufferPointer = (nint)buffer;
                UnmanagedStream stream = new(bufferPointer, 64);
                literalValue.Write(&stream);

                Push(literalValue.Type, bufferPointer + sizeof(DSharpLiteralType));
            }
            else
            {
                throw new ArgumentException($"Unsupported literal value ({literalValue.Type})", nameof(literalValue));
            }
        }
        public void PushReference(nint value) => PushReference((DSharpObject*)value);
        public FrameInfo PushReference(DSharpObject* value, bool force = false)
        {
            if (value != null && !value->IsReferenceObject && !force)
            {
                return PushStructure(value, true);
            }

            return *AllocateValue(DSharpStackValueType.Reference, (nint)value);
        }
        public DSharpMethodExecutor* PushMethodExecutor(DSharpRuntimeMethodInfo* methodInfo, int reservedSize = 0)
        {
            var scope = StartScope();
            var frame = AllocateSized(DSharpStackValueType.MethodCallingInfo, sizeof(DSharpMethodExecutor) + reservedSize);
            var executor = (DSharpMethodExecutor*)frame->StackPointer;
            executor->Scope = scope;
            executor->MethodInfo = methodInfo;

            return executor;
        }
        public FrameInfo* PushStructureRef(DSharpRuntimeTypeInfo* type, bool asArray = false) => CreateStructure(type, new(0, 0), asArray);
        public FrameInfo PushStructure(DSharpRuntimeTypeInfo* type, bool asArray = false) => *CreateStructure(type, new(0, 0), asArray);
        public FrameInfo PushStructure(DSharpRuntimeTypeInfo* type, UnmanagedArray<byte> dataBuffer) => *CreateStructure(type, dataBuffer, false);
        public FrameInfo PushStructure(DSharpRuntimeTypeInfo* type, UnmanagedArray<byte> dataBuffer, bool asArray) => *CreateStructure(type, dataBuffer, asArray);
        public FrameInfo PushStructure(DSharpObject* structure, bool unbox)
        {
            if (structure->IsReferenceObject && !unbox)
            {
                return PushReference(structure);
            }

            var size = DSharpObject.GetTotalSize(structure);
            var frame = AllocateSized(DSharpStackValueType.Structure, size);
            var obj = (DSharpObject*)frame->StackPointer;

            RuntimeExtensions.FillZero(obj, size);
            DSharpObject.Copy(structure, obj);

            obj->Placement = DSharpObjectPlacement.Buffer;

            return *frame;
        }
        public FrameInfo* Push(DSharpStackValueType type, int size) => AllocateSized(type, size);

        public Buffer* AllocateBuffer(int size)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DSharpStack), "Stack has been disposed");
            }
            if (0 >= size)
            {
                throw new ArgumentException($"Size should be greater then 0: {size}", nameof(size));
            }
            if (_allocatedStackSize + size > _currentStackCapacity)
            {
                throw new StackOverflowException();
            }

            Buffer* previousBuffer = null;

            if (Capacity > _currentStackCapacity)
            {
                previousBuffer = (Buffer*)(_stack + _currentStackCapacity);
            }

            var totalSize = size + sizeof(Buffer);
            var bufferStart = _currentStackCapacity - totalSize;
            Buffer* buffer = (Buffer*)(_stack + bufferStart);

            Buffer.Init(buffer, size);
            buffer->Clear();

            if (previousBuffer != null)
            {
                buffer->PreviousBuffer = previousBuffer;
                previousBuffer->NextBuffer = buffer;
            }

            _currentStackCapacity -= totalSize;

            return buffer;
        }
        public bool FreeBuffer(Buffer* buffer)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DSharpStack), "Stack has been disposed");
            }
            if (!Buffer.IsValid(buffer))
            {
                return false;
            }

            var totalSize = buffer->Size + sizeof(Buffer);
            
            if (buffer->NextBuffer != null)
            {
                var currentBuffer = buffer->NextBuffer;
                void* sourceCopyAddress = null;
                int copySize = 0;

                currentBuffer->PreviousBuffer = buffer->PreviousBuffer;

                while (currentBuffer != null)
                {
                    sourceCopyAddress = currentBuffer;
                    copySize += sizeof(Buffer) + currentBuffer->Size;

                    if (currentBuffer->FrameInfo != null)
                    {
                        currentBuffer->FrameInfo->Buffer = (Buffer*)((nint)currentBuffer + totalSize);
                    }
                    if (currentBuffer->PreviousBuffer != null)
                    {
                        currentBuffer->PreviousBuffer = (Buffer*)((nint)currentBuffer->PreviousBuffer + totalSize);
                    }
                    if (currentBuffer->NextBuffer != null)
                    {
                        currentBuffer->NextBuffer = (Buffer*)((nint)currentBuffer->NextBuffer + totalSize);
                    }

                    currentBuffer = currentBuffer->NextBuffer;
                }

                if (copySize > 0)
                {
                    System.Buffer.MemoryCopy(sourceCopyAddress, (void*)((nint)sourceCopyAddress + totalSize), copySize, copySize);
                }
            }

            _currentStackCapacity += totalSize;

            return true;
        }

        public void Pop(uint offset = 0) => Pop(offset, 1);
        public void Pop(uint offset, uint count)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DSharpStack), "Stack has been disposed");
            }

            int index = (int)(_frameIndex - offset) + 1;
            int startIndex = index - (int)count;
            int framesRemains;

            if (0 > startIndex)
            {
                startIndex = 0;
                framesRemains = (int)offset - 1;
            }
            else
            {
                framesRemains = _frameIndex - (index - startIndex);
            }

            int totalCount = (int)Count - index;
            int removedStackSize = 0;
            int destinationStackOffset = 0;

            for (int i = 0; i < offset; i++)
            {
                var destinationFrame = &_frames[startIndex + i];
                var sourceFrame = &_frames[index + i];
                var destinationStack = (byte*)destinationFrame->StackPointer + destinationStackOffset;
                removedStackSize += destinationFrame->Size;

                if (destinationFrame->Buffer != null)
                {
                    FreeBuffer(destinationFrame->Buffer);
                }
                if (sourceFrame->Buffer != null)
                {
                    sourceFrame->Buffer->FrameInfo = destinationFrame;
                }

                for (int d = 0; d < sourceFrame->Size; d++)
                {
                    destinationStack[d] = *(byte*)(sourceFrame->StackPointer + d);
                }

                destinationStackOffset += sourceFrame->Size - destinationFrame->Size;
                sourceFrame->StackPointer = (nint)destinationStack;
                *destinationFrame = *sourceFrame;
            }

            _frameIndex = framesRemains;
            int size = 0;

            for (int i = 0; i < Count; i++)
            {
                size += _frames[i].Size;
            }

            _allocatedStackSize = size;
        }

        public Scope StartScope()
        {
            uint stackCount = Count;
            var frame = AllocateSized(DSharpStackValueType.Scope, sizeof(Scope));
            var scope = (Scope*)frame->StackPointer;
            scope->StackCount = stackCount;

            return *scope;
        }
        public uint CloseScope(Scope scope, uint offset)
        {
            uint scopeStackCount = scope.StackCount - offset;

            if (scopeStackCount >= Count)
            {
                return 0;
            }

            uint delta = Count - scope.StackCount - offset;
            Pop(offset, delta);

            return delta;
        }

        private FrameInfo* AllocateValue<T>(DSharpStackValueType type, T value)
            where T : unmanaged
        {
            var size = sizeof(T);
            var frame = AllocateSized(type, size);
            frame->Write(0, value);

            return frame;
        }
        private FrameInfo* AllocateSized(DSharpStackValueType type, int size)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DSharpStack), "Stack has been disposed");
            }
            if (0 > size)
            {
                throw new ArgumentException($"Size can not be negative: {size}", nameof(size));
            }
            if (_allocatedStackSize + size > _currentStackCapacity)
            {
                throw new StackOverflowException();
            }

            nint stackPointer;

            if (_frameIndex >= 0)
            {
                var previousFrame = _frames[_frameIndex];
                stackPointer = previousFrame.StackPointer + previousFrame.Size;
            }
            else
            {
                stackPointer = (nint)_stack;
            }

            _frameIndex++;
            _allocatedStackSize += size;

            var frame = &_frames[_frameIndex];
            frame->ValueType = type;
            frame->Size = size;
            frame->IsNumber = false;
            frame->StackPointer = stackPointer;
            frame->Buffer = null;

            return frame;
        }

        private FrameInfo PushNumber<T>(DSharpRuntimeTypeInfo* type, T value)
            where T : unmanaged
        {
            UnmanagedArray<byte> dataBuffer = new((byte*)&value, sizeof(T));
            return *CreateStructure(type, dataBuffer);
        }
        private FrameInfo* CreateStructure(DSharpRuntimeTypeInfo* type, UnmanagedArray<byte> dataBuffer, bool asArray = false)
        {
            int size = type->Size;

            if (asArray)
            {
                size += sizeof(DSharpArray);
            }
            else
            {
                size += sizeof(DSharpObject);
            }

            var frame = AllocateSized(DSharpStackValueType.Structure, size);
            frame->IsNumber = type->BuildInValueTypeIndex != -1;
            UnmanagedArray<byte> objectBuffer = new((byte*)frame->StackPointer, size);

            RuntimeExtensions.FillZero((void*)frame->StackPointer, size);
            DSharpObjectsContainer.CreateStructure(type, dataBuffer, objectBuffer, asArray);

            return frame;
        }

        #endregion

        #region Enumerable

        public IEnumerator<FrameInfo> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return Peek((uint)i);
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Disposing

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (_memoryManager.IsDisposed)
            {
                return;
            }

            _memoryManager.Free(_frames);
            _memoryManager.Free(_stack);
        }

        #endregion

        #region Frames

        [StructLayout(LayoutKind.Sequential)]
        public struct Buffer
        {
            private fixed char Magic[2];
            public int Size;
            public nint StackPointer;
            public FrameInfo* FrameInfo;
            public Buffer* NextBuffer;
            public Buffer* PreviousBuffer;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void Clear()
            {
                RuntimeExtensions.FillZero((void*)StackPointer, Size);
            }

            public static bool IsValid(Buffer* buffer)
            {
                return buffer->Magic[0] == 'b' && buffer->Magic[1] == 'f';
            }
            public static void Init(Buffer* buffer, int size)
            {
                buffer->Magic[0] = 'b';
                buffer->Magic[1] = 'f';
                buffer->Size = size;
                buffer->StackPointer = (nint)buffer + sizeof(Buffer);
                buffer->FrameInfo = null;
                buffer->NextBuffer = null;
                buffer->PreviousBuffer = null;
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct FrameInfo
        {
            public readonly DSharpRuntimeTypeInfo* ObjectType
            {
                get
                {
                    DSharpObject* obj;

                    if (ValueType == DSharpStackValueType.Structure)
                    {
                        obj = (DSharpObject*)StackPointer;
                    }
                    else if (ValueType == DSharpStackValueType.Reference)
                    {
                        obj = *(DSharpObject**)StackPointer;
                    }
                    else
                    {
                        return null;
                    }

                    return obj->Type;
                }
            }
            public readonly DSharpMethodExecutor* MethodExecutor
            {
                get
                {
                    if (ValueType != DSharpStackValueType.MethodCallingInfo)
                    {
                        return null;
                    }

                    return (DSharpMethodExecutor*)StackPointer;
                }
            }
            public readonly bool HasBuffer => Buffer != null;

            public DSharpStackValueType ValueType;
            public int Size;
            public bool IsNumber;
            public nint StackPointer;
            public Buffer* Buffer;

            public readonly byte this[int index]
            {
                get => Read<byte>(index);
                set => Write(index, value);
            }

            public readonly void SetNullValue()
            {
                byte* values = (byte*)StackPointer;

                for (int i = 0; i < Size; i++)
                {
                    values[i] = 0;
                }
            }
            public readonly void Write(FrameInfo frameWithValue)
            {
                if (frameWithValue.ValueType == DSharpStackValueType.Null)
                {
                    SetNullValue();
                    return;
                }

                byte* source = (byte*)frameWithValue.StackPointer;
                byte* destination = (byte*)StackPointer;

                for (int i = 0; i < Math.Min(Size, frameWithValue.Size); i++)
                {
                    destination[i] = source[i];
                }
            }
            public readonly void Write<T>(int index, T value) where T : unmanaged
            {
                if (0 > index || index >= Size)
                {
                    throw new IndexOutOfRangeException();
                }

                T* items = (T*)StackPointer;
                items[index] = value;
            }
            public readonly void Write<T>(T value) where T : unmanaged
            {
                Write(0, value);
            }
            public readonly T Read<T>(int index) where T : unmanaged
            {
                if (0 > index || index >= Size)
                {
                    throw new IndexOutOfRangeException();
                }

                T* items = (T*)StackPointer;
                return items[index];
            }
            public readonly T Read<T>() where T : unmanaged
            {
                return Read<T>(0);
            }
            public readonly decimal? ReadAsDecimal()
            {
                DSharpObject* obj = ReadAsObject();

                if (obj == null)
                {
                    return null;
                }
                if (obj->Type->Converter == null)
                {
                    return null;
                }

                return DSharpObjectConverter.ToObject<decimal>(obj);
            }
            public readonly bool ReadAsBoolean()
            {
                DSharpObject* obj = ReadAsObject();

                if (obj == null)
                {
                    return false;
                }

                return DSharpObjectConverter.ToBoolean(obj);
            }
            public readonly nint ReadReference()
            {
                if (ValueType != DSharpStackValueType.Reference)
                {
                    return 0;
                }

                return *(nint*)StackPointer;
            }
            public readonly DSharpObject* ReadAsObject()
            {
                if (ValueType == DSharpStackValueType.Structure)
                {
                    return (DSharpObject*)StackPointer;
                }
                else if (ValueType == DSharpStackValueType.Reference)
                {
                    return (DSharpObject*)ReadReference();
                }

                return null;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void Clear()
            {
                RuntimeExtensions.FillZero((void*)StackPointer, Size);
            }

            public readonly override string ToString()
            {
                try
                {
                    var result = $"{ValueType}:{Size}";
                    var objectType = ObjectType;

                    if (objectType != null && objectType->BuildInValueTypeIndex != -1 &&
                        DSharpBuildInTypes.TryGetValueTypeByIndex(objectType->BuildInValueTypeIndex, out var typeInfo))
                    {
                        result += $" ({typeInfo})";
                    }

                    return result;
                }
                catch (Exception error)
                {
                    return error.ToString();
                }
            }

            public static implicit operator UnmanagedArray<byte>(FrameInfo frame) => new(frame.StackPointer, frame.Size);

            public static bool ValueEquals(FrameInfo left, FrameInfo right)
            {
                if (left.ValueType == right.ValueType)
                {
                    if (left.ValueType == DSharpStackValueType.Null)
                    {
                        return true;
                    }

                    return StackValueEquals(left, right);
                }
                if (left.Size == right.Size)
                {
                    return StackValueEquals(left, right);
                }
                else if (left.IsNumber && right.IsNumber)
                {
                    return left.ReadAsDecimal().GetValueOrDefault() == right.ReadAsDecimal().GetValueOrDefault();
                }

                return false;
            }
            private static bool StackValueEquals(FrameInfo left, FrameInfo right)
            {
                for (int i = 0; i < left.Size; i++)
                {
                    if (left[i] != right[i])
                    {
                        return false;
                    }
                }

                return true;
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct Scope
        {
            public uint StackCount;
        }

        #endregion
    }
}

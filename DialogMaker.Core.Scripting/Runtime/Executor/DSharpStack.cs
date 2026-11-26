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
        : DSharpDisposable, IEnumerable<DSharpStack.FrameInfo>
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
        /// <summary>
        /// Available to allocate size
        /// </summary>
        public int AvailableSize => _currentStackCapacity - _allocatedStackSize;

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal DSharpObject* FastStructurePeek(int offset = 0)
        {
            return _frames[_frameIndex - offset].ReadAsStructure();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal T* FastStructureDataPeek<T>(int offset = 0)
            where T : unmanaged
        {
            return _frames[_frameIndex - offset].ReadAsStructureData<T>();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal DSharpNumber FastNumberPeek(int offset = 0)
        {
            return _frames[_frameIndex - offset].ReadAsNumber();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FrameInfo PushTypedNull(DSharpRuntimeTypeInfo* typeInfo)
        {
            if (typeInfo->IsValueType)
            {
                return PushStructure(typeInfo);
            }

            return PushReference(null);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FrameInfo PushNull() => *AllocateSized(DSharpStackValueType.Null, 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(byte value) => PushNumber(_runtimeInformationProvider.Byte, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(sbyte value) => PushNumber(_runtimeInformationProvider.SByte, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(short value) => PushNumber(_runtimeInformationProvider.Int16, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(ushort value) => PushNumber(_runtimeInformationProvider.UInt16, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(int value) => PushNumber(_runtimeInformationProvider.Int32, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(uint value) => PushNumber(_runtimeInformationProvider.UInt32, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(long value) => PushNumber(_runtimeInformationProvider.Int64, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(ulong value) => PushNumber(_runtimeInformationProvider.UInt64, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(bool value) => PushNumber(_runtimeInformationProvider.Boolean, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(char value) => PushNumber(_runtimeInformationProvider.Char, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(decimal value) => PushNumber(_runtimeInformationProvider.Decimal, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(double value) => PushNumber(_runtimeInformationProvider.Double, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(float value) => PushNumber(_runtimeInformationProvider.Single, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(nint value) => PushNumber(_runtimeInformationProvider.IntPtr, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(nuint value) => PushNumber(_runtimeInformationProvider.UIntPtr, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(void* value) => Push((nint)value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(DSharpNumber* number)
        {
            var type = _runtimeInformationProvider.GetRuntimeInfo(number->_type);
            var size = DSharpBuildInTypes.LiteralSizes[(int)number->_type];
            var pointer = &number;

            var frame = AllocateSized(DSharpStackValueType.Structure, size + sizeof(DSharpObject));
            frame->IsNumber = true;

            DSharpObject obj = default;
            obj.Type = type;
            obj.Attributes = DSharpObjectAttributes.StoredInBuffer;

            Unsafe.WriteUnaligned((void*)frame->StackPointer, obj);
            Unsafe.CopyBlockUnaligned((void*)(frame->StackPointer + sizeof(DSharpObject)), number, (uint)size);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(DSharpLiteralType type, nint valuePointer)
        {
            switch (type)
            {
                case DSharpLiteralType.Null:
                    PushNull();
                    break;
                case DSharpLiteralType.Bool:
                    Push(*(bool*)valuePointer);
                    break;
                case DSharpLiteralType.Byte:
                    Push(*(byte*)valuePointer);
                    break;
                case DSharpLiteralType.SByte:
                    Push(*(sbyte*)valuePointer);
                    break;
                case DSharpLiteralType.Char:
                    Push(*(char*)valuePointer);
                    break;
                case DSharpLiteralType.Short:
                    Push(*(short*)valuePointer);
                    break;
                case DSharpLiteralType.UShort:
                    Push(*(ushort*)valuePointer);
                    break;
                case DSharpLiteralType.Int:
                    Push(*(int*)valuePointer);
                    break;
                case DSharpLiteralType.UInt:
                    Push(*(uint*)valuePointer);
                    break;
                case DSharpLiteralType.Long:
                    Push(*(long*)valuePointer);
                    break;
                case DSharpLiteralType.ULong:
                    Push(*(ulong*)valuePointer);
                    break;
                case DSharpLiteralType.NInt:
                    Push(*(nint*)valuePointer);
                    break;
                case DSharpLiteralType.NUInt:
                    Push(*(nuint*)valuePointer);
                    break;
                case DSharpLiteralType.Float:
                    Push(*(float*)valuePointer);
                    break;
                case DSharpLiteralType.Double:
                    Push(*(double*)valuePointer);
                    break;
                case DSharpLiteralType.Decimal:
                    Push(*(decimal*)valuePointer);
                    break;
                default:
                    throw new ArgumentException($"Unsupported literal type: {type}");
            }
            ;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushReference(nint value, bool isPointerToPointer = false) => PushReference((DSharpObject*)value, isPointerToPointer: isPointerToPointer);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FrameInfo PushReference(DSharpObject* value, bool force = false, bool isPointerToPointer = false)
        {
            if (!isPointerToPointer && value != null && !value->IsReferenceObject && !force)
            {
                return PushStructure(value, true);
            }

            var frame = AllocateValue(DSharpStackValueType.Reference, (nint)value);

            if (isPointerToPointer)
            {
                frame->Mode = DSharpStackValueMode.PointerToPointer;
            }

            return *frame;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DSharpMethodExecutor* PushMethodExecutor(DSharpRuntimeMethodInfo* methodInfo, int reservedSize = 0, uint scopeOffset = 0)
        {
            var scope = StartScope();
            var frame = AllocateSized(DSharpStackValueType.MethodCallingInfo, sizeof(DSharpMethodExecutor) + reservedSize);
            var executor = (DSharpMethodExecutor*)frame->StackPointer;
            scope.StackCount -= scopeOffset;
            executor->Scope = scope;
            executor->MethodInfo = methodInfo;

            return executor;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FrameInfo PushRedirectObject(DSharpRuntimeTypeInfo* type, nint dataAddress)
        {
            var frame = AllocateSized(DSharpStackValueType.Structure, sizeof(DSharpObject));
            frame->IsNumber = type->BuildInValueTypeIndex != -1;
            DSharpObject* instance = (DSharpObject*)frame->StackPointer;
            *instance = new()
            {
                Type = type,
                Attributes = DSharpObjectAttributes.StoredInBuffer |
                             DSharpObjectAttributes.Initialized |
                             DSharpObjectAttributes.RedirectData,
                Extra = (void*)dataAddress
            };

            return *frame;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FrameInfo* PushStructureRef(DSharpRuntimeTypeInfo* type, bool asArray = false) => CreateStructure(type, new(0, 0), asArray);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FrameInfo PushStructure(DSharpRuntimeTypeInfo* type, bool asArray = false) => *CreateStructure(type, new(0, 0), asArray);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FrameInfo PushStructure(DSharpRuntimeTypeInfo* type, UnmanagedArray<byte> dataBuffer) => *CreateStructure(type, dataBuffer, false);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FrameInfo PushStructure(DSharpRuntimeTypeInfo* type, UnmanagedArray<byte> dataBuffer, bool asArray) => *CreateStructure(type, dataBuffer, asArray);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FrameInfo PushStructure<T>(DSharpRuntimeTypeInfo* type, T value)
            where T : unmanaged
        {
            T* valuePointer = &value;
            return PushStructure(type, new UnmanagedArray<byte>((byte*)valuePointer, sizeof(T)));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FrameInfo PushStructure(DSharpObject* structure, bool unbox)
        {
            if (structure->IsReferenceObject && !unbox)
            {
                return PushReference(structure);
            }

            var size = DSharpObject.GetTotalSize(structure);
            var frame = AllocateSized(DSharpStackValueType.Structure, size);
            var obj = (DSharpObject*)frame->StackPointer;

            DSharpObject.Copy(structure, obj);

            obj->Placement = DSharpObjectPlacement.Buffer;

            return *frame;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FrameInfo* Push(DSharpStackValueType type, int size) => AllocateSized(type, size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PopCount(uint count)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DSharpStack), "Stack has been disposed");
            }

            var frameIndex = _frameIndex;

            for (int i = 0; i < count; i++)
            {
                var buffer = _frames[frameIndex - i].Buffer;

                if (buffer != null)
                {
                    FreeBuffer(buffer);
                }
            }

            _frameIndex -= (int)count;
            var currentFrame = &_frames[_frameIndex + 1];
            _allocatedStackSize = (int)(currentFrame->StackPointer - (nint)_stack);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pop(uint offset = 0)
        {
            if (offset == 0)
            {
                PopCount(1);
                return;
            }

            Pop(offset, 1);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pop(uint offset, uint count)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DSharpStack), "Stack has been disposed");
            }
            if (offset == 0)
            {
                PopCount(count);
                return;
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

                Unsafe.CopyBlockUnaligned(destinationStack, (void*)sourceFrame->StackPointer, (uint)sourceFrame->Size);

                destinationStackOffset += sourceFrame->Size - destinationFrame->Size;
                sourceFrame->StackPointer = (nint)destinationStack;
                *destinationFrame = *sourceFrame;
            }

            _frameIndex = framesRemains;
            var currentFrame = &_frames[_frameIndex];
            _allocatedStackSize = (int)((currentFrame->StackPointer + currentFrame->Size) - (nint)_stack);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Move(uint offset, int moveOffset)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            if (0 > (int)offset + moveOffset)
            {
                throw new IndexOutOfRangeException();
            }
            if (moveOffset == 0)
            {
                return;
            }

            int index = (int)(_frameIndex - offset);
            int endIndex = index - moveOffset;
            FrameInfo* frame = &_frames[index];
            int frameSize = frame->Size;
            byte* tempBuffer = stackalloc byte[frameSize];
            FrameInfo* tempFrame = stackalloc FrameInfo[1];
            int sourceOffset = moveOffset > 0 ? -1 : 1;

            *tempFrame = *frame;

            if (frameSize > 0)
            {
                System.Buffer.MemoryCopy((void*)frame->StackPointer, tempBuffer, frameSize, frameSize);
            }

            if (moveOffset > 0)
            {
                nint? startStackPointer = null;
                int stackSize = 0;

                for (int i = index; i > endIndex; i--)
                {
                    var currentFrame = &_frames[i - 1];
                    var destinationFrame = &_frames[i];

                    stackSize += currentFrame->Size;
                    startStackPointer ??= currentFrame->StackPointer;
                    currentFrame->StackPointer += frameSize;

                    if (currentFrame->Buffer != null)
                    {
                        currentFrame->Buffer->FrameInfo = destinationFrame;
                    }

                    *destinationFrame = *currentFrame;
                }

                if (startStackPointer != null)
                {
                    var stackPointer = (byte*)startStackPointer.Value;
                    System.Buffer.MemoryCopy(stackPointer, stackPointer + frameSize, stackSize, stackSize);
                }

                var finalFrame = &_frames[endIndex];
                *finalFrame = *tempFrame;
                finalFrame->StackPointer = startStackPointer ?? 0;

                System.Buffer.MemoryCopy(tempBuffer, (void*)finalFrame->StackPointer, frameSize, frameSize);

                return;
            }

            int endPointIndex = index - moveOffset;
            int moveCount = Math.Abs(moveOffset);
            int sizeToMove = sizeof(FrameInfo) * moveCount;

            if (moveOffset > 0)
            {
                var previousFrame = &_frames[index - 1];
                System.Buffer.MemoryCopy(previousFrame, frame, sizeToMove, sizeToMove);

                if (frameSize > 0)
                {
                    var previousStackPointer = (byte*)frame->StackPointer;
                    System.Buffer.MemoryCopy(previousStackPointer, previousStackPointer + frameSize, sizeToMove, sizeToMove);
                }
            }
            else
            {
                for (int i = moveCount - 1; i >= 0; i--)
                {
                    _frames[index - i] = _frames[index - i - 1];
                }
            }


            FrameInfo* endPointFrame = &_frames[endPointIndex];
            *endPointFrame = *tempFrame;

            if (endPointFrame->HasBuffer)
            {
                endPointFrame->Buffer->FrameInfo = endPointFrame;
            }

            int stackOffset = frameSize;
            int totalSize = 0;
            FrameInfo* lastFrame = null;

            for (int i = 0; i < moveCount; i++)
            {
                FrameInfo* movedFrame = &_frames[index + i];
                totalSize += movedFrame->Size;

                if (movedFrame->HasBuffer)
                {
                    movedFrame->Buffer->FrameInfo = movedFrame;
                }
                if (movedFrame->StackPointer != 0)
                {
                    movedFrame->StackPointer += frameSize * moveOffset;
                }

                lastFrame = movedFrame;
            }

            if (lastFrame != null)
            {
                //System.Buffer.MemoryCopy((void*)(frame->StackPointer + frameSize * -moveOffset), (void*)frame->StackPointer, totalSize, totalSize);
                endPointFrame->StackPointer = lastFrame->StackPointer + lastFrame->Size;
                System.Buffer.MemoryCopy(tempBuffer, (void*)endPointFrame->StackPointer, frameSize, frameSize);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Scope StartScope()
        {
            uint stackCount = Count;
            var frame = AllocateSized(DSharpStackValueType.Scope, sizeof(Scope));
            var scope = (Scope*)frame->StackPointer;
            scope->StackCount = stackCount;

            return *scope;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private FrameInfo* AllocateValue<T>(DSharpStackValueType type, T value)
            where T : unmanaged
        {
            var size = sizeof(T);
            var frame = AllocateSized(type, size);
            frame->Write(0, value);

            return frame;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        private FrameInfo* CreateStructure(DSharpRuntimeTypeInfo* type, in UnmanagedArray<byte> dataBuffer, bool asArray = false)
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

                    return obj != null ? obj->Type : null;
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
            public readonly bool IsPointerToPointer => Mode == DSharpStackValueMode.PointerToPointer;
            public bool IsNumber
            {
                readonly get => Mode == DSharpStackValueMode.Number;
                set
                {
                    if (value)
                    {
                        Mode = DSharpStackValueMode.Number;
                    }
                    else if (Mode == DSharpStackValueMode.Number)
                    {
                        Mode = DSharpStackValueMode.Default;
                    }
                }
            }

            public DSharpStackValueType ValueType;
            public DSharpStackValueMode Mode;
            public int Size;
            public nint StackPointer;
            public Buffer* Buffer;

            public readonly byte this[int index]
            {
                get => Read<byte>(index);
                set => Write(index, value);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void SetNullValue()
            {
                if (Size > 0)
                {
                    RuntimeExtensions.FillZero((void*)StackPointer, Size);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void Write(ref FrameInfo frameWithValue)
            {
                if (frameWithValue.ValueType == DSharpStackValueType.Null ||
                    0 >= frameWithValue.Size)
                {
                    SetNullValue();
                    return;
                }

                Write((void*)frameWithValue.StackPointer, frameWithValue.Size);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void Write(void* source, int sourceSize)
            {
                if (Size == 0)
                {
                    return;
                }

                Unsafe.CopyBlockUnaligned((byte*)StackPointer, source, (uint)Math.Min(Size, sourceSize));
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void Write<T>(int index, T value) where T : unmanaged
            {
#if DEBUG
                if (0 > index || index >= Size)
                {
                    throw new IndexOutOfRangeException();
                }
#endif

                Unsafe.WriteUnaligned(&((T*)StackPointer)[index], value);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void Write<T>(T value) where T : unmanaged
            {
                Unsafe.WriteUnaligned((void*)StackPointer, value);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly T Read<T>(int index) where T : unmanaged
            {
#if DEBUG
                if (0 > index || index >= Size)
                {
                    throw new IndexOutOfRangeException();
                }
#endif

                return Unsafe.ReadUnaligned<T>(&((T*)StackPointer)[index]);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly T Read<T>() where T : unmanaged
            {
                return Unsafe.ReadUnaligned<T>((void*)StackPointer);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly DSharpNumber ReadAsNumber()
            {
                var obj = ReadAsStructure();
                return DSharpNumber.FromObject(obj);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly decimal? ReadAsDecimal()
            {
                DSharpObject* obj = ReadAsObject();

                if (obj == null)
                {
                    return null;
                }
                if (obj->Type->ConverterInfo.IsNull)
                {
                    return null;
                }

                return DSharpObjectConverter.ToObject<decimal>(obj);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool ReadAsBoolean()
            {
                if (ValueType == DSharpStackValueType.Reference)
                {
                    return *(bool*)DSharpObject.GetData((DSharpObject*)ReadReference());
                }

                return Unsafe.ReadUnaligned<bool>(ReadAsStructureData<byte>());
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly nint ReadReference()
            {
                if (ValueType != DSharpStackValueType.Reference)
                {
                    return 0;
                }
                if (IsPointerToPointer)
                {
                    return **(nint**)StackPointer;
                }

                return *(nint*)StackPointer;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly T* ReadAsStructureData<T>()
                where T : unmanaged
            {
                return (T*)DSharpObject.GetData(ReadAsStructure());
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly DSharpObject* ReadAsStructure()
            {
                return (DSharpObject*)StackPointer;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            public readonly bool WhiteAsObject(DSharpObject* instance)
            {
                if (instance == null)
                {
                    if (ValueType == DSharpStackValueType.Structure)
                    {
                        var type = ObjectType;
                        SetNullValue();
                        DSharpObjectsContainer.CreateStructure(type, new(StackPointer, Size), false);
                        return true;
                    }
                    else if (ValueType == DSharpStackValueType.Reference)
                    {
                        SetNullValue();
                        return true;
                    }
                }
                else
                {
                    if (ValueType == DSharpStackValueType.Structure &&
                        instance->Type->IsValueType)
                    {
                        Write(instance, DSharpObject.GetTotalSize(instance));
                        return true;
                    }
                    else if (ValueType == DSharpStackValueType.Reference &&
                             instance->IsReferenceObject)
                    {
                        Write((nint)instance);
                        return true;
                    }
                }

                return false;
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool ValueEquals(ref FrameInfo left, ref FrameInfo right)
            {
                static bool IsNull(ref FrameInfo frame)
                {
                    if (0 >= frame.Size)
                    {
                        return true;
                    }

                    var instance = frame.ReadAsObject();

                    return instance == null ||
                           frame.ValueType == DSharpStackValueType.Structure &&
                           RuntimeExtensions.IsEmpty(DSharpObject.GetData(instance), DSharpObject.GetSize(instance));
                }

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
                else if (left.ValueType == DSharpStackValueType.Null)
                {
                    return IsNull(ref right);
                }
                else if (right.ValueType == DSharpStackValueType.Null)
                {
                    return IsNull(ref left);
                }
                else if (left.IsNumber && right.IsNumber)
                {
                    return left.ReadAsNumber()._decimal == right.ReadAsNumber()._decimal;
                }

                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool StackValueEquals(FrameInfo left, FrameInfo right)
            {
                return RuntimeExtensions.Equals((void*)left.StackPointer, (void*)right.StackPointer, Math.Min(left.Size, right.Size));
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

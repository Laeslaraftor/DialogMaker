using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Drawing;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    public unsafe class DSharpStack(int stackCapacity) : Disposable
    {
        public DSharpStack() : this(DSharpThread.DefaultStackCapacity)
        {
        }

        public int Capacity { get; } = stackCapacity;
        public uint Count => (uint)(_frameIndex + 1);

        private readonly FrameInfo* _frames = (FrameInfo*)Marshal.AllocHGlobal(sizeof(FrameInfo) * stackCapacity);
        private readonly byte* _stack = (byte*)Marshal.AllocHGlobal(stackCapacity * 1024);
        private int _frameIndex = -1;
        private int _allocatedStackSize;

        #region Controls

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

        public void Push() => AllocateSized(DSharpStackValueType.Null, 0);
        public void Push(byte value) => AllocateValue(DSharpStackValueType.Byte, value);
        public void Push(sbyte value) => AllocateValue(DSharpStackValueType.SByte, value);
        public void Push(short value) => AllocateValue(DSharpStackValueType.Short, value);
        public void Push(ushort value) => AllocateValue(DSharpStackValueType.UShort, value);
        public void Push(int value) => AllocateValue(DSharpStackValueType.Int, value);
        public void Push(uint value) => AllocateValue(DSharpStackValueType.UInt, value);
        public void Push(long value) => AllocateValue(DSharpStackValueType.Long, value);
        public void Push(ulong value) => AllocateValue(DSharpStackValueType.ULong, value);
        public void Push(bool value) => AllocateValue(DSharpStackValueType.Bool, value);
        public void Push(char value) => AllocateValue(DSharpStackValueType.Char, value);
        public void Push(decimal value) => AllocateValue(DSharpStackValueType.Decimal, value);
        public void Push(double value) => AllocateValue(DSharpStackValueType.Double, value);
        public void Push(float value) => AllocateValue(DSharpStackValueType.Float, value);
        public void Push(nint value) => AllocateValue(DSharpStackValueType.Nint, value);
        public void Push(nuint value) => AllocateValue(DSharpStackValueType.Nuint, value);
        public void Push(void* value) => Push((nint)value);
        public bool Push(DSharpStackValueType type, decimal value)
        {
            if (type == DSharpStackValueType.Byte)
            {
                value = Math.Clamp(value, byte.MinValue, byte.MaxValue);
                Push(decimal.ToByte(value));
            }
            else if (type == DSharpStackValueType.SByte)
            {
                value = Math.Clamp(value, sbyte.MinValue, sbyte.MaxValue);
                Push(decimal.ToSByte(value));
            }
            else if (type == DSharpStackValueType.Char)
            {
                value = Math.Clamp(value, char.MinValue, char.MaxValue);
                Push((char)decimal.ToInt16(value));
            }
            else if (type == DSharpStackValueType.Short)
            {
                value = Math.Clamp(value, short.MinValue, short.MaxValue);
                Push(decimal.ToInt16(value));
            }
            else if (type == DSharpStackValueType.UShort)
            {
                value = Math.Clamp(value, ushort.MinValue, ushort.MaxValue);
                Push(decimal.ToUInt16(value));
            }
            else if (type == DSharpStackValueType.Int)
            {
                value = Math.Clamp(value, int.MinValue, int.MaxValue);
                Push(decimal.ToInt32(value));
            }
            else if (type == DSharpStackValueType.UInt)
            {
                value = Math.Clamp(value, uint.MinValue, uint.MaxValue);
                Push(decimal.ToUInt32(value));
            }
            else if (type == DSharpStackValueType.Long)
            {
                value = Math.Clamp(value, long.MinValue, long.MaxValue);
                Push(decimal.ToInt64(value));
            }
            else if (type == DSharpStackValueType.ULong)
            {
                value = Math.Clamp(value, ulong.MinValue, ulong.MaxValue);
                Push(decimal.ToUInt64(value));
            }
            else if (type == DSharpStackValueType.Nint)
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
            else if (type == DSharpStackValueType.Nuint)
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
            else if (type == DSharpStackValueType.Float)
            {
                Push(decimal.ToSingle(value));
            }
            else if (type == DSharpStackValueType.Double)
            {
                Push(decimal.ToDouble(value));
            }
            else if (type == DSharpStackValueType.Decimal)
            {
                Push(value);
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
                Push();
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
        public void PushReference(nint value) => AllocateValue(DSharpStackValueType.Reference, value);
        public void PushReference(DSharpObject* value) => PushReference((nint)value);
        public DSharpMethodExecutor* PushMethodExecutor(DSharpRuntimeMethodInfo* methodInfo, int reservedSize = 0)
        {
            var scope = StartScope();
            var frame = AllocateSized(DSharpStackValueType.MethodCallingInfo, sizeof(DSharpMethodExecutor) + reservedSize);
            var executor = (DSharpMethodExecutor*)frame->StackPointer;
            executor->Scope = scope;
            executor->MethodInfo = methodInfo;

            return executor;
        }
        public FrameInfo PushStructure(int size) => *AllocateSized(DSharpStackValueType.Structure, size);
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

                for (int d = 0; d < sourceFrame->Size; d++)
                {
                    destinationStack[d] = *(byte*)(sourceFrame->StackPointer + d);
                }

                destinationStackOffset += sourceFrame->Size - destinationFrame->Size;
                sourceFrame->StackPointer = (nint)destinationStack;
                *destinationFrame = *sourceFrame;
            }

            _frameIndex = framesRemains;
            _allocatedStackSize -= removedStackSize;
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

            uint delta = Count - scopeStackCount;
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
            frame->StackPointer = stackPointer;

            return frame;
        }

        #endregion

        #region Disposing

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Marshal.FreeHGlobal((nint)_frames);
            Marshal.FreeHGlobal((nint)_stack);
        }

        #endregion

        #region Frames

        [StructLayout(LayoutKind.Sequential)]
        public struct FrameInfo
        {
            public readonly bool IsNumber => ValueType != DSharpStackValueType.Null &&
                                             ValueType != DSharpStackValueType.Structure &&
                                             ValueType != DSharpStackValueType.Reference &&
                                             ValueType != DSharpStackValueType.Bool &&
                                             ValueType != DSharpStackValueType.MethodCallingInfo &&
                                             ValueType != DSharpStackValueType.Scope;

            public DSharpStackValueType ValueType;
            public int Size;
            public nint StackPointer;

            public readonly byte this[int index]
            {
                get => Read<byte>(index);
                set => Write(index, value);
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
                return ValueType switch
                {
                    DSharpStackValueType.Byte => Read<byte>(),
                    DSharpStackValueType.SByte => Read<sbyte>(),
                    DSharpStackValueType.Char => Read<char>(),
                    DSharpStackValueType.Short => Read<short>(),
                    DSharpStackValueType.UShort => Read<ushort>(),
                    DSharpStackValueType.Int => Read<int>(),
                    DSharpStackValueType.UInt => Read<uint>(),
                    DSharpStackValueType.Long => Read<long>(),
                    DSharpStackValueType.ULong => Read<ulong>(),
                    DSharpStackValueType.Nint => Read<nint>(),
                    DSharpStackValueType.Nuint => Read<nuint>(),
                    DSharpStackValueType.Float => (decimal)Read<float>(),
                    DSharpStackValueType.Double => (decimal)Read<double>(),
                    DSharpStackValueType.Decimal => Read<decimal>(),
                    _ => null
                };
            }

            public readonly override string ToString()
            {
                return $"{ValueType}:{Size}";
            }

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

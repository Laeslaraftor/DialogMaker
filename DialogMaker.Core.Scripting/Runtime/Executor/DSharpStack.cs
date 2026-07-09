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
        public void PushReference(nint value) => AllocateValue(DSharpStackValueType.Reference, value);
        public FrameInfo PushStructure(int size) => *AllocateSized(DSharpStackValueType.Structure, size);
        public void Pop(uint offset = 0)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DSharpStack), "Stack has been disposed");
            }

            int index = (int)(_frameIndex - offset);

            if (0 > index)
            {
                throw new IndexOutOfRangeException();
            }

            var frame = _frames[index];
            var poppedStackSize = frame.Size;

            if (poppedStackSize > 0)
            {
                nint allocatedBlockEnd = (nint)_stack + _allocatedStackSize;

                for (nint i = frame.StackPointer + poppedStackSize; i < allocatedBlockEnd; i++)
                {
                    *(byte*)(i - poppedStackSize) = *(byte*)i;
                }

                _allocatedStackSize -= poppedStackSize;
            }

            for (int i = index + 1; i < _frameIndex + 1; i++)
            {
                var currentFrame = _frames[i];

                if (currentFrame.StackPointer != IntPtr.Zero)
                {
                    currentFrame.StackPointer -= poppedStackSize;
                }

                _frames[i - 1] = currentFrame;
            }

            _frameIndex--;
        }
        public void Pop(uint offset, uint count)
        {
            for (uint i = 0; i < count; i++)
            {
                Pop(offset);
            }
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
            public readonly T Read<T>(int index) where T : unmanaged
            {
                if (0 > index || index >= Size)
                {
                    throw new IndexOutOfRangeException();
                }

                T* items = (T*)StackPointer;
                return items[index];
            }
        }

        #endregion
    }
}

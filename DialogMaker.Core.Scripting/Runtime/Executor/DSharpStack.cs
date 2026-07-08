using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    public unsafe class DSharpStack(int stackSize) : Disposable
    {
        public int Capacity { get; } = stackSize;
        public uint AllocatedSize { get; private set; }
        public Frame* CurrentFrame { get; private set; }

        private readonly nint _stackPointer = Marshal.AllocHGlobal(stackSize);

        #region Controls

#pragma warning disable CS8500 // Это принимает адрес, получает размер или объявляет указатель на управляемый тип
        private T* Allocate<T>(DSharpStackValueType type, int size)
            where T : struct
        {
            Frame* currentFrame = CurrentFrame;
            Frame* frame = (Frame*)_stackPointer + AllocatedSize;
            frame->ValueType = type;
            frame->PreviousFrame = currentFrame;
            
            if (currentFrame != null)
            {
                currentFrame->NextFrame = frame;
            }

            AllocatedSize += (uint)size + sizeof(DSharpStackValueType);

            if (AllocatedSize > Capacity)
            {
                throw new StackOverflowException();
            }

            return (T*)frame;
#pragma warning restore CS8500 // Это принимает адрес, получает размер или объявляет указатель на управляемый тип
        }
        private void Free(Frame* frame)
        {
            uint size = GetSize(frame);

            if (CurrentFrame == frame)
            {
                CurrentFrame = frame->PreviousFrame;
            }

            if (frame->NextFrame == null)
            {
                AllocatedSize -= size;

                if (frame->PreviousFrame != null)
                {
                    frame->PreviousFrame->NextFrame = null;
                }

                return;
            }

            var nextFrame = frame->NextFrame;

            while (nextFrame != null)
            {
                var currentSize = GetSize(nextFrame);

                if (nextFrame->NextFrame != null)
                {
                    nextFrame->NextFrame = (Frame*)((uint)nextFrame->NextFrame - size);
                }
                if (nextFrame->PreviousFrame != null)
                {
                    nextFrame->PreviousFrame = (Frame*)((uint)nextFrame->PreviousFrame - size);
                }

                Unsafe.CopyBlock((Frame*)((uint)nextFrame - size), nextFrame, currentSize);

                nextFrame = nextFrame->NextFrame;
            }
        }
        private uint GetSize(Frame* frame)
        {
            if (frame->PreviousFrame == null && frame->NextFrame == null)
            {
                return AllocatedSize;
            }
            else if (frame->NextFrame != null)
            {
                return (uint)frame->NextFrame - (uint)frame;
            }
            else if (frame->NextFrame == null)
            {
                return AllocatedSize - (uint)frame;
            }

            throw new ArgumentException("Invalid frame", nameof(frame));
        }

        #endregion

        #region Disposing

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (!IsDisposed)
            {
                Marshal.FreeHGlobal(_stackPointer);
            }
        }

        #endregion

        #region Frames

        [StructLayout(LayoutKind.Sequential)]
        public struct Frame
        {
            public DSharpStackValueType ValueType;
            public Frame* PreviousFrame;
            public Frame* NextFrame;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct ValueFrame<T> where T : struct
        {
            public Frame Frame;
            public T Value;
        }

        #endregion
    }
}

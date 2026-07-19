#if NETCOREAPP3_0_OR_GREATER
using System.Numerics;
#endif
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# virtual machine memory manager
    /// </summary>
    public class DSharpVmMemoryManager : Disposable
    {
        /// <summary>
        /// Amount of memory that currently used in bytes
        /// </summary>
        public int UsedMemory { get; private set; }
        /// <summary>
        /// Amount of free memory in bytes. This memory can be reused
        /// </summary>
        public int FreeMemory { get; private set; }
        /// <summary>
        /// Total amount of allocated memory in bytes
        /// </summary>
        public int AllocatedMemory { get; private set; }
        /// <summary>
        /// Collection of used memory blocks
        /// </summary>
        public ICollection<MemoryBlock> UsedBlocks => _usedBlocks.Values;

        private readonly Dictionary<nint, MemoryBlock> _usedBlocks = [];
        private readonly Dictionary<int, List<nint>> _freeBlocks = [];

#if NET9_0_OR_GREATER
        private readonly Lock _lock = new();
#else
        private readonly object _lock = new();
#endif

        #region Controls

        /// <summary>
        /// Allocated memory with specified size
        /// </summary>
        /// <param name="type">Memory block type for allocation</param>
        /// <param name="roundedSize">Size of memory for allocation</param>
        /// <returns>Address to allocated memory</returns>
        /// <exception cref="ObjectDisposedException">Unable to allocate memory at disposed memory manager</exception>
        /// <exception cref="ArgumentException">Size for allocation should be greater then 0</exception>
        public nint Allocate(DSharpMemoryBlockType type, int size)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DSharpVmMemoryManager), "Unable to allocate memory at disposed memory manager");
            }
            if (type == DSharpMemoryBlockType.Free)
            {
                throw new ArgumentException($"Invalid memory block type for allocation: {type}", nameof(type));
            }
            if (0 > size)
            {
                throw new ArgumentException("Size for allocation should be greater then 0", nameof(size));
            }

            int roundedSize = RoundUpSize(size);
            nint address;

            lock (_lock)
            {
                if (_freeBlocks.TryGetValue(roundedSize, out var blocks) &&
                    blocks.Count > 0)
                {
                    int lastIndex = blocks.Count - 1;
                    address = blocks[lastIndex];
                    blocks.RemoveAt(lastIndex);
                }
                else if (!TryTakeMemoryFromLargerSizes(roundedSize, out address))
                {
                    address = Marshal.AllocHGlobal(roundedSize);
                    AllocatedMemory += roundedSize;
                }

                _usedBlocks.Add(address, new(type, size, roundedSize));
                UsedMemory += size;
            }

            return address;
        }
        /// <summary>
        /// Free allocated memory
        /// </summary>
        /// <param name="address">Address of allocated memory</param>
        /// <returns>Is memory successfully free</returns>
        /// <exception cref="ObjectDisposedException">Unable to allocate memory at disposed memory manager</exception>
        public bool Free(nint address)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DSharpVmMemoryManager), "Unable to free memory at disposed memory manager");
            }
            
            lock (_lock)
            {
                if (_usedBlocks.TryGetValue(address, out var block))
                {
                    UsedMemory -= block.RequestedSize;
                    FreeMemory += block.RequestedSize;
                    _usedBlocks.Remove(address);
                    AddBlock(block.RequestedSize, address);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Defragment free blocks
        /// </summary>
        /// <returns>Amount of defragmented blocks</returns>
        public int DefragmentFreeBlocks()
        {
            lock (_lock)
            {
                return DefragmentFreeBlocksThreadUnsafe();
            }
        }

        private int DefragmentFreeBlocksThreadUnsafe()
        {
            bool isUpdated;
            int defragmentedBlocks = 0;

            do
            {
                isUpdated = false;

                foreach (var info in _freeBlocks)
                {
                    var size = info.Key;

                    foreach (var address in info.Value)
                    {
                        var endAddress = size + address;

                        foreach (var otherAddress in info.Value)
                        {
                            if (otherAddress != endAddress)
                            {
                                continue;
                            }

                            var newSize = RoundUpSize(size + 1);
                            info.Value.Remove(address);
                            info.Value.Remove(otherAddress);
                            AddBlock(newSize, address);

                            defragmentedBlocks++;
                            isUpdated = true;
                            break;
                        }

                        if (isUpdated)
                        {
                            break;
                        }
                    }

                    if (isUpdated)
                    {
                        break;
                    }
                }
            }
            while (isUpdated);

            return defragmentedBlocks;
        }

        private void AddBlock(int size, nint address)
        {
            if (!_freeBlocks.TryGetValue(size, out var stack))
            {
                stack = [];
                _freeBlocks.Add(size, stack);
            }

            stack.Add(address);
        }
        private bool TryTakeMemoryFromLargerSizes(int size, out nint address)
        {
            foreach (var info in _freeBlocks)
            {
                if (size >= info.Key)
                {
                    continue;
                }

                int roundedDownSize = RoundDownSize(info.Key - 1);

                if (size > roundedDownSize || info.Value.Count == 0)
                {
                    continue;
                }

                int lastIndex = info.Value.Count - 1;
                address = info.Value[lastIndex];
                info.Value.RemoveAt(lastIndex);

                do
                {
                    AddBlock(roundedDownSize, address + roundedDownSize);
                    roundedDownSize = RoundDownSize(roundedDownSize - 1);
                }
                while (roundedDownSize > size);

                return true;
            }

            address = 0;
            return false;
        }

        private static int RoundDownSize(int size)
        {
            if (size <= MinBlockSize)
            {
                return MinBlockSize;
            }

#if NETCOREAPP3_0_OR_GREATER
            int leading = 32 - BitOperations.LeadingZeroCount((uint)size);
            return 1 << (leading - 1);
#else
            size |= size >> 1;
            size |= size >> 2;
            size |= size >> 4;
            size |= size >> 8;
            size |= size >> 16;

            return size - (size >> 1);
#endif
        }
        private static int RoundUpSize(int size)
        {
            if (size <= MinBlockSize)
            {
                return MinBlockSize;
            }

#if NETCOREAPP3_0_OR_GREATER
            int leading = 32 - BitOperations.LeadingZeroCount((uint)(size - 1));
            return 1 << leading;
#else
            size--;
            size |= size >> 1;
            size |= size >> 2;
            size |= size >> 4;
            size |= size >> 8;
            size |= size >> 16;

            return size + 1;
#endif
        }

        #endregion

        #region Disposing

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            foreach (var stack in _freeBlocks.Values)
            {
                foreach (var address in stack)
                {
                    Marshal.FreeHGlobal(address);
                }

                stack.Clear();
            }
            foreach (var address in _usedBlocks.Keys)
            {
                Marshal.FreeHGlobal(address);
            }

            _freeBlocks.Clear();
            _usedBlocks.Clear();
            UsedMemory = 0;
            FreeMemory = 0;
            AllocatedMemory = 0;
        }

        #endregion

        #region Structs

        /// <summary>
        /// Memory block that used by D# virtual machine
        /// </summary>
        /// <param name="type">Type of this block</param>
        /// <param name="requestedSize">Memory block size that allocation requested</param>
        /// <param name="size">Real memory block size</param>
        public readonly struct MemoryBlock(DSharpMemoryBlockType type, int requestedSize, int size)
        {
            /// <summary>
            /// Type of this block
            /// </summary>
            public DSharpMemoryBlockType Type { get; } = type;
            /// <summary>
            /// Memory block size that allocation requested
            /// </summary>
            public int RequestedSize { get; } = requestedSize;
            /// <summary>
            /// Real memory block size
            /// </summary>
            public int BlockSize { get; } = size;
        }

        #endregion

        #region Constants

        private const int MinBlockSize = 8;

        #endregion
    }
}

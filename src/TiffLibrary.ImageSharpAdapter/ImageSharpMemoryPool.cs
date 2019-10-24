using System.Buffers;
using SixLabors.Memory;

namespace TiffLibrary.ImageSharpAdapter
{
    internal sealed class ImageSharpMemoryPool : MemoryPool<byte>
    {
        private readonly MemoryAllocator _memoryAllocator;

        public ImageSharpMemoryPool(MemoryAllocator memoryAllocator)
        {
            _memoryAllocator = memoryAllocator;
        }

        public override int MaxBufferSize => int.MaxValue;

        public override IMemoryOwner<byte> Rent(int minBufferSize = -1)
        {
            return _memoryAllocator.Allocate<byte>(minBufferSize, AllocationOptions.None);
        }

        protected override void Dispose(bool disposing)
        {
            // Noop
        }
    }
}

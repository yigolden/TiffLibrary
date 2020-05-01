using System;
using System.Buffers;
using SixLabors.ImageSharp.Memory;

namespace TiffLibrary.ImageSharpAdapter
{
    internal sealed class ImageSharpMemoryPool : MemoryPool<byte>
    {
        private readonly MemoryAllocator _memoryAllocator;

        private const int MinimumBufferSize = 4096;

        public ImageSharpMemoryPool(MemoryAllocator memoryAllocator)
        {
            _memoryAllocator = memoryAllocator;
        }

        public override int MaxBufferSize => int.MaxValue;

        public override IMemoryOwner<byte> Rent(int minBufferSize = -1)
        {
            return _memoryAllocator.AllocateManagedByteBuffer(Math.Max(minBufferSize, MinimumBufferSize));
        }

        protected override void Dispose(bool disposing)
        {
            // Noop
        }
    }
}

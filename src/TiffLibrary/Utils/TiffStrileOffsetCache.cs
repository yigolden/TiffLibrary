using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary.Utils
{
    internal class TiffStrileOffsetCache : MemoryManager<ulong>
    {
        private readonly bool _cacheEnabled;

        private readonly TiffFieldReader? _fieldReader;
        private readonly TiffValueCollection<ulong> _offsets;
        private readonly TiffValueCollection<ulong> _counts;
        private readonly TiffImageFileDirectoryEntry _offsetsEntry;
        private readonly TiffImageFileDirectoryEntry _countsEntry;
        private readonly int _entryCount;
        private readonly int _cacheSize;

        private int _cacheOffset;
        private int _cacheCount;
        private byte[]? _cache;

        private bool _memorySwitchForOffset;

        public TiffStrileOffsetCache(TiffValueCollection<ulong> offsets, TiffValueCollection<ulong> counts)
        {
            _cacheEnabled = false;
            _fieldReader = null;
            _offsets = offsets;
            _counts = counts;

            if (offsets.Count != counts.Count)
            {
                throw new InvalidDataException();
            }
            _entryCount = offsets.Count;
        }

        public TiffStrileOffsetCache(TiffFieldReader fieldReader, TiffImageFileDirectoryEntry offsets, TiffImageFileDirectoryEntry counts, int cacheSize)
        {
            _cacheEnabled = true;
            _fieldReader = fieldReader;
            _offsetsEntry = offsets;
            _countsEntry = counts;
            _cacheSize = cacheSize;

            if (offsets.ValueCount != counts.ValueCount)
            {
                throw new InvalidDataException();
            }
            _entryCount = (int)offsets.ValueCount;
        }

        public ValueTask<TiffStreamRegion> GetOffsetAndCountAsync(int index, CancellationToken cancellationToken)
        {
            if (index >= _entryCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (!_cacheEnabled)
            {
                return new ValueTask<TiffStreamRegion>(new TiffStreamRegion((long)_offsets[index], checked((int)_counts[index])));
            }
            int cacheOffset = _cacheOffset;
            if (index >= cacheOffset && index < (cacheOffset + _cacheCount))
            {
                return new ValueTask<TiffStreamRegion>(GetCacheItem(index - cacheOffset));
            }
            return new ValueTask<TiffStreamRegion>(AcquireAsync(index, cancellationToken));
        }

        private TiffStreamRegion GetCacheItem(int offset)
        {
            Span<byte> cache = _cache;
            ulong strileOffset = MemoryMarshal.Read<ulong>(cache.Slice(offset * 8));
            ulong strileCount = MemoryMarshal.Read<ulong>(cache.Slice((_cacheSize + offset) * 8));
            return new TiffStreamRegion((long)strileOffset, checked((int)strileCount));
        }

        private async Task<TiffStreamRegion> AcquireAsync(int index, CancellationToken cancellationToken)
        {
            EnsureCacheAllocated();

            int cacheCount = Math.Min(_cacheSize, _entryCount - index);
            _memorySwitchForOffset = true;
            await _fieldReader!.ReadLong8FieldAsync(_offsetsEntry, index, Memory.Slice(0, cacheCount), cancellationToken: cancellationToken).ConfigureAwait(false);

            _memorySwitchForOffset = false;
            await _fieldReader!.ReadLong8FieldAsync(_countsEntry, index, Memory.Slice(0, cacheCount), cancellationToken: cancellationToken).ConfigureAwait(false);

            _cacheOffset = index;
            _cacheCount = cacheCount;

            return GetCacheItem(0);
        }

        private void EnsureCacheAllocated()
        {
            if (_cache is null)
            {
                _cache = ArrayPool<byte>.Shared.Rent(_cacheSize * 16);
            }
        }

        public override Span<ulong> GetSpan()
        {
            if (_memorySwitchForOffset)
            {
                return MemoryMarshal.Cast<byte, ulong>(_cache.AsSpan(0, _cacheSize * 8));
            }
            else
            {
                return MemoryMarshal.Cast<byte, ulong>(_cache.AsSpan(_cacheSize * 8, _cacheSize * 8));
            }
        }

        public override unsafe MemoryHandle Pin(int elementIndex = 0)
        {
            var handle = GCHandle.Alloc(_cache, GCHandleType.Pinned);
            if (_memorySwitchForOffset)
            {
                return new MemoryHandle((void*)handle.AddrOfPinnedObject(), handle);
            }
            else
            {
                return new MemoryHandle((void*)(handle.AddrOfPinnedObject() + _cacheSize * 8), handle);
            }
        }

        public override void Unpin()
        {
            // Do nothing
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!(_cache is null))
                {
                    ArrayPool<byte>.Shared.Return(_cache);
                }
            }
            _cache = null;
            _cacheOffset = 0;
            _cacheCount = 0;
        }
    }
}

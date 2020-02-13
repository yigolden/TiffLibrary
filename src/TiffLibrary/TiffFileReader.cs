using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;

namespace TiffLibrary
{
    /// <summary>
    /// A reader class that read content from TIFF stream.
    /// </summary>
    public sealed class TiffFileReader : IDisposable, IAsyncDisposable
    {
        private ITiffFileContentSource _contentSource;
        private TiffOperationContext? _operationContext;
        private readonly long _imageFileDirectoryOffset;
        private readonly bool _leaveOpen;

        /// <summary>
        /// Gets the offset of the first IFD.
        /// </summary>
        public TiffStreamOffset FirstImageFileDirectoryOffset => new TiffStreamOffset(_imageFileDirectoryOffset);

        /// <summary>
        /// Gets whether this file is little-endian.
        /// </summary>
        public bool IsLittleEndian => _operationContext?.IsLittleEndian ?? throw new ObjectDisposedException(nameof(TiffFileReader));

        /// <summary>
        /// Gets whether this file is a standard TIFF file.
        /// </summary>
        public bool IsStandardTiff
        {
            get
            {
                TiffOperationContext? operationContext = _operationContext;
                if (operationContext is null)
                {
                    throw new ObjectDisposedException(nameof(TiffFileReader));
                }
                TiffOperationContext standardTiff = TiffOperationContext.StandardTIFF;
                return operationContext.ByteCountOfImageFileDirectoryCountField == standardTiff.ByteCountOfImageFileDirectoryCountField
                    && operationContext.ByteCountOfValueOffsetField == standardTiff.ByteCountOfValueOffsetField;
            }
        }

        /// <summary>
        /// Gets whether this file is a BigTIFF file.
        /// </summary>
        public bool IsBigTiff
        {
            get
            {
                TiffOperationContext? operationContext = _operationContext;
                if (operationContext is null)
                {
                    throw new ObjectDisposedException(nameof(TiffFileReader));
                }
                TiffOperationContext bigTiff = TiffOperationContext.BigTIFF;
                return operationContext.ByteCountOfImageFileDirectoryCountField == bigTiff.ByteCountOfImageFileDirectoryCountField
                    && operationContext.ByteCountOfValueOffsetField == bigTiff.ByteCountOfValueOffsetField;
            }
        }

        #region Constuction

        internal TiffFileReader(ITiffFileContentSource contentSource, in TiffFileHeader header, bool leaveOpen)
        {
            Debug.Assert(!(contentSource is null));
            _contentSource = contentSource!;
            _operationContext = header.CreateOperationContext(out long ifdOffset);
            _imageFileDirectoryOffset = ifdOffset;
            _leaveOpen = leaveOpen;
        }

        /// <summary>
        /// Initialize the instance with the specified <see cref="ITiffFileContentSource"/> and parameters.
        /// </summary>
        /// <param name="contentSource">The TIFF content source.</param>
        /// <param name="operationContext">Parameters of how the TIFF file should be parsed.</param>
        public TiffFileReader(ITiffFileContentSource contentSource, TiffOperationContext operationContext)
        {
            _contentSource = contentSource ?? throw new ArgumentNullException(nameof(contentSource));
            _operationContext = operationContext ?? throw new ArgumentNullException(nameof(operationContext));
            _leaveOpen = true;
        }

        /// <summary>
        /// Opens a TIFF file and creates <see cref="TiffFileReader"/>.
        /// </summary>
        /// <param name="fileName">The TIFF file.</param>
        /// <returns>A <see cref="Task"/> that completes when the TIFF header is read and returns <see cref="TiffFileReader"/>.</returns>
        public static Task<TiffFileReader> OpenAsync(string fileName)
        {
            var contentSource = TiffFileContentSource.Create(fileName, preferAsync: true);
            return OpenAsync(contentSource, leaveOpen: false);
        }

        /// <summary>
        /// Wraps the specified stream and creates <see cref="TiffFileReader"/>.
        /// </summary>
        /// <param name="stream">The stream to wrap.</param>
        /// <param name="leaveOpen">Whether the stream should be left open when the <see cref="TiffFileReader"/> is disposed or we failed to create <see cref="TiffFileReader"/>.</param>
        /// <returns>A <see cref="Task"/> that completes when the TIFF header is read and returns <see cref="TiffFileReader"/>.</returns>
        public static Task<TiffFileReader> OpenAsync(Stream stream, bool leaveOpen = false)
        {
            var contentSource = TiffFileContentSource.Create(stream, leaveOpen);
            return OpenAsync(contentSource, leaveOpen: false);
        }

        /// <summary>
        /// Opens a TIFF file in memory and creates <see cref="TiffFileReader"/>.
        /// </summary>
        /// <param name="memory">The in-memory TIFF file.</param>
        /// <returns>A <see cref="Task"/> that completes when the TIFF header is read and returns <see cref="TiffFileReader"/>.</returns>
        public static Task<TiffFileReader> OpenAsync(ReadOnlyMemory<byte> memory)
        {
            var contentSource = TiffFileContentSource.Create(memory);
            return OpenAsync(contentSource, leaveOpen: false);
        }

        /// <summary>
        /// Opens a TIFF file in memory and creates <see cref="TiffFileReader"/>.
        /// </summary>
        /// <param name="buffer">The buffer for in-memory TIFF file.</param>
        /// <param name="offset">The offset in the buffer.</param>
        /// <param name="count">The byte count of the TIFF file.</param>
        /// <returns>A <see cref="Task"/> that completes when the TIFF header is read and returns <see cref="TiffFileReader"/>.</returns>
        public static Task<TiffFileReader> OpenAsync(byte[] buffer, int offset, int count)
        {
            var contentSource = TiffFileContentSource.Create(buffer, offset, count);
            return OpenAsync(contentSource, leaveOpen: false);
        }

        /// <summary>
        /// Opens a TIFF file and creates <see cref="TiffFileReader"/>.
        /// </summary>
        /// <param name="fileName">The TIFF file.</param>
        /// <returns>The reader instance.</returns>
        public static TiffFileReader Open(string fileName)
        {
            var contentSource = TiffFileContentSource.Create(fileName, preferAsync: false);
            return Open(contentSource, leaveOpen: false);
        }

        /// <summary>
        /// Wraps the specified stream and creates <see cref="TiffFileReader"/>.
        /// </summary>
        /// <param name="stream">The stream to wrap.</param>
        /// <param name="leaveOpen">Whether the stream should be left open when the <see cref="TiffFileReader"/> is disposed or we failed to create <see cref="TiffFileReader"/>.</param>
        /// <returns>The reader instance.</returns>
        public static TiffFileReader Open(Stream stream, bool leaveOpen = false)
        {
            var contentSource = TiffFileContentSource.Create(stream, leaveOpen);
            return Open(contentSource, leaveOpen: false);
        }

        /// <summary>
        /// Opens a TIFF file in memory and creates <see cref="TiffFileReader"/>.
        /// </summary>
        /// <param name="memory">The in-memory TIFF file.</param>
        /// <returns>The reader instance.</returns>
        public static TiffFileReader Open(ReadOnlyMemory<byte> memory)
        {
            var contentSource = TiffFileContentSource.Create(memory);
            return Open(contentSource, leaveOpen: false);
        }

        /// <summary>
        /// Opens a TIFF file in memory and creates <see cref="TiffFileReader"/>.
        /// </summary>
        /// <param name="buffer">The buffer for in-memory TIFF file.</param>
        /// <param name="offset">The offset in the buffer.</param>
        /// <param name="count">The byte count of the TIFF file.</param>
        /// <returns>The reader instance.</returns>
        public static TiffFileReader Open(byte[] buffer, int offset, int count)
        {
            var contentSource = TiffFileContentSource.Create(buffer, offset, count);
            return Open(contentSource, leaveOpen: false);
        }

        /// <summary>
        /// Uses the specified stream source and creates <see cref="TiffFileReader"/>.
        /// </summary>
        /// <param name="contentSource">The content source to use.</param>
        /// <param name="leaveOpen">Whether the stream source should be left open when the <see cref="TiffFileReader"/> is disposed or when we failed to create <see cref="TiffFileReader"/>.</param>
        /// <returns>The reader instance.</returns>
        public static TiffFileReader Open(ITiffFileContentSource contentSource, bool leaveOpen = true)
        {
            return OpenAsync(new TiffSyncFileContentSource(contentSource), leaveOpen).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Uses the specified stream source and creates <see cref="TiffFileReader"/>.
        /// </summary>
        /// <param name="contentSource">The content source to use.</param>
        /// <param name="leaveOpen">Whether the stream source should be left open when the <see cref="TiffFileReader"/> is disposed or when we failed to create <see cref="TiffFileReader"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="Task"/> that completes when the TIFF header is read and returns <see cref="TiffFileReader"/>.</returns>
        public static async Task<TiffFileReader> OpenAsync(ITiffFileContentSource contentSource, bool leaveOpen = true, CancellationToken cancellationToken = default)
        {
            if (contentSource is null)
            {
                throw new ArgumentNullException(nameof(contentSource));
            }

            try
            {
                TiffFileContentReader reader = await contentSource.OpenReaderAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    byte[] buffer = ArrayPool<byte>.Shared.Rent(16);
                    try
                    {
                        // Read tiff header
                        int readCount = await reader.ReadAsync(0, new ArraySegment<byte>(buffer, 0, 16)).ConfigureAwait(false);
                        if (!TiffFileHeader.TryParse(new ReadOnlySpan<byte>(buffer, 0, readCount), out TiffFileHeader header))
                        {
                            throw new InvalidDataException();
                        }

                        return new TiffFileReader(Interlocked.Exchange(ref contentSource, null!), header, leaveOpen);
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(buffer);
                    }
                }
                finally
                {
                    await reader.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                if (!leaveOpen && !(contentSource is null))
                {
                    await contentSource.DisposeAsync().ConfigureAwait(false);
                }
            }

        }

        #endregion

        #region Read IFDs

        /// <summary>
        /// Read the first IFD.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="Task"/> that completes when the IFD is read and returns <see cref="TiffImageFileDirectory"/>.</returns>
        public Task<TiffImageFileDirectory> ReadImageFileDirectoryAsync(CancellationToken cancellationToken = default)
        {
            return ReadImageFileDirectoryAsync(FirstImageFileDirectoryOffset, cancellationToken);
        }

        /// <summary>
        /// Read the IFD from the specified offset.
        /// </summary>
        /// <param name="offset">The offset of the IFD.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="Task"/> that completes when the IFD is read and returns <see cref="TiffImageFileDirectory"/>.</returns>
        public async Task<TiffImageFileDirectory> ReadImageFileDirectoryAsync(TiffStreamOffset offset, CancellationToken cancellationToken = default)
        {
            if (_operationContext is null)
            {
                throw new ObjectDisposedException(nameof(TiffFileReader));
            }

            TiffFileContentReader reader = await _contentSource.OpenReaderAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return await ReadImageFileDirectoryAsync(reader, _operationContext, offset.ToInt64(), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await reader.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Read the first IFD.
        /// </summary>
        /// <returns>The <see cref="TiffImageFileDirectory"/> instance.</returns>
        public TiffImageFileDirectory ReadImageFileDirectory()
        {
            return ReadImageFileDirectory(FirstImageFileDirectoryOffset);
        }

        /// <summary>
        /// Read the IFD from the specified offset.
        /// </summary>
        /// <param name="offset">The offset of the IFD.</param>
        /// <returns>The <see cref="TiffImageFileDirectory"/> instance.</returns>
        public TiffImageFileDirectory ReadImageFileDirectory(TiffStreamOffset offset)
        {
            if (_operationContext is null)
            {
                throw new ObjectDisposedException(nameof(TiffFileReader));
            }

            using TiffFileContentReader reader = TiffSyncFileContentSource.WrapReader(_contentSource.OpenReader());
            return ReadImageFileDirectoryAsync(reader, _operationContext, offset.ToInt64(), CancellationToken.None).GetAwaiter().GetResult();
        }

        internal static int ParseImageFileDirectoryEntryCount(TiffOperationContext context, Span<byte> buffer)
        {
            Debug.Assert(buffer.Length >= 8);
            buffer.Slice(context.ByteCountOfImageFileDirectoryCountField).Clear();
            if (!context.IsLittleEndian)
            {
                buffer.Slice(0, context.ByteCountOfImageFileDirectoryCountField).Reverse();
            }

            ulong count = BinaryPrimitives.ReadUInt64LittleEndian(buffer);
            return checked((int)count);
        }
        internal static long ParseImageFileDirectoryOffset(TiffOperationContext context, Span<byte> buffer)
        {
            Debug.Assert(buffer.Length >= 8);
            buffer.Slice(context.ByteCountOfValueOffsetField).Clear();
            if (!context.IsLittleEndian)
            {
                buffer.Slice(0, context.ByteCountOfValueOffsetField).Reverse();
            }

            return BinaryPrimitives.ReadInt64LittleEndian(buffer);
        }

        internal static async Task<TiffImageFileDirectory> ReadImageFileDirectoryAsync(TiffFileContentReader reader, TiffOperationContext context, long offset, CancellationToken cancellationToken)
        {
            Debug.Assert(offset != 0);

            // Attemp to read 8 bytes even though the size of IFD may be less then 8 bytes.
            int count;
            byte[]? smallBuffer = null;
            try
            {
                smallBuffer = ArrayPool<byte>.Shared.Rent(8);
                await reader.ReadAsync(offset, new ArraySegment<byte>(smallBuffer, 0, 8), cancellationToken).ConfigureAwait(false);
                count = ParseImageFileDirectoryEntryCount(context, smallBuffer);
            }
            finally
            {
                if (!(smallBuffer is null))
                {
                    ArrayPool<byte>.Shared.Return(smallBuffer);
                }
            }

            // Read every entry.
            var ifd = new TiffImageFileDirectory(count);
            TiffImageFileDirectoryEntry[] entries = ifd.Entries;
            Debug.Assert(entries.Length == count);

            // entryFieldLength should be 12 (Standard TIFF) or 20 (BigTiff)
            int entryFieldLength = 4 + context.ByteCountOfValueOffsetField + context.ByteCountOfValueOffsetField;
            offset = offset + context.ByteCountOfImageFileDirectoryCountField;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(1024); // 1024 should be large enough to contain 50 entries plus a offset ((20 * 50 + 8) < 1024)
            try
            {
                // Loop through entries
                int index = 0;
                int readCount;
                while (count >= 50)
                {
                    readCount = await reader.ReadAsync(offset, new ArraySegment<byte>(buffer, 0, entryFieldLength * 50), cancellationToken).ConfigureAwait(false);
                    if (readCount != entryFieldLength * 50)
                    {
                        throw new InvalidDataException();
                    }
                    offset += readCount;
                    for (int i = 0; i < entryFieldLength * 50; i += entryFieldLength)
                    {
                        if (!TiffImageFileDirectoryEntry.TryParse(context, new ReadOnlySpan<byte>(buffer, i, entryFieldLength), out entries[index]))
                        {
                            throw new InvalidDataException();
                        }
                        index++;
                    }

                    count -= 50;
                }

                // Read remaining entries + the offset.
                count = (short)(entryFieldLength * count);
                readCount = await reader.ReadAsync(offset, new ArraySegment<byte>(buffer, 0, count + context.ByteCountOfValueOffsetField), cancellationToken).ConfigureAwait(false);
                if (readCount != (count + context.ByteCountOfValueOffsetField))
                {
                    throw new InvalidDataException();
                }
                for (int i = 0; i < count - context.ByteCountOfValueOffsetField; i += entryFieldLength)
                {
                    if (!TiffImageFileDirectoryEntry.TryParse(context, new ReadOnlySpan<byte>(buffer, i, entryFieldLength), out entries[index]))
                    {
                        throw new InvalidDataException();
                    }
                    index++;
                }

                ifd._nextOffset = ParseImageFileDirectoryOffset(context, buffer.AsSpan(count));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return ifd;
        }

        #endregion

        #region Content Reader

        /// <summary>
        /// Createa a <see cref="TiffFileContentReader"/> to read bytes from TIFF file source.
        /// </summary>
        /// <returns>The <see cref="TiffFileContentReader"/> instance.</returns>
        public TiffFileContentReader CreateContentReader()
        {
            return _contentSource.OpenReader();
        }

        /// <summary>
        /// Createa a <see cref="TiffFileContentReader"/> to read bytes from TIFF file source.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>The <see cref="TiffFileContentReader"/> instance.</returns>
        public async Task<TiffFileContentReader> CreateContentReaderAsync(CancellationToken cancellationToken = default)
        {
            return await _contentSource.OpenReaderAsync(cancellationToken).ConfigureAwait(false);
        }

        #endregion

        #region Field Reader

        /// <summary>
        /// Creates a <see cref="TiffFieldReader"/> to read field values. A new stream will be created from stream source if possible.
        /// </summary>
        /// <returns>The field reader.</returns>
        public TiffFieldReader CreateFieldReader()
        {
            if (_operationContext is null)
            {
                throw new ObjectDisposedException(nameof(TiffFileReader));
            }

            TiffFileContentReader reader = _contentSource.OpenReader();
            return new TiffFieldReader(reader, _operationContext, CancellationToken.None);
        }

        /// <summary>
        /// Creates a <see cref="TiffFieldReader"/> to read field values. A new stream will be created from stream source if possible.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that is preserved and used by the <see cref="TiffFieldReader"/> created. Cancel the token will cause the field reader to stop the on-going reading tasks and turns the field reader into an unusable state.</param>
        /// <returns>The field reader.</returns>
        public async Task<TiffFieldReader> CreateFieldReaderAsync(CancellationToken cancellationToken = default)
        {
            if (_operationContext is null)
            {
                throw new ObjectDisposedException(nameof(TiffFileReader));
            }

            TiffFileContentReader reader = await _contentSource.OpenReaderAsync(cancellationToken).ConfigureAwait(false);
            return new TiffFieldReader(reader, _operationContext, cancellationToken);
        }

        #endregion

        #region Image Decoder

        /// <summary>
        /// Creates a <see cref="TiffImageDecoder"/> for the first IFD with the default decoding options.
        /// </summary>
        /// <returns>An image decoder.</returns>
        public TiffImageDecoder CreateImageDecoder()
            => CreateImageDecoder((TiffImageDecoderOptions?)null);

        /// <summary>
        /// Creates a <see cref="TiffImageDecoder"/> for the first IFD with the default decoding options.
        /// </summary>
        /// <param name="options">The options to use when decoding image.</param>
        /// <returns>An image decoder.</returns>
        public TiffImageDecoder CreateImageDecoder(TiffImageDecoderOptions? options)
        {
            if (_operationContext is null)
            {
                throw new ObjectDisposedException(nameof(TiffFileReader));
            }

            TiffFileContentSource contentSource = TiffSyncFileContentSource.WrapSource(_contentSource);
            using TiffFileContentReader reader = contentSource.OpenReader();
            try
            {
                TiffImageFileDirectory ifd = ReadImageFileDirectoryAsync(reader, _operationContext, _imageFileDirectoryOffset, default).GetAwaiter().GetResult();
                return TiffDefaultImageDecoderFactory.CreateImageDecoderAsync(_operationContext, contentSource, reader, ifd, options, default).GetAwaiter().GetResult();
            }
            finally
            {
                reader.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a <see cref="TiffImageDecoder"/> for the specified IFD with the default decoding options.
        /// </summary>
        /// <param name="ifdOffset">The offset to the IFD.</param>
        /// <returns>An image decoder.</returns>
        public TiffImageDecoder CreateImageDecoder(TiffStreamOffset ifdOffset)
            => CreateImageDecoder(ifdOffset, null);

        /// <summary>
        /// Creates a <see cref="TiffImageDecoder"/> for the specified IFD with the default decoding options.
        /// </summary>
        /// <param name="ifdOffset">The offset to the IFD.</param>
        /// <param name="options">The options to use when decoding image.</param>
        /// <returns>An image decoder.</returns>
        public TiffImageDecoder CreateImageDecoder(TiffStreamOffset ifdOffset, TiffImageDecoderOptions? options)
        {
            if (_operationContext is null)
            {
                throw new ObjectDisposedException(nameof(TiffFileReader));
            }

            TiffFileContentSource contentSource = TiffSyncFileContentSource.WrapSource(_contentSource);
            using TiffFileContentReader reader = contentSource.OpenReader();
            try
            {
                TiffImageFileDirectory ifd = ReadImageFileDirectoryAsync(reader, _operationContext, ifdOffset.ToInt64(), default).GetAwaiter().GetResult();
                return TiffDefaultImageDecoderFactory.CreateImageDecoderAsync(_operationContext, contentSource, reader, ifd, options, default).GetAwaiter().GetResult();
            }
            finally
            {
                reader.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a <see cref="TiffImageDecoder"/> for the specified IFD with the default decoding options.
        /// </summary>
        /// <param name="ifd">The ifd to decode.</param>
        /// <returns>An image decoder.</returns>
        public TiffImageDecoder CreateImageDecoder(TiffImageFileDirectory ifd)
            => CreateImageDecoder(ifd, null);

        /// <summary>
        /// Creates a <see cref="TiffImageDecoder"/> for the specified IFD.
        /// </summary>
        /// <param name="ifd">The ifd to decode.</param>
        /// <param name="options">The options to use when decoding image.</param>
        /// <returns>An image decoder.</returns>
        public TiffImageDecoder CreateImageDecoder(TiffImageFileDirectory ifd, TiffImageDecoderOptions? options)
        {
            if (ifd is null)
            {
                throw new ArgumentNullException(nameof(ifd));
            }
            if (_operationContext is null)
            {
                throw new ObjectDisposedException(nameof(TiffFileReader));
            }

            return TiffDefaultImageDecoderFactory.CreateImageDecoderAsync(_operationContext, TiffSyncFileContentSource.WrapSource(_contentSource), ifd, options, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a <see cref="TiffImageDecoder"/> for the the first IFD with the default decoding options.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that fires if the user wants to stop the initialization process of <see cref="TiffImageDecoder"/>.</param>
        /// <returns>An image decoder.</returns>
        public Task<TiffImageDecoder> CreateImageDecoderAsync(CancellationToken cancellationToken = default)
            => CreateImageDecoderAsync((TiffImageDecoderOptions?)null, cancellationToken);

        /// <summary>
        /// Creates a <see cref="TiffImageDecoder"/> for the first IFD with the default decoding options.
        /// </summary>
        /// <param name="options">The options to use when decoding image.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that fires if the user wants to stop the initialization process of <see cref="TiffImageDecoder"/>.</param>
        /// <returns>An image decoder.</returns>
        public async Task<TiffImageDecoder> CreateImageDecoderAsync(TiffImageDecoderOptions? options, CancellationToken cancellationToken = default)
        {
            if (_operationContext is null)
            {
                throw new ObjectDisposedException(nameof(TiffFileReader));
            }

            TiffFileContentReader reader = await _contentSource.OpenReaderAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                TiffImageFileDirectory ifd = await ReadImageFileDirectoryAsync(reader, _operationContext, _imageFileDirectoryOffset, cancellationToken).ConfigureAwait(false);
                return await TiffDefaultImageDecoderFactory.CreateImageDecoderAsync(_operationContext, _contentSource, reader, ifd, options, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await reader.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a <see cref="TiffImageDecoder"/> for the specified IFD with the default decoding options.
        /// </summary>
        /// <param name="ifdOffset">The offset to the IFD.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that fires if the user wants to stop the initialization process of <see cref="TiffImageDecoder"/>.</param>
        /// <returns>An image decoder.</returns>
        public Task<TiffImageDecoder> CreateImageDecoderAsync(TiffStreamOffset ifdOffset, CancellationToken cancellationToken = default)
            => CreateImageDecoderAsync(ifdOffset, null, cancellationToken);

        /// <summary>
        /// Creates a <see cref="TiffImageDecoder"/> for the specified IFD with the default decoding options.
        /// </summary>
        /// <param name="ifdOffset">The offset to the IFD.</param>
        /// <param name="options">The options to use when decoding image.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that fires if the user wants to stop the initialization process of <see cref="TiffImageDecoder"/>.</param>
        /// <returns>An image decoder.</returns>
        public async Task<TiffImageDecoder> CreateImageDecoderAsync(TiffStreamOffset ifdOffset, TiffImageDecoderOptions? options, CancellationToken cancellationToken = default)
        {
            if (_operationContext is null)
            {
                throw new ObjectDisposedException(nameof(TiffFileReader));
            }

            TiffFileContentReader reader = await _contentSource.OpenReaderAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                TiffImageFileDirectory ifd = await ReadImageFileDirectoryAsync(reader, _operationContext, ifdOffset.ToInt64(), cancellationToken).ConfigureAwait(false);
                return await TiffDefaultImageDecoderFactory.CreateImageDecoderAsync(_operationContext, _contentSource, reader, ifd, options, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await reader.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a <see cref="TiffImageDecoder"/> for the specified IFD with the default decoding options.
        /// </summary>
        /// <param name="ifd">The ifd to decode.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that fires if the user wants to stop the initialization process of <see cref="TiffImageDecoder"/>.</param>
        /// <returns>An image decoder.</returns>
        public Task<TiffImageDecoder> CreateImageDecoderAsync(TiffImageFileDirectory ifd, CancellationToken cancellationToken = default)
            => CreateImageDecoderAsync(ifd, null, cancellationToken);

        /// <summary>
        /// Creates a <see cref="TiffImageDecoder"/> for the specified IFD.
        /// </summary>
        /// <param name="ifd">The ifd to decode.</param>
        /// <param name="options">The options to use when decoding image.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that fires if the user wants to stop the initialization process of <see cref="TiffImageDecoder"/>.</param>
        /// <returns>An image decoder.</returns>
        public Task<TiffImageDecoder> CreateImageDecoderAsync(TiffImageFileDirectory ifd, TiffImageDecoderOptions? options, CancellationToken cancellationToken = default)
        {
            if (ifd is null)
            {
                throw new ArgumentNullException(nameof(ifd));
            }
            if (_operationContext is null)
            {
                throw new ObjectDisposedException(nameof(TiffFileReader));
            }

            return TiffDefaultImageDecoderFactory.CreateImageDecoderAsync(_operationContext, _contentSource, ifd, options, cancellationToken);
        }

        #endregion

        #region IDisposable Support

        /// <inheritdoc />
        public void Dispose()
        {
            ITiffFileContentSource contentSource = Interlocked.Exchange(ref _contentSource, null!);
            if (!(contentSource is null) && !_leaveOpen)
            {
                contentSource.Dispose();
            }
            _operationContext = null;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            ITiffFileContentSource contentSource = Interlocked.Exchange(ref _contentSource, null!);
            if (!(contentSource is null) && !_leaveOpen)
            {
                await contentSource.DisposeAsync().ConfigureAwait(false);
            }
            _operationContext = null;
        }

        #endregion

    }
}

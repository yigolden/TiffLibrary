using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TiffLibrary.Utils
{
    /// <summary>
    /// Encodes and compresses the image data using dynamic Lempel-Ziv compression.
    /// </summary>
    /// <remarks>
    /// Adapted from Jef Poskanzer's Java port by way of J. M. G. Elliott. K Weiner 12/00
    /// <para>
    /// GIFCOMPR.C       - GIF Image compression routines
    /// </para>
    /// <para>
    /// Lempel-Ziv compression based on 'compress'.  GIF modifications by
    /// David Rowley (mgardi@watdcsu.waterloo.edu)
    /// </para>
    /// GIF Image compression - modified 'compress'
    /// <para>
    /// Based on: compress.c - File compression ala IEEE Computer, June 1984.
    /// By Authors:  Spencer W. Thomas      (decvax!harpo!utah-cs!utah-gr!thomas)
    ///              Jim McKie              (decvax!mcvax!jim)
    ///              Steve Davies           (decvax!vax135!petsd!peora!srd)
    ///              Ken Turkowski          (decvax!decwrl!turtlevax!ken)
    ///              James A. Woods         (decvax!ihnp4!ames!jaw)
    ///              Joe Orost              (decvax!vax135!petsd!joe)
    /// </para>
    /// </remarks>
    internal ref struct TiffLzwEncoder
    {
        /// <summary>
        /// 80% occupancy
        /// </summary>
        private const int HashSize = 5003;

        /// <summary>
        /// Mask used when shifting pixel values
        /// </summary>
        private static readonly int[] Masks =
        {
            0x0000, 0x0001, 0x0003, 0x0007, 0x000F, 0x001F, 0x003F, 0x007F, 0x00FF,
            0x01FF, 0x03FF, 0x07FF, 0x0FFF, 0x1FFF, 0x3FFF, 0x7FFF, 0xFFFF
        };

        /// <summary>
        /// The maximum number of bits/code.
        /// </summary>
        private const int MaxBits = 12;

        /// <summary>
        /// Should NEVER generate this code.
        /// </summary>
        private const int MaxMaxCode = 1 << MaxBits;

        /// <summary>
        /// The initial code size.
        /// </summary>
        private int _initialCodeSize;

        /// <summary>
        /// The hash table.
        /// </summary>
        private Span<int> _hashTable;

        /// <summary>
        /// The code table.
        /// </summary>
        private Span<int> _codeTable;

        /// <summary>
        /// Define the storage for the packet accumulator.
        /// </summary>
        private Span<byte> _accumulators;

        /// <summary>
        /// For dynamic table sizing
        /// </summary>
        private int _hsize;

        /// <summary>
        /// The current position within the pixelArray.
        /// </summary>
        private int _position;

        /// <summary>
        /// Number of bits/code
        /// </summary>
        private int _bitCount;

        /// <summary>
        /// maximum code, given bitCount
        /// </summary>
        private int _maxCode;

        /// <summary>
        /// First unused entry
        /// </summary>
        private int _freeEntry;

        /// <summary>
        /// Block compression parameters -- after all codes are used up,
        /// and compression rate changes, start over.
        /// </summary>
        private bool _clearFlag;

        /// <summary>
        /// Algorithm:  use open addressing double hashing (no chaining) on the
        /// prefix code / next character combination.  We do a variant of Knuth's
        /// algorithm D (vol. 3, sec. 6.4) along with G. Knott's relatively-prime
        /// secondary probe.  Here, the modular division first probe is gives way
        /// to a faster exclusive-or manipulation.  Also do block compression with
        /// an adaptive reset, whereby the code table is cleared when the compression
        /// ratio decreases, but after the table fills.  The variable-length output
        /// codes are re-sized at this point, and a special CLEAR code is generated
        /// for the decompressor.  Late addition:  construct the table according to
        /// file size for noticeable speed improvement on small files.  Please direct
        /// questions about this implementation to ames!jaw.
        /// </summary>
        private int _globalInitialBits;

        /// <summary>
        /// The clear code.
        /// </summary>
        private int _clearCode;

        /// <summary>
        /// The end-of-file code.
        /// </summary>
        private int _eofCode;

        /// <summary>
        /// Output the given code.
        /// Inputs:
        ///      code:   A bitCount-bit integer.  If == -1, then EOF.  This assumes
        ///              that bitCount =&lt; wordsize - 1.
        /// Outputs:
        ///      Outputs code to the file.
        /// Assumptions:
        ///      Chars are 8 bits long.
        /// Algorithm:
        ///      Maintain a BITS character long buffer (so that 8 codes will
        /// fit in it exactly).  Use the VAX insv instruction to insert each
        /// code in turn.  When the buffer fills up empty it and start over.
        /// </summary>
        private int _currentAccumulator;

        /// <summary>
        /// The current bits.
        /// </summary>
        private int _currentBits;

        /// <summary>
        /// Number of characters so far in this 'packet'
        /// </summary>
        private int _accumulatorCount;

        private byte[]? _buffer;

        private ReadOnlySpan<byte> _input;

        private IBufferWriter<byte>? _outputWriter;


        public void Initialize(ReadOnlySpan<byte> input, IBufferWriter<byte> outputWriter)
        {
            _hsize = HashSize;
            _initialCodeSize = 8;
            _buffer = ArrayPool<byte>.Shared.Rent(2 * HashSize * sizeof(int));
            Array.Clear(_buffer, 0, _buffer.Length);
            _hashTable = MemoryMarshal.Cast<byte, int>(_buffer).Slice(0, HashSize);
            _codeTable = MemoryMarshal.Cast<byte, int>(_buffer).Slice(HashSize, HashSize);
            _input = input;
            _outputWriter = outputWriter;
        }

        public void Dispose()
        {
            if (_outputWriter is not null)
            {
                _accumulators = default;
                _input = default;
                _outputWriter = default;
            }
            if (_buffer is not null)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
                _buffer = default;
                _hashTable = default;
                _codeTable = default;
            }
        }

        public void Encode()
        {
            _position = 0;

            // Compress and write the pixel data
            Compress(_input, _initialCodeSize + 1);
        }

        /// <summary>
        /// Gets the maximum code value.
        /// </summary>
        /// <param name="bitCount">The number of bits</param>
        /// <returns>See <see cref="int"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetMaxcode(int bitCount)
        {
            return (1 << bitCount) - 1;
        }

        /// <summary>
        /// Add a character to the end of the current packet, and if it is 4096 characters,
        /// flush the packet to disk.
        /// </summary>
        /// <param name="c">The character to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddCharacter(byte c)
        {
            Debug.Assert(_outputWriter != null);

            if (_accumulators.Length < 4096)
            {
                _accumulators = _outputWriter!.GetSpan(4096);
            }

            _accumulators[_accumulatorCount++] = c;

            if (_accumulatorCount >= 4096)
            {
                FlushPacket();
            }
        }

        /// <summary>
        /// Table clear for block compress.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearBlock()
        {
            ResetCodeTable();
            _freeEntry = _clearCode + 2;
            _clearFlag = true;

            Output(_clearCode);
        }

        /// <summary>
        /// Reset the code table.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetCodeTable()
        {
            _hashTable.Fill(-1);
        }

        /// <summary>
        /// Reads the next pixel from the image.
        /// </summary>
        /// <param name="indexedPixels">The span of indexed pixels.</param>
        /// <returns>
        /// The <see cref="int"/>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int NextPixel(ReadOnlySpan<byte> indexedPixels)
        {
            return indexedPixels[_position++];
        }

        /// <summary>
        /// Compress the packets to the stream.
        /// </summary>
        /// <param name="indexedPixels">The span of indexed pixels.</param>
        /// <param name="initialBits">The initial bits.</param>
        private void Compress(ReadOnlySpan<byte> indexedPixels, int initialBits)
        {
            int fcode;
            int c;
            int ent;
            int hsizeReg;
            int hshift;

            // Set up the globals: globalInitialBits - initial number of bits
            _globalInitialBits = initialBits;

            // Set up the necessary values
            _clearFlag = false;
            _bitCount = _globalInitialBits;
            _maxCode = GetMaxcode(_bitCount);

            _clearCode = 1 << (initialBits - 1);
            _eofCode = _clearCode + 1;
            _freeEntry = _clearCode + 2;

            _accumulatorCount = 0; // clear packet

            ent = NextPixel(indexedPixels);

            // TODO: PERF: It looks likt hshift could be calculated once statically.
            hshift = 0;
            for (fcode = _hsize; fcode < 65536; fcode *= 2)
            {
                ++hshift;
            }

            hshift = 8 - hshift; // set hash code range bound

            hsizeReg = _hsize;

            ResetCodeTable(); // clear hash table

            Output(_clearCode);

            ref int hashTableRef = ref MemoryMarshal.GetReference(_hashTable);
            ref int codeTableRef = ref MemoryMarshal.GetReference(_codeTable);

            while (_position < indexedPixels.Length)
            {
                c = NextPixel(indexedPixels);

                fcode = (c << MaxBits) + ent;
                int i = (c << hshift) ^ ent /* = 0 */;

                if (Unsafe.Add(ref hashTableRef, i) == fcode)
                {
                    ent = Unsafe.Add(ref codeTableRef, i);
                    continue;
                }

                // Non-empty slot
                if (Unsafe.Add(ref hashTableRef, i) >= 0)
                {
                    int disp = 1;
                    if (i != 0)
                    {
                        disp = hsizeReg - i;
                    }

                    do
                    {
                        if ((i -= disp) < 0)
                        {
                            i += hsizeReg;
                        }

                        if (Unsafe.Add(ref hashTableRef, i) == fcode)
                        {
                            ent = Unsafe.Add(ref codeTableRef, i);
                            break;
                        }
                    }
                    while (Unsafe.Add(ref hashTableRef, i) >= 0);

                    if (Unsafe.Add(ref hashTableRef, i) == fcode)
                    {
                        continue;
                    }
                }

                Output(ent);
                ent = c;
                if (_freeEntry < MaxMaxCode)
                {
                    Unsafe.Add(ref codeTableRef, i) = _freeEntry++; // code -> hashtable
                    Unsafe.Add(ref hashTableRef, i) = fcode;
                }
                else
                {
                    ClearBlock();
                }
            }

            // Put out the final code.
            Output(ent);

            Output(_eofCode);
        }

        /// <summary>
        /// Flush the packet to disk and reset the accumulator.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FlushPacket()
        {
            Debug.Assert(_outputWriter != null);
            _outputWriter!.Advance(_accumulatorCount);
            _accumulators = _accumulators.Slice(_accumulatorCount);
            _accumulatorCount = 0;
        }

        /// <summary>
        /// Output the current code to the stream.
        /// </summary>
        /// <param name="code">The code.</param>
        private void Output(int code)
        {
            _currentAccumulator &= Masks[_currentBits];

            if (_currentBits > 0)
            {
                _currentAccumulator |= code << _currentBits;
            }
            else
            {
                _currentAccumulator = code;
            }

            _currentBits += _bitCount;

            while (_currentBits >= 8)
            {
                AddCharacter((byte)_currentAccumulator);
                _currentAccumulator >>= 8;
                _currentBits -= 8;
            }

            // If the next entry is going to be too big for the code size,
            // then increase it, if possible.
            if (_freeEntry > _maxCode || _clearFlag)
            {
                if (_clearFlag)
                {
                    _maxCode = GetMaxcode(_bitCount = _globalInitialBits);
                    _clearFlag = false;
                }
                else
                {
                    ++_bitCount;
                    _maxCode = _bitCount == MaxBits
                        ? MaxMaxCode
                        : GetMaxcode(_bitCount);
                }
            }

            if (code == _eofCode)
            {
                // At EOF, write the rest of the buffer.
                while (_currentBits > 0)
                {
                    AddCharacter((byte)_currentAccumulator);
                    _currentAccumulator >>= 8;
                    _currentBits -= 8;
                }

                if (_accumulatorCount > 0)
                {
                    FlushPacket();
                }
            }
        }
    }
}

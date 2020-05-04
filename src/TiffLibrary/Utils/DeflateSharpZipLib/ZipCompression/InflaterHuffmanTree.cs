using System;
using System.Diagnostics;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace ICSharpCode.SharpZipLib.Zip.Compression
{
    /// <summary>
    /// Huffman tree used for inflation
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class InflaterHuffmanTree
    {
        #region Constants

        private const int MAX_BITLEN = 15;

        #endregion Constants

        #region Instance Fields

        private short[]? tree;

        #endregion Instance Fields

        /// <summary>
        /// Literal length tree
        /// </summary>
        public static InflaterHuffmanTree defLitLenTree = new InflaterHuffmanTree(codeLengthsForLitLenTree);

        /// <summary>
        /// Distance tree
        /// </summary>
        public static InflaterHuffmanTree defDistTree = new InflaterHuffmanTree(codeLengthsForDistTree);

        private static ReadOnlySpan<byte> codeLengthsForLitLenTree => new byte[]
        {
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
        };

        private static ReadOnlySpan<byte> codeLengthsForDistTree => new byte[]
        {
            5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
            5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5
        };

        #region Constructors

        /// <summary>
        /// Constructs a Huffman tree from the array of code lengths.
        /// </summary>
        /// <param name = "codeLengths">
        /// the array of code lengths
        /// </param>
        public InflaterHuffmanTree(ReadOnlySpan<byte> codeLengths)
        {
            BuildTree(codeLengths);
        }

        #endregion Constructors

        private void BuildTree(ReadOnlySpan<byte> codeLengths)
        {
            int[] blCount = new int[MAX_BITLEN + 1];
            int[] nextCode = new int[MAX_BITLEN + 1];

            for (int i = 0; i < codeLengths.Length; i++)
            {
                int bits = codeLengths[i];
                if (bits > 0)
                {
                    blCount[bits]++;
                }
            }

            int code = 0;
            int treeSize = 512;
            for (int bits = 1; bits <= MAX_BITLEN; bits++)
            {
                nextCode[bits] = code;
                code += blCount[bits] << (16 - bits);
                if (bits >= 10)
                {
                    /* We need an extra table for bit lengths >= 10. */
                    int start = nextCode[bits] & 0x1ff80;
                    int end = code & 0x1ff80;
                    treeSize += (end - start) >> (16 - bits);
                }
            }

            /* -jr comment this out! doesnt work for dynamic trees and pkzip 2.04g
                        if (code != 65536)
                        {
                            throw new InvalidDataException("Code lengths don't add up properly.");
                        }
            */
            /* Now create and fill the extra tables from longest to shortest
            * bit len.  This way the sub trees will be aligned.
            */
            tree = new short[treeSize];
            int treePtr = 512;
            for (int bits = MAX_BITLEN; bits >= 10; bits--)
            {
                int end = code & 0x1ff80;
                code -= blCount[bits] << (16 - bits);
                int start = code & 0x1ff80;
                for (int i = start; i < end; i += 1 << 7)
                {
                    tree[DeflaterHuffman.BitReverse(i)] = (short)((-treePtr << 4) | bits);
                    treePtr += 1 << (bits - 9);
                }
            }

            for (int i = 0; i < codeLengths.Length; i++)
            {
                int bits = codeLengths[i];
                if (bits == 0)
                {
                    continue;
                }
                code = nextCode[bits];
                int revcode = DeflaterHuffman.BitReverse(code);
                if (bits <= 9)
                {
                    do
                    {
                        tree[revcode] = (short)((i << 4) | bits);
                        revcode += 1 << bits;
                    } while (revcode < 512);
                }
                else
                {
                    int subTree = tree[revcode & 511];
                    int treeLen = 1 << (subTree & 15);
                    subTree = -(subTree >> 4);
                    do
                    {
                        tree[subTree | (revcode >> 9)] = (short)((i << 4) | bits);
                        revcode += 1 << bits;
                    } while (revcode < treeLen);
                }
                nextCode[bits] = code + (1 << (16 - bits));
            }
        }

        /// <summary>
        /// Reads the next symbol from input.  The symbol is encoded using the
        /// huffman tree.
        /// </summary>
        /// <param name="input">
        /// input the input source.
        /// </param>
        /// <returns>
        /// the next symbol, or -1 if not enough input is available.
        /// </returns>
        public int GetSymbol(StreamManipulator input)
        {
            Debug.Assert(tree != null);

            int lookahead, symbol;
            if ((lookahead = input.PeekBits(9)) >= 0)
            {
                if ((symbol = tree![lookahead]) >= 0)
                {
                    input.DropBits(symbol & 15);
                    return symbol >> 4;
                }
                int subtree = -(symbol >> 4);
                int bitlen = symbol & 15;
                if ((lookahead = input.PeekBits(bitlen)) >= 0)
                {
                    symbol = tree[subtree | (lookahead >> 9)];
                    input.DropBits(symbol & 15);
                    return symbol >> 4;
                }
                else
                {
                    int bits = input.AvailableBits;
                    lookahead = input.PeekBits(bits);
                    symbol = tree[subtree | (lookahead >> 9)];
                    if ((symbol & 15) <= bits)
                    {
                        input.DropBits(symbol & 15);
                        return symbol >> 4;
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            else // Less than 9 bits
            {
                int bits = input.AvailableBits;
                lookahead = input.PeekBits(bits);
                symbol = tree![lookahead];
                if (symbol >= 0 && (symbol & 15) <= bits)
                {
                    input.DropBits(symbol & 15);
                    return symbol >> 4;
                }
                else
                {
                    return -1;
                }
            }
        }
    }
}

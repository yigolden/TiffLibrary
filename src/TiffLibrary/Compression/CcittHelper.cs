using System;
using System.IO;

namespace TiffLibrary.Compression
{
    internal static class CcittHelper
    {
        internal static void SwapTable<T>(ref T table1, ref T table2)
        {
            T temp = table1;
            table1 = table2;
            table2 = temp;
        }

        internal static int DecodeRun(ref BitReader bitReader, CcittDecodingTable table, byte fillColor, Span<byte> destination)
        {
            int unpacked = 0;
            while (true)
            {
                // Read next code word
                if (!table.TryLookup(bitReader.Peek(16), out CcittCodeValue tableEntry))
                {
                    throw new InvalidDataException();
                }

                if (tableEntry.IsEndOfLine)
                {
                    throw new InvalidDataException();
                }

                // Fill this run
                int runLength = tableEntry.RunLength;
                destination.Slice(0, runLength).Fill(fillColor);
                destination = destination.Slice(runLength);
                unpacked += runLength;
                bitReader.Advance(tableEntry.BitsRequired);

                // Terminating code is met.
                if (tableEntry.IsTerminatingCode)
                {
                    return unpacked;
                }
            }
        }
    }
}

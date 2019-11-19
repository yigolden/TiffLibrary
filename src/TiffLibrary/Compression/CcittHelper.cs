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
                CcittCodeValue tableEntry = table.Lookup(bitReader.Peek(16));

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
                else if (runLength == 0)
                {
                    throw new InvalidDataException();
                }
            }
        }
    }
}

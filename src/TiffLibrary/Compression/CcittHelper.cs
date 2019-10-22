using System;
using System.IO;

namespace TiffLibrary.Compression
{
    internal static class CcittHelper
    {
        internal static void SwapTable(ref ReadOnlySpan<CcittCodeLookupTable.Entry> table1, ref ReadOnlySpan<CcittCodeLookupTable.Entry> table2)
        {
            ReadOnlySpan<CcittCodeLookupTable.Entry> temp = table1;
            table1 = table2;
            table2 = temp;
        }

        internal static int DecodeRun(ref BitReader bitReader, ReadOnlySpan<CcittCodeLookupTable.Entry> table, byte fillColor, Span<byte> destination)
        {
            int unpacked = 0;
            while (true)
            {
                // Read next code word
                int value = (int)bitReader.Peek(13);
                CcittCodeLookupTable.Entry tableEntry = table[value];

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

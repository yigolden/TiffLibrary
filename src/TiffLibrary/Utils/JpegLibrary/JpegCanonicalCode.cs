using System;
using System.Collections.Generic;

namespace JpegLibrary
{
    internal struct JpegCanonicalCode
    {
        public ushort Code;
        public byte Symbol;
        public byte CodeLength;

        public override string ToString()
        {
            return $"JpegCanonicalCode(Symbol={Symbol},Code={Convert.ToString(Code, 2).PadLeft(CodeLength, '0')},CodeLength={CodeLength})";
        }
    }

    internal class JpegCanonicalCodeCompareByCodeLen : Comparer<JpegCanonicalCode>
    {
        public static JpegCanonicalCodeCompareByCodeLen Instance { get; } = new JpegCanonicalCodeCompareByCodeLen();

        public override int Compare(JpegCanonicalCode x, JpegCanonicalCode y)
        {
            if (x.CodeLength > y.CodeLength)
            {
                return 1;
            }
            else if (x.CodeLength < y.CodeLength)
            {
                return -1;
            }
            else
            {
                if (x.Symbol > y.Symbol)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
        }
    }
}

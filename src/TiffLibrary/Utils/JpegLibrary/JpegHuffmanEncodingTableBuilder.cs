#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JpegLibrary
{
    internal class JpegHuffmanEncodingTableBuilder
    {
        private int[] _frequencies;

        public JpegHuffmanEncodingTableBuilder()
        {
            _frequencies = new int[256];
        }

        public void IncrementCodeCount(int symbol)
        {
            Debug.Assert(symbol <= 255);
            _frequencies[symbol]++;
        }

        public void Reset()
        {
            _frequencies.AsSpan().Clear();
        }

        struct Symbol
        {
            public int Frequency;
            public short Value;
            public ushort CodeSize;
            public short Others;

            public override string ToString()
            {
                return $"Symbol[{Value}](Frequency={Frequency}, CodeSize={CodeSize})";
            }
        }

        public JpegHuffmanEncodingTable Build(bool optimal = false)
        {
            return optimal ? BuildUsingPackageMerge() : BuildUsingStandardMethod();
        }

        #region Standard Method

        private JpegHuffmanEncodingTable BuildUsingStandardMethod()
        {
            // Find code count
            int codeCount = 0;
            int[] frequencies = _frequencies;
            for (int i = 0; i < frequencies.Length; i++)
            {
                if (frequencies[i] > 0)
                {
                    codeCount++;
                }
            }

            if (codeCount == 0)
            {
                throw new InvalidOperationException("No symbol is recorded.");
            }

            // Build symbol list
            Symbol[] symbols = new Symbol[codeCount + 1];
            int index = 0;
            for (int i = 0; i < frequencies.Length; i++)
            {
                if (frequencies[i] > 0)
                {
                    symbols[index++] = new Symbol
                    {
                        Value = (short)i,
                        Frequency = frequencies[i],
                        CodeSize = 0,
                        Others = -1
                    };
                }
            }
            symbols[index] = new Symbol
            {
                Value = -1,
                Frequency = 1,
                CodeSize = 0,
                Others = -1
            };

            // Figure K.1 – Procedure to find Huffman code sizes
            FindHuffmanCodeSize(symbols);

            // Figure K.2 – Procedure to find the number of codes of each size
            byte[] bits = new byte[32];
            for (int i = 0; i < symbols.Length; i++)
            {
                if (symbols[i].CodeSize > 0)
                {
                    bits[symbols[i].CodeSize - 1]++;
                }
            }

            // Figure K.3 – Procedure for limiting code lengths to 16 bits
            index = 31;
            while (true)
            {
                while (bits[index] > 0)
                {
                    int j = index - 1;
                    do
                    {
                        j -= 1;
                    } while (bits[j] == 0);

                    bits[index] -= 2;
                    bits[index - 1] += 1;
                    bits[j + 1] += 2;
                    bits[j] = bits[j] -= 1;
                }

                index -= 1;

                if (index != 15)
                {
                    continue;
                }

                while (bits[index] == 0)
                {
                    index--;
                }

                bits[index]--;
                break;
            }

            // Sort symbols
            for (int i = 0; i < symbols.Length; i++)
            {
                if (symbols[i].Value == -1)
                {
                    symbols[i].CodeSize = ushort.MaxValue;
                }
            }
            Array.Sort(symbols, (x, y) => x.CodeSize.CompareTo(y.CodeSize));

            // Figure K.4 – Sorting of input values according to code size
            JpegCanonicalCode[] codes = BuildCanonicalCode(bits, symbols.AsSpan(0, codeCount));

            return new JpegHuffmanEncodingTable(codes);
        }

        private static void FindHuffmanCodeSize(Span<Symbol> symbols)
        {
            while (true)
            {
                int v1 = -1, v2 = -1;
                int v1frequency = -1, v2frequency = -1;

                // Find V1 for least value of FREQ(V1) > 0
                for (int i = 0; i < symbols.Length; i++)
                {
                    int frequency = symbols[i].Frequency;
                    if (frequency >= 0)
                    {
                        if (v1 == -1 || frequency < v1frequency)
                        {
                            v1 = i;
                            v1frequency = frequency;
                        }
                    }
                }

                // Find V2 for next least value of FREQ(V2) > 0
                for (int i = 0; i < symbols.Length; i++)
                {
                    int frequency = symbols[i].Frequency;
                    if (frequency >= 0 && i != v1)
                    {
                        if (v2 == -1 || frequency < v2frequency)
                        {
                            v2 = i;
                            v2frequency = frequency;
                        }
                    }
                }

                // V2 exists
                if (v2 == -1)
                {
                    break;
                }

                symbols[v1].Frequency += symbols[v2].Frequency;
                symbols[v2].Frequency = -1;

                while (true)
                {
                    symbols[v1].CodeSize++;

                    if (symbols[v1].Others == -1)
                    {
                        break;
                    }

                    v1 = symbols[v1].Others;
                }

                symbols[v1].Others = (short)v2;

                while (true)
                {
                    symbols[v2].CodeSize++;

                    if (symbols[v2].Others == -1)
                    {
                        break;
                    }

                    v2 = symbols[v2].Others;
                }
            }
        }

        private static JpegCanonicalCode[] BuildCanonicalCode(ReadOnlySpan<byte> bits, ReadOnlySpan<Symbol> symbols)
        {
            int codeCount = symbols.Length;
            var codes = new JpegCanonicalCode[codeCount];

            int currentCodeLength = 1;
            ref byte codeLengthsRef = ref MemoryMarshal.GetReference(bits);

            for (int i = 0; i < codes.Length; i++)
            {
                while (codeLengthsRef == 0)
                {
                    codeLengthsRef = ref Unsafe.Add(ref codeLengthsRef, 1);
                    currentCodeLength++;
                }
                codeLengthsRef--;

                codes[i].Symbol = (byte)symbols[i].Value;
                codes[i].CodeLength = (byte)currentCodeLength;
            }

            ushort bitCode = codes[0].Code = 0;
            int bitCount = codes[0].CodeLength;

            for (int i = 1; i < codes.Length; i++)
            {
                ref JpegCanonicalCode code = ref codes[i];

                if (code.CodeLength > bitCount)
                {
                    bitCode++;
                    bitCode <<= (code.CodeLength - bitCount);
                    code.Code = bitCode;
                    bitCount = code.CodeLength;
                }
                else
                {
                    code.Code = ++bitCode;
                }
            }

            return codes;
        }

        #endregion

        #region Package Merge Method

        private JpegHuffmanEncodingTable BuildUsingPackageMerge()
        {
            // Find code count
            int codeCount = 0;
            int[] frequencies = _frequencies;
            for (int i = 0; i < frequencies.Length; i++)
            {
                if (frequencies[i] > 0)
                {
                    codeCount++;
                }
            }

            // Build symbol list
            Symbol[] symbols = new Symbol[codeCount + 1];
            int index = 0;
            for (int i = 0; i < frequencies.Length; i++)
            {
                if (frequencies[i] > 0)
                {
                    symbols[index++] = new Symbol
                    {
                        Value = (short)i,
                        Frequency = frequencies[i],
                        CodeSize = 0
                    };
                }
            }
            symbols[index] = new Symbol
            {
                Value = -1,
                Frequency = 0,
                CodeSize = 0
            };

            RunPackageMerge(symbols);

            Array.Sort(symbols, SymbolComparer.Instance);

            index = 0;
            for (int i = symbols.Length - 1; i >= 0; i--)
            {
                if (symbols[i].Value == -1)
                {
                    index = i;
                    break;
                }
            }

            for (int i = index; i < symbols.Length - 1; i++)
            {
                symbols[i] = symbols[i + 1];
            }

            JpegCanonicalCode[] codes = BuildCanonicalCode(symbols.AsSpan(0, codeCount));

            return new JpegHuffmanEncodingTable(codes);
        }

        private static void RunPackageMerge(Symbol[] symbols)
        {
            Array.Sort(symbols, (x, y) => y.Frequency.CompareTo(x.Frequency)); // descending
            int codeCount = symbols.Length;

            // Initialize
            var levels = new List<Node>[16];
            for (int l = levels.Length - 1, nodeCount = codeCount; l >= 0; l--, nodeCount += nodeCount / 2)
            {
                var nodes = new List<Node>(nodeCount);
                for (int i = 0; i < codeCount; i++)
                {
                    var node = new Node();
                    node.Set((short)i, symbols[i].Frequency);
                    nodes.Add(node);
                }
                levels[l] = nodes;
            }

            // Run package merge
            for (int l = levels.Length - 1; l > 0; l--)
            {
                List<Node> nodes = levels[l];
                List<Node> nextLevelNodes = levels[l - 1];
                nodes.Sort((x, y) => y.Frequency.CompareTo(x.Frequency)); // descending
                for (int nodeCount = nodes.Count; nodeCount >= 2; nodeCount = nodes.Count)
                {
                    // Take last two nodes
                    Node node1 = nodes[nodeCount - 1];
                    Node node2 = nodes[nodeCount - 2];
                    nodes.RemoveAt(nodeCount - 1);
                    nodes.RemoveAt(nodeCount - 2);

                    // Package
                    var node = new Node();
                    node.Set(node1, node2);

                    // Merge
                    nextLevelNodes.Add(node);
                }
            }

            List<Node> level0 = levels[0];
            level0.Sort((x, y) => x.Frequency.CompareTo(y.Frequency)); // ascending
            int selectCount = Math.Max(1, 2 * (codeCount - 1));
            for (int i = 0; i < selectCount; i++)
            {
                TraverseNode(level0[i], symbols);
            }

            static void TraverseNode(Node? node, Symbol[] symbols)
            {
                if (node is null)
                {
                    return;
                }
                else if (node.Left is null)
                {
                    symbols[node.Index].CodeSize++;
                }
                else
                {
                    TraverseNode(node.Left, symbols);
                    TraverseNode(node.Right, symbols);
                }
            }
        }

        class SymbolComparer : Comparer<Symbol>
        {
            public static SymbolComparer Instance { get; } = new SymbolComparer();

            public override int Compare(Symbol x, Symbol y)
            {
                if (x.CodeSize > y.CodeSize)
                {
                    return 1;
                }
                if (x.CodeSize < y.CodeSize)
                {
                    return -1;
                }
                if (x.Frequency > y.Frequency)
                {
                    return -1;
                }
                if (x.Frequency < y.Frequency)
                {
                    return 1;
                }
                return 0;
            }
        }

        class Node
        {
            public short Index { get; set; }
            public int Frequency { get; set; }
            public Node? Left { get; set; }
            public Node? Right { get; set; }

            public void Set(short index, int frequency)
            {
                Index = index;
                Frequency = frequency;
            }

            public void Set(Node left, Node right)
            {
                Frequency = left.Frequency + right.Frequency;
                Left = left;
                Right = right;
            }
        }

        private static JpegCanonicalCode[] BuildCanonicalCode(ReadOnlySpan<Symbol> symbols)
        {
            int codeCount = symbols.Length;
            var codes = new JpegCanonicalCode[codeCount];

            for (int i = 0; i < codes.Length; i++)
            {
                codes[i].Symbol = (byte)symbols[i].Value;
                codes[i].CodeLength = (byte)symbols[i].CodeSize;
            }

            ushort bitCode = codes[0].Code = 0;
            int bitCount = codes[0].CodeLength;

            for (int i = 1; i < codes.Length; i++)
            {
                ref JpegCanonicalCode code = ref codes[i];

                if (code.CodeLength > bitCount)
                {
                    bitCode++;
                    bitCode <<= (code.CodeLength - bitCount);
                    code.Code = bitCode;
                    bitCount = code.CodeLength;
                }
                else
                {
                    code.Code = ++bitCode;
                }
            }

            return codes;
        }

        #endregion
    }
}

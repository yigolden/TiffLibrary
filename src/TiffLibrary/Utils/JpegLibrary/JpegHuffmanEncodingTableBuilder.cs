#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace JpegLibrary
{
    internal class JpegHuffmanEncodingTableBuilder
    {
        private Symbol[] _symbols;

        public JpegHuffmanEncodingTableBuilder()
        {
            //_allocator = allocator ?? JpegHuffmanTableAllocator.Default;
            _symbols = new Symbol[256];
            for (int i = 0; i < _symbols.Length; i++)
            {
                _symbols[i].Value = (byte)i;
            }
        }

        public void IncrementCodeCount(int symbol)
        {
            Symbol[] symbols = _symbols;
            if (symbols is null)
            {
                ThrowInvalidOperationException();
            }
            Debug.Assert((uint)symbol < 256);
            symbols[symbol].Count++;
        }

        public JpegHuffmanEncodingTable Build()
        {
            int codeCount = CalculateCodeCount();
            if (codeCount == 0)
            {
                throw new InvalidOperationException("No code is specified.");
            }

            Array.Sort(_symbols, (x, y) => y.Count.CompareTo(x.Count)); // descending

            RunPackageMerge(codeCount);

            Array.Sort(_symbols, (x, y) => x.CodeLength.CompareTo(y.CodeLength));

            JpegCanonicalCode[] codes = BuildCanonicalCode(codeCount);

            return new JpegHuffmanEncodingTable(codes);
        }

        private int CalculateCodeCount()
        {
            int count = 0;
            foreach (Symbol item in _symbols)
            {
                if (item.Count > 0)
                {
                    count++;
                }
            }
            return count;
        }

        private void RunPackageMerge(int codeCount)
        {
            // Initialize
            var levels = new List<Node>[16];
            for (int l = levels.Length - 1, nodeCount = codeCount; l >= 0; l--, nodeCount += nodeCount / 2)
            {
                var nodes = new List<Node>(nodeCount);
                for (int i = 0; i < codeCount; i++)
                {
                    var node = new Node();
                    node.Set(_symbols[i]);
                    nodes.Add(node);
                }
                levels[l] = nodes;
            }

            // Run package merge
            for (int l = levels.Length - 1; l > 0; l--)
            {
                List<Node> nodes = levels[l];
                List<Node> nextLevelNodes = levels[l - 1];
                nodes.Sort((x, y) => y.Count.CompareTo(x.Count)); // descending
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

            Array.Sort(_symbols, (x, y) => x.Value.CompareTo(y.Value));
            List<Node> level0 = levels[0];
            level0.Sort((x, y) => x.Count.CompareTo(y.Count)); // ascending
            int selectCount = Math.Max(1, 2 * (codeCount - 1));
            for (int i = 0; i < selectCount; i++)
            {
                TraverseNode(level0[i], _symbols);
            }

            static void TraverseNode(Node? node, Symbol[] symbols)
            {
                if (node is null)
                {
                    return;
                }
                else if (node.Left is null)
                {
                    symbols[node.Symbol].CodeLength++;
                }
                else
                {
                    TraverseNode(node.Left, symbols);
                    TraverseNode(node.Right, symbols);
                }
            }
        }

        private JpegCanonicalCode[] BuildCanonicalCode(int codeCount)
        {
            var codes = new JpegCanonicalCode[codeCount];
            int offset = 256 - codeCount;

            for (int i = 0; i < codes.Length; i++)
            {
                codes[i].Symbol = _symbols[offset + i].Value;
                codes[i].CodeLength = _symbols[offset + i].CodeLength;
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

        [DoesNotReturn]
        private static void ThrowInvalidOperationException()
        {
            throw new InvalidOperationException();
        }

        struct Symbol
        {
            public byte Value;
            public byte CodeLength;
            public ushort Count;

            public override string ToString()
            {
                return $"Symbol[{Value}](Count={Count}, CodeLength={CodeLength})";
            }
        }

        class Node
        {
            public byte Symbol { get; set; }
            public uint Count { get; set; }
            public Node? Left { get; set; }
            public Node? Right { get; set; }

            public void Reset()
            {
                Left = null;
                Right = null;
            }

            public void Set(Symbol symbol)
            {
                Symbol = symbol.Value;
                Count = symbol.Count;
            }

            public void Set(Node left, Node right)
            {
                Count = left.Count + right.Count;
                Left = left;
                Right = right;
            }
        }
    }
}

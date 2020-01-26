﻿#nullable enable

using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JpegLibrary
{
    internal sealed class JpegBlockAllocator : IDisposable
    {
        private readonly JpegBlockOutputWriter _writer;
        private byte[]? _buffer;
        private ComponentAllocation[]? _components;

        internal JpegBlockAllocator(JpegBlockOutputWriter writer)
        {
            _writer = writer;
            _buffer = null;
            _components = null;
        }

        public void Allocate(JpegFrameHeader frameHeader)
        {
            // Compute maximum sampling factor
            int maxHorizontalSampling = 1;
            int maxVerticalSampling = 1;
            foreach (JpegFrameComponentSpecificationParameters currentFrameComponent in frameHeader.Components!)
            {
                maxHorizontalSampling = Math.Max(maxHorizontalSampling, currentFrameComponent.HorizontalSamplingFactor);
                maxVerticalSampling = Math.Max(maxVerticalSampling, currentFrameComponent.VerticalSamplingFactor);
            }

            int horizontalBlockCount = (frameHeader.SamplesPerLine + 7) / 8;
            int verticalBlockCount = (frameHeader.NumberOfLines + 7) / 8;

            ComponentAllocation[] componentAllocations = _components = new ComponentAllocation[frameHeader.NumberOfComponents];
            int index = 0;
            foreach (JpegFrameComponentSpecificationParameters component in frameHeader.Components)
            {
                int horizontalSubsamplingFactor = maxHorizontalSampling / component.HorizontalSamplingFactor;
                int verticalSubsamplingFactor = maxVerticalSampling / component.VerticalSamplingFactor;

                int horizontalComponentBlockCount = (horizontalBlockCount + horizontalSubsamplingFactor - 1) / horizontalSubsamplingFactor;
                int verticalComponentBlockCount = (verticalBlockCount + verticalSubsamplingFactor - 1) / verticalSubsamplingFactor;

                componentAllocations[index++] = new ComponentAllocation
                {
                    HorizontalComponentBlock = horizontalComponentBlockCount,
                    VerticalComponentBlock = verticalComponentBlockCount,
                    HorizontalSubsamplingFactor = horizontalSubsamplingFactor,
                    VerticalSubsamplingFactor = verticalSubsamplingFactor,
                };
            }

            // allocate an additional block to act as a dummy buffer.
            index = 1;
            for (int i = 0; i < componentAllocations.Length; i++)
            {
                componentAllocations[i].ComponentBlockOffset = index;
                index += componentAllocations[i].HorizontalComponentBlock * componentAllocations[i].VerticalComponentBlock;
            }

            int length = index * Unsafe.SizeOf<JpegBlock8x8>();
            byte[] buffer = _buffer = ArrayPool<byte>.Shared.Rent(length);
            buffer.AsSpan(0, length).Fill(0);
        }

        public ref JpegBlock8x8 GetBlockReference(int componentIndex, int x, int y)
        {
            ComponentAllocation[]? components = _components;
            if (components is null)
            {
                throw new InvalidOperationException();
            }
            Debug.Assert(_buffer != null);
            if ((uint)componentIndex >= (uint)components.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(componentIndex));
            }
            ComponentAllocation component = components[componentIndex];

            int horizontalSubsamplingFactor = component.HorizontalSubsamplingFactor;
            int verticalSubsamplingFactor = component.VerticalSubsamplingFactor;

            Debug.Assert((x % (8 * horizontalSubsamplingFactor)) == 0);
            Debug.Assert((y % (8 * verticalSubsamplingFactor)) == 0);

            int blockX = x / (8 * horizontalSubsamplingFactor);
            int blockY = y / (8 * verticalSubsamplingFactor);

            ref JpegBlock8x8 blockRef = ref Unsafe.As<byte, JpegBlock8x8>(ref MemoryMarshal.GetReference(_buffer.AsSpan()));
            if (blockX >= component.HorizontalComponentBlock || blockY >= component.VerticalComponentBlock)
            {
                return ref blockRef;
            }

            return ref Unsafe.Add(ref blockRef, component.ComponentBlockOffset + blockY * component.HorizontalComponentBlock + blockX);
        }

        public void Flush()
        {
            ComponentAllocation[]? components = _components;
            if (!(components is null))
            {
                for (int i = 0; i < components.Length; i++)
                {
                    ComponentAllocation component = components[i];
                    ref JpegBlock8x8 componentBlockRef = ref Unsafe.Add(ref Unsafe.As<byte, JpegBlock8x8>(ref MemoryMarshal.GetReference(_buffer.AsSpan())), component.ComponentBlockOffset);

                    for (int row = 0; row < component.VerticalComponentBlock; row++)
                    {
                        ref JpegBlock8x8 rowRef = ref Unsafe.Add(ref componentBlockRef, row * component.HorizontalComponentBlock);
                        for (int col = 0; col < component.HorizontalComponentBlock; col++)
                        {
                            ref JpegBlock8x8 blockRef = ref Unsafe.Add(ref rowRef, col);
                            WriteBlock(blockRef, i, col * component.HorizontalSubsamplingFactor * 8, row * component.VerticalSubsamplingFactor * 8, component.HorizontalSubsamplingFactor, component.VerticalSubsamplingFactor);
                        }
                    }
                }
            }
        }

        private void WriteBlock(in JpegBlock8x8 block, int componentIndex, int x, int y, int horizontalSamplingFactor, int verticalSamplingFactor)
        {
            JpegBlockOutputWriter? outputWriter = _writer;
            Debug.Assert(!(outputWriter is null));

            if (horizontalSamplingFactor == 1 && verticalSamplingFactor == 1)
            {
                outputWriter!.WriteBlock(block, componentIndex, x, y);
            }
            else
            {
                JpegBlock8x8 tempBlock = default;

                int hShift = CalculateShiftFactor(horizontalSamplingFactor);
                int vShift = CalculateShiftFactor(verticalSamplingFactor);

                ref short blockRef = ref Unsafe.As<JpegBlock8x8, short>(ref Unsafe.AsRef(block));
                ref short tempRef = ref Unsafe.As<JpegBlock8x8, short>(ref Unsafe.AsRef(tempBlock));

                for (int v = 0; v < verticalSamplingFactor; v++)
                {
                    for (int h = 0; h < horizontalSamplingFactor; h++)
                    {
                        int vBlock = 8 * v;
                        int hBlock = 8 * h;
                        // Fill tempBlock
                        for (int i = 0; i < 8; i++)
                        {
                            ref short tempRowRef = ref Unsafe.Add(ref tempRef, 8 * i);
                            ref short blockRowRef = ref Unsafe.Add(ref blockRef, ((vBlock + i) >> vShift) * 8);
                            for (int j = 0; j < 8; j++)
                            {
                                Unsafe.Add(ref tempRowRef, j) = Unsafe.Add(ref blockRowRef, (hBlock + j) >> hShift);
                            }
                        }

                        // Write tempBlock to output
                        outputWriter!.WriteBlock(tempBlock, componentIndex, x + 8 * h, y + 8 * v);
                    }
                }
            }
        }

        private static int CalculateShiftFactor(int value)
        {
            int shift = 0;
            while ((value = value / 2) != 0) shift++;
            return shift;
        }

        public void Dispose()
        {
            if (!(_buffer is null))
            {
                ArrayPool<byte>.Shared.Return(_buffer);
            }
        }

        struct ComponentAllocation
        {
            public int HorizontalSubsamplingFactor;
            public int VerticalSubsamplingFactor;
            public int HorizontalComponentBlock;
            public int VerticalComponentBlock;
            public int ComponentBlockOffset;
        }
    }
}
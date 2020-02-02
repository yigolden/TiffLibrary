using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JpegLibrary
{
    internal class JpegScanlineAllocator : IDisposable
    {
        private readonly JpegBlockOutputWriter _writer;
        private byte[]? _buffer;
        private ComponentAllocation[]? _components;

        internal JpegScanlineAllocator(JpegBlockOutputWriter writer)
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

            ComponentAllocation[] componentAllocations = _components = new ComponentAllocation[frameHeader.NumberOfComponents];
            int index = 0;
            foreach (JpegFrameComponentSpecificationParameters component in frameHeader.Components)
            {
                int horizontalSubsamplingFactor = maxHorizontalSampling / component.HorizontalSamplingFactor;
                int verticalSubsamplingFactor = maxVerticalSampling / component.VerticalSamplingFactor;

                int width = (frameHeader.SamplesPerLine + horizontalSubsamplingFactor - 1) / horizontalSubsamplingFactor;
                int height = (frameHeader.NumberOfLines + verticalSubsamplingFactor - 1) / verticalSubsamplingFactor;

                componentAllocations[index++] = new ComponentAllocation
                {
                    Width = width,
                    Height = height,
                    HorizontalSubsamplingFactor = horizontalSubsamplingFactor,
                    VerticalSubsamplingFactor = verticalSubsamplingFactor,
                };
            }

            index = 0;
            for (int i = 0; i < componentAllocations.Length; i++)
            {
                componentAllocations[i].ComponentSampleOffset = index;
                index += componentAllocations[i].Width * componentAllocations[i].Height;
            }

            int length = index * Unsafe.SizeOf<short>();
            byte[] buffer = _buffer = ArrayPool<byte>.Shared.Rent(length);
            buffer.AsSpan(0, length).Clear();
        }

        public Span<short> GetScanlineSpan(int componentIndex, int y)
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

            if ((uint)y >= (uint)component.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }

            Span<short> bufferSpan = MemoryMarshal.Cast<byte, short>(_buffer.AsSpan());
            return bufferSpan.Slice(component.ComponentSampleOffset).Slice(component.Width * y, component.Width);
        }

        public void Flush()
        {
            ComponentAllocation[]? components = _components;

            if (components is null)
            {
                return;
            }

            JpegBlock8x8 block = default;
            JpegBlockOutputWriter? outputWriter = _writer;
            Debug.Assert(!(outputWriter is null));

            for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                ComponentAllocation component = components[componentIndex];
                int width = component.Width;
                int height = component.Height;
                ref short componentSampleRef = ref Unsafe.Add(ref Unsafe.As<byte, short>(ref MemoryMarshal.GetReference(_buffer.AsSpan())), component.ComponentSampleOffset);

                for (int y = 0; y < height; y += 8)
                {
                    int writeHeight = Math.Min(height - y, 8);
                    ref short componentRowRef = ref Unsafe.Add(ref componentSampleRef, width * y);
                    for (int x = 0; x < width; x += 8)
                    {
                        int writeWidth = Math.Min(width - x, 8);

                        ref short componentRowColRef = ref Unsafe.Add(ref componentRowRef, x);
                        ref short blockRef = ref Unsafe.As<JpegBlock8x8, short>(ref block);

                        if (writeHeight == 8 && writeWidth == 8)
                        {
                            Unsafe.As<short, long>(ref blockRef) = Unsafe.As<short, long>(ref componentRowColRef);
                            Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 1) = Unsafe.Add(ref Unsafe.As<short, long>(ref componentRowColRef), 1);
                            componentRowColRef = ref Unsafe.Add(ref componentRowColRef, width);
                            Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 2) = Unsafe.As<short, long>(ref componentRowColRef);
                            Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 3) = Unsafe.Add(ref Unsafe.As<short, long>(ref componentRowColRef), 1);
                            componentRowColRef = ref Unsafe.Add(ref componentRowColRef, width);
                            Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 4) = Unsafe.As<short, long>(ref componentRowColRef);
                            Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 5) = Unsafe.Add(ref Unsafe.As<short, long>(ref componentRowColRef), 1);
                            componentRowColRef = ref Unsafe.Add(ref componentRowColRef, width);
                            Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 6) = Unsafe.As<short, long>(ref componentRowColRef);
                            Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 7) = Unsafe.Add(ref Unsafe.As<short, long>(ref componentRowColRef), 1);
                            componentRowColRef = ref Unsafe.Add(ref componentRowColRef, width);
                            Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 8) = Unsafe.As<short, long>(ref componentRowColRef);
                            Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 9) = Unsafe.Add(ref Unsafe.As<short, long>(ref componentRowColRef), 1);
                            componentRowColRef = ref Unsafe.Add(ref componentRowColRef, width);
                            Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 10) = Unsafe.As<short, long>(ref componentRowColRef);
                            Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 11) = Unsafe.Add(ref Unsafe.As<short, long>(ref componentRowColRef), 1);
                            componentRowColRef = ref Unsafe.Add(ref componentRowColRef, width);
                            Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 12) = Unsafe.As<short, long>(ref componentRowColRef);
                            Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 13) = Unsafe.Add(ref Unsafe.As<short, long>(ref componentRowColRef), 1);
                            componentRowColRef = ref Unsafe.Add(ref componentRowColRef, width);
                            Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 14) = Unsafe.As<short, long>(ref componentRowColRef);
                            Unsafe.Add(ref Unsafe.As<short, long>(ref blockRef), 15) = Unsafe.Add(ref Unsafe.As<short, long>(ref componentRowColRef), 1);
                        }
                        else
                        {
                            for (int i = 0; i < writeHeight; i++)
                            {
                                for (int j = 0; j < writeWidth; j++)
                                {
                                    Unsafe.Add(ref blockRef, j) = Unsafe.Add(ref componentRowColRef, i * width + j);
                                }
                                blockRef = ref Unsafe.Add(ref blockRef, 8);
                            }
                        }

                        WriteBlock(outputWriter!, block, componentIndex, component.HorizontalSubsamplingFactor * x, component.VerticalSubsamplingFactor * y, component.HorizontalSubsamplingFactor, component.VerticalSubsamplingFactor);
                    }
                }
            }
        }

        private static void WriteBlock(JpegBlockOutputWriter outputWriter, in JpegBlock8x8 block, int componentIndex, int x, int y, int horizontalSamplingFactor, int verticalSamplingFactor)
        {
            ref short blockRef = ref Unsafe.As<JpegBlock8x8, short>(ref Unsafe.AsRef(block));

            if (horizontalSamplingFactor == 1 && verticalSamplingFactor == 1)
            {
                outputWriter!.WriteBlock(ref blockRef, componentIndex, x, y);
            }
            else
            {
                JpegBlock8x8 tempBlock = default;

                int hShift = JpegMathHelper.Log2((uint)horizontalSamplingFactor);
                int vShift = JpegMathHelper.Log2((uint)verticalSamplingFactor);

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
                        outputWriter!.WriteBlock(ref tempRef, componentIndex, x + 8 * h, y + 8 * v);
                    }
                }
            }
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
            public int Width;
            public int Height;
            public int ComponentSampleOffset;
        }
    }
}

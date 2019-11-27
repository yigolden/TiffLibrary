using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TiffLibrary.ImageDecoder
{
    internal sealed class Jpeg16BitEndianessCorrectionMiddleware : ITiffImageDecoderMiddleware
    {
        public ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            if (context.OperationContext.IsLittleEndian != BitConverter.IsLittleEndian)
            {
                Span<ushort> uncompressedData = MemoryMarshal.Cast<byte, ushort>(context.UncompressedData.Span);

                for (int i = 0; i < uncompressedData.Length; i++)
                {
                    uncompressedData[i] = BinaryPrimitives.ReverseEndianness(uncompressedData[i]);
                }
            }

            return next.RunAsync(context);
        }
    }
}

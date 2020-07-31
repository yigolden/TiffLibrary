using System;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.ImageDecoder
{
    internal sealed class TiffEmptyStrileWriter
    {
        private readonly TiffRgba32 _fillColor;

        public TiffEmptyStrileWriter(TiffRgba32 fillColor)
        {
            _fillColor = fillColor;
        }

        public void Write(TiffImageDecoderContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            TiffRgba32 fillColor = _fillColor;
            using TiffPixelBufferWriter<TiffRgba32> writer = context.GetWriter<TiffRgba32>();

            int rows = context.ReadSize.Height;
            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffRgba32> pixelSpanHandle = writer.GetRowSpan(row);
                pixelSpanHandle.GetSpan().Fill(fillColor);
            }
        }
    }
}

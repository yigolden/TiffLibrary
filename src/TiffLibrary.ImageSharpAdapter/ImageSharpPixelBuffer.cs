using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace TiffLibrary.ImageSharpAdapter
{
    internal sealed class ImageSharpPixelBuffer<TImageSharpPixel, TTiffPixel> : ITiffPixelBuffer<TTiffPixel> where TImageSharpPixel : struct, IPixel<TImageSharpPixel> where TTiffPixel : unmanaged
    {
        private readonly Image<TImageSharpPixel> _image;

        public ImageSharpPixelBuffer(Image<TImageSharpPixel> image)
        {
            Debug.Assert(Unsafe.SizeOf<TImageSharpPixel>() == Unsafe.SizeOf<TTiffPixel>());
            _image = image;
        }

        public int Width => _image.Width;

        public int Height => _image.Height;

        public Span<TTiffPixel> GetRowSpan(int rowIndex)
        {
            return MemoryMarshal.Cast<TImageSharpPixel, TTiffPixel>(_image.GetPixelRowSpan(rowIndex));
        }

        public Span<TTiffPixel> GetSpan()
        {
            return MemoryMarshal.Cast<TImageSharpPixel, TTiffPixel>(_image.GetPixelSpan());
        }
    }
}

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.PixelFormats;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.ImageSharpAdapter
{
    internal sealed class ImageSharpConversionPixelBufferWriter<TImageSharpPixel, TIntermediate, TTiffPixel> : TiffPixelConverter<TImageSharpPixel, TTiffPixel>
        where TImageSharpPixel : unmanaged, IPixel<TImageSharpPixel>
        where TIntermediate : unmanaged, IPixel<TIntermediate>
        where TTiffPixel : unmanaged
    {
        public ImageSharpConversionPixelBufferWriter(ITiffPixelBufferWriter<TTiffPixel> writer) : base(writer)
        {
            Debug.Assert(Unsafe.SizeOf<TIntermediate>() == Unsafe.SizeOf<TTiffPixel>());
        }

        public override void Convert(ReadOnlySpan<TImageSharpPixel> source, Span<TTiffPixel> destination)
        {
            TIntermediate intermediate = default;
            for (int i = 0; i < source.Length; i++)
            {
                intermediate.FromScaledVector4(source[i].ToScaledVector4());
                destination[i] = Unsafe.As<TIntermediate, TTiffPixel>(ref intermediate);
            }
        }
    }

    internal sealed class ImageSharpConversionPixelBufferWriter2<TTiffPixel, TIntermediate, TImageSharpPixel> : TiffPixelConverter<TTiffPixel, TImageSharpPixel>
        where TTiffPixel : unmanaged
        where TIntermediate : unmanaged, IPixel<TIntermediate>
        where TImageSharpPixel : unmanaged, IPixel<TImageSharpPixel>
    {
        public ImageSharpConversionPixelBufferWriter2(ITiffPixelBufferWriter<TImageSharpPixel> writer) : base(writer)
        {
            Debug.Assert(Unsafe.SizeOf<TIntermediate>() == Unsafe.SizeOf<TTiffPixel>());
        }

        public override void Convert(ReadOnlySpan<TTiffPixel> source, Span<TImageSharpPixel> destination)
        {
            ref TTiffPixel sourceRef = ref MemoryMarshal.GetReference(source);
            for (int i = 0; i < source.Length; i++)
            {
                destination[i].FromScaledVector4(Unsafe.As<TTiffPixel, TIntermediate>(ref Unsafe.Add(ref sourceRef, i)).ToScaledVector4());
            }
        }
    }
}

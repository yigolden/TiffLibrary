using System.IO;
using TiffLibrary.PixelBuffer;

namespace TiffLibrary.ImageDecoder
{
    internal sealed class TiffOrientatedImageDecoderContext : TiffDelegatingImageDecoderContext
    {
        private readonly bool _isFlipOrOrientation; // false = flip, true = orientation
        private readonly bool _flipLeftRigt;
        private readonly bool _flipTopBottom;

        public override TiffSize ReadSize
        {
            get
            {
                TiffSize size = InnerContext.ReadSize;
                return _isFlipOrOrientation ? new TiffSize(size.Height, size.Width) : size;
            }
            set
            {
                InnerContext.ReadSize = _isFlipOrOrientation ? new TiffSize(value.Height, value.Width) : value;
            }
        }

        public TiffOrientatedImageDecoderContext(TiffImageDecoderContext innerContext, TiffOrientation orientation) : base(innerContext)
        {
            if (orientation == 0)
            {
                _isFlipOrOrientation = false;
                _flipLeftRigt = false;
                _flipTopBottom = false;
                return;
            }
            switch (orientation)
            {
                case TiffOrientation.TopLeft:
                    _isFlipOrOrientation = false;
                    _flipLeftRigt = false;
                    _flipTopBottom = false;
                    break;
                case TiffOrientation.TopRight:
                    _isFlipOrOrientation = false;
                    _flipLeftRigt = true;
                    _flipTopBottom = false;
                    break;
                case TiffOrientation.BottomRight:
                    _isFlipOrOrientation = false;
                    _flipLeftRigt = true;
                    _flipTopBottom = true;
                    break;
                case TiffOrientation.BottomLeft:
                    _isFlipOrOrientation = false;
                    _flipLeftRigt = false;
                    _flipTopBottom = true;
                    break;
                case TiffOrientation.LeftTop:
                    _isFlipOrOrientation = true;
                    _flipLeftRigt = false;
                    _flipTopBottom = false;
                    break;
                case TiffOrientation.RightTop:
                    _isFlipOrOrientation = true;
                    _flipLeftRigt = true;
                    _flipTopBottom = false;
                    break;
                case TiffOrientation.RightBottom:
                    _isFlipOrOrientation = true;
                    _flipLeftRigt = true;
                    _flipTopBottom = true;
                    break;
                case TiffOrientation.LeftBottom:
                    _isFlipOrOrientation = true;
                    _flipLeftRigt = false;
                    _flipTopBottom = true;
                    break;
                default:
                    throw new InvalidDataException("Unknown orientation tag.");
            }
        }

        public override TiffPixelBufferWriter<TPixel> GetWriter<TPixel>()
        {
            TiffPixelBufferWriter<TPixel> writer = InnerContext.GetWriter<TPixel>();
            if (_isFlipOrOrientation)
            {
                return new TiffOrientedPixelBufferWriter<TPixel>(writer, _flipLeftRigt, _flipTopBottom).AsPixelBufferWriter();
            }
            return new TiffFlippedPixelBufferWriter<TPixel>(writer, _flipLeftRigt, _flipTopBottom).AsPixelBufferWriter();
        }
    }
}

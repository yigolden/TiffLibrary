using System.IO;
using TiffLibrary.PixelBuffer;

namespace TiffLibrary.ImageEncoder
{
    internal sealed class TiffOrientatedImageEncoderContext<TPixel> : TiffDelegatingImageEncoderContext<TPixel> where TPixel : unmanaged
    {
        private readonly bool _isFlipOrOrientation; // false = flip, true = orientation
        private readonly bool _flipLeftRigt;
        private readonly bool _flipTopBottom;

        public override TiffSize ImageSize
        {
            get
            {
                TiffSize size = InnerContext.ImageSize;
                return _isFlipOrOrientation ? new TiffSize(size.Height, size.Width) : size;
            }
            set
            {
                InnerContext.ImageSize = _isFlipOrOrientation ? new TiffSize(value.Height, value.Width) : value;
            }
        }

        public TiffOrientatedImageEncoderContext(TiffImageEncoderContext<TPixel> innerContext, TiffOrientation orientation) : base(innerContext)
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

        public override TiffPixelBufferReader<TPixel> GetReader()
        {
            TiffPixelBufferReader<TPixel> reader = InnerContext.GetReader();
            if (_isFlipOrOrientation)
            {
                return new TiffOrientedPixelBufferReader<TPixel>(reader, _flipLeftRigt, _flipTopBottom).AsPixelBufferReader();
            }
            return new TiffFlippedPixelBufferReader<TPixel>(reader, _flipLeftRigt, _flipTopBottom).AsPixelBufferReader();
        }

    }
}

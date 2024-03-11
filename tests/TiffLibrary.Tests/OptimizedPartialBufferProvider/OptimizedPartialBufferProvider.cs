using System;
using System.Drawing;
using TiffLibrary.PixelFormats;
using TiffLibrary.Utils;

namespace TiffLibrary.Tests.OptimizedPartialBufferProvider
{
    public class OptimizedPartialBufferProvider : IPartialBufferProvider<TiffGray8>
    {
        private readonly TiffSize _baseLevelSize;

        private int _layer;
        private TiffSize _levelSize;

        public OptimizedPartialBufferProvider(TiffSize baseLevelSize)
        {
            _baseLevelSize = baseLevelSize;
            SetLayer(0);
        }

        public TiffSize ImageSize => _levelSize;

        public void SetLayer(int layer)
        {
            _layer = layer;

            int inverseScale = (int) Math.Pow(2, layer);

            int thisLevelWidth = (int) Math.Ceiling(_baseLevelSize.Width / (double) inverseScale);
            int thisLevelHeight = (int) Math.Ceiling(_baseLevelSize.Height / (double) inverseScale);

            _levelSize = new TiffSize(thisLevelWidth, thisLevelHeight);
        }

        /// <summary>
        /// The return the memory buffer. Given the region on the image. If it is multi-resolution tiff. Please optimize extend the class to manage different levels.
        /// </summary>
        public TiffMemoryPixelBuffer<TiffGray8> GetMemoryBuffer(Rectangle imageRegion)
        {
            var tmp = new TiffGray8[imageRegion.Width * imageRegion.Height];
            return new TiffMemoryPixelBuffer<TiffGray8>(tmp, imageRegion.Width, imageRegion.Height, false);
        }

    }
}

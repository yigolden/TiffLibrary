using JpegLibrary;

namespace TiffLibrary.Compression
{
    internal class LegacyJpegIdentificationDecoder : JpegDecoder
    {
        private bool _isStartOfImage;
        private bool _app0Encountered;

        public bool IsJfifFile => _app0Encountered;

        protected override bool ProcessMarkerForIdentification(JpegMarker marker, ref JpegReader reader, bool loadQuantizationTables)
        {
            // The APP0 marker is used to identify a JPEG FIF file. The JPEG FIF APP0 marker is mandatory right after the SOI marker.
            _app0Encountered = _app0Encountered || (marker == JpegMarker.App0 && _isStartOfImage);
            _isStartOfImage = marker == JpegMarker.StartOfImage;

            return base.ProcessMarkerForIdentification(marker, ref reader, loadQuantizationTables);
        }
    }
}

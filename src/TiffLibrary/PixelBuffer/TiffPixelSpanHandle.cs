using System;
using System.Diagnostics.CodeAnalysis;

namespace TiffLibrary.PixelBuffer
{
    /// <summary>
    /// Represents a temporary consecutive memory buffer of a span of pixels.  
    /// </summary>
    /// <typeparam name="TPixel">The type of pixel in the span.</typeparam>
    [SuppressMessage("Design", "CA1063: Implement IDisposable correctly", Justification = "The semantics of Dispose method is different.")]
    public abstract class TiffPixelSpanHandle<TPixel> : IDisposable where TPixel : unmanaged
    {
        /// <summary>
        /// The number of pixels that this buffer contains.
        /// </summary>
        public virtual int Length => GetSpan().Length;

        /// <summary>
        /// Gets a <see cref="Span{TPixel}"/> representing the consecutive memory buffer of a span of pixels
        /// </summary>
        /// <returns></returns>
        public abstract Span<TPixel> GetSpan();

        /// <summary>
        /// Flush the pixels into underlying storage, and release all the resources related to this object. The consumer of this object should no longer use it after Dispose is called.
        /// </summary>
        public abstract void Dispose();
    }
}

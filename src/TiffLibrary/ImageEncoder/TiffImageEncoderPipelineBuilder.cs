using System;
using System.Collections.Generic;

namespace TiffLibrary.ImageEncoder
{
    /// <summary>
    /// A builder that builds the encoder pipeline by connecting all the middlewares added to this builder.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type.</typeparam>
    [CLSCompliant(false)]
    public sealed class TiffImageEncoderPipelineBuilder<TPixel> where TPixel : unmanaged
    {
        private List<ITiffImageEncoderMiddleware<TPixel>> _middlewares;

        /// <summary>
        /// Initialize the builder.
        /// </summary>
        public TiffImageEncoderPipelineBuilder()
        {
            _middlewares = new List<ITiffImageEncoderMiddleware<TPixel>>();
        }

        /// <summary>
        /// Adds a middleware to the end of the pipeline.
        /// </summary>
        /// <param name="middleware">The middleware to add.</param>
        public void Add(ITiffImageEncoderMiddleware<TPixel> middleware)
        {
            if (middleware is null)
            {
                throw new ArgumentNullException(nameof(middleware));
            }

            _middlewares.Add(middleware);
        }

        /// <summary>
        /// Adds a middleware to the front of the pipeline.
        /// </summary>
        /// <param name="middleware">The middleware to add.</param>
        public void InsertFirst(ITiffImageEncoderMiddleware<TPixel> middleware)
        {
            if (middleware is null)
            {
                throw new ArgumentNullException(nameof(middleware));
            }

            _middlewares.Insert(0, middleware);
        }

        /// <summary>
        /// Builds a linked list of middlewares from the middlewares added to the builder.
        /// </summary>
        /// <returns>The beginning of the middleware list.</returns>
        public ITiffImageEncoderPipelineNode<TPixel> Build()
        {
            List<ITiffImageEncoderMiddleware<TPixel>> middleware = _middlewares;
            int count = middleware.Count;
            if (count == 0)
            {
                throw new InvalidOperationException("No middleware is set up.");
            }

            var nodes = new TiffImageEncoderPipelineNode<TPixel>[count];
            for (int i = 0; i < count; i++)
            {
                nodes[i] = new TiffImageEncoderPipelineNode<TPixel>(middleware[i]);
            }

            for (int i = 0; i < count - 1; i++)
            {
                nodes[i].Next = nodes[i + 1];
            }

            return nodes[0];
        }
    }
}

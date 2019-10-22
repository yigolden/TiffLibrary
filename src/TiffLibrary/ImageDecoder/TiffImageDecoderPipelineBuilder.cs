using System;
using System.Collections.Generic;

namespace TiffLibrary.ImageDecoder
{
    /// <summary>
    /// A builder that builds the decoder pipeline by connecting all the middlewares added to this builder.
    /// </summary>
    public sealed class TiffImageDecoderPipelineBuilder
    {
        private List<ITiffImageDecoderMiddleware> _middlewares;

        /// <summary>
        /// Initialize the builder.
        /// </summary>
        public TiffImageDecoderPipelineBuilder()
        {
            _middlewares = new List<ITiffImageDecoderMiddleware>();
        }

        /// <summary>
        /// Adds a middleware to the end of the pipeline.
        /// </summary>
        /// <param name="middleware">The middleware to add.</param>
        public void Add(ITiffImageDecoderMiddleware middleware)
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
        public void InsertFirst(ITiffImageDecoderMiddleware middleware)
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
        public ITiffImageDecoderPipelineNode Build()
        {
            List<ITiffImageDecoderMiddleware> middleware = _middlewares;
            int count = middleware.Count;
            if (count == 0)
            {
                throw new InvalidOperationException("No middleware is set up.");
            }

            var nodes = new TiffImageDecoderPipelineNode[count];
            for (int i = 0; i < count; i++)
            {
                nodes[i] = new TiffImageDecoderPipelineNode(middleware[i]);
            }

            for (int i = 0; i < count - 1; i++)
            {
                nodes[i].Next = nodes[i + 1];
            }

            return nodes[0];
        }
    }
}

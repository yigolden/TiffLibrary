﻿using System;
using System.Threading.Tasks;

namespace TiffLibrary.ImageDecoder
{
    internal class TiffParallelMutexMiddleware : ITiffImageDecoderMiddleware
    {
        public async ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            IDisposable? lockObj = null;
            var mutexService = context.GetService(typeof(ITiffParallelMutexService)) as ITiffParallelMutexService;
            if (mutexService is not null)
            {
                lockObj = await mutexService.LockAsync(context.CancellationToken).ConfigureAwait(false);
            }

            try
            {
                await next.RunAsync(context).ConfigureAwait(false);
            }
            finally
            {
                if (lockObj is not null)
                {
                    lockObj.Dispose();
                }
            }
        }


    }
}

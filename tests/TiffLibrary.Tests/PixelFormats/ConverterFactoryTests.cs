using System;
using System.Collections.Generic;
using System.Reflection;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelConverter;
using TiffLibrary.PixelFormats;
using Xunit;

namespace TiffLibrary.Tests.PixelFormats
{
    public class ConverterFactoryTests
    {
        private static Type[] s_allPixelFormats = new Type[]
                    {
                typeof(TiffBgr24), typeof(TiffBgra32), typeof(TiffBgra64), typeof(TiffRgba64),
                typeof(TiffCmyk32),typeof(TiffCmyk64), typeof(TiffGray16), typeof(TiffGray8),
                typeof(TiffMask),typeof(TiffRgb24), typeof(TiffRgba32)
                    };

        private static Type[] s_supportedOutputFormats = new Type[]
            {
                typeof(TiffGray8), typeof(TiffGray16),
                typeof(TiffBgr24), typeof(TiffBgra32),
                typeof(TiffBgra64), typeof(TiffRgba64),
                typeof(TiffRgb24), typeof(TiffRgba32),
            };

        public static IEnumerable<object[]> GetConvertiblePixelFormatPairs()
        {
            foreach (Type pixelFormat in s_allPixelFormats)
            {
                foreach (Type supportedPixelFormat in s_supportedOutputFormats)
                {
                    yield return new object[] { pixelFormat, supportedPixelFormat };
                }
            }
        }

        [Fact]
        public void TestPixelFormatsConvertible()
        {
            ITiffPixelConverterFactory factory = TiffDefaultPixelConverterFactory.Instance;
            foreach (object[] items in GetConvertiblePixelFormatPairs())
            {
                AssertConvertible(factory, (Type)items[0], (Type)items[1]);
            }
        }

        private static void AssertConvertible(ITiffPixelConverterFactory factory, Type source, Type destination)
        {
            MethodInfo isConvertibleMethod = typeof(ITiffPixelConverterFactory).GetMethod("IsConvertible").MakeGenericMethod(source, destination);
            object isConvertibleResult = isConvertibleMethod.Invoke(factory, null);
            Assert.IsType<bool>(isConvertibleResult);
            Assert.True((bool)isConvertibleResult);

            ConstructorInfo emptyPixelBufferConstructor = typeof(EmptyPixelBuffer<>).MakeGenericType(destination).GetConstructor(Array.Empty<Type>());
            object pixelBuffer = emptyPixelBufferConstructor.Invoke(null);

            MethodInfo createMethod = typeof(ITiffPixelConverterFactory).GetMethod("CreateConverter").MakeGenericMethod(source, destination);
            object createResult = createMethod.Invoke(factory, new object[] { pixelBuffer });
            Assert.NotNull(createResult);
        }


        internal class EmptyPixelBuffer<TPixel> : ITiffPixelBufferWriter<TPixel> where TPixel : unmanaged
        {
            public int Width => 0;

            public int Height => 0;

            public void Dispose()
            {
                // Noop
            }

            public TiffPixelSpanHandle<TPixel> GetRowSpan(int rowIndex, int start, int length)
            {
                return default;
            }

            public TiffPixelSpanHandle<TPixel> GetColumnSpan(int colIndex, int start, int length)
            {
                return default;
            }
        }
    }
}

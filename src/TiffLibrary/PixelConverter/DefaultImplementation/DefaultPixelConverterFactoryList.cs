using System;
using System.Collections.Generic;
using TiffLibrary.PixelFormats;
using TiffLibrary.PixelFormats.Converters;

namespace TiffLibrary.PixelConverter
{
    internal class DefaultPixelConverterFactoryList
    {
        private List<ConverterFactoryDescriptor> _factories;

        private DefaultPixelConverterFactoryList()
        {
            _factories = new List<ConverterFactoryDescriptor>();
            SetupDefaultConverters();
        }

        internal static List<ConverterFactoryDescriptor> Factories => Instance._factories;

        internal static DefaultPixelConverterFactoryList Instance { get; } = new DefaultPixelConverterFactoryList();

        private void SetupDefaultConverters()
        {
            Register<TiffGray8, TiffMask>(TiffGray8ToMaskPixelConverter.FactoryInstance);
            Register<TiffGray8, TiffGray16>(TiffGray8ToGray16PixelConverter.FactoryInstance);
            Register<TiffGray8, TiffBgra32>(TiffGray8ToBgra32PixelConverter.FactoryInstance);
            Register<TiffGray8, TiffBgra64>(TiffGray8ToBgra64PixelConverter.FactoryInstance);
            Register<TiffGray8, TiffRgba64>(TiffGray8ToRgba64PixelConverter.FactoryInstance);
            Register<TiffGray8, TiffRgba32>(TiffGray8ToRgba32PixelConverter.FactoryInstance);
            Register<TiffGray8, TiffCmyk32>(TiffGray8ToCmyk32PixelConverter.FactoryInstance);
            Register<TiffGray8, TiffCmyk64>(TiffGray8ToCmyk64PixelConverter.FactoryInstance);
            Register<TiffGray8, TiffBgr24>(TiffGray8ToBgr24PixelConverter.FactoryInstance);
            Register<TiffGray8, TiffRgb24>(TiffGray8ToRgb24PixelConverter.FactoryInstance);

            Register<TiffMask, TiffGray8>(TiffMaskToGray8PixelConverter.FactoryInstance);
            Register<TiffMask, TiffGray16>(TiffCombinedPixelConverterFactory.Create<TiffMask, TiffGray8, TiffGray16>(TiffMaskToGray8PixelConverter.SpanConverter, TiffGray8ToGray16PixelConverter.SpanConverter));
            Register<TiffMask, TiffBgra32>(TiffCombinedPixelConverterFactory.Create<TiffMask, TiffGray8, TiffBgra32>(TiffMaskToGray8PixelConverter.SpanConverter, TiffGray8ToBgra32PixelConverter.SpanConverter));
            Register<TiffMask, TiffBgra64>(TiffCombinedPixelConverterFactory.Create<TiffMask, TiffGray8, TiffBgra64>(TiffMaskToGray8PixelConverter.SpanConverter, TiffGray8ToBgra64PixelConverter.SpanConverter));
            Register<TiffMask, TiffRgba64>(TiffCombinedPixelConverterFactory.Create<TiffMask, TiffGray8, TiffRgba64>(TiffMaskToGray8PixelConverter.SpanConverter, TiffGray8ToRgba64PixelConverter.SpanConverter));
            Register<TiffMask, TiffRgba32>(TiffCombinedPixelConverterFactory.Create<TiffMask, TiffGray8, TiffRgba32>(TiffMaskToGray8PixelConverter.SpanConverter, TiffGray8ToRgba32PixelConverter.SpanConverter));
            Register<TiffMask, TiffCmyk32>(TiffCombinedPixelConverterFactory.Create<TiffMask, TiffGray8, TiffCmyk32>(TiffMaskToGray8PixelConverter.SpanConverter, TiffGray8ToCmyk32PixelConverter.SpanConverter));
            Register<TiffMask, TiffCmyk64>(TiffCombinedPixelConverterFactory.Create<TiffMask, TiffGray8, TiffCmyk64>(TiffMaskToGray8PixelConverter.SpanConverter, TiffGray8ToCmyk64PixelConverter.SpanConverter));
            Register<TiffMask, TiffBgr24>(TiffCombinedPixelConverterFactory.Create<TiffMask, TiffGray8, TiffBgr24>(TiffMaskToGray8PixelConverter.SpanConverter, TiffGray8ToBgr24PixelConverter.SpanConverter));
            Register<TiffMask, TiffRgb24>(TiffCombinedPixelConverterFactory.Create<TiffMask, TiffGray8, TiffRgb24>(TiffMaskToGray8PixelConverter.SpanConverter, TiffGray8ToRgb24PixelConverter.SpanConverter));

            Register<TiffGray16, TiffMask>(TiffCombinedPixelConverterFactory.Create<TiffGray16, TiffGray8, TiffMask>(TiffGray16ToGray8PixelConverter.SpanConverter, TiffGray8ToMaskPixelConverter.SpanConverter));
            Register<TiffGray16, TiffGray8>(TiffGray16ToGray8PixelConverter.FactoryInstance);
            Register<TiffGray16, TiffBgra32>(TiffGray16ToBgra32PixelConverter.FactoryInstance);
            Register<TiffGray16, TiffBgra64>(TiffGray16ToBgra64PixelConverter.FactoryInstance);
            Register<TiffGray16, TiffRgba64>(TiffGray16ToRgba64PixelConverter.FactoryInstance);
            Register<TiffGray16, TiffRgba32>(TiffGray16ToRgba32PixelConverter.FactoryInstance);
            Register<TiffGray16, TiffCmyk32>(TiffGray16ToCmyk32PixelConverter.FactoryInstance);
            Register<TiffGray16, TiffCmyk64>(TiffGray16ToCmyk64PixelConverter.FactoryInstance);
            Register<TiffGray16, TiffBgr24>(TiffCombinedPixelConverterFactory.Create<TiffGray16, TiffGray8, TiffBgr24>(TiffGray16ToGray8PixelConverter.SpanConverter, TiffGray8ToBgr24PixelConverter.SpanConverter));
            Register<TiffGray16, TiffRgb24>(TiffCombinedPixelConverterFactory.Create<TiffGray16, TiffGray8, TiffRgb24>(TiffGray16ToGray8PixelConverter.SpanConverter, TiffGray8ToRgb24PixelConverter.SpanConverter));

            Register<TiffBgra32, TiffMask>(TiffCombinedPixelConverterFactory.Create<TiffBgra32, TiffGray8, TiffMask>(TiffBgra32ToGray8PixelConverter.SpanConverter, TiffGray8ToMaskPixelConverter.SpanConverter));
            Register<TiffBgra32, TiffGray8>(TiffBgra32ToGray8PixelConverter.FactoryInstance);
            Register<TiffBgra32, TiffGray16>(TiffBgra32ToGray16PixelConverter.FactoryInstance);
            Register<TiffBgra32, TiffBgra64>(TiffBgra32ToBgra64PixelConverter.FactoryInstance);
            Register<TiffBgra32, TiffRgba64>(TiffCombinedPixelConverterFactory.Create<TiffBgra32, TiffRgba32, TiffRgba64>(TiffBgra32ToRgba32PixelConverter.SpanConverter, TiffRgba32ToRgba64PixelConverter.SpanConverter));
            Register<TiffBgra32, TiffRgba32>(TiffBgra32ToRgba32PixelConverter.FactoryInstance);
            Register<TiffBgra32, TiffCmyk32>(TiffBgra32ToCmyk32PixelConverter.FactoryInstance);
            Register<TiffBgra32, TiffCmyk64>(TiffCombinedPixelConverterFactory.Create<TiffBgra32, TiffBgra64, TiffCmyk64>(TiffBgra32ToBgra64PixelConverter.SpanConverter, TiffBgra64ToCmyk64PixelConverter.SpanConverter));
            Register<TiffBgra32, TiffBgr24>(TiffBgra32ToBgr24PixelConverter.FactoryInstance);
            Register<TiffBgra32, TiffRgb24>(TiffBgra32ToRgb24PixelConverter.FactoryInstance);

            Register<TiffBgra64, TiffMask>(TiffCombinedPixelConverterFactory.Create<TiffBgra64, TiffGray8, TiffMask>(TiffBgra64ToGray8PixelConverter.SpanConverter, TiffGray8ToMaskPixelConverter.SpanConverter));
            Register<TiffBgra64, TiffGray8>(TiffBgra64ToGray8PixelConverter.FactoryInstance);
            Register<TiffBgra64, TiffGray16>(TiffBgra64ToGray16PixelConverter.FactoryInstance);
            Register<TiffBgra64, TiffBgra32>(TiffBgra64ToBgra32PixelConverter.FactoryInstance);
            Register<TiffBgra64, TiffRgba64>(TiffBgra64ToRgba64PixelConverter.FactoryInstance);
            Register<TiffBgra64, TiffRgba32>(TiffBgra64ToRgba32PixelConverter.FactoryInstance);
            Register<TiffBgra64, TiffCmyk32>(TiffBgra64ToCmyk32PixelConverter.FactoryInstance);
            Register<TiffBgra64, TiffCmyk64>(TiffBgra64ToCmyk64PixelConverter.FactoryInstance);
            Register<TiffBgra64, TiffBgr24>(TiffBgra64ToBgr24PixelConverter.FactoryInstance);
            Register<TiffBgra64, TiffRgb24>(TiffBgra64ToRgb24PixelConverter.FactoryInstance);

            Register<TiffRgba64, TiffMask>(TiffCombinedPixelConverterFactory.Create<TiffRgba64, TiffGray8, TiffMask>(TiffRgba64ToGray8PixelConverter.SpanConverter, TiffGray8ToMaskPixelConverter.SpanConverter));
            Register<TiffRgba64, TiffGray8>(TiffRgba64ToGray8PixelConverter.FactoryInstance);
            Register<TiffRgba64, TiffGray16>(TiffRgba64ToGray16PixelConverter.FactoryInstance);
            Register<TiffRgba64, TiffBgra32>(TiffRgba64ToBgra32PixelConverter.FactoryInstance);
            Register<TiffRgba64, TiffBgra64>(TiffRgba64ToBgra64PixelConverter.FactoryInstance);
            Register<TiffRgba64, TiffRgba32>(TiffRgba64ToRgba32PixelConverter.FactoryInstance);
            Register<TiffRgba64, TiffCmyk32>(TiffRgba64ToCmyk32PixelConverter.FactoryInstance);
            Register<TiffRgba64, TiffCmyk64>(TiffRgba64ToCmyk64PixelConverter.FactoryInstance);
            Register<TiffRgba64, TiffBgr24>(TiffRgba64ToBgr24PixelConverter.FactoryInstance);
            Register<TiffRgba64, TiffRgb24>(TiffRgba64ToRgb24PixelConverter.FactoryInstance);

            Register<TiffRgba32, TiffMask>(TiffCombinedPixelConverterFactory.Create<TiffRgba32, TiffGray8, TiffMask>(TiffRgba32ToGray8PixelConverter.SpanConverter, TiffGray8ToMaskPixelConverter.SpanConverter));
            Register<TiffRgba32, TiffGray8>(TiffRgba32ToGray8PixelConverter.FactoryInstance);
            Register<TiffRgba32, TiffGray16>(TiffRgba32ToGray16PixelConverter.FactoryInstance);
            Register<TiffRgba32, TiffBgra32>(TiffRgba32ToBgra32PixelConverter.FactoryInstance);
            Register<TiffRgba32, TiffBgra64>(TiffCombinedPixelConverterFactory.Create<TiffRgba32, TiffBgra32, TiffBgra64>(TiffRgba32ToBgra32PixelConverter.SpanConverter, TiffBgra32ToBgra64PixelConverter.SpanConverter));
            Register<TiffRgba32, TiffRgba64>(TiffRgba32ToRgba64PixelConverter.FactoryInstance);
            Register<TiffRgba32, TiffCmyk32>(TiffRgba32ToCmyk32PixelConverter.FactoryInstance);
            Register<TiffRgba32, TiffCmyk64>(TiffCombinedPixelConverterFactory.Create<TiffRgba32, TiffRgba64, TiffCmyk64>(TiffRgba32ToRgba64PixelConverter.SpanConverter, TiffRgba64ToCmyk64PixelConverter.SpanConverter));
            Register<TiffRgba32, TiffBgr24>(TiffRgba32ToBgr24PixelConverter.FactoryInstance);
            Register<TiffRgba32, TiffRgb24>(TiffRgba32ToRgb24PixelConverter.FactoryInstance);

            Register<TiffBgr24, TiffMask>(TiffCombinedPixelConverterFactory.Create<TiffBgr24, TiffGray8, TiffMask>(TiffBgr24ToGray8PixelConverter.SpanConverter, TiffGray8ToMaskPixelConverter.SpanConverter));
            Register<TiffBgr24, TiffGray8>(TiffBgr24ToGray8PixelConverter.FactoryInstance);
            Register<TiffBgr24, TiffGray16>(TiffCombinedPixelConverterFactory.Create<TiffBgr24, TiffGray8, TiffGray16>(TiffBgr24ToGray8PixelConverter.SpanConverter, TiffGray8ToGray16PixelConverter.SpanConverter));
            Register<TiffBgr24, TiffBgra32>(TiffBgr24ToBgra32PixelConverter.FactoryInstance);
            Register<TiffBgr24, TiffBgra64>(TiffCombinedPixelConverterFactory.Create<TiffBgr24, TiffBgra32, TiffBgra64>(TiffBgr24ToBgra32PixelConverter.SpanConverter, TiffBgra32ToBgra64PixelConverter.SpanConverter));
            Register<TiffBgr24, TiffRgba64>(TiffCombinedPixelConverterFactory.Create<TiffBgr24, TiffRgba32, TiffRgba64>(TiffBgr24ToRgba32PixelConverter.SpanConverter, TiffRgba32ToRgba64PixelConverter.SpanConverter));
            Register<TiffBgr24, TiffRgba32>(TiffBgr24ToRgba32PixelConverter.FactoryInstance);
            Register<TiffBgr24, TiffCmyk32>(TiffCombinedPixelConverterFactory.Create<TiffBgr24, TiffBgra32, TiffCmyk32>(TiffBgr24ToBgra32PixelConverter.SpanConverter, TiffBgra32ToCmyk32PixelConverter.SpanConverter));
            Register<TiffBgr24, TiffCmyk64>(TiffChainedPixelConverterFactory.Create<TiffBgr24, TiffBgra32, TiffBgra64, TiffCmyk64>(TiffBgr24ToBgra32PixelConverter.FactoryInstance, TiffBgra32ToBgra64PixelConverter.FactoryInstance, TiffBgra64ToCmyk64PixelConverter.FactoryInstance));
            Register<TiffBgr24, TiffRgb24>(TiffBgr24ToRgb24PixelConverter.FactoryInstance);

            Register<TiffRgb24, TiffMask>(TiffCombinedPixelConverterFactory.Create<TiffRgb24, TiffGray8, TiffMask>(TiffRgb24ToGray8PixelConverter.SpanConverter, TiffGray8ToMaskPixelConverter.SpanConverter));
            Register<TiffRgb24, TiffGray8>(TiffRgb24ToGray8PixelConverter.FactoryInstance);
            Register<TiffRgb24, TiffGray16>(TiffCombinedPixelConverterFactory.Create<TiffRgb24, TiffGray8, TiffGray16>(TiffRgb24ToGray8PixelConverter.SpanConverter, TiffGray8ToGray16PixelConverter.SpanConverter));
            Register<TiffRgb24, TiffBgra32>(TiffRgb24ToBgra32PixelConverter.FactoryInstance);
            Register<TiffRgb24, TiffBgra64>(TiffCombinedPixelConverterFactory.Create<TiffRgb24, TiffBgra32, TiffBgra64>(TiffRgb24ToBgra32PixelConverter.SpanConverter, TiffBgra32ToBgra64PixelConverter.SpanConverter));
            Register<TiffRgb24, TiffRgba64>(TiffCombinedPixelConverterFactory.Create<TiffRgb24, TiffRgba32, TiffRgba64>(TiffRgb24ToRgba32PixelConverter.SpanConverter, TiffRgba32ToRgba64PixelConverter.SpanConverter));
            Register<TiffRgb24, TiffRgba32>(TiffRgb24ToRgba32PixelConverter.FactoryInstance);
            Register<TiffRgb24, TiffCmyk32>(TiffCombinedPixelConverterFactory.Create<TiffRgb24, TiffRgba32, TiffCmyk32>(TiffRgb24ToRgba32PixelConverter.SpanConverter, TiffRgba32ToCmyk32PixelConverter.SpanConverter));
            Register<TiffRgb24, TiffCmyk64>(TiffChainedPixelConverterFactory.Create<TiffRgb24, TiffRgba32, TiffRgba64, TiffCmyk64>(TiffRgb24ToRgba32PixelConverter.FactoryInstance, TiffRgba32ToRgba64PixelConverter.FactoryInstance, TiffRgba64ToCmyk64PixelConverter.FactoryInstance));
            Register<TiffRgb24, TiffBgr24>(TiffRgb24ToBgr24PixelConverter.FactoryInstance);

            Register<TiffCmyk32, TiffMask>(TiffCombinedPixelConverterFactory.Create<TiffCmyk32, TiffGray8, TiffMask>(TiffCmyk32ToGray8PixelConverter.SpanConverter, TiffGray8ToMaskPixelConverter.SpanConverter));
            Register<TiffCmyk32, TiffGray8>(TiffCmyk32ToGray8PixelConverter.FactoryInstance);
            Register<TiffCmyk32, TiffGray16>(TiffCmyk32ToGray16PixelConverter.FactoryInstance);
            Register<TiffCmyk32, TiffBgra32>(TiffCmyk32ToBgra32PixelConverter.FactoryInstance);
            Register<TiffCmyk32, TiffBgra64>(TiffCombinedPixelConverterFactory.Create<TiffCmyk32, TiffBgra32, TiffBgra64>(TiffCmyk32ToBgra32PixelConverter.SpanConverter, TiffBgra32ToBgra64PixelConverter.SpanConverter));
            Register<TiffCmyk32, TiffRgba64>(TiffCombinedPixelConverterFactory.Create<TiffCmyk32, TiffRgba32, TiffRgba64>(TiffCmyk32ToRgba32PixelConverter.SpanConverter, TiffRgba32ToRgba64PixelConverter.SpanConverter));
            Register<TiffCmyk32, TiffRgba32>(TiffCmyk32ToRgba32PixelConverter.FactoryInstance);
            Register<TiffCmyk32, TiffBgr24>(TiffCombinedPixelConverterFactory.Create<TiffCmyk32, TiffBgra32, TiffBgr24>(TiffCmyk32ToBgra32PixelConverter.SpanConverter, TiffBgra32ToBgr24PixelConverter.SpanConverter));
            Register<TiffCmyk32, TiffRgb24>(TiffCombinedPixelConverterFactory.Create<TiffCmyk32, TiffRgba32, TiffRgb24>(TiffCmyk32ToRgba32PixelConverter.SpanConverter, TiffRgba32ToRgb24PixelConverter.SpanConverter));
            Register<TiffCmyk32, TiffCmyk64>(TiffCmyk32ToCmyk64PixelConverter.FactoryInstance);

            Register<TiffCmyk64, TiffMask>(TiffChainedPixelConverterFactory.Create<TiffCmyk64, TiffBgra64, TiffGray8, TiffMask>(TiffCmyk64ToBgra64PixelConverter.FactoryInstance, TiffBgra64ToGray8PixelConverter.FactoryInstance, TiffGray8ToMaskPixelConverter.FactoryInstance));
            Register<TiffCmyk64, TiffGray8>(TiffCombinedPixelConverterFactory.Create<TiffCmyk64, TiffBgra64, TiffGray8>(TiffCmyk64ToBgra64PixelConverter.SpanConverter, TiffBgra64ToGray8PixelConverter.SpanConverter));
            Register<TiffCmyk64, TiffGray16>(TiffCombinedPixelConverterFactory.Create<TiffCmyk64, TiffBgra64, TiffGray16>(TiffCmyk64ToBgra64PixelConverter.SpanConverter, TiffBgra64ToGray16PixelConverter.SpanConverter));
            Register<TiffCmyk64, TiffBgra32>(TiffCombinedPixelConverterFactory.Create<TiffCmyk64, TiffBgra64, TiffBgra32>(TiffCmyk64ToBgra64PixelConverter.SpanConverter, TiffBgra64ToBgra32PixelConverter.SpanConverter));
            Register<TiffCmyk64, TiffBgra64>(TiffCmyk64ToBgra64PixelConverter.FactoryInstance);
            Register<TiffCmyk64, TiffRgba64>(TiffCmyk64ToRgba64PixelConverter.FactoryInstance);
            Register<TiffCmyk64, TiffRgba32>(TiffCombinedPixelConverterFactory.Create<TiffCmyk64, TiffBgra64, TiffRgba32>(TiffCmyk64ToBgra64PixelConverter.SpanConverter, TiffBgra64ToRgba32PixelConverter.SpanConverter));
            Register<TiffCmyk64, TiffBgr24>(TiffCombinedPixelConverterFactory.Create<TiffCmyk64, TiffBgra64, TiffBgr24>(TiffCmyk64ToBgra64PixelConverter.SpanConverter, TiffBgra64ToBgr24PixelConverter.SpanConverter));
            Register<TiffCmyk64, TiffRgb24>(TiffCombinedPixelConverterFactory.Create<TiffCmyk64, TiffRgba64, TiffRgb24>(TiffCmyk64ToRgba64PixelConverter.SpanConverter, TiffRgba64ToRgb24PixelConverter.SpanConverter));
            Register<TiffCmyk64, TiffCmyk32>(TiffCmyk64ToCmyk32PixelConverter.FactoryInstance);
        }

        private void Register<TSource, TDestination>(ITiffPixelConverterFactory factory) where TSource : unmanaged where TDestination : unmanaged
        {
            if (!factory.IsConvertible<TSource, TDestination>())
            {
                throw new InvalidOperationException();
            }
            _factories.Add(new ConverterFactoryDescriptor(typeof(TSource), typeof(TDestination), factory));
        }
    }
}

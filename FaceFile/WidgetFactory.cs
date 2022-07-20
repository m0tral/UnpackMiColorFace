using System;
using UnpackMiColorFace.Helpers;

namespace UnpackMiColorFace.FaceFile
{
    internal class WidgetFactory
    {
        internal static FaceWidget Get(uint itemType, byte[] bin)
        {
            switch (itemType)
            {
                case 27: // AnalogClock
                    return FaceWidgetAnalogClock.Get(bin);

                case 29: // CircleProgress
                    return FaceWidgetCircleProgress.Get(bin);

                case 42: // CircleProgressPlus
                    return FaceWidgetCircleProgressPlus.Get(bin);

                case 30: // Image
                    return FaceWidgetImage.Get(bin);

                case 31: // ImageList
                    return FaceWidgetImageList.Get(bin);

                case 32: // DigitalNumImage
                    return FaceWidgetDigitalNum.Get(bin);

                case 33: // TextPlus
                    return FaceWidgetTextPlus.Get(bin);

                case 34: // Container
                    return FaceWidgetContainer.Get(bin);

                case 39: // Concatenate
                    return FaceWidgetConcatenate.Get(bin);

                case 41: // ImageCacheList
                    return FaceWidgetImageCacheList.Get(bin);

                case 43: // CircularGauge
                    return FaceWidgetCircularGauge.Get(bin);

                default:
                    return FaceWidget.Get(bin);
            }
        }
    }
}
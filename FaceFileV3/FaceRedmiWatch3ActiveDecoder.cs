using System;
using System.Collections.Generic;
using System.Linq;
using XiaomiWatch.Common.FaceFile;

namespace UnpackMiColorFace.FaceFileV3
{
    public class FaceRedmiWatch3ActiveDecoder: IFaceFileV3Decoder
    {
        public FaceRedmiWatch3ActiveDecoder()
        {
        }

        public FaceWidget GetWidget(int sectionId, List<string> imageNameList, int posX, int posY, int width, int height)
        {
            int percentLevel = -10;
            int numIndex = 0;
            return sectionId switch
            {
                0x04 => new FaceWidgetImageList()
                {
                    Name = $"imgList_{sectionId:X2}_batt",
                    BitmapList = imageNameList.Aggregate("", (final, next) => final + (final.Length > 0 ? "|":"") + $"({percentLevel += 10}):{next}"),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    DefaultIndex = 50,
                    DataSrcIndex = FaceItemDataSrc.Battery,
                },
                0x09 => new FaceWidgetDigitalNum()
                {
                    Name = $"num_{sectionId:X2}_batt",
                    BitmapList = string.Join("|", imageNameList.Append(imageNameList[0])),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    Digits = 3,
                    Blanking = 1,
                    DataSrcValue = FaceItemDataSrc.Battery,
                },
                0x0A => new FaceWidgetImage()
                {
                    Name = $"delim_{sectionId:X2}",
                    Bitmap = imageNameList[0],
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                },
                0x0D => new FaceWidgetDigitalNum()
                {
                    Name = $"num_{sectionId:X2}_month",
                    BitmapList = string.Join("|", imageNameList.Append(imageNameList[0])),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    Digits = 2,
                    DataSrcValue = FaceItemDataSrc.Month,
                },
                0x0E => new FaceWidgetImage()
                {
                    Name = $"delim_{sectionId:X2}",
                    Bitmap = imageNameList[0],
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                },
                0x0F => new FaceWidgetDigitalNum()
                {
                    Name = $"num_{sectionId:X2}_day",
                    BitmapList = string.Join("|", imageNameList.Append(imageNameList[0])),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    Digits = 2,
                    DataSrcValue = FaceItemDataSrc.Day,
                },
                0x10 => new FaceWidgetImageList()
                {
                    Name = $"imgList_{sectionId:X2}_weekCN1",
                    BitmapList = imageNameList.Aggregate("", (final, next) => final + (final.Length > 0 ? "|" : "") + $"({numIndex+=1}):{next}"),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0,
                    DefaultIndex = 1,
                    DataSrcIndex = FaceItemDataSrc.Weekday,
                },
                0x11 => new FaceWidgetImageList()
                {
                    Name = $"imgList_{sectionId:X2}_weekEN",
                    BitmapList = imageNameList.Aggregate("", (final, next) => final + (final.Length > 0 ? "|" : "") + $"({numIndex+=1}):{next}"),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    DefaultIndex = 1,
                    DataSrcIndex = FaceItemDataSrc.Weekday,
                },
                0x12 => new FaceWidgetImageList()
                {
                    Name = $"imgList_{sectionId:X2}_weekCN2",
                    BitmapList = imageNameList.Aggregate("", (final, next) => final + (final.Length > 0 ? "|" : "") + $"({numIndex+=1}):{next}"),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0,
                    DefaultIndex = 1,
                    DataSrcIndex = FaceItemDataSrc.Weekday,
                },
                0x18 => new FaceWidgetImageList()
                {
                    Name = $"imgList_{sectionId:X2}_stepsPercent",
                    BitmapList = imageNameList.Aggregate("", (final, next) => final + (final.Length > 0 ? "|" : "") + $"({percentLevel += 10}):{next}"),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    DefaultIndex = 50,
                    DataSrcIndex = FaceItemDataSrc.StepsPercent,
                },
                0x1C => new FaceWidgetDigitalNum()
                {
                    Name = $"num_{sectionId:X2}_steps",
                    BitmapList = string.Join("|", imageNameList.Append(imageNameList[0])),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    Digits = 5,
                    Blanking = 1,
                    DataSrcValue = FaceItemDataSrc.Steps,
                },
                0x23 => new FaceWidgetImageList()
                {
                    Name = $"imgList_{sectionId:X2}_calPercent",
                    BitmapList = imageNameList.Aggregate("", (final, next) => final + (final.Length > 0 ? "|" : "") + $"({percentLevel += 10}):{next}"),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    DefaultIndex = 50,
                    DataSrcIndex = FaceItemDataSrc.CaloriesPercent,
                },
                0x27 => new FaceWidgetDigitalNum()
                {
                    Name = $"num_{sectionId:X2}_calories",
                    BitmapList = string.Join("|", imageNameList.Append(imageNameList[0])),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    Digits = 4,
                    Blanking = 1,
                    DataSrcValue = FaceItemDataSrc.Calories,
                },
                0x31 => new FaceWidgetDigitalNum()
                {
                    Name = $"num_{sectionId:X2}_hrm",
                    BitmapList = string.Join("|", imageNameList.Append(imageNameList[0])),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    Digits = 3,
                    Blanking = 1,
                    DataSrcValue = FaceItemDataSrc.Hrm,
                },
                0x32 => new FaceWidgetImage()
                {
                    Name = $"img_{sectionId:X2}_noHrm",
                    Bitmap = imageNameList[0],
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                },
                0x38 => new FaceWidgetDigitalNum()
                {
                    Name = $"num_{sectionId:X2}_hourHigh",
                    BitmapList = string.Join("|", imageNameList.Append(imageNameList[0])),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    Digits = 1,
                    DataSrcValue = FaceItemDataSrc.HourHigh,
                },
                0x39 => new FaceWidgetDigitalNum()
                {
                    Name = $"num_{sectionId:X2}_hourLow",
                    BitmapList = string.Join("|", imageNameList.Append(imageNameList[0])),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    Digits = 1,
                    DataSrcValue = FaceItemDataSrc.HourLow,
                },
                0x3A => new FaceWidgetImage()
                {
                    Name = $"delim_{sectionId:X2}",
                    Bitmap = imageNameList[0],
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                },
                0x3B => new FaceWidgetDigitalNum()
                {
                    Name = $"num_{sectionId:X2}_minHigh",
                    BitmapList = string.Join("|", imageNameList.Append(imageNameList[0])),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    Digits = 1,
                    DataSrcValue = FaceItemDataSrc.MinuteHigh,
                },
                0x3C => new FaceWidgetDigitalNum()
                {
                    Name = $"num_{sectionId:X2}_minLow",
                    BitmapList = string.Join("|", imageNameList.Append(imageNameList[0])),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    Digits = 1,
                    DataSrcValue = FaceItemDataSrc.MinuteLow,
                },
                0x42 => new FaceWidgetImageList()
                {
                    Name = $"imgList_{sectionId:X2}_hour",
                    BitmapList = imageNameList.Aggregate("", (final, next) => final + (final.Length > 0 ? "|" : "") + $"({numIndex++}):{next}"),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    DefaultIndex = 10,
                    DataSrcIndex = FaceItemDataSrc.Hour,
                },
                0x43 => new FaceWidgetImageList()
                {
                    Name = $"imgList_{sectionId:X2}_min",
                    BitmapList = imageNameList.Aggregate("", (final, next) => final + (final.Length > 0 ? "|" : "") + $"({numIndex++}):{next}"),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    DataSrcIndex = FaceItemDataSrc.Minute,
                },
                0x44 => new FaceWidgetImageList()
                {
                    Name = $"imgList_{sectionId:X2}_second",
                    BitmapList = imageNameList.Aggregate("", (final, next) => final + (final.Length > 0 ? "|" : "") + $"({numIndex++}):{next}"),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    DefaultIndex = 5,
                    DataSrcIndex = FaceItemDataSrc.Second,
                },
                _ => throw new NotSupportedException($"Can't detect widget for sectionId: {sectionId:X2}"),
            };
        }
    }
}
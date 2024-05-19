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
                    Name = $"{sectionId:X2}_imgList_batt",
                    BitmapList = imageNameList.Aggregate("", (final, next) => final + (final.Length > 0 ? "|":"") + $"({percentLevel += 10}):{next}"),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    DefaultIndex = 50,
                    DataSrcIndex = FaceItemDataSrc.Battery,
                },
                0x05 => new FaceWidgetImage()
                {
                    Name = $"{sectionId:X2}_img_battCN1",
                    Bitmap = imageNameList[0],
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0,
                },
                0x06 => new FaceWidgetImage()
                {
                    Name = $"{sectionId:X2}_img_battEN",
                    Bitmap = imageNameList[0],
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                },
                0x07 => new FaceWidgetImage()
                {
                    Name = $"{sectionId:X2}_img_battCN2",
                    Bitmap = imageNameList[0],
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0,
                },
                0x09 => new FaceWidgetDigitalNum()
                {
                    Name = $"{sectionId:X2}_num_batt",
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
                    Name = $"{sectionId:X2}_delim",
                    Bitmap = imageNameList[0],
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                },
                0x0D => new FaceWidgetDigitalNum()
                {
                    Name = $"{sectionId:X2}_num_month",
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
                    Name = $"{sectionId:X2}_delim",
                    Bitmap = imageNameList[0],
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                },
                0x0F => new FaceWidgetDigitalNum()
                {
                    Name = $"{sectionId:X2}_num_day",
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
                    Name = $"{sectionId:X2}_imgList_weekCN1",
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
                    Name = $"{sectionId:X2}_imgList_weekEN",
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
                    Name = $"{sectionId:X2}_imgList_weekCN2",
                    BitmapList = imageNameList.Aggregate("", (final, next) => final + (final.Length > 0 ? "|" : "") + $"({numIndex+=1}):{next}"),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0,
                    DefaultIndex = 1,
                    DataSrcIndex = FaceItemDataSrc.Weekday,
                },
                0x13 => new FaceWidgetImageList()
                {
                    Name = $"{sectionId:X2}_imgList_AMPM-CN1",
                    BitmapList = imageNameList.Aggregate("", (final, next) => final + (final.Length > 0 ? "|" : "") + $"({numIndex += 1}):{next}"),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0,
                    DefaultIndex = 1,
                    DataSrcIndex = FaceItemDataSrc.AMPM,
                },
                0x14 => new FaceWidgetImageList()
                {
                    Name = $"{sectionId:X2}_imgList_AMPM-EN",
                    BitmapList = imageNameList.Aggregate("", (final, next) => final + (final.Length > 0 ? "|" : "") + $"({numIndex += 1}):{next}"),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    DefaultIndex = 1,
                    DataSrcIndex = FaceItemDataSrc.AMPM,
                },
                0x15 => new FaceWidgetImageList()
                {
                    Name = $"{sectionId:X2}_imgList_AMPM-CN2",
                    BitmapList = imageNameList.Aggregate("", (final, next) => final + (final.Length > 0 ? "|" : "") + $"({numIndex += 1}):{next}"),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0,
                    DefaultIndex = 1,
                    DataSrcIndex = FaceItemDataSrc.AMPM,
                },
                0x18 => new FaceWidgetImageList()
                {
                    Name = $"{sectionId:X2}_imgList_stepsPercent",
                    BitmapList = imageNameList.Aggregate("", (final, next) => final + (final.Length > 0 ? "|" : "") + $"({percentLevel += 10}):{next}"),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    DefaultIndex = 50,
                    DataSrcIndex = FaceItemDataSrc.StepsPercent,
                },
                0x19 => new FaceWidgetImage()
                {
                    Name = $"{sectionId:X2}_img_stepsCN1",
                    Bitmap = imageNameList[0],
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0,
                },
                0x1A => new FaceWidgetImage()
                {
                    Name = $"{sectionId:X2}_img_stepsEN",
                    Bitmap = imageNameList[0],
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                },
                0x1B => new FaceWidgetImage()
                {
                    Name = $"{sectionId:X2}_img_stepsCN2",
                    Bitmap = imageNameList[0],
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0,
                },
                0x1C => new FaceWidgetDigitalNum()
                {
                    Name = $"{sectionId:X2}_num_steps",
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
                    Name = $"{sectionId:X2}_imgList_calPercent",
                    BitmapList = imageNameList.Aggregate("", (final, next) => final + (final.Length > 0 ? "|" : "") + $"({percentLevel += 10}):{next}"),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    DefaultIndex = 50,
                    DataSrcIndex = FaceItemDataSrc.CaloriesPercent,
                },
                0x24 => new FaceWidgetImage()
                {
                    Name = $"{sectionId:X2}_img_calCN1",
                    Bitmap = imageNameList[0],
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0,
                },
                0x25 => new FaceWidgetImage()
                {
                    Name = $"{sectionId:X2}_img_calEN",
                    Bitmap = imageNameList[0],
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                },
                0x26 => new FaceWidgetImage()
                {
                    Name = $"{sectionId:X2}_img_calCN2",
                    Bitmap = imageNameList[0],
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0,
                },
                0x27 => new FaceWidgetDigitalNum()
                {
                    Name = $"{sectionId:X2}_num_calories",
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
                    Name = $"{sectionId:X2}_num_hrm",
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
                    Name = $"{sectionId:X2}_img_noHrm",
                    Bitmap = imageNameList[0],
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                },
                0x33 => new FaceWidgetImage()
                {
                    Name = $"{sectionId:X2}_img_hrmCN1",
                    Bitmap = imageNameList[0],
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0,
                },
                0x34 => new FaceWidgetImage()
                {
                    Name = $"{sectionId:X2}_img_hrmEN",
                    Bitmap = imageNameList[0],
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                },
                0x35 => new FaceWidgetImage()
                {
                    Name = $"{sectionId:X2}_img_hrmCN2",
                    Bitmap = imageNameList[0],
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0,
                },
                0x38 => new FaceWidgetDigitalNum()
                {
                    Name = $"{sectionId:X2}_num_hourHigh",
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
                    Name = $"{sectionId:X2}_num_hourLow",
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
                    Name = $"{sectionId:X2}_delim",
                    Bitmap = imageNameList[0],
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                },
                0x3B => new FaceWidgetDigitalNum()
                {
                    Name = $"{sectionId:X2}_num_minHigh",
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
                    Name = $"{sectionId:X2}_num_minLow",
                    BitmapList = string.Join("|", imageNameList.Append(imageNameList[0])),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    Digits = 1,
                    DataSrcValue = FaceItemDataSrc.MinuteLow,
                },
                0x3D => new FaceWidgetImage()
                {
                    Name = $"{sectionId:X2}_delim",
                    Bitmap = imageNameList[0],
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                },
                0x3E => new FaceWidgetDigitalNum()
                {
                    Name = $"{sectionId:X2}_num_secHigh",
                    BitmapList = string.Join("|", imageNameList.Append(imageNameList[0])),
                    X = posX,
                    Y = posY,
                    Width = width,
                    Height = height,
                    Alpha = 0xFF,
                    Digits = 1,
                    DataSrcValue = FaceItemDataSrc.MinuteLow,
                },
                0x3F => new FaceWidgetDigitalNum()
                {
                    Name = $"{sectionId:X2}_num_secLow",
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
                    Name = $"{sectionId:X2}_imgList_hour",
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
                    Name = $"{sectionId:X2}_imgList_min",
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
                    Name = $"{sectionId:X2}_imgList_second",
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
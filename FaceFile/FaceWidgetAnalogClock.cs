using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnpackMiColorFace.Helpers;

namespace UnpackMiColorFace.FaceFile
{
    public class FaceWidgetAnalogClock : FaceWidget
    {
        [XmlAttribute("HourHandCorrection_En")]
        public int CorrectionEnableHourHand { get; set; }

        [XmlAttribute("MinuteHandCorrection_En")]
        public int CorrectionEnableMinuteHand { get; set; }

        [XmlAttribute("Background_ImageName")]
        public string BackgroundImage { get; set; }

        [XmlAttribute("BgImage_rotate_xc")]
        public int BackImageRotateX { get; set; }
        [XmlAttribute("BgImage_rotate_yc")]
        public int BackImageRotateY { get; set; }

        [XmlAttribute("HourHand_ImageName")]
        public string HourHandImage { get; set; }

        [XmlAttribute("HourImage_rotate_xc")]
        public int HourImageRotateX { get; set; }
        [XmlAttribute("HourImage_rotate_yc")]
        public int HourImageRotateY { get; set; }

        [XmlAttribute("MinuteHand_Image")]
        public string MinuteHandImage { get; set; }

        [XmlAttribute("MinuteImage_rotate_xc")]
        public int MinuteImageRotateX { get; set; }
        [XmlAttribute("MinuteImage_rotate_yc")]
        public int MinuteImageRotateY { get; set; }

        [XmlAttribute("SecondHand_Image")]
        public string SecondHandImage { get; set; }

        [XmlAttribute("SecondImage_rotate_xc")]
        public int SecondImageRotateX { get; set; }
        [XmlAttribute("SecondImage_rotate_yc")]
        public int SecondImageRotateY { get; set; }


        public new static FaceWidgetAnalogClock Get(byte[] bin)
        {
            int index = (int)bin.GetDWord(0);
            int itemType = (int)bin.GetDWord(4);

            int x = bin.GetWord(0x08);
            int y = bin.GetWord(0x0C);

            int width = bin.GetWord(0x10);
            int height = bin.GetWord(0x14);

            int dbSrcDisplay = bin.GetWord(0x18);

            int alfa = bin.GetByte(0x1C);

            uint extraLen = bin.GetDWord(0x2C);
            byte[] extra = bin.GetByteArray(0x30, extraLen);

            int imgIdxBack = (int)extra.GetDWord(0);
            int rotateXBack = (int)extra.GetDWord(4);
            int rotateYBack = (int)extra.GetDWord(8);

            int imgIdxHour = (int)extra.GetDWord(0x0C);
            int rotateXHour = (int)extra.GetDWord(0x10);
            int rotateYHour = (int)extra.GetDWord(0x14);

            int imgIdxMin = (int)extra.GetDWord(0x18);
            int rotateXMin = (int)extra.GetDWord(0x1C);
            int rotateYMin = (int)extra.GetDWord(0x20);

            int imgIdxSec = (int)extra.GetDWord(0x24);
            int rotateXSec = (int)extra.GetDWord(0x28);
            int rotateYSec = (int)extra.GetDWord(0x2C);

            int enaCorrHour = extra.GetByte(0x30);
            int enaCorrMin = extra.GetByte(0x31);

            return new FaceWidgetAnalogClock()
            {
                Shape = (int)itemType,
                Name = $"analogClock_{index}",
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Alpha = alfa,
                DataSrcDisplay = dbSrcDisplay,
                CorrectionEnableHourHand = enaCorrHour,
                CorrectionEnableMinuteHand = enaCorrMin,
                BackgroundImage = imgIdxBack == -1 ? "" : $"img_{imgIdxBack:D4}.png",
                BackImageRotateX = rotateXBack,
                BackImageRotateY = rotateYBack,
                HourHandImage = $"img_{imgIdxHour:D4}.png",
                HourImageRotateX = rotateXHour,
                HourImageRotateY = rotateYHour,
                MinuteHandImage = $"img_{imgIdxMin:D4}.png",
                MinuteImageRotateX = rotateXMin,
                MinuteImageRotateY = rotateYMin,
                SecondHandImage = $"img_{imgIdxSec:D4}.png",
                SecondImageRotateX = rotateXSec,
                SecondImageRotateY = rotateYSec,
            };
        }
    }
}

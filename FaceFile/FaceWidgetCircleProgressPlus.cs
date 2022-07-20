using System;
using System.Xml;
using System.Xml.Serialization;
using UnpackMiColorFace.Helpers;

namespace UnpackMiColorFace.FaceFile
{
    public class FaceWidgetCircleProgressPlus : FaceWidget
    {
        [XmlAttribute()]
        public int Radius { get; set; }

        [XmlAttribute("Rotate_xc")]
        public int RotateX { get; set; }

        [XmlAttribute("Rotate_yc")]
        public int RotateY { get; set; }

        [XmlAttribute()]
        public int StartAngle { get; set; }

        [XmlAttribute()]
        public int EndAngle { get; set; }

        [XmlAttribute("Line_Width")]
        public int LineWidth { get; set; }

        [XmlAttribute("Range_Min")]
        public int RangeMin { get; set; }
        [XmlAttribute("Range_Max")]
        public int RangeMax { get; set; }
        [XmlAttribute("Range_MinStep")]
        public int RangeMinStep { get; set; }
        [XmlAttribute("Range_Step")]
        public int RangeStep { get; set; }
        [XmlAttribute("Range_Max_Src")]
        public int DbSrcRangeMax { get; set; }
        [XmlAttribute("Range_Val_Src")]
        public int DbSrcRangeValue { get; set; }

        [XmlAttribute("Background_ImageName")]
        public string BackgroundImage { get; set; }

        [XmlAttribute("Foreground_ImageName")]
        public string ForegroundImage { get; set; }
        
        [XmlAttribute("Butt_cap_ending_style_En")]
        public int ButtonCapEndingStyleEnabled { get; set; }

        public new static FaceWidgetCircleProgressPlus Get(byte[] bin)
        {
            int index = (int)bin.GetDWord(0);
            int itemType = (int)bin.GetDWord(4);

            int x = bin.GetWord(0x08);
            int y = bin.GetWord(0x0C);

            int width = bin.GetWord(0x10);
            int height = bin.GetWord(0x14);

            int alfa = bin.GetByte(0x1C);

            uint extraLen = bin.GetDWord(0x2C);
            byte[] extra = bin.GetByteArray(0x30, extraLen);

            int radius = (int)extra.GetDWord(0);
            int rotateX = (int)extra.GetDWord(4);
            int rotateY = (int)extra.GetDWord(8);
            int lineWidth = (int)extra.GetDWord(0x0C);

            int angleStart = (int)extra.GetDWord(0x10);
            int angleEnd = (int)extra.GetDWord(0x14);

            int rangeMin = extra.GetWord(0x18);
            int rangeMax = extra.GetWord(0x1A);

            int rangeMinStep = extra.GetWord(0x1C);
            int rangeStep = extra.GetWord(0x1E);

            int imgBgd = (int)extra.GetDWord(0x20);
            int imgFor = (int)extra.GetDWord(0x24);

            int dbSrcRangeMax = extra.GetByte(0x2C);
            int dbSrcRangeVal = extra.GetByte(0x2D);

            return new FaceWidgetCircleProgressPlus()
            {
                Shape = itemType,
                Name = $"circleProgPlus_{index}",
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Alpha = alfa,
                Radius = radius,
                RotateX = rotateX,
                RotateY = rotateY,
                LineWidth = lineWidth,
                StartAngle = angleStart,
                EndAngle = angleEnd,
                RangeMin = rangeMin,
                RangeMax = rangeMax,
                RangeMinStep = rangeMinStep,
                RangeStep = rangeStep,
                DbSrcRangeMax = dbSrcRangeMax,
                DbSrcRangeValue = dbSrcRangeVal,
                BackgroundImage = imgBgd == -1 ? "" : $"img_{imgBgd:D4}.png",
                ForegroundImage = imgFor == -1 ? "" : $"img_{imgFor:D4}.png",
            };
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnpackMiColorFace.Helpers;

namespace UnpackMiColorFace.FaceFile
{
    public class FaceWidgetCircularGauge : FaceWidget
    {        
        [XmlAttribute("Range_Max_Src")]
        public int DataSrcMax { get; set; }

        [XmlAttribute("Range_Val_Src")]
        public string DataSrcVal { get; set; }

        [XmlAttribute("Origo_x")]
        public int GaugeX { get; set; }
        [XmlAttribute("Origo_y")]
        public int GaugeY { get; set; }

        [XmlAttribute("StartAngle")]
        public int StartAngle { get; set; }
        [XmlAttribute("EndAngle")]
        public int EndAngle { get; set; }

        [XmlAttribute("Range_Min")]
        public int RangeMin { get; set; }
        [XmlAttribute("Range_Max")]
        public int RangeMax { get; set; }

        [XmlAttribute("Range_MinStep")]
        public int RangeMinStep { get; set; }
        [XmlAttribute("Range_Step")]
        public int RangeStep { get; set; }

        [XmlAttribute("Pointer_Bitmap")]
        public string PointerImage { get; set; }

        [XmlAttribute("Pointer_Rotate_xc")]
        public int PointerRotateX { get; set; }
        [XmlAttribute("Pointer_Rotate_yc")]
        public int PointerRotateY { get; set; }


        [XmlAttribute("Render_Height_Quality_En")]
        public int RenderHeightQualityEnabled { get; set; }

        public new static FaceWidgetCircularGauge Get(byte[] bin)
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

            int rotateX = (int)extra.GetDWord(0);
            int rotateY = (int)extra.GetDWord(4);

            int valX = (int)extra.GetDWord(8);
            int valY = (int)extra.GetDWord(0x0C);

            int min = extra.GetWord(0x10);
            int max = extra.GetWord(0x12);
            int minStep = extra.GetWord(0x14);
            int step = extra.GetWord(0x16);

            int imgIdx = extra.GetWord(0x18);

            int rotX = extra.GetWord(0x1C);
            int rotY = extra.GetWord(0x20);

            int corr = extra.GetByte(0x24);
            int dataMax = extra.GetByte(0x25);
            int dataVal = extra.GetByte(0x26);

            return new FaceWidgetCircularGauge()
            {
                Shape = itemType,
                Name = $"circularGauge_{index}",
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Alpha = alfa,
                GaugeX = rotateX,
                GaugeY = rotateY,
                StartAngle = valX,
                EndAngle = valY,
                RangeMin = min,
                RangeMax = max,
                RangeMinStep = minStep,
                RangeStep = step,
                PointerImage = imgIdx == -1 ? "" : $"img_{imgIdx:D4}.png",
                PointerRotateX = rotX,
                PointerRotateY = rotY,
                RenderHeightQualityEnabled = corr,
                DataSrcMax = dataMax,
                DataSrcVal = dataVal.ToString(),
            };
        }
    }
}

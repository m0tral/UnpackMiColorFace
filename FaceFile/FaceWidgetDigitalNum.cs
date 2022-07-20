using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnpackMiColorFace.Helpers;

namespace UnpackMiColorFace.FaceFile
{
    public class FaceWidgetDigitalNum : FaceWidget
    {
        [XmlAttribute()]
        public int Digits { get; set; }

        [XmlAttribute()]
        public int Alignment { get; set; }

        [XmlAttribute()]
        public int Spacing { get; set; }

        [XmlAttribute()]
        public int Blanking { get; set; }

        [XmlAttribute("Value_Src")]
        public string DataSrcValue { get; set; }

        [XmlAttribute()]
        public string BitmapList { get; set; }

        public new static FaceWidgetDigitalNum Get(byte[] bin)
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

            int DbSrcValue = extra.GetByte(0);
            int DbSrcDisplay = extra.GetByte(1);
            int spacing = extra.GetByte(2);
            int align = extra.GetByte(3);
            int digits = extra.GetByte(4);

            extraLen = extraLen - 5;
            extra = extra.GetByteArray(5, extraLen);

            string bmpList = "";
            do
            {
                uint bmpIdx = extra.GetDWord(0);
                bmpList += $"img_{bmpIdx:D4}.png";

                extraLen -= 4;
                if (extraLen > 0)
                {
                    bmpList += "|";
                    extra = extra.GetByteArray(4, extraLen);
                }

            } while (extraLen > 0);

            return new FaceWidgetDigitalNum()
            {
                Shape = (int)itemType,
                Name = $"digitNum_{index}",
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Alpha = alfa,
                Spacing = spacing,
                Alignment = align,
                Digits = digits,
                DataSrcValue = DbSrcValue.ToString(),
                DataSrcDisplay = DbSrcDisplay,
                BitmapList = bmpList,
            };
        }
    }
}

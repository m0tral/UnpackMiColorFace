using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnpackMiColorFace.Helpers;

namespace UnpackMiColorFace.FaceFile
{
    public class FaceWidgetConcatenate : FaceWidget
    {
        [XmlAttribute()]
        public int Alignment { get; set; }

        [XmlAttribute()]
        public int Spacing { get; set; }

        [XmlAttribute("AutoWidth_En")]
        public int AutoWidthEnabled { get; set; }

        public new static FaceWidgetConcatenate Get(byte[] bin)
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

            return new FaceWidgetConcatenate()
            {
                Shape = itemType,
                Name = $"concate_{index}",
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Alpha = alfa,
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnpackMiColorFace.Helpers;

namespace UnpackMiColorFace.FaceFile
{
    public class FaceWidgetImageCacheList : FaceWidget
    {
        [XmlAttribute()]
        public string BitmapList { get; set; }

        public new static FaceWidgetImageCacheList Get(byte[] bin)
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

            int imgCount = extra.GetByte(0x10);

            extraLen = extraLen - 0x11;
            extra = extra.GetByteArray(0x11, extraLen);

            string bmpList = "img_00{";

            int bmpIdx = -1;

            string bmpNamePrev = "";
            do
            {
                bmpIdx = (int)extra.GetDWord(0);

                bmpNamePrev = $"{bmpIdx:D2}";
                bmpList += bmpNamePrev;

                extraLen -= 4;
                if (extraLen > 0)
                {
                    bmpList += ",";
                    extra = extra.GetByteArray(4, extraLen);
                }
            } while (extraLen > 0);

            bmpList += "}.png";

            return new FaceWidgetImageCacheList()
            {
                Shape = itemType,
                Name = $"imageCacheList_{index}",
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Alpha = alfa,
                DataSrcDisplay = dbSrcDisplay,
                BitmapList = bmpList,
            };
        }
    }
}

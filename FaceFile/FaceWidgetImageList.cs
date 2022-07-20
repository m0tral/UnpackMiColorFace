using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnpackMiColorFace.Helpers;

namespace UnpackMiColorFace.FaceFile
{
    public class FaceWidgetImageList : FaceWidget
    {
        [XmlAttribute("Index_Src")]
        public string DataSrcIndex { get; set; }

        [XmlAttribute()]
        public int DefaultIndex { get; set; }

        [XmlAttribute()]
        public string BitmapList { get; set; }

        public new static FaceWidgetImageList Get(byte[] bin)
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

            int defaultIndex = extra.GetByte(0);
            int dbSrcIndex = extra.GetByte(1);
            int imgCount = extra.GetByte(2);

            extraLen = extraLen - 3;
            extra = extra.GetByteArray(3, extraLen);

            string bmpList = "";

            int bmpIdx = -1;
            int bmpIdxPrev = 0;

            int fromValue = 0;
            int toValue = 0;

            int posX = 0;
            int posY = 0;

            uint bmpCount = 0;
            string bmpNamePrev = "";
            string range = "";
            do
            {
                bmpIdx = (int)extra.GetDWord(0);

                if (bmpIdx == bmpIdxPrev)
                {
                    fromValue = (int)extra.GetDWord(4);
                    toValue = (int)extra.GetDWord(8);
                    posX = (int)extra.GetDWord(0x0C);
                    posY = (int)extra.GetDWord(0x10);

                    if (fromValue == toValue)
                        range += $",{fromValue}";
                    else
                        range += $",{fromValue}/{toValue}";

                    string coord = "";
                    if (posX != x || posY != y)
                        coord = $"[{posX},{posY}]";

                    bmpList = bmpList.Substring(0, bmpList.Length - bmpNamePrev.Length);
                    if (bmpList.Length > 0)
                        bmpList = bmpList.Substring(0, bmpList.Length - 1);

                    bmpNamePrev = $"({range}){coord}:img_{bmpIdx:D4}.png";
                }
                else
                {
                    fromValue = (int)extra.GetDWord(4);
                    toValue = (int)extra.GetDWord(8);
                    posX = (int)extra.GetDWord(0x0C);
                    posY = (int)extra.GetDWord(0x10);

                    range = "";
                    if (fromValue == toValue)
                        range = $"{fromValue}";
                    else
                        range = $"{fromValue}/{toValue}";

                    string coord = "";
                    if (posX != x || posY != y)
                        coord = $"[{posX},{posY}]";

                    bmpNamePrev = $"({range}){coord}:img_{bmpIdx:D4}.png";
                }

                bmpList += bmpNamePrev;

                extraLen -= 0x14;
                if (extraLen > 0)
                {
                    bmpList += "|";
                    extra = extra.GetByteArray(0x14, extraLen);
                }

                bmpIdxPrev = bmpIdx;

            } while (extraLen > 0);

            return new FaceWidgetImageList()
            {
                Shape = itemType,
                Name = $"imageList_{index}",
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Alpha = alfa,
                DataSrcDisplay = dbSrcDisplay,
                DataSrcIndex = dbSrcIndex.ToString(),
                DefaultIndex = defaultIndex,
                BitmapList = bmpList,
            };
        }
    }
}

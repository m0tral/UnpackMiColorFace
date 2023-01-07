using System;
using System.Xml;
using System.Xml.Serialization;
using UnpackMiColorFace.Helpers;

namespace UnpackMiColorFace.FaceFile
{
    [XmlInclude(typeof(FaceWidgetImage))]
    [XmlInclude(typeof(FaceWidgetDigitalNum))]
    [XmlInclude(typeof(FaceWidgetImageList))]
    [XmlInclude(typeof(FaceWidgetCircleProgress))]
    [XmlInclude(typeof(FaceWidgetCircleProgressPlus))]
    [XmlInclude(typeof(FaceWidgetAnalogClock))]
    [XmlInclude(typeof(FaceWidgetCircularGauge))]
    [XmlInclude(typeof(FaceWidgetContainer))]
    [XmlInclude(typeof(FaceWidgetConcatenate))]
    [XmlInclude(typeof(FaceWidgetImageCacheList))]
    [XmlInclude(typeof(FaceWidgetTextPlus))]
    public class FaceWidget
    {
        [XmlAttribute]
        public int Shape { get; set; }
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public int X { get; set; }
        [XmlAttribute]
        public int Y { get; set; }

        [XmlAttribute]
        public int Width { get; set; }
        [XmlAttribute]
        public int Height { get; set; }

        [XmlAttribute]
        public int Alpha { get; set; }

        [XmlAttribute("Visible_Src")]
        public int DataSrcDisplay { get; set; }

        [XmlIgnore]
        public byte[] RawData { get; internal set; }
        [XmlIgnore]
        public uint TargetId { get; internal set; }
        [XmlIgnore]
        public uint Id { get; internal set; }
        [XmlIgnore]
        public byte Digits { get; internal set; }
        [XmlIgnore]
        public byte Align { get; internal set; }
        [XmlIgnore]
        public int TypeId { get; internal set; }

        public static FaceWidget Get(byte[] bin)
        {
            int index = (int)bin.GetDWord(0);
            int itemType = (int)bin.GetDWord(4);

            int x = bin.GetWord(0x08);
            int y = bin.GetWord(0x0C);

            int width = bin.GetWord(0x10);
            int height = bin.GetWord(0x14);

            int alfa = bin.GetByte(0x1C);

            return new FaceWidget()
            {
                Shape = itemType,
                Name = $"unknown_{index}",
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Alpha = alfa,
            };
        }
    }
}
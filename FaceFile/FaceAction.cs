using System.Xml.Serialization;

namespace UnpackMiColorFace
{
    internal class FaceAction
    {
        public uint ActionId { get; internal set; }
        public uint AppId { get; internal set; }
        [XmlIgnore]
        public uint Id { get; internal set; }
        public string ImageName { get; internal set; }
        public byte[] RawData { get; internal set; }
    }
}
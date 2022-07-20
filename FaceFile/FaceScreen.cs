using System.Collections.Generic;
using System.Xml.Serialization;

namespace UnpackMiColorFace.FaceFile
{
    [XmlType("Screen")]
    public class FaceScreen
    {
        [XmlAttribute]
        public string Title { get; set; }

        [XmlAttribute]
        public string Bitmap { get; set; }

        [XmlElement("Widget")]        
        public List<FaceWidget> Widgets { get; set; }

        public FaceScreen()
        {
            Widgets = new List<FaceWidget>();
        }
    }
}
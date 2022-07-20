using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace UnpackMiColorFace.FaceFile
{
    public class FaceProject
    {
        [XmlAttribute]
        public int DeviceType { get; set; }
        
        public FaceScreen Screen { get; set; }

        public FaceProject()
        {
            Screen = new FaceScreen();
        }

        public string Serialize()
        {
            var xns = new XmlSerializerNamespaces();
            xns.Add(string.Empty, string.Empty);

            var xmlserializer = new XmlSerializer(typeof(FaceProject));
            var stringWriter = new StringWriter();

            using (var writer = XmlWriter.Create(stringWriter))
            {
                xmlserializer.Serialize(writer, this, xns);
                return stringWriter.ToString();
            }
        }
    }
}
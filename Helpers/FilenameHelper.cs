using System.IO;

namespace UnpackMiColorFace.Helpers
{
    internal class FilenameHelper
    {
        private string filename;

        public FilenameHelper(string filename)
        {
            this.filename = filename;
        }

        public string NameNoExt => Path.GetFileNameWithoutExtension(filename);
    }
}
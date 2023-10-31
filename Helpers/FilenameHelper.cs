using System;
using System.IO;
using XiaomiWatch.Common;

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

        public string GetFaceSlotImagesFolder(WatchType watchType, int slotId, int subversion) => watchType switch
        {
            WatchType.Gen3 => NameNoExt + @"\images",
            WatchType.MiWatchS3 => NameNoExt + @"\images",
            WatchType.MiBand8Pro => NameNoExt + @"\images",
            _ => (slotId == 0 ? NameNoExt + @"\images" : ((slotId == 1) ? NameNoExt + @"\AOD\images" : NameNoExt + $@"\images_{slotId}") )
        };

        internal string GetFaceSlotFilename(WatchType watchType, int slotId, int subversion)
        {
            string facefile = slotId > 0 ? $"{NameNoExt}_{slotId}" : NameNoExt;

            if (watchType == WatchType.Gen3
                || watchType == WatchType.MiWatchS3
                || watchType == WatchType.MiBand8Pro)
            {
                if ((subversion & 0x04) > 0 && slotId == 1)
                    facefile = $"{NameNoExt}_AOD";
            }
            else
            {
                if (slotId == 1) facefile = "AOD\\" + NameNoExt;
            }

            return facefile;
        }
    }
}
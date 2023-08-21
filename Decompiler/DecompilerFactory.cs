using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnpackMiColorFace.Helpers;

namespace UnpackMiColorFace.Decompiler
{
    class DecompilerFactory
    {
        public static IFaceDecompiler GetDecompiler(int version, string filename)
        {
            var filenameHelper = new FilenameHelper(filename);

            switch (version) {
                case 3:
                    return new FaceV3Decompiler(filenameHelper);
                case 2:
                    return new FaceV2Decompiler(filenameHelper);
                case 1:
                    return new FaceV1Decompiler(filenameHelper);
                default:
                    throw new ApplicationException($"Wrong watchface file version: {version}");
            }
        }
    }
}

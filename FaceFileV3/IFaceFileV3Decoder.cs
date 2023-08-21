using System.Collections.Generic;
using XiaomiWatch.Common.FaceFile;

namespace UnpackMiColorFace.FaceFileV3
{
    public interface IFaceFileV3Decoder
    {
        FaceWidget GetWidget(int sectionId, List<string> imageNameList, int posX, int posY, int width, int height);
    }
}
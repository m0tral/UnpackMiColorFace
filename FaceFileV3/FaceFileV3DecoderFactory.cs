using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XiaomiWatch.Common;

namespace UnpackMiColorFace.FaceFileV3
{
    class FaceFileV3DecoderFactory
    {
        public static IFaceFileV3Decoder GetDecoder(WatchType watchType)
        {
            return watchType switch
            {
                WatchType.RedmiWatch3Active => new FaceRedmiWatch3ActiveDecoder(),
                _ => throw new NotSupportedException($"Can't find decoder for watch: {watchType}")
            };
        }
    }
}

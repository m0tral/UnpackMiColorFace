using XiaomiWatch.Common;

namespace UnpackMiColorFace.Decompiler
{
    public interface IFaceDecompiler
    {
        void Process(WatchType watchType, byte[] data);
    }
}
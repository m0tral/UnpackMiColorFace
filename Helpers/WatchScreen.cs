using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnpackMiColorFace.Helpers
{
    internal class WatchScreen
    {
        internal static int GetScreenHeight(int watchType)
        {
            switch ((WatchType)watchType)
            {
                case WatchType.Gen2: return 466;
                case WatchType.Gen3: return 480;
                case WatchType.RedmiWatch2: return 360;
                case WatchType.RedmiWatch3: return 450;
                case WatchType.Band7Pro: return 456;
                case WatchType.Gen1:
                default: return 454;
            }
        }

        internal static int GetScreenWidth(int watchType)
        {
            switch ((WatchType)watchType)
            {
                case WatchType.Gen2: return 466;
                case WatchType.Gen3: return 480;
                case WatchType.RedmiWatch2: return 320;
                case WatchType.RedmiWatch3: return 390;
                case WatchType.Band7Pro: return 280;
                case WatchType.Gen1:
                default: return 454;
            }
        }
    }
}

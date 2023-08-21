using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnpackMiColorFace.Helpers;
using XiaomiWatch.Common;
using XiaomiWatch.Common.FaceFile;

namespace UnpackMiColorFace.Decompiler
{
    class FaceV1Decompiler : IFaceDecompiler
    {
        FilenameHelper filenameHelper;

        public FaceV1Decompiler(FilenameHelper filename)
        {
            filenameHelper = filename;
        }

        public void Process(WatchType watchType, byte[] data)
        {
            var dir = Directory.CreateDirectory(filenameHelper.NameNoExt);
            var path = dir.FullName + "\\";

            dir = Directory.CreateDirectory(filenameHelper.NameNoExt + "\\images");
            var pathImages = dir.FullName + "\\";

            uint offset = 0x14A;
            uint blockCount = data.GetDWord(offset);

            offset += 4;
            int found = 0;

            for (int i = 0; i < blockCount; i++)
            {
                uint blockType = data.GetDWord(offset);
                if (blockType == 3)
                {
                    found = found | 0x01;
                    ExtractImages(data, offset, pathImages);
                }
                else if (blockType == 0)
                {
                    found = found | 0x02;
                    BuildFaceFile(data, offset, path + filenameHelper.NameNoExt);
                }

                offset += 0x10;
            }

            if (found != 0x03)
                throw new MissingMemberException();
        }

        private void ExtractImages(byte[] data, uint offset, string path)
        {
            offset = data.GetDWord(offset + 0x0C);

            uint size = 0;
            size = data.GetDWord(offset);

            uint imgBase = offset;
            offset += 4;

            for (int i = 0; i < size; i++)
            {
                try
                {
                    uint idx = data.GetDWord(offset);

                    uint dataLen = data.GetDWord(offset + 0x08);
                    uint dataOfs = data.GetDWord(offset + 0x0C);

                    byte[] bin = data.GetByteArray(imgBase + dataOfs, dataLen);
                    //File.WriteAllBytes(path + $"img_{idx:D4}.bin", bin);

                    uint width = bin.GetWord(0x09);
                    uint height = bin.GetWord(0x0B);

                    int type = (int)(bin.GetWord(0x05) / width);

                    byte[] clut = null;
                    byte[] pxls = bin.GetByteArray(0x15, (uint)bin.Length - 0x15);

                    if (type == 1)
                    {
                        uint pxlsLen = 0x15 + (width * height);
                        clut = bin.GetByteArray(pxlsLen + 4, (uint)bin.Length - pxlsLen - 4);
                        pxls = bin.GetByteArray(0x15, pxlsLen - 0x15);
                    }

                    byte[] bmp = BmpHelper.ConvertToBmpGTR(pxls, (int)width, (int)height, type, clut);

                    string bmpFile = path + $"img_{idx:D4}.bmp";
                    string pngFile = path + $"img_{idx:D4}.png";
                    File.WriteAllBytes(bmpFile, bmp);

                    using (var magik = new MagickImage())
                    {
                        magik.Read(bmpFile);
                        magik.ColorType = ColorType.TrueColorAlpha;
                        magik.Transparent(MagickColor.FromRgba(0, 0, 0, 0));
                        magik.Format = MagickFormat.Png32;
                        magik.Write(pngFile);
                    }

                    File.Delete(bmpFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("image processing err: " + ex);
                }

                offset += 0x10;
            };
        }

        private void BuildFaceFile(byte[] data, uint offset, string facefile)
        {
            string title = data.GetUnicodeString(0x1A);
            uint previewIndex = data.GetDWord(0x46);

            offset = data.GetDWord(offset + 0x0C);

            uint size = 0;
            size = data.GetDWord(offset);

            uint imgBase = offset;
            offset += 4;

            FaceProject face = new FaceProject();
            face.DeviceType = 1;
            face.Screen.Title = title;
            face.Screen.Bitmap = $"img_{previewIndex:D4}.png";

            for (int i = 0; i < size; i++)
            {
                uint idx = data.GetDWord(offset);

                uint dataLen = data.GetDWord(offset + 0x08);
                uint dataOfs = data.GetDWord(offset + 0x0C);

                byte[] bin = data.GetByteArray(imgBase + dataOfs, dataLen);
                //File.WriteAllBytes(path + $"img_{idx:D4}.bin", bin);

                uint itemIndex = bin.GetDWord(0x00);
                uint itemType = bin.GetDWord(0x04);

                int x = bin.GetWord(0x08);
                int y = bin.GetWord(0x0C);

                int width = bin.GetWord(0x10);
                int height = bin.GetWord(0x14);

                int alfa = bin.GetByte(0x1C);

                face.Screen.Widgets.Add(WidgetFactory.Get(itemType, bin));

                offset += 0x10;
            };

            string xml = face.Serialize();
            xml = xml.Replace("<FaceProject", "\r\n<FaceProject");
            xml = xml.Replace("<Screen", "\r\n<Screen");
            xml = xml.Replace("</FaceProject", "\r\n</FaceProject");
            xml = xml.Replace("</Screen", "\r\n</Screen");
            xml = xml.Replace("<Widget", "\r\n<Widget");
            File.WriteAllText($"{facefile}.fprj", xml);
        }
    }
}

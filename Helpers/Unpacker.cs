using ImageMagick;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnpackMiColorFace.FaceFile;
using UnpackMiColorFace.Helpers;

namespace UnpackMiColorFace
{
    internal class Unpacker
    {
        internal static void Exec(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException("filename");

            string path = Path.GetDirectoryName(filename);
            string pathImages = string.Empty;
            string name = Path.GetFileName(filename);
            string nameNoExt = Path.GetFileNameWithoutExtension(filename);

            byte[] data = File.ReadAllBytes(filename);

            const uint magic_v1_1 = 0x46616365;
            const uint magic_v1_2 = 0x46696C65;

            const uint magic_v2 = 0x5AA53412;

            int version = 0;
            var watchType = WatchType.Undefined;

            if (data.GetDWord(0, 1) == magic_v1_1 && data.GetDWord(4, 1) == magic_v1_2)
                version = 1;
            if (data.GetDWord(0, 1) == magic_v2)
                version = 2;

            if (version == 0)
                throw new MissingFieldException();

            if (version == 1)
            {
                watchType = WatchType.Gen1;
                var dir = Directory.CreateDirectory(nameNoExt);
                path = dir.FullName + "\\";

                dir = Directory.CreateDirectory(nameNoExt + "\\images");
                pathImages = dir.FullName + "\\";

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
                        ProcessImages(data, offset, pathImages);
                    }
                    else if (blockType == 0)
                    {
                        found = found | 0x02;
                        ProcessFaceFile(data, offset, path + nameNoExt);
                    }

                    offset += 0x10;
                }

                if (found != 0x03)
                    throw new MissingMemberException();
            }
            else if (version == 2)
            {
                watchType = WatchType.Gen2;
                var dir = Directory.CreateDirectory(nameNoExt);
                path = dir.FullName + "\\";

                if (File.Exists(path + "source.bin"))
                    File.Delete(path + "source.bin");

                File.Copy(filename, path + "source.bin");

                uint offsetPreview = 0x20;
                uint offset = 0xA8;

                int count = data.GetByte(0x1C);
                int shiftWords = data.GetByte(0x1D);
                int subVersion = data.GetWord(0x1E);    // S1 Pro subversion or at location 0x04 ??

                //if (subVersion >= 4)  // can be 4 or 6 currently
                //{
                //    watchType = WathType.Gen3;
                //}
                //else
                //{
                watchType = GetWatchType(data, watchType, offsetPreview);
                //}

                ProcessPreview(watchType, data, offsetPreview, path);
                string title = data.GetUTF8String(0x68);

                offset += (uint)(shiftWords * 0x04);

                for (int c = 0; c < count; c++)
                {
                    string imagesFolder = (c == 0
                        ? nameNoExt + @"\images"
                        : ((c == 1)
                            ? nameNoExt + @"\AOD\images"
                            : nameNoExt + $@"\images_{c}")
                        );

                    if (watchType == WatchType.Gen3)
                        imagesFolder = nameNoExt + @"\images";

                    if (Directory.Exists(imagesFolder))
                        dir = new DirectoryInfo(imagesFolder);
                    else
                        dir = Directory.CreateDirectory(imagesFolder);

                    pathImages = dir.FullName + "\\";

                    List<FaceElement> lste = null;
                    List<FaceWidget> lstw = null;
                    List<FaceImage> lsti = null;
                    List<FaceImageList> lstil = null;

                    // get back image + preview
                    uint backImageId = data.GetDWord(offset);
                    uint previewImageOffset = data.GetDWord(offset + 4);
                    offset += 8;

                    for (int i = 0; i < 10; i++)
                    {
                        if (i == 0)
                            lste = ProcessElements(data, offset, path);
                        if (i == 2)
                            lsti = ProcessImageSingle(watchType, data, offset, pathImages);
                        if (i == 3)
                            lstil = ProcessImageList(watchType, data, offset, pathImages);
                        if (i == 7)
                            lstw = ProcessWidgets(data, offset, path);

                        offset += 8;
                    }

                    if (watchType == WatchType.Gen3)
                        lste.Insert(0, new FaceElement(backImageId));

                    bool isAOD = c == 1;

                    string facefile = c > 1 ? $"{nameNoExt}_{c}" : nameNoExt;
                    if (isAOD) facefile = "AOD\\" + nameNoExt;

                    if (watchType == WatchType.Gen3 && isAOD)
                        facefile = $"{nameNoExt}_AOD";

                    var face = BuildFaceFile(title, watchType, lste, lsti, lstil, lstw, path + facefile);
                    BuildPreview(face, watchType, imagesFolder, path + facefile);
                }
            }
        }

        private static WatchType GetWatchType(byte[] data, WatchType watchType, uint offsetPreview)
        {
            uint offsetPreviewImg = data.GetDWord(offsetPreview);
            int width = data.GetWord(offsetPreviewImg + 4);
            int height = data.GetWord(offsetPreviewImg + 6);

            if (width == 280)
                watchType = WatchType.Gen3;
            else if (width == 178 && height == 200)
                watchType = WatchType.RedmiWatch2;
            else if (width == 234 && height == 270)
                watchType = WatchType.RedmiWatch3;
            else if (width == 220 && height == 358)
                watchType = WatchType.Band7Pro;
            else if (width == 110 && height == 208)
                watchType = WatchType.RedmiBandPro;
            return watchType;
        }

        private static void BuildPreview(FaceProject face, WatchType watchType, string imagesFolder, string facefile)
        {
            int index = 0;
            int displayWidth = GetScreenWidth(watchType);
            int displayHeight = GetScreenHeight(watchType);

            using (var preview = new MagickImage(MagickColor.FromRgb(0, 0, 0), displayWidth, displayHeight))
            {
                //preview.BackgroundColor = MagickColor.FromRgb(0, 0, 0);
                //preview.FloodFill(MagickColor.FromRgb(0, 0, 0), 0, 0);
                foreach (var widget in face.Screen.Widgets)
                {
                    switch (widget.Shape)
                    {
                        case 30: // image
                            var widgetImage = widget as FaceWidgetImage;
                            string pathImage = $"{imagesFolder}\\{widgetImage.Bitmap}";
                            using (var overlay = new MagickImage())
                            {
                                overlay.Read(pathImage);
                                preview.Composite(overlay, widget.X, widget.Y, CompositeOperator.Over);
                            }
                            break;
                        case 43: // CircularGauge
                            var widgetCirc = widget as FaceWidgetCircularGauge;                            
                            if (!string.IsNullOrEmpty(widgetCirc.PointerImage))
                            {
                                string pathCirc = $"{imagesFolder}\\{widgetCirc.PointerImage}";
                                ApplyCircleImage(preview, widgetCirc, pathCirc, 150);
                            }
                            break;
                        case 31: // imageList
                            var wImgList = widget as FaceWidgetImageList;
                            var imgList = wImgList.BitmapList.Split('|');
                            string digitIndex = GetStringSource(wImgList.DataSrcIndex);
                            int digitInt = int.Parse(digitIndex);
                            if (digitInt > imgList.Length)
                                digitInt = 1;
                            var pathFirst = imgList[digitInt].Split(':')[1];
                            string pathList = $"{imagesFolder}\\{pathFirst}";
                            using (var overlay = new MagickImage())
                            {
                                overlay.Read(pathList);
                                preview.Composite(overlay, widget.X, widget.Y, CompositeOperator.Over);
                            }
                            break;
                        case 32: // image
                            var widgetNum = widget as FaceWidgetDigitalNum;
                            using (var overlay = new MagickImage())
                            {
                                int width = widgetNum.Width;
                                int spacing = widgetNum.Spacing;

                                string digit = GetStringSource(widgetNum.DataSrcValue);
                                //string digit = "12000000";
                                int posX = widget.X;
                                int posY = widget.Y;
                                int maxLen = widgetNum.Digits > digit.Length ? digit.Length : widgetNum.Digits;

                                if (widgetNum.Alignment == 0    // center
                                    && (watchType == WatchType.Gen2
                                        || watchType == WatchType.Gen3
                                        || watchType == WatchType.RedmiWatch2
                                        || watchType == WatchType.Band7Pro))
                                {
                                    posX -= (((width + spacing) * maxLen) / 2);
                                }

                                string[] bitmaps = widgetNum.BitmapList.Split('|');

                                for (int i = 0; i < maxLen; i++)
                                {
                                    string pathNum = $"{imagesFolder}\\{bitmaps[int.Parse(digit[i].ToString())]}";
                                    overlay.Read(pathNum);
                                    preview.Composite(overlay, posX, posY, CompositeOperator.Over);
                                    posX += width + spacing;
                                }
                            }
                            break;
                        case 27: // analog clock
                            var clock = widget as FaceWidgetAnalogClock;
                            if (!string.IsNullOrEmpty(clock.HourHandImage))
                            {
                                string pathHand = $"{imagesFolder}\\{clock.HourHandImage}";
                                ApplyClockImage(preview, clock, clock.HourImageRotateX, clock.HourImageRotateY, pathHand, -60);
                            }
                            if (!string.IsNullOrEmpty(clock.MinuteHandImage))
                            {
                                string pathHand = $"{imagesFolder}\\{clock.MinuteHandImage}";
                                ApplyClockImage(preview, clock, clock.MinuteImageRotateX, clock.MinuteImageRotateY, pathHand, 50);
                            }
                            if (!string.IsNullOrEmpty(clock.SecondHandImage))
                            {
                                string pathHand = $"{imagesFolder}\\{clock.SecondHandImage}";
                                ApplyClockImage(preview, clock, clock.SecondImageRotateX, clock.SecondImageRotateY, pathHand, 120);
                            }
                            break;
                        default:
                            break;
                    }
                    index++;
                }

                if (watchType == WatchType.Gen1
                    || watchType == WatchType.Gen2
                    || watchType == WatchType.Gen3)
                {
                    // cut image by circle
                    preview.Alpha(AlphaOption.Set);
                    using (var copy = preview.Clone())
                    {
                        copy.Distort(DistortMethod.DePolar, 0);
                        copy.VirtualPixelMethod = VirtualPixelMethod.HorizontalTile;
                        copy.BackgroundColor = MagickColors.None;
                        copy.Distort(DistortMethod.Polar, 0);

                        preview.Compose = CompositeOperator.DstIn;
                        preview.Composite(copy, CompositeOperator.CopyAlpha);
                    }
                }
                
                preview.Write($"{facefile}_preview.png");                
            }
        }

        private static void ApplyCircleImage(MagickImage preview, FaceWidgetCircularGauge gauge, string pathImage, int angle)
        {
            using (var overlay = new MagickImage(MagickColor.FromRgba(0, 0, 0, 0), gauge.Width, gauge.Height))
            {
                int posX = ((gauge.Width - gauge.X) / 2) - gauge.PointerRotateX;
                int posY = ((gauge.Height - gauge.Y) / 2) - gauge.PointerRotateY;

                var img = new MagickImage(pathImage);
                overlay.Composite(img, posX, posY, CompositeOperator.Over);
                overlay.Distort(DistortMethod.ScaleRotateTranslate, gauge.Width / 2, gauge.Height / 2, 1, angle);
                preview.Composite(overlay, 0, 0, CompositeOperator.Over);
            }
        }

        private static string GetStringSource(string dataSrcValue)
        {
            switch (dataSrcValue)
            {
                case FaceItemDataSrc.Month: return "10";
                case FaceItemDataSrc.Day: return "21";
                case FaceItemDataSrc.Weekday: return "3";

                case FaceItemDataSrc.Hour: return "12";
                case FaceItemDataSrc.HourHigh: return "1";
                case FaceItemDataSrc.HourLow: return "2";
                case FaceItemDataSrc.Minute: return "45";
                case FaceItemDataSrc.MinuteHigh: return "4";
                case FaceItemDataSrc.MinuteLow: return "5";
                case FaceItemDataSrc.Second: return "28";
                case FaceItemDataSrc.SecondHigh: return "2";
                case FaceItemDataSrc.SecondLow: return "8";

                case FaceItemDataSrc.Steps: return "15652";
                case FaceItemDataSrc.Calories: return "12175";
                case FaceItemDataSrc.Hrm: return "174";

                case FaceItemDataSrc.ActivityCount: return "10";
                case FaceItemDataSrc.Stress: return "42";
                case FaceItemDataSrc.Sleep: return "60";

                case FaceItemDataSrc.Battery: return "100";
                case FaceItemDataSrc.WeatherTemp: return "23";
                case FaceItemDataSrc.WeatherType: return "1";
                case FaceItemDataSrc.WeatherAir: return "2";

                default: return "120000";
            }
        }

        private static void ApplyClockImage(MagickImage preview, FaceWidgetAnalogClock clock, int rotateX, int rotateY, string pathImage, int angle)
        {
            using (var overlay = new MagickImage(MagickColor.FromRgba(0, 0, 0, 0), clock.Width, clock.Height))
            {
                int posX = ((clock.Width - clock.X) / 2) - rotateX;
                int posY = ((clock.Height - clock.Y) / 2) - rotateY;

                var img = new MagickImage(pathImage);
                overlay.Composite(img, posX, posY, CompositeOperator.Over);
                overlay.Distort(DistortMethod.ScaleRotateTranslate, clock.Width / 2, clock.Height / 2, 1, angle);
                preview.Composite(overlay, 0, 0, CompositeOperator.Over);
            }
        }

        private static FaceProject BuildFaceFile(string title, WatchType watchType,
                List<FaceElement> lste, List<FaceImage> lsti,
                List<FaceImageList> lstil, List<FaceWidget> lstw, string facefile)
        {
            FaceProject face = new FaceProject();
            face.DeviceType = (int)watchType;
            face.Screen.Title = title;
            face.Screen.Bitmap = "preview.png";

            int idx = 0;
            foreach (var e in lste)
            {
                if (e.TargetId >> 24 == 02)
                {
                    var img = lsti.Find(c => c.Id == e.TargetId);

                    face.Screen.Widgets.Add(new FaceWidgetImage()
                    {
                        Shape = 30,
                        Name = $"image_{idx:D2}",
                        X = e.PosX,
                        Y = e.PosY,
                        Width = img.Width,
                        Height = img.Height,
                        Alpha = 0xFF,
                        Bitmap = img.Name,
                    });
                }
                else if (e.TargetId >> 24 == 07)
                {
                    var wdgt = lstw.Find(c => c.Id == e.TargetId);
                    var imgl = lstil.Find(c => c.Id == wdgt.TargetId);
                    var imgi = lsti.Find(c => c.Id == wdgt.TargetId);
                    if (imgl == null && imgi == null) continue;

                    if (wdgt.TypeId == 0x01)
                    {
                        if (imgl.NameList.Count() == 10)
                        {
                            var _list = imgl.NameList.ToList();
                            _list.Add(imgl.NameList[0]);
                            imgl.NameList = _list.ToArray();
                        }

                        var digits = wdgt.Digits;

                        string digit = GetStringSource($"{wdgt.Shape:X2}{wdgt.DataSrcDisplay:X2}");
                        int maxLen = digits > digit.Length ? digit.Length : digits;

                        int posX = e.PosX;
                        int width = imgl.Width;
                        int spacing = 0;
                        if (wdgt.Align == 2    // center
                            && (watchType == WatchType.Gen2
                                || watchType == WatchType.Gen3
                                || watchType == WatchType.RedmiBandPro
                                || watchType == WatchType.RedmiWatch2
                                || watchType == WatchType.Band7Pro))
                        {
                            posX -= ((width + spacing) * maxLen) / 2;
                        }
                        else if (wdgt.Align == 0 // right
                            && (watchType == WatchType.Gen2
                                || watchType == WatchType.Gen3
                                || watchType == WatchType.RedmiBandPro
                                || watchType == WatchType.RedmiWatch2
                                || watchType == WatchType.Band7Pro))

                        {
                            posX -= ((width + spacing) * maxLen);
                        }

                        face.Screen.Widgets.Add(new FaceWidgetDigitalNum()
                        {
                            Shape = 32,
                            Name = $"widget_{idx:D2}",
                            X = posX,
                            Y = e.PosY,
                            Width = imgl.Width,
                            Height = imgl.Height,
                            Digits = digits,
                            Alignment = GetTextAlignment(wdgt.Align),
                            Alpha = 0xFF,
                            DataSrcValue = $"{wdgt.Shape:X2}{wdgt.DataSrcDisplay:X2}",
                            BitmapList = string.Join("|", imgl.NameList),
                        });
                    }
                    else if (wdgt.TypeId == 0x02) {

                        bool hasCustomValues = wdgt.RawData.GetWord(0x0c) == 0x200;
                        string bmpList = "";
                        for (int x = 0; x < imgl.NameList.Length; x++)
                        {
                            int value = 0;
                            try
                            {
                                if (hasCustomValues)
                                    value = (int)wdgt.RawData.GetDWord(0x10 + (x * 4));
                            }
                            catch {}

                            if (bmpList.Length > 0)
                                bmpList += "|";
                            bmpList += $"({(hasCustomValues ? value : x)}):{imgl.NameList[x]}";
                        }
                        face.Screen.Widgets.Add(new FaceWidgetImageList()
                        {
                            Shape = 31,
                            Name = $"widget_{idx:D2}",
                            X = e.PosX,
                            Y = e.PosY,
                            Width = imgl.Width,
                            Height = imgl.Height,
                            Alpha = 0xFF,
                            DataSrcIndex = $"{wdgt.Shape:X2}{wdgt.DataSrcDisplay:X2}",
                            BitmapList = bmpList,
                        });
                    }
                    else if (wdgt.TypeId == 0x03)
                    {
                        if (wdgt.DataSrcDisplay == 0x11)
                        {
                            if (!face.Screen.Widgets.Any(w => w.Shape == 27))
                            {
                                face.Screen.Widgets.Add(new FaceWidgetAnalogClock()
                                {
                                    Shape = 27,
                                    Name = $"analogClock_{idx:D2}",
                                    X = 0,
                                    Y = 0,
                                    Width = GetScreenWidth(watchType), 
                                    Height = GetScreenHeight(watchType),
                                    Alpha = 0xFF,                                    
                                });
                            }

                            FaceWidgetAnalogClock clock = face.Screen.Widgets.First(w => w.Shape == 27) as FaceWidgetAnalogClock;

                            if (wdgt.Shape == 0x08)
                            {
                                clock.HourHandImage = imgi.Name;
                                clock.HourImageRotateX = wdgt.X;
                                clock.HourImageRotateY = wdgt.Y;
                            }
                            else if (wdgt.Shape == 0x10)
                            {
                                clock.MinuteHandImage = imgi.Name;
                                clock.MinuteImageRotateX = wdgt.X;
                                clock.MinuteImageRotateY = wdgt.Y;
                            }
                            else if (wdgt.Shape == 0x18)
                            {
                                clock.SecondHandImage = imgi.Name;
                                clock.SecondImageRotateX = wdgt.X;
                                clock.SecondImageRotateY = wdgt.Y;
                            }

                        }
                        else
                        {
                            face.Screen.Widgets.Add(new FaceWidgetCircularGauge()
                            {
                                Shape = 43,
                                Name = $"widget_{idx:D2}",
                                X = e.PosX,
                                Y = e.PosY,
                                Width = imgi.Width,
                                Height = imgi.Height,
                                Alpha = 0xFF,
                                DataSrcVal  = $"{wdgt.Shape:X2}{wdgt.DataSrcDisplay:X2}",
                                PointerImage = imgi.Name,
                            });
                        }
                    }
                }

                idx++;
            }            

            string xml = face.Serialize();
            xml = xml.Replace("<FaceProject", "\r\n<FaceProject");
            xml = xml.Replace("<Screen", "\r\n<Screen");
            xml = xml.Replace("</FaceProject", "\r\n</FaceProject");
            xml = xml.Replace("</Screen", "\r\n</Screen");
            xml = xml.Replace("<Widget", "\r\n<Widget");
            Debug.WriteLine(xml);
            File.WriteAllText($"{facefile}.fprj", xml);

            return face;
        }

        private static int GetTextAlignment(byte align)
        {
            if (align == 2) return 1;
            if (align == 1) return 2;
            return 0;
        }

        private static int GetScreenHeight(WatchType watchType)
        {
            switch (watchType)
            {
                case WatchType.Gen2: return 466;
                case WatchType.Gen3: return 480;
                case WatchType.RedmiWatch2: return 360;
                case WatchType.RedmiWatch3: return 450;
                case WatchType.Band7Pro: return 456;
                case WatchType.RedmiBandPro: return 368;
                case WatchType.Gen1:
                default: return 454;
            }
        }

        private static int GetScreenWidth(WatchType watchType)
        {
            switch (watchType)
            {
                case WatchType.Gen2: return 466;
                case WatchType.Gen3: return 480;
                case WatchType.RedmiWatch2: return 320;
                case WatchType.RedmiWatch3: return 390;
                case WatchType.Band7Pro: return 280;
                case WatchType.RedmiBandPro: return 194;
                case WatchType.Gen1:
                default: return 454;
            }
        }

        private static List<FaceElement> ProcessElements(byte[] data, uint offset, string path)
        {
            uint blockCount = data.GetDWord(offset);
            uint blockOffset = data.GetDWord(offset + 4);

            var lst = new List<FaceElement>();
            offset = blockOffset;

            for (int i = 0; i < blockCount; i++)
            {
                try
                {
                    uint idx = data.GetWord(offset);

                    uint dataOfs = data.GetDWord(offset + 0x08);
                    uint dataLen = data.GetDWord(offset + 0x0C);

                    byte[] bin = data.GetByteArray(dataOfs, dataLen);
                    //File.WriteAllBytes(path + $"img_{idx:D4}.bin", bin);

                    lst.Add(new FaceElement()
                    {
                        TargetId = bin.GetDWord(0),
                        PosX = bin.GetWord(0x04),
                        PosY = bin.GetWord(0x06)
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("image processing err: " + ex);
                }

                offset += 0x10;
            };

            return lst;
        }

        private static List<FaceWidget> ProcessWidgets(byte[] data, uint offset, string path)
        {
            uint blockCount = data.GetDWord(offset);
            uint blockOffset = data.GetDWord(offset + 4);

            var lst = new List<FaceWidget>();
            offset = blockOffset;

            for (int i = 0; i < blockCount; i++)
            {
                try
                {
                    uint idx = data.GetWord(offset);
                    uint id = data.GetDWord(offset);

                    uint dataOfs = data.GetDWord(offset + 0x08);
                    uint dataLen = data.GetDWord(offset + 0x0C);

                    byte[] bin = data.GetByteArray(dataOfs, dataLen);
                    //File.WriteAllBytes(path + $"img_{idx:D4}.bin", bin);

                    var digits = bin[2];
                    if (bin.GetWord(0) == 0x1109
                        || bin.GetWord(0) == 0x110A
                        || bin.GetWord(0) == 0x1111
                        || bin.GetWord(0) == 0x1112
                        || bin.GetWord(0) == 0x1119
                        || bin.GetWord(0) == 0x111A)
                    {
                        digits = 1;
                    }

                    lst.Add(new FaceWidget()
                    {
                        RawData = bin,
                        Shape = bin[0],
                        DataSrcDisplay = bin[1],
                        X = bin.Length >= 0x20 ? bin.GetWord(0x14) : 0,
                        Y = bin.Length >= 0x20 ? bin.GetWord(0x16) : 0,
                        Width = 0,
                        Height = 0,
                        Id = id,
                        Digits = digits,
                        Align = (byte)(bin[3] & 0x03),
                        TypeId = bin[3] >> 4,
                        TargetId = bin.GetDWord(0x08)
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("image processing err: " + ex);
                }

                offset += 0x10;
            };

            return lst;
        }

        private static void ProcessFaceFile(byte[] data, uint offset, string facefile)
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
            Debug.WriteLine(xml);
            File.WriteAllText($"{facefile}.fprj", xml);
        }

        private static void ProcessImages(byte[] data, uint offset, string path)
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

                    if (type == 1) {
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

        private static void ProcessPreview(WatchType watchType, byte[] data, uint offset, string path)
        {        
            uint dataOfs = data.GetDWord(offset);
            uint dataLen = data.GetDWord(dataOfs + 8);

            byte[] bin = data.GetByteArray(dataOfs, dataLen + 0x0C);
            //File.WriteAllBytes(path + $"img_{idx:D4}.bin", bin);

            uint width = bin.GetWord(0x04);
            uint height = bin.GetWord(0x06);

            int rle = bin[0];
            int type = bin[1];

            if (bin.GetDWord(0) == 0)
            {
                type = 4;
            }
            else if (watchType == WatchType.RedmiWatch2
                    || watchType == WatchType.RedmiBandPro
                    || watchType == WatchType.Band7Pro)
            {
                type = bin[0] & 0xF;
                if (type == 4) type = 2;
            }

            uint magic = 0;
            byte[] clut = null;
            byte[] pxls = bin.GetByteArray(0x0C, (uint)bin.Length - 0x0C);

            magic = bin.GetDWord(0x0C);

            if (magic != 0x5AA521E0)
            {
                if (type == 1)
                {
                    uint pxlsLen = 0x15 + (width * height);
                    clut = bin.GetByteArray(pxlsLen + 4, (uint)bin.Length - pxlsLen - 4);
                    pxls = bin.GetByteArray(0x0C, pxlsLen - 0x0C);
                }
            }
            else
            {                
                byte[] cpr = bin.GetByteArray(0xCu + 8, dataLen - 8);
                type = bin[0x10] & 0x0F;
                int decLen = (int)bin.GetDWord(0x10) >> 4;

                if (rle == 0x10)
                    pxls = BmpHelper.UncompressRLEv20(cpr, decLen);
                else
                {
                    if (watchType == WatchType.RedmiWatch2 || watchType == WatchType.Band7Pro)
                        pxls = BmpHelper.UncompressRLEv11(cpr, decLen, (byte)type);
                    else
                        pxls = BmpHelper.UncompressRLEv10(cpr, decLen, (byte)type);
                }

                string binFile = path + "preview_cpr.bin";
                File.WriteAllBytes(binFile, cpr);

                binFile = path + "preview_dec.bin";
                File.WriteAllBytes(binFile, pxls);
            }

            byte[] bmp = null;
                
            if (magic == 0x5AA521E0)
                bmp = BmpHelper.ConvertToBmpGTRv2(pxls, (int)width, (int)height, type);
            else
                bmp = BmpHelper.ConvertToBmpGTR(pxls, (int)width, (int)height, type, clut);

            string bmpFile = path + "preview.bmp";
            string pngFile = path + "preview.png";
            string pngFileCopy = path + "images\\preview.png";

            if (File.Exists(pngFile))
                File.Delete(pngFile);

            File.WriteAllBytes(bmpFile, bmp);

            using (var magik = new MagickImage())
            {
                magik.Read(bmpFile);
                magik.ColorType = ColorType.TrueColorAlpha;
                magik.Transparent(MagickColor.FromRgba(0, 0, 0, 0));
                magik.Format = MagickFormat.Png32;
                magik.Write(pngFile);
            }

            //File.Delete(bmpFile);

            if (!Directory.Exists(Path.GetDirectoryName(pngFileCopy)))
                Directory.CreateDirectory(Path.GetDirectoryName(pngFileCopy));

            if (File.Exists(pngFileCopy))
                File.Delete(pngFileCopy);

            File.Copy(pngFile, pngFileCopy);
        }

        private static List<FaceImage> ProcessImageSingle(WatchType watchType, byte[] data, uint offset, string path)
        {
            uint version = data.GetDWord(0x10);

            uint blockCount = data.GetDWord(offset);
            uint blockOffset = data.GetDWord(offset + 4);

            var lst = new List<FaceImage>();

            offset = blockOffset;
            for (int i = 0; i < blockCount; i++)
            {
                try
                {
                    uint idx = data.GetWord(offset);
                    uint id = data.GetDWord(offset);

                    uint dataOfs = data.GetDWord(offset + 0x08);
                    uint dataLen = data.GetDWord(offset + 0x0C);

                    byte[] bin = data.GetByteArray(dataOfs, dataLen);
                    //File.WriteAllBytes(path + $"img_{idx:D4}.bin", bin);

                    int width = bin.GetWord(0x04);
                    int height = bin.GetWord(0x06);

                    int rle = bin[0];
                    int type = bin[1];

                    if (bin.GetDWord(0) == 0) type = 4;

                    if (watchType == WatchType.Gen2 && version == 0x800)
                    {
                        if (bin.GetByte(0) == 0) type = 4;
                        if (bin.GetByte(0) == 3) type = 2;
                    }

                    uint magic = bin.GetDWord(0x0C);
                    byte[] clut = null;
                    byte[] alfa = null;
                    byte[] pxls = bin.GetByteArray(0x0C, (uint)bin.Length - 0x0C);

                    if (watchType == WatchType.RedmiWatch2
                        || watchType == WatchType.RedmiBandPro
                        || watchType == WatchType.Band7Pro)
                    {
                        if ((rle & 0x0F) == 0x04)
                        {
                            type = 2;
                            uint pxlLen = (uint)(width * 2 * height);
                            alfa = pxls.GetByteArray(pxlLen, (uint)(width * height));
                            pxls = pxls.GetByteArray(0, pxlLen);
                        }
                    }

                    //if (watchType == WatchType.Gen2 && version == 0x800)
                    //{
                    //    if ((rle & 0x0F) == 0x03)
                    //    {
                    //        type = 2;
                    //        uint pxlLen = (uint)(width * 2 * height);
                    //        alfa = pxls.GetByteArray(pxlLen, (uint)(width * height));
                    //        pxls = pxls.GetByteArray(0, pxlLen);
                    //    }
                    //}

                    if (magic != 0x5AA521E0)
                    {
                        if (type == 1)
                        {
                            uint pxlsLen = 0x15 + ((uint)width * (uint)height);
                            clut = bin.GetByteArray(pxlsLen + 4, (uint)bin.Length - pxlsLen - 4);
                            pxls = bin.GetByteArray(0x0C, pxlsLen - 0x0C);
                        }
                    }
                    else
                    {
                        // error in data structure size
                        // reloading binary for correct size

                        bin = data.GetByteArray(dataOfs, dataLen + 4);
                        uint cprLen = bin.GetDWord(0x08);
                        byte[] cpr = bin.GetByteArray(0xCu + 8, cprLen - 8);
                        type = bin[0x10] & 0x0F;
                        int decLen = (int)bin.GetDWord(0x10) >> 4;

                        Console.WriteLine($"got compressed image: {dataOfs:X8}, type:{type:X2}");

                        if (rle == 0x10)
                            pxls = BmpHelper.UncompressRLEv20(cpr, decLen);
                        else
                        {
                            if (watchType == WatchType.RedmiWatch2 || watchType == WatchType.Band7Pro)
                                pxls = BmpHelper.UncompressRLEv11(cpr, decLen, (byte)type);
                            else
                                pxls = BmpHelper.UncompressRLEv10(cpr, decLen, (byte)type);
                        }

                        //string binFile = path + $"img_{idx:D4}.bin";
                        //File.WriteAllBytes(binFile, pxls);
                    }

                    byte[] bmp = null;
                    if (magic == 0x5AA521E0)
                        bmp = BmpHelper.ConvertToBmpGTRv2(pxls, (int)width, (int)height, type);
                    else
                        bmp = BmpHelper.ConvertToBmpGTR(pxls, (int)width, (int)height, type, clut);

                    string bmpFile = path + $"img_{idx:D4}_{type}_{clut}.bmp";
                    string pngFile = path + $"img_{idx:D4}.png";
                    File.WriteAllBytes(bmpFile, bmp);

                    using (var magik = new MagickImage())
                    {
                        magik.Read(bmpFile);
                        magik.ColorType = ColorType.TrueColorAlpha;
                        if (magic == 0x5AA521E0 && watchType != WatchType.RedmiWatch3)
                            magik.Transparent(MagickColor.FromRgba(0, 0, 0, 0xFF));
                        else
                            magik.Transparent(MagickColor.FromRgba(0, 0, 0, 0));
                        magik.Format = MagickFormat.Png32;

                        if (type == 2 &&
                                (
                                    (watchType == WatchType.RedmiWatch2 || watchType == WatchType.Band7Pro)
                                    //|| (watchType == WatchType.Gen2 && version == 0x800)
                                )
                           )
                        {
                            int pixelIndex = 0;
                            int rowIndex = 0;
                            foreach (var px in magik.GetPixels())
                            {
                                if (px.Channels == 4)
                                {
                                    if (rowIndex >= width)
                                    {
                                        px.SetChannel(3, 0);
                                    }
                                    else
                                    {
                                        px.SetChannel(3, (ushort)(alfa[pixelIndex++] << 8 | 0xFF));
                                    }

                                    rowIndex++;
                                    if (rowIndex >= magik.Width)
                                        rowIndex = 0;
                                }
                            }
                        }

                        magik.Write(pngFile);
                    }

                    File.Delete(bmpFile);

                    lst.Add(new FaceImage()
                    {
                        Id = id,
                        Width = width,
                        Height = height,
                        Name = Path.GetFileName(pngFile)
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("image processing err: " + ex);
                }

                offset += 0x10;
            };

            return lst;
        }

        private static List<FaceImageList> ProcessImageList(WatchType watchType, byte[] data, uint offset, string path)
        {
            uint blockCount = data.GetDWord(offset);
            uint blockOffset = data.GetDWord(offset + 4);

            var lst = new List<FaceImageList>();

            offset = blockOffset;
            for (int i = 0; i < blockCount; i++)
            {
                try
                {
                    uint idx = data.GetWord(offset);
                    uint id = data.GetDWord(offset);

                    uint dataOfs = data.GetDWord(offset + 0x08);
                    uint dataLen = data.GetDWord(offset + 0x0C);

                    byte[] bin = data.GetByteArray(dataOfs, dataLen);
                    //File.WriteAllBytes(path + $"img_{idx:D4}.bin", bin);

                    int width = bin.GetWord(0x04);
                    int height = bin.GetWord(0x06);

                    int rle = bin[0];
                    int type = bin[2];

                    if (rle == 0 && type == 0) type = 4;

                    uint imgSize = (uint)width * 4 * (uint)height;
                    uint alfSize = 0;

                    if (watchType == WatchType.RedmiWatch2
                        || watchType == WatchType.RedmiBandPro
                        || watchType == WatchType.RedmiWatch3
                        || watchType == WatchType.Band7Pro)
                    {
                        if ((rle & 0x0F) == 0x04)
                        {
                            type = 2;
                            imgSize = (uint)width * 2 * (uint)height;
                            alfSize = (uint)width * (uint)height;
                        }
                        if (watchType == WatchType.RedmiWatch3 && (rle & 0x0F) == 0x03) {
                            type = 2;
                            imgSize = (uint)width * 2 * (uint)height;
                            alfSize = 0;
                        }
                    }

                    uint arrCount = bin[1];
                    uint maxSize = bin.GetDWord(8) + 0x0C;
                    uint magic = bin.GetDWord(0x0C + (4 * arrCount));

                    Console.WriteLine($"got compressed imageList: {dataOfs:X8}, type:{type:X2}");

                    List<string> nameList = new List<string>();

                    for (int j = 0; j < arrCount; j++)
                    {
                        //int type = 4; // (int)(bin.GetWord(0x05) / width);

                        //Console.WriteLine($"image processing: {idx}/{id:X} {dataOfs:X}/{dataLen:X} {width}x{height} {j}/{arrCount}");

                        //if (dataOfs == 0x3CEA6C) Debugger.Break();

                        uint startOffset = 0x0C + (uint)(j * (imgSize + alfSize));
                        uint leftSize = (maxSize - startOffset) >= imgSize
                            ? imgSize
                            : maxSize - startOffset;

                        byte[] pxls = null;
                        byte[] pxlsFull = null;
                        byte[] alfa = null;

                        if (magic == 0x5AA521E0)
                        {
                            uint arrOffset = 0x0C + (uint)(4 * arrCount);
                            startOffset = arrOffset;
                            for (int x = 0; x < j; x++)
                                startOffset += bin.GetDWord(0x0C + (uint)(x * 4));

                            leftSize = (maxSize - startOffset) >= imgSize
                                ? imgSize
                                : maxSize - startOffset;

                            //bin = data.GetByteArray(dataOfs, dataLen + 4);
                            uint cprLen = bin.GetDWord(0x0C + (uint)(j * 4));
                            byte[] cpr = bin.GetByteArray(startOffset + 8, cprLen - 8);
                            type = bin[startOffset + 4] & 0x0F;
                            int decLen = (int)bin.GetDWord(startOffset + 4) >> 4;

                            if (rle == 0x10)
                                pxls = BmpHelper.UncompressRLEv20(cpr, decLen);
                            else
                            {
                                if (watchType == WatchType.RedmiWatch2 || watchType == WatchType.Band7Pro)
                                    pxls = BmpHelper.UncompressRLEv11(cpr, decLen);
                                else
                                    pxls = BmpHelper.UncompressRLEv10(cpr, decLen);
                            }

                            pxlsFull = pxls;
                            //string binFile = path + $"img_{idx:D4}.bin";
                            //File.WriteAllBytes(binFile, pxls);
                        }
                        else
                        {
                            pxls = bin.GetByteArray(startOffset, leftSize);
                            pxlsFull = new byte[imgSize];
                            Array.Copy(pxls, pxlsFull, pxls.Length);

                            if (type == 2 &&
                                (watchType == WatchType.RedmiWatch2 || watchType == WatchType.Band7Pro))
                            {
                                alfa = new byte[alfSize];
                                Array.Copy(bin.GetByteArray(startOffset + imgSize, alfSize), alfa, alfSize);
                            }
                        }

                        byte[] bmp = null;
                        if (magic == 0x5AA521E0)
                            bmp = BmpHelper.ConvertToBmpGTRv2(pxls, width, height, type);
                        else
                            bmp = BmpHelper.ConvertToBmpGTR(pxlsFull, width, height, type, null);

                        string bmpFile = path + $"img_arr_{idx:D4}_{j:D2}.bmp";
                        string pngFile = path + $"img_arr_{idx:D4}_{j:D2}.png";
                        File.WriteAllBytes(bmpFile, bmp);

                        nameList.Add(Path.GetFileName(pngFile));

                        using (var magik = new MagickImage())
                        {
                            magik.Read(bmpFile);
                            magik.ColorType = ColorType.TrueColorAlpha;
                            if (magic == 0x5AA521E0)
                                magik.Transparent(MagickColor.FromRgba(0, 0, 0, 0xFF));
                            else
                                magik.Transparent(MagickColor.FromRgba(0, 0, 0, 0));
                            magik.Format = MagickFormat.Png32;

                            if (type == 2 &&
                                (watchType == WatchType.RedmiWatch2
                                || watchType == WatchType.Band7Pro))
                            {
                                int pixelIndex = 0;
                                int rowIndex = 0;
                                foreach (var px in magik.GetPixels())
                                {
                                    if (px.Channels == 4)
                                    {
                                        if (rowIndex >= width)
                                        {
                                            px.SetChannel(3, 0);
                                        }
                                        else
                                        {
                                            px.SetChannel(3, (ushort)(alfa[pixelIndex++] << 8 | 0xFF));
                                        }

                                        rowIndex++;
                                        if (rowIndex >= magik.Width)
                                            rowIndex = 0;
                                    }
                                }
                            }

                            magik.Write(pngFile);
                        }

                        File.Delete(bmpFile);
                    }

                    lst.Add(new FaceImageList()
                    {
                        Id = id,
                        Width = width,
                        Height = height,
                        NameList = nameList.ToArray()
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("image processing err: " + ex);
                }

                offset += 0x10;
            };

            return lst;
        }
    }
}
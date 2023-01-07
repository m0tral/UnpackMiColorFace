using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnpackMiColorFace.Helpers
{
    public static class BinaryExtender
    {
        public static string ExtendTo(this int val, int cnt)
        {
            string s = val.ToString();

            for (int i = s.Length; i < cnt; i++)
                s = " " + s;

            return s;
        }

        public static bool IsArabic(this string val)
        {
            if (string.IsNullOrEmpty(val)) return false;

            for (int i = 0; i < val.Length; i++)
            {
                if ((val[i] & 0xFF00) == 0xFE00)
                    return true;
            }

            return false;
        }

        public static int GetDWordAligned(this int v)
        {
            int c = v % 4;
            int w = v / 4 * 4;

            return c > 0 ? w + 4 : w; 
        }

        public static uint GetDWordAligned(this uint v)
        {
            uint c = v % 4;
            uint w = v / 4 * 4;

            return c > 0 ? w + 4 : w;
        }

        public static byte[] ReverseDWord(this byte[] data, int cprType)
        {
            byte a, b, c, d;

            if (cprType != 1)
                return data;

            for (int i = 0; i < data.Length-4; i+=4)
            {
                a = data[i];
                b = data[i+1];
                c = data[i+2];
                d = data[i+3];

                //if (d == 0xFF) Debugger.Break();

                data[i] = c;
                data[i + 1] = b;
                data[i + 2] = a;
                data[i + 3] = d;
            }

            return data;
        }

        public static byte[] To32Argb(this byte[] data, int cprType)
        {
            byte a, b, c, d;
            byte[] ndata = data;

            if (cprType == 0)
            {
                int newLen = (data.Length / 3) * 4;
                ndata = new byte[newLen];

                int x = 0;
                for (int i = 0; i < newLen - 4; i += 4)
                {
                    a = data[x];
                    b = data[x + 1];
                    c = data[x + 2];
                    d = 0xFF;

                    ndata[i] = c;
                    ndata[i + 1] = b;
                    ndata[i + 2] = a;
                    ndata[i + 3] = d;

                    x += 3;
                }
            }
            else if (cprType == 3)
            {
                int newLen = data.Length * 4;
                ndata = new byte[newLen];

                int x = 0;
                for (int i = 0; i < newLen - 4; i += 4)
                {
                    ndata[i] = data[x];
                    ndata[i + 1] = data[x];
                    ndata[i + 2] = data[x];
                    ndata[i + 3] = 0xFF;

                    x++;
                }
            }

            return ndata;
        }

        public static byte[] AlignByWord(this byte[] data)
        {
            if (data == null) return null;

            int len = data.Length % 2;
            if (len > 0)
            {
                len = data.Length + 1;
                byte[] ndata = new byte[len];
                Array.Copy(data, ndata, len - 1);

                data = ndata;
            }

            return data;
        }

        public static byte[] AlignByDWord(this byte[] data)
        {
            if (data == null) return null;

            int len = data.Length % 4;
            if (len > 0)
            {
                len = ((data.Length / 4) * 4) + 4;
                byte[] ndata = new byte[len];
                Array.Copy(data, ndata, data.Length);

                data = ndata;
            }

            return data;
        }

        public static byte[] AppendZero(this byte[] data)
        {
            if (data == null) return null;

            Array.Resize(ref data, data.Length + 1);

            return data;
        }

        public static byte[] AppendZero(this byte[] data, int len)
        {
            if (data == null) return null;

            Array.Resize(ref data, data.Length + len);

            return data;
        }

        public static uint GetDWord(this byte[] data, int offset = 0, uint bigEndian = 0)
        {
            return data.GetDWord((uint)offset, bigEndian);
        }

        public static uint GetDWord(this byte[] data, uint offset = 0, uint bigEndian = 0)
        {
            byte[] size = new byte[4];
            Array.Copy(data, offset, size, 0, size.Length);
            if (bigEndian == 1)
                size = size.Reverse().ToArray();

            return BitConverter.ToUInt32(size, 0);
        }

        public static void SetDWord(this byte[] data, uint offset = 0, uint val = 0, uint bigEndian = 0)
        {
            byte[] size = BitConverter.GetBytes(val);

            if (bigEndian == 1)
                size = size.Reverse().ToArray();

            Array.Copy(size, 0, data, offset, size.Length);
        }

        public static string GetAsciiString(this byte[] data, uint offset = 0)
        {
            uint len = 0;
            do
            {
                int ending = data[offset + len];
                if (ending == 0)
                    break;
                len += 1;
            } while (true);

            byte[] strdata = new byte[len];
            Array.Copy(data, offset, strdata, 0, len);

            return Encoding.ASCII.GetString(strdata);
        }

        public static string GetUnicodeString(this byte[] data, uint offset = 0, uint bigEndian = 0)
        {
            uint len = 0;
            do
            {
                int ending = data.GetWord(offset + len, bigEndian);
                if (ending == 0)
                    break;
                len += 2;
            } while (true);

            byte[] strdata = new byte[len];
            Array.Copy(data, offset, strdata, 0, len);

            if (bigEndian == 1)
            {
                for (int i = 0; i < len; i += 2)
                {
                    byte tmp = strdata[i + 1];
                    strdata[i + 1] = strdata[i];
                    strdata[i] = tmp;
                }
            }

            return UnicodeEncoding.Unicode.GetString(strdata);
        }

        public static int SetUnicodeString(this byte[] data, uint offset = 0, string str = "", uint bigEndian = 0)
        {
            if (string.IsNullOrEmpty(str))
                return 0;

            byte[] strdata = UnicodeEncoding.Unicode.GetBytes(str).AppendZero().AlignByDWord();

            int len = strdata.Length;

            if (bigEndian == 1)
            {
                for (int i = 0; i < len; i += 2)
                {
                    byte tmp = strdata[i + 1];
                    strdata[i + 1] = strdata[i];
                    strdata[i] = tmp;
                }
            }

            Array.Copy(strdata, 0, data, offset, len);

            return len;
        }

        public static int SetUnicodeStringNoAlign(this byte[] data, uint offset = 0, string str = "", uint bigEndian = 0)
        {
            if (string.IsNullOrEmpty(str))
                return 0;

            byte[] strdata = UnicodeEncoding.Unicode.GetBytes(str);

            int len = strdata.Length;

            if (bigEndian == 1)
            {
                for (int i = 0; i < len; i += 2)
                {
                    byte tmp = strdata[i + 1];
                    strdata[i + 1] = strdata[i];
                    strdata[i] = tmp;
                }
            }

            Array.Copy(strdata, 0, data, offset, len);

            return len;
        }

        public static string GetUTF8String(this byte[] data, uint offset = 0, int strlen = 0, uint bigEndian = 0)
        {
            int len = 0;

            if (strlen > 0)
            {
                len = strlen;
            }
            else
            {
                do
                {
                    int ending = data[offset + len];
                    if (ending == 0)
                        break;
                    len++;
                } while (true);
            }

            byte[] strdata = new byte[len];
            Array.Copy(data, offset, strdata, 0, len);

            if (bigEndian == 1)
            {
                for (int i = 0; i < len; i += 2)
                {
                    byte tmp = strdata[i + 1];
                    strdata[i + 1] = strdata[i];
                    strdata[i] = tmp;
                }
            }

            return UnicodeEncoding.UTF8.GetString(strdata);
        }

        public static byte[] GetUnicodeArray(this byte[] data, uint offset = 0, uint bigEndian = 0)
        {
            uint len = 0;
            do
            {
                int ending = data.GetWord(offset + len, bigEndian);
                if (ending == 0)
                    break;
                len += 2;
            } while (true);

            byte[] strdata = new byte[len];
            Array.Copy(data, offset, strdata, 0, len);

            if (bigEndian == 1)
            {
                for (int i = 0; i < len; i += 2)
                {
                    byte tmp = strdata[i + 1];
                    strdata[i + 1] = strdata[i];
                    strdata[i] = tmp;
                }
            }

            return strdata;
        }

        public static string GetUnicodeMultiString(this byte[] data, uint[] offsetList)
        {
            return string.Join("|", offsetList.Select(o => data.GetUnicodeString(o)));
        }

        public static ushort GetWord(this byte[] data, uint offset = 0, uint bigEndian = 0)
        {
            byte[] size = new byte[2];
            Array.Copy(data, offset, size, 0, size.Length);
            if (bigEndian == 1)
                size = size.Reverse().ToArray();

            return (ushort)((size[1] << 8) | size[0]);
            //return BitConverter.ToUInt16(size, 0);
        }

        public static void SetWord(this byte[] data, uint offset = 0, ushort val = 0, uint bigEndian = 0)
        {
            byte[] size = BitConverter.GetBytes(val);

            if (bigEndian == 1)
                size = size.Reverse().ToArray();

            Array.Copy(size, 0, data, offset, size.Length);
        }
        public static void SetWord(this byte[] data, uint offset = 0, uint val = 0, uint bigEndian = 0)
        {
            data.SetWord(offset, (ushort)val, bigEndian);
        }
        public static void SetWord(this byte[] data, uint offset = 0, int val = 0, uint bigEndian = 0)
        {
            data.SetWord(offset, (ushort)val, bigEndian);
        }

        public static byte GetByte(this byte[] data, uint offset = 0)
        {
            return data[offset];
        }

        public static byte[] GetByteArray(this byte[] data, uint offset = 0, uint len = 1)
        {
            byte[] size = new byte[len];
            Array.Copy(data, offset, size, 0, size.Length);
            return size;
        }

        public static void SetByteArray(this byte[] data, int offset, byte[] src)
        {
            Array.Copy(src, 0, data, offset, src.Length);
        }

        public static uint GetLeftAlignedDWord(this byte[] data)
        {
            int len = data.Length;
            byte[] uintData = new byte[4];
            Array.Copy(data, uintData, len);
            uint v = (uint)(uintData[0] << 24)
                | (uint)(uintData[1] << 16)
                | (uint)(uintData[2] << 8)
                | (uint)(uintData[3]);
            return v;
        }

        public static byte[] GetBytes(this uint data, int length = 4)
        {
            byte[] byteData = BitConverter.GetBytes(data).Reverse().ToArray();

            byte[] res = new byte[length];
            Array.Copy(byteData, res, length);

            return res;
        }

        public static byte[] HexToByteArray(this string hexString, bool rev = true)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] HexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < HexAsBytes.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                HexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            if (rev)
                return HexAsBytes.Reverse().ToArray();

            return HexAsBytes;
        }

        public static uint CountBits(this uint value)
        {
            uint i = value;

            //Console.Write($"count bits: {value:X2}, bits - ");

            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            uint res = (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;

            return res;
        }

        public static uint CountLeadingZeroes(this uint x)
        {
            const uint numIntBits = sizeof(uint) * 8; //compile time constant
            //do the smearing
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            //count the ones
            x -= x >> 1 & 0x55555555;
            x = (x >> 2 & 0x33333333) + (x & 0x33333333);
            x = (x >> 4) + x & 0x0f0f0f0f;
            x += x >> 8;
            x += x >> 16;
            return numIntBits - (x & 0x0000003f); //subtract # of 1s from 32
        }

        public static IEnumerable<String> SplitInParts(this String s, Int32 partLength)
        {
            if (s == null)
                throw new ArgumentNullException("s");
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", "partLength");

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

        public static string ToHex(this byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];

            byte b;

            for (int bx = 0, cx = 0; bx < bytes.Length; ++bx, ++cx)
            {
                b = ((byte)(bytes[bx] >> 4));
                c[cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);

                b = ((byte)(bytes[bx] & 0x0F));
                c[++cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);
            }

            return new string(c);
        }

        public static byte[] HexToBytes(this string str)
        {
            if (str.Length == 0 || str.Length % 2 != 0)
                return new byte[0];

            byte[] buffer = new byte[str.Length / 2];
            char c;
            for (int bx = 0, sx = 0; bx < buffer.Length; ++bx, ++sx)
            {
                // Convert first half of byte
                c = str[sx];
                buffer[bx] = (byte)((c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0')) << 4);

                // Convert second half of byte
                c = str[++sx];
                buffer[bx] |= (byte)(c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0'));
            }

            return buffer;
        }

    }
}

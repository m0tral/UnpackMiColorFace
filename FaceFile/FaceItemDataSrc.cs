using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnpackMiColorFace.FaceFile
{
    class FaceItemDataSrc
    {
        public const string Hour = "0811";
        public const string HourLow = "0911";
        public const string HourHigh = "10911"; // 0A11
        public const string Minute = "1011";
        public const string MinuteLow = "1111";
        public const string MinuteHigh = "1211";
        public const string Second = "1811";
        public const string SecondLow = "1911";
        public const string SecondHigh = "11911"; //1A11

        public const string Month = "1012";
        public const string Day = "1812";
        public const string DayLow = "1912";
        public const string DayHigh = "11912";    //1A12
        public const string Weekday = "2012";

        public const string Steps = "0821";
        public const string Hrm = "0822";
        public const string Calories = "0823";

        public const string ActivityCount = "0824";
        public const string Stress = "0826";
        public const string Sleep = "0828";

        public const string Battery = "0841";

        public const string WeatherTemp = "2031";
        public const string WeatherType = "3031";
        public const string WeatherAir = "5031";
    }
}

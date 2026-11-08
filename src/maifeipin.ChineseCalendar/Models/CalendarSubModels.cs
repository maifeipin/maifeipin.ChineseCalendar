using System;
using maifeipin.ChineseCalendar.Enums;

namespace maifeipin.ChineseCalendar.Models
{
    /// <summary>
    /// 节气详细信息（包含具体交节公历时间，精确到分）
    /// </summary>
    public sealed class SolarTermDetail
    {
        public SolarTerm Term { get; set; }
        public string Name => ChineseCalendarConstants.SolarTermNames[(int)Term];
        public DateTime ExactTime { get; set; }
        public int Day => ExactTime.Day;
        public int Hour => ExactTime.Hour;
        public int Minute => ExactTime.Minute;

        public override string ToString() => $"{Name} ({ExactTime:yyyy-MM-dd HH:mm})";
    }

    /// <summary>
    /// 太阳升降与时空天文信息（真太阳时、日出日落）
    /// </summary>
    public sealed class SunTimeInfo
    {
        public string Sunrise { get; set; } = "--:--";
        public string Sunset { get; set; } = "--:--";
        public string Transit { get; set; } = "--:--";      // 日上中天时刻
        public string DayLength { get; set; } = "--:--";    // 昼长
        public string Dawn { get; set; } = "--:--";         // 拂晓 (天亮)
        public string Dark { get; set; } = "--:--";         // 昏影 (天黑)
        public double Declination { get; set; }             // 太阳赤纬角
        public double EquationOfTimeMinutes { get; set; }   // 真太阳时时差方程 (分)
    }

    /// <summary>
    /// 地理位置坐标
    /// </summary>
    public sealed class GeoLocation
    {
        public string City { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public GeoLocation(string city, string region, double lng, double lat)
        {
            City = city;
            Region = region;
            Longitude = lng;
            Latitude = lat;
        }

        public override string ToString() => $"{City} (东经 {Longitude}°, 北纬 {Latitude}°)";
    }

    /// <summary>
    /// 时辰干支项（12时辰）
    /// </summary>
    public sealed class HourGanZhiItem
    {
        public int Index { get; set; }
        public string TimeRange { get; set; } = string.Empty;
        public GanZhiPair GanZhi { get; set; }
        public bool IsCurrentHour { get; set; }

        public override string ToString() => $"{TimeRange} {GanZhi}时";
    }
}


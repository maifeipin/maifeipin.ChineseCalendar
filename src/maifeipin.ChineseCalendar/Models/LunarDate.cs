using System;
using maifeipin.ChineseCalendar.Enums;

namespace maifeipin.ChineseCalendar.Models
{
    /// <summary>
    /// 农历日期与完整国学历法数据模型
    /// </summary>
    public sealed class LunarDate
    {
        /// <summary>
        /// 对应公历日期
        /// </summary>
        public DateTime SolarDate { get; set; }

        /// <summary>
        /// 农历年份 (如 2026)
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 农历月份 (1~12)
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// 农历日期 (1~30)
        /// </summary>
        public int Day { get; set; }

        /// <summary>
        /// 是否闰月
        /// </summary>
        public bool IsLeapMonth { get; set; }

        /// <summary>
        /// 农历中文月名称 (如 "正月"、"闰二月"、"冬月"、"腊月")
        /// </summary>
        public string MonthName { get; set; } = string.Empty;

        /// <summary>
        /// 农历中文日名称 (如 "初一"、"十五"、"廿八"、"三十")
        /// </summary>
        public string DayName { get; set; } = string.Empty;

        /// <summary>
        /// 简要农历全称 (如 "二月十五" 或 "初一")
        /// </summary>
        public string FullChineseDate => $"农历 {MonthName}{DayName}";

        /// <summary>
        /// 年干支
        /// </summary>
        public GanZhiPair YearGanZhi { get; set; }

        /// <summary>
        /// 月干支
        /// </summary>
        public GanZhiPair MonthGanZhi { get; set; }

        /// <summary>
        /// 日干支
        /// </summary>
        public GanZhiPair DayGanZhi { get; set; }

        /// <summary>
        /// 生肖
        /// </summary>
        public ZodiacAnimal Zodiac => YearGanZhi.Zodiac;
        public string ZodiacName => YearGanZhi.ZodiacName;

        /// <summary>
        /// 十二星座
        /// </summary>
        public ZodiacSign Constellation { get; set; }
        public string ConstellationName => ChineseCalendarConstants.ZodiacSignNames[(int)Constellation];

        /// <summary>
        /// 当日交接的二十四节气（若无则为 null）
        /// </summary>
        public SolarTermDetail? SolarTerm { get; set; }

        /// <summary>
        /// 当日节日与传统纪念日（如 "元旦", "除夕", "中秋" 等）
        /// </summary>
        public string Festivals { get; set; } = string.Empty;

        /// <summary>
        /// 儒略日计数 (Julian Day)
        /// </summary>
        public long JulianDay { get; set; }

        /// <summary>
        /// 月相标记 (如 "朔", "望")
        /// </summary>
        public string MoonPhase { get; set; } = string.Empty;

        public override string ToString() => $"{SolarDate:yyyy-MM-dd} {FullChineseDate} ({YearGanZhi}年【{ZodiacName}】 {MonthGanZhi}月 {DayGanZhi}日) {ConstellationName}";
    }
}


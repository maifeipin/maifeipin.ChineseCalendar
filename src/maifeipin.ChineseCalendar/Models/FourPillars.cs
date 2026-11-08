namespace maifeipin.ChineseCalendar.Models
{
    /// <summary>
    /// 四柱八字（年柱、月柱、日柱、时柱）
    /// </summary>
    public sealed class FourPillars
    {
        /// <summary>
        /// 年柱（以立春交节时刻换年）
        /// </summary>
        public GanZhiPair YearPillar { get; set; }

        /// <summary>
        /// 月柱（以二十四节气之“节”换月）
        /// </summary>
        public GanZhiPair MonthPillar { get; set; }

        /// <summary>
        /// 日柱
        /// </summary>
        public GanZhiPair DayPillar { get; set; }

        /// <summary>
        /// 时柱（以真太阳时与早夜子时换算）
        /// </summary>
        public GanZhiPair HourPillar { get; set; }

        /// <summary>
        /// 八字全称（如：甲子年 丙寅月 戊午日 庚申时）
        /// </summary>
        public string FullString => $"{YearPillar}年 {MonthPillar}月 {DayPillar}日 {HourPillar}时";

        public override string ToString() => FullString;
    }
}


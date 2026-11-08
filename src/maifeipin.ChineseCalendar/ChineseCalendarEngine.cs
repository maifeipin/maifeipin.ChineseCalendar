using System;
using System.Collections.Generic;
using System.Globalization;
using maifeipin.ChineseCalendar.Enums;
using maifeipin.ChineseCalendar.Models;

namespace maifeipin.ChineseCalendar
{
    /// <summary>
    /// 中华万年历与国学历法核心计算引擎
    /// 提供公历/农历转换、节气精确推算、四柱干支、生肖星座与真太阳时计算
    /// </summary>
    public static class ChineseCalendarEngine
    {
        /// <summary>
        /// 引擎支持的最小公历日期 (1900-01-31，农历庚子年正月初一)
        /// </summary>
        public static readonly DateTime MinSupportedDate = new(1900, 1, 31);

        /// <summary>
        /// 引擎支持的最大公历日期 (2100-12-31，农历庚申年十二月初一)
        /// </summary>
        public static readonly DateTime MaxSupportedDate = new(2100, 12, 31);

        private static readonly ChineseLunisolarCalendar BclCalendar = new();
        private static readonly DateTime BclMinDate = new(1901, 2, 19);
        private static readonly DateTime BaseDate1900 = new(1900, 1, 31);
        private static readonly DateTime BaseGanZhiDate = new(1900, 1, 1);
        private static readonly DateTime SolarTermEpochUtc = new DateTime(1900, 1, 6, 2, 5, 0, DateTimeKind.Utc).AddMilliseconds(-21960000);

        #region 1. 核心公历转农历转换 (GetLunarDate)

        /// <summary>
        /// 将公历日期转换为完整的农历及历法数据模型
        /// </summary>
        /// <param name="solarDate">公历日期 (支持 1900-01-31 至 2100-12-31)</param>
        /// <returns>农历完整数据结构</returns>
        public static LunarDate GetLunarDate(DateTime solarDate)
        {
            DateTime dateOnly = solarDate.Date;
            if (dateOnly < MinSupportedDate || dateOnly > MaxSupportedDate)
            {
                throw new ArgumentOutOfRangeException(nameof(solarDate), $"计算范围支持 {MinSupportedDate:yyyy-MM-dd} 至 {MaxSupportedDate:yyyy-MM-dd}");
            }

            int y = dateOnly.Year;
            int m = dateOnly.Month;
            int d = dateOnly.Day;

            int lYear;
            int lMonth;
            int lDay;
            bool isLeap;

            if (dateOnly >= BclMinDate)
            {
                lYear = BclCalendar.GetYear(dateOnly);
                int mRaw = BclCalendar.GetMonth(dateOnly);
                int leap = BclCalendar.GetLeapMonth(lYear);
                isLeap = (leap > 0 && mRaw == leap);
                lMonth = (leap > 0 && mRaw >= leap) ? mRaw - 1 : mRaw;
                lDay = BclCalendar.GetDayOfMonth(dateOnly);
            }
            else
            {
                // 1900-01-31 至 1901-02-18 (农历 1900 庚子年)
                int offset = (int)(dateOnly - BaseDate1900).TotalDays;
                int leap = 8;
                isLeap = false;
                lYear = 1900;
                int iM = 1;
                int daysOfMonth = 0;
                for (; iM <= 12 && offset > 0; iM++)
                {
                    if (iM == (leap + 1) && !isLeap)
                    {
                        --iM;
                        isLeap = true;
                        daysOfMonth = GetLeapDays(1900);
                    }
                    else
                    {
                        daysOfMonth = GetMonthDays(1900, iM);
                    }

                    if (isLeap && iM == (leap + 1))
                    {
                        isLeap = false;
                    }

                    offset -= daysOfMonth;
                }

                if (offset == 0 && iM == leap + 1)
                {
                    if (isLeap)
                    {
                        isLeap = false;
                    }
                    else
                    {
                        isLeap = true;
                        --iM;
                    }
                }
                if (offset < 0)
                {
                    offset += daysOfMonth;
                    --iM;
                }

                lMonth = iM;
                lDay = offset + 1;
            }

            string monthName = (isLeap ? "闰" : "") + ChineseCalendarConstants.MonthNames[lMonth];
            string dayName = (lDay >= 1 && lDay <= 30) ? ChineseCalendarConstants.DayNames[lDay] : string.Empty;

            // 节气判断
            int termIdx1 = m * 2 - (d < 15 ? 2 : 1);
            SolarTermDetail? termDetail = null;
            if (termIdx1 >= 0 && termIdx1 < 24)
            {
                var termCheck = GetSolarTermDetail(y, (SolarTerm)termIdx1);
                if (termCheck.Day == d)
                {
                    termDetail = termCheck;
                }
            }

            // 干支
            var yearGZ = GetYearGanZhi(lYear);
            var monthGZ = GetMonthGanZhi(y, m, d);
            var dayGZ = GetDayGanZhi(y, m, d);

            // 星座
            var star = GetZodiacSign(m, d);

            // 月相 (朔日为初一，望日为十五或十六)
            string moonPhase = string.Empty;
            if (lDay == 1) moonPhase = "朔";
            else if (lDay == 15) moonPhase = "望";

            // 节日
            string festivals = GetFestivals(y, m, d, lMonth, lDay, isLeap);

            // 儒略日计算
            long julianDay = (long)Math.Floor((solarDate.Date.Ticks / 864000000000.0) + 1721425.5);

            return new LunarDate
            {
                SolarDate = solarDate.Date,
                Year = lYear,
                Month = lMonth,
                Day = lDay,
                IsLeapMonth = isLeap,
                MonthName = monthName,
                DayName = dayName,
                YearGanZhi = yearGZ,
                MonthGanZhi = monthGZ,
                DayGanZhi = dayGZ,
                Constellation = star,
                SolarTerm = termDetail,
                Festivals = festivals,
                MoonPhase = moonPhase,
                JulianDay = julianDay
            };
        }

        #endregion

        #region 2. 农历大小月与闰月基础位运算

        public static int GetLunarYearDays(int y)
        {
            if (y < 1900 || y > 2100) return 348;
            int sum = 348;
            int info = ChineseCalendarConstants.LunarInfo[y - 1900];
            for (int i = 0x8000; i > 0x8; i >>= 1)
            {
                sum += ((info & i) != 0) ? 1 : 0;
            }
            return sum + GetLeapDays(y);
        }

        public static int GetLeapDays(int y)
        {
            if (GetLeapMonth(y) > 0)
            {
                return ((ChineseCalendarConstants.LunarInfo[y - 1900] & 0x10000) != 0) ? 30 : 29;
            }
            return 0;
        }

        public static int GetLeapMonth(int y)
        {
            if (y < 1900 || y > 2100) return 0;
            return ChineseCalendarConstants.LunarInfo[y - 1900] & 0xf;
        }

        public static int GetMonthDays(int y, int m)
        {
            if (y < 1900 || y > 2100 || m < 1 || m > 12) return 29;
            return ((ChineseCalendarConstants.LunarInfo[y - 1900] & (0x10000 >> m)) != 0) ? 30 : 29;
        }

        #endregion

        #region 3. 二十四节气公历估算时刻推算

        /// <summary>
        /// 获取某年某个节气的公历估算交节时间（中国标准时间 UTC+8）
        /// 注：节气时刻基于经典经验天文回归模型估算，受地球公转微扰等摄动影响存在数分钟固有偏差，适合民俗历法与节气月分界参考，非专业高精度天文星历。
        /// </summary>
        public static SolarTermDetail GetSolarTermDetail(int year, SolarTerm term)
        {
            int n = (int)term;
            double ms = (31556925974.7 * (year - 1900)) + (ChineseCalendarConstants.SolarTermInfo[n] * 60000.0);
            DateTime utcTerm = SolarTermEpochUtc.AddMilliseconds(ms);
            DateTime chinaTime = utcTerm.AddHours(8);

            return new SolarTermDetail
            {
                Term = term,
                ExactTime = chinaTime
            };
        }

        /// <summary>
        /// 获取某年某月份包含的两个节气详情
        /// </summary>
        public static (SolarTermDetail Term1, SolarTermDetail Term2) GetMonthSolarTerms(int year, int month)
        {
            var t1 = GetSolarTermDetail(year, (SolarTerm)((month - 1) * 2));
            var t2 = GetSolarTermDetail(year, (SolarTerm)((month - 1) * 2 + 1));
            return (t1, t2);
        }

        #endregion

        #region 4. 天干地支与四柱八字计算 (BaZi / FourPillars)

        /// <summary>
        /// 根据年份获取年干支 (以公历年或农历年序号)
        /// </summary>
        public static GanZhiPair GetYearGanZhi(int year)
        {
            int idx = (year - 1864) % 60;
            if (idx < 0) idx += 60;
            return new GanZhiPair(idx % 10, idx % 12);
        }

        /// <summary>
        /// 获取日干支
        /// </summary>
        public static GanZhiPair GetDayGanZhi(int year, int month, int day)
        {
            var target = new DateTime(year, month, day);
            int offset = (int)(target - BaseGanZhiDate).TotalDays;
            int idx = (10 + offset) % 60;
            if (idx < 0) idx += 60;
            return new GanZhiPair(idx % 10, idx % 12);
        }

        /// <summary>
        /// 获取月干支（遵循五虎遁年起月诀，以当日中午 12:00 估算）
        /// </summary>
        public static GanZhiPair GetMonthGanZhi(int year, int month, int day)
        {
            return GetMonthGanZhi(new DateTime(year, month, day, 12, 0, 0));
        }

        /// <summary>
        /// 获取指定时刻的月干支（遵循五虎遁年起月诀，以二十四节气中十二“节”精确交节时刻为分界）
        /// </summary>
        public static GanZhiPair GetMonthGanZhi(DateTime dt, int? knownSolarYear = null)
        {
            int solarYear;
            if (knownSolarYear.HasValue)
            {
                solarYear = knownSolarYear.Value;
            }
            else
            {
                var liChun = GetSolarTermDetail(dt.Year, SolarTerm.LiChun).ExactTime;
                solarYear = dt < liChun ? dt.Year - 1 : dt.Year;
            }

            // 12节（寅月立春 ~ 丑月小寒/立春）
            DateTime[] termTimes = new DateTime[13];
            termTimes[0] = GetSolarTermDetail(solarYear, SolarTerm.LiChun).ExactTime;
            termTimes[1] = GetSolarTermDetail(solarYear, SolarTerm.JingZhe).ExactTime;
            termTimes[2] = GetSolarTermDetail(solarYear, SolarTerm.QingMing).ExactTime;
            termTimes[3] = GetSolarTermDetail(solarYear, SolarTerm.LiXia).ExactTime;
            termTimes[4] = GetSolarTermDetail(solarYear, SolarTerm.MangZhong).ExactTime;
            termTimes[5] = GetSolarTermDetail(solarYear, SolarTerm.XiaoShu).ExactTime;
            termTimes[6] = GetSolarTermDetail(solarYear, SolarTerm.LiQiu).ExactTime;
            termTimes[7] = GetSolarTermDetail(solarYear, SolarTerm.BaiLu).ExactTime;
            termTimes[8] = GetSolarTermDetail(solarYear, SolarTerm.HanLu).ExactTime;
            termTimes[9] = GetSolarTermDetail(solarYear, SolarTerm.LiDong).ExactTime;
            termTimes[10] = GetSolarTermDetail(solarYear, SolarTerm.DaXue).ExactTime;
            termTimes[11] = GetSolarTermDetail(solarYear + 1, SolarTerm.XiaoHan).ExactTime;
            termTimes[12] = GetSolarTermDetail(solarYear + 1, SolarTerm.LiChun).ExactTime;

            int mOffset = 0;
            for (int i = 0; i < 12; i++)
            {
                if (dt >= termTimes[i] && (i == 11 || dt < termTimes[i + 1]))
                {
                    mOffset = i;
                    break;
                }
            }

            int yearGan = (solarYear - 1864) % 10;
            if (yearGan < 0) yearGan += 10;

            int startStem = ((yearGan % 5) * 2 + 2) % 10;
            int stem = (startStem + mOffset) % 10;
            int branch = (2 + mOffset) % 12; // 寅月地支为 2
            return new GanZhiPair(stem, branch);
        }

        /// <summary>
        /// 获取时干支（五鼠遁日起时诀）
        /// </summary>
        /// <param name="dayGanZhi">当日日干支</param>
        /// <param name="hour">公历时（0~23）</param>
        public static GanZhiPair GetHourGanZhi(GanZhiPair dayGanZhi, int hour)
        {
            int dayGan = (int)dayGanZhi.Stem;
            int startGan = (dayGan % 5) * 2;

            // 23:00~01:00 为子时 (0), 01:00~03:00 为丑时 (1)...
            int zhiIndex = (hour >= 23) ? 0 : (hour + 1) / 2;
            int stemIndex = (startGan + zhiIndex) % 10;

            return new GanZhiPair(stemIndex, zhiIndex);
        }

        /// <summary>
        /// 计算完整的四柱八字（以立春交节时刻换年，以十二“节”精确交节时刻换月）
        /// </summary>
        public static FourPillars GetFourPillars(DateTime dt)
        {
            var liChunThisYear = GetSolarTermDetail(dt.Year, SolarTerm.LiChun).ExactTime;
            int solarYear = dt < liChunThisYear ? dt.Year - 1 : dt.Year;
            var yearGz = GetYearGanZhi(solarYear);
            var monthGz = GetMonthGanZhi(dt, solarYear);
            var dayGz = GetDayGanZhi(dt.Year, dt.Month, dt.Day);
            var hourGz = GetHourGanZhi(dayGz, dt.Hour);

            return new FourPillars
            {
                YearPillar = yearGz,
                MonthPillar = monthGz,
                DayPillar = dayGz,
                HourPillar = hourGz
            };
        }

        /// <summary>
        /// 获取某日全天十二时辰干支对照表
        /// </summary>
        public static IReadOnlyList<HourGanZhiItem> GetDayHourTable(DateTime dt)
        {
            var dayGz = GetDayGanZhi(dt.Year, dt.Month, dt.Day);
            int dayGan = (int)dayGz.Stem;
            int startGan = (dayGan % 5) * 2;

            bool isToday = (dt.Date == DateTime.Today);
            int curHour = isToday ? DateTime.Now.Hour : -1;

            var list = new List<HourGanZhiItem>(12);
            for (int i = 0; i < 12; i++)
            {
                int hStart = (i == 0) ? 23 : (i * 2) - 1;
                int hEnd = (i == 0) ? 1 : (i * 2) + 1;
                string rangeStr = $"{hStart:D2}~{hEnd:D2}";

                int stemIdx = (startGan + i) % 10;
                var gz = new GanZhiPair(stemIdx, i);

                bool isCurrent = false;
                if (curHour != -1)
                {
                    if (i == 0 && (curHour >= 23 || curHour < 1)) isCurrent = true;
                    else if (curHour >= hStart && curHour < hEnd) isCurrent = true;
                }

                list.Add(new HourGanZhiItem
                {
                    Index = i,
                    TimeRange = rangeStr,
                    GanZhi = gz,
                    IsCurrentHour = isCurrent
                });
            }

            return list;
        }

        #endregion

        #region 5. 十二星座与节日计算

        /// <summary>
        /// 根据公历月日计算所属星座
        /// </summary>
        public static ZodiacSign GetZodiacSign(int month, int day)
        {
            // 各星座月份分割日
            int[] splitDays = { 20, 19, 21, 20, 21, 22, 23, 23, 23, 24, 23, 22 };
            // 各月分割日之后所属星座:
            // 1月20日后为水瓶, 2月19日后为双鱼, 3月21日后为白羊, 4月20日后为金牛...
            ZodiacSign[] signs =
            {
                ZodiacSign.Aquarius,    // 0: 1月
                ZodiacSign.Pisces,      // 1: 2月
                ZodiacSign.Aries,       // 2: 3月
                ZodiacSign.Taurus,      // 3: 4月
                ZodiacSign.Gemini,      // 4: 5月
                ZodiacSign.Cancer,      // 5: 6月
                ZodiacSign.Leo,         // 6: 7月
                ZodiacSign.Virgo,       // 7: 8月
                ZodiacSign.Libra,       // 8: 9月
                ZodiacSign.Scorpio,     // 9: 10月
                ZodiacSign.Sagittarius, // 10: 11月
                ZodiacSign.Capricorn    // 11: 12月
            };

            int mIdx = month - 1;
            if (day < splitDays[mIdx])
            {
                return signs[(mIdx + 11) % 12];
            }
            return signs[mIdx];
        }

        private static string GetFestivals(int y, int m, int d, int lMonth, int lDay, bool isLeap)
        {
            var list = new List<string>();

            // 公历节日
            if (m == 1 && d == 1) list.Add("元旦");
            else if (m == 2 && d == 14) list.Add("情人节");
            else if (m == 3 && d == 8) list.Add("妇女节");
            else if (m == 3 && d == 12) list.Add("植树节");
            else if (m == 4 && d == 1) list.Add("愚人节");
            else if (m == 5 && d == 1) list.Add("劳动节");
            else if (m == 5 && d == 4) list.Add("青年节");
            else if (m == 6 && d == 1) list.Add("儿童节");
            else if (m == 7 && d == 1) list.Add("建党节");
            else if (m == 8 && d == 1) list.Add("建军节");
            else if (m == 9 && d == 10) list.Add("教师节");
            else if (m == 10 && d == 1) list.Add("国庆节");
            else if (m == 12 && d == 25) list.Add("圣诞节");

            // 农历传统节日 (闰月不计传统节日)
            if (!isLeap)
            {
                if (lMonth == 1 && lDay == 1) list.Add("春节");
                else if (lMonth == 1 && lDay == 15) list.Add("元宵节");
                else if (lMonth == 2 && lDay == 2) list.Add("龙抬头");
                else if (lMonth == 5 && lDay == 5) list.Add("端午节");
                else if (lMonth == 7 && lDay == 7) list.Add("七夕节");
                else if (lMonth == 7 && lDay == 15) list.Add("中元节");
                else if (lMonth == 8 && lDay == 15) list.Add("中秋节");
                else if (lMonth == 9 && lDay == 9) list.Add("重阳节");
                else if (lMonth == 12 && lDay == 8) list.Add("腊八节");
                else if (lMonth == 12 && lDay == 23) list.Add("小年");
                else if (lMonth == 12 && (lDay == 29 || lDay == 30))
                {
                    int mDays = GetMonthDays(y, 12);
                    if (lDay == mDays) list.Add("除夕");
                }
            }

            return string.Join(" ", list);
        }

        #endregion

        #region 6. 真太阳时、日出日落与天文时角推算

        /// <summary>
        /// 计算指定日期与地理经纬度的太阳升降、中天时刻与真太阳时校正
        /// </summary>
        /// <param name="date">公历日期</param>
        /// <param name="latitude">地理纬度 (北纬为正，如北京 39.9)</param>
        /// <param name="longitude">地理经度 (东经为正，如北京 116.4)</param>
        public static SunTimeInfo GetSunTimes(DateTime date, double latitude, double longitude)
        {
            int dayOfYear = date.DayOfYear;

            // 太阳赤纬 (Declination)
            double decl = 23.45 * Math.Sin((284 + dayOfYear) * 0.0172);
            double latRad = latitude * (Math.PI / 180.0);
            double declRad = decl * (Math.PI / 180.0);

            double tanTan = -Math.Tan(latRad) * Math.Tan(declRad);
            double hHourAngle;
            if (tanTan < -1) hHourAngle = 180;
            else if (tanTan > 1) hHourAngle = 0;
            else hHourAngle = Math.Acos(tanTan) * (180.0 / Math.PI);

            double durationHours = 2 * hHourAngle / 15.0;

            // 经度与北京时间(东经120°)时差: 每度4分钟
            double noonOffsetMinutes = (120.0 - longitude) * 4.0;
            double transitMinutes = 12 * 60 + noonOffsetMinutes;

            // 真太阳时时差方程 (Equation of Time)
            double bRad = (360.0 * (dayOfYear - 81) / 365.0) * (Math.PI / 180.0);
            double eot = 9.87 * Math.Sin(2 * bRad) - 7.53 * Math.Cos(bRad) - 1.5 * Math.Sin(bRad);
            transitMinutes -= eot;

            double riseMinutes = transitMinutes - (durationHours / 2.0 * 60.0);
            double setMinutes = transitMinutes + (durationHours / 2.0 * 60.0);

            static string FmtTime(double minutes)
            {
                if (double.IsNaN(minutes) || double.IsInfinity(minutes)) return "--:--";
                int mTotal = (int)Math.Round(minutes);
                int h = (mTotal / 60) % 24;
                if (h < 0) h += 24;
                int mm = mTotal % 60;
                return $"{h:D2}:{mm:D2}";
            }

            static string FmtDuration(double hours)
            {
                if (double.IsNaN(hours) || double.IsInfinity(hours)) return "--:--";
                int h = (int)Math.Floor(hours);
                int m = (int)Math.Round((hours - h) * 60.0);
                return $"{h:D2}:{m:D2}";
            }

            return new SunTimeInfo
            {
                Sunrise = FmtTime(riseMinutes),
                Sunset = FmtTime(setMinutes),
                Transit = FmtTime(transitMinutes),
                DayLength = FmtDuration(durationHours),
                Dawn = FmtTime(riseMinutes - 30),
                Dark = FmtTime(setMinutes + 30),
                Declination = Math.Round(decl, 2),
                EquationOfTimeMinutes = Math.Round(eot, 2)
            };
        }

        #endregion
    }
}

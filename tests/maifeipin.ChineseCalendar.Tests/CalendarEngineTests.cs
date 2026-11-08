using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using maifeipin.ChineseCalendar.Enums;

namespace maifeipin.ChineseCalendar.Tests
{
    [TestClass]
    public class CalendarEngineTests
    {
        [TestMethod]
        public void TestSpringFestival2024()
        {
            // 2024年2月10日 是 农历甲辰年正月初一 (龙年春节)
            var dt = new DateTime(2024, 2, 10);
            var lunar = ChineseCalendarEngine.GetLunarDate(dt);

            Assert.AreEqual(2024, lunar.Year);
            Assert.AreEqual(1, lunar.Month);
            Assert.AreEqual(1, lunar.Day);
            Assert.IsFalse(lunar.IsLeapMonth);
            Assert.AreEqual("正月", lunar.MonthName);
            Assert.AreEqual("初一", lunar.DayName);
            Assert.AreEqual("甲辰", lunar.YearGanZhi.Name);
            Assert.AreEqual(ZodiacAnimal.Dragon, lunar.Zodiac);
            StringAssert.Contains(lunar.Festivals, "春节");
        }

        [TestMethod]
        public void TestSpringFestival2026()
        {
            // 2026年2月17日 是 农历丙午年正月初一 (马年春节)
            var dt = new DateTime(2026, 2, 17);
            var lunar = ChineseCalendarEngine.GetLunarDate(dt);

            Assert.AreEqual(2026, lunar.Year);
            Assert.AreEqual(1, lunar.Month);
            Assert.AreEqual(1, lunar.Day);
            Assert.IsFalse(lunar.IsLeapMonth);
            Assert.AreEqual("丙午", lunar.YearGanZhi.Name);
            Assert.AreEqual(ZodiacAnimal.Horse, lunar.Zodiac);
            StringAssert.Contains(lunar.Festivals, "春节");
        }

        [TestMethod]
        public void TestLeapMonth2023()
        {
            // 2023年有闰二月 (农历癸卯年)
            int leapMonth = ChineseCalendarEngine.GetLeapMonth(2023);
            Assert.AreEqual(2, leapMonth, "2023年应该闰二月");

            // 2023年4月1日 处于 农历闰二月十一
            var dt = new DateTime(2023, 4, 1);
            var lunar = ChineseCalendarEngine.GetLunarDate(dt);

            Assert.AreEqual(2, lunar.Month);
            Assert.IsTrue(lunar.IsLeapMonth);
            Assert.AreEqual("闰二月", lunar.MonthName);
            Assert.AreEqual("十一", lunar.DayName);
        }

        [TestMethod]
        public void TestTodayDate2026()
        {
            // 测试 2026年11月8日
            var dt = new DateTime(2026, 11, 8);
            var lunar = ChineseCalendarEngine.GetLunarDate(dt);

            Assert.AreEqual(2026, lunar.Year);
            Assert.AreEqual(ZodiacSign.Scorpio, lunar.Constellation);
            Assert.AreEqual("天蝎座", lunar.ConstellationName);
            Assert.IsNotNull(lunar.YearGanZhi.Name);
            Assert.IsNotNull(lunar.MonthGanZhi.Name);
            Assert.IsNotNull(lunar.DayGanZhi.Name);
        }

        [TestMethod]
        public void TestSolarTerms2026()
        {
            // 2026年立春: 应该在 2026年2月4日
            var liChun = ChineseCalendarEngine.GetSolarTermDetail(2026, SolarTerm.LiChun);
            Assert.AreEqual(SolarTerm.LiChun, liChun.Term);
            Assert.AreEqual("立春", liChun.Name);
            Assert.AreEqual(2026, liChun.ExactTime.Year);
            Assert.AreEqual(2, liChun.ExactTime.Month);
            Assert.AreEqual(4, liChun.ExactTime.Day);

            // 2026年冬至: 应该在 2026年12月22日
            var dongZhi = ChineseCalendarEngine.GetSolarTermDetail(2026, SolarTerm.DongZhi);
            Assert.AreEqual(SolarTerm.DongZhi, dongZhi.Term);
            Assert.AreEqual("冬至", dongZhi.Name);
            Assert.AreEqual(2026, dongZhi.ExactTime.Year);
            Assert.AreEqual(12, dongZhi.ExactTime.Month);
            Assert.AreEqual(22, dongZhi.ExactTime.Day);

            // 验证 12 个月份每月各有两个节气
            for (int m = 1; m <= 12; m++)
            {
                var (t1, t2) = ChineseCalendarEngine.GetMonthSolarTerms(2026, m);
                Assert.IsNotNull(t1.Name);
                Assert.IsNotNull(t2.Name);
                Assert.IsTrue(t1.Day > 0 && t1.Day <= 31);
                Assert.IsTrue(t2.Day > 0 && t2.Day <= 31);
            }
        }

        [TestMethod]
        public void TestZodiacSigns()
        {
            // 3月21日 - 白羊座
            Assert.AreEqual(ZodiacSign.Aries, ChineseCalendarEngine.GetZodiacSign(3, 21));
            // 4月20日 - 金牛座
            Assert.AreEqual(ZodiacSign.Taurus, ChineseCalendarEngine.GetZodiacSign(4, 20));
            // 8月8日 - 狮子座
            Assert.AreEqual(ZodiacSign.Leo, ChineseCalendarEngine.GetZodiacSign(8, 8));
            // 11月8日 - 天蝎座
            Assert.AreEqual(ZodiacSign.Scorpio, ChineseCalendarEngine.GetZodiacSign(11, 8));
            // 12月25日 - 摩羯座
            Assert.AreEqual(ZodiacSign.Capricorn, ChineseCalendarEngine.GetZodiacSign(12, 25));
        }

        [TestMethod]
        public void TestFourPillarsAndHourTable()
        {
            var dt = new DateTime(2026, 11, 8, 14, 30, 0);
            var fourPillars = ChineseCalendarEngine.GetFourPillars(dt);

            Assert.IsNotNull(fourPillars.YearPillar.Name);
            Assert.IsNotNull(fourPillars.MonthPillar.Name);
            Assert.IsNotNull(fourPillars.DayPillar.Name);
            Assert.IsNotNull(fourPillars.HourPillar.Name);

            // 14点为未时 (Wei)
            Assert.AreEqual(EarthlyBranch.Wei, fourPillars.HourPillar.Branch);

            // 十二时辰对照表应该包含恰好 12 个时辰
            var hourTable = ChineseCalendarEngine.GetDayHourTable(dt);
            Assert.AreEqual(12, hourTable.Count);
            Assert.AreEqual(EarthlyBranch.Zi, hourTable[0].GanZhi.Branch);
            Assert.AreEqual(EarthlyBranch.Hai, hourTable[11].GanZhi.Branch);
        }

        [TestMethod]
        public void TestSunTimesCalculation()
        {
            // 北京 (经度 116.4, 纬度 39.9)
            var dt = new DateTime(2026, 6, 21); // 夏至
            var sun = ChineseCalendarEngine.GetSunTimes(dt, 39.9, 116.4);

            Assert.AreNotEqual("--:--", sun.Sunrise);
            Assert.AreNotEqual("--:--", sun.Sunset);
            Assert.AreNotEqual("--:--", sun.Transit);
            Assert.AreNotEqual("--:--", sun.DayLength);

            // 乌鲁木齐 (经度 87.6, 纬度 43.8) 相比北京有明显时差
            var sunUrumqi = ChineseCalendarEngine.GetSunTimes(dt, 43.8, 87.6);
            Assert.AreNotEqual(sun.Transit, sunUrumqi.Transit, "乌鲁木齐日上中天应晚于北京时间约2小时");
        }
        [TestMethod]
        public void TestReviewCases2051AndFourPillarsAndBounds()
        {
            // 1. 2051-02-11 香港天文台与国家标准：正月初一 (春节)
            var lunar2051 = ChineseCalendarEngine.GetLunarDate(new DateTime(2051, 2, 11));
            Assert.AreEqual(2051, lunar2051.Year);
            Assert.AreEqual(1, lunar2051.Month);
            Assert.AreEqual(1, lunar2051.Day);
            Assert.IsFalse(lunar2051.IsLeapMonth);

            // 2. 2100-12-31 边界农历
            var lunar2100 = ChineseCalendarEngine.GetLunarDate(new DateTime(2100, 12, 31));
            Assert.AreEqual(2100, lunar2100.Year);
            Assert.AreEqual(12, lunar2100.Month);
            Assert.AreEqual(1, lunar2100.Day);

            // 3. 边界抛出 ArgumentOutOfRangeException
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ChineseCalendarEngine.GetLunarDate(new DateTime(1900, 1, 1)));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ChineseCalendarEngine.GetLunarDate(new DateTime(2101, 1, 1)));

            // 4. 四柱立春交节时刻换年换月 (2026-02-04 04:10 立春)
            var early = ChineseCalendarEngine.GetFourPillars(new DateTime(2026, 2, 4, 0, 0, 0));
            Assert.AreEqual("乙巳", early.YearPillar.Name);
            Assert.AreEqual("己丑", early.MonthPillar.Name);

            var late = ChineseCalendarEngine.GetFourPillars(new DateTime(2026, 2, 4, 23, 0, 0));
            Assert.AreEqual("丙午", late.YearPillar.Name);
            Assert.AreEqual("庚寅", late.MonthPillar.Name);
        }
    }
}

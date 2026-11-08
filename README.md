# maifeipin.ChineseCalendar

中华万年历、农历、二十四节气、干支四柱、生肖星座与真太阳时核心计算类库。

专为中华传统文化、万年历、命理八字排盘、风水择吉、天文星座等应用打造的**纯 C# 跨平台核心公共组件**。

---

## 🌟 特性

- **百年农历换算**：支持 1900 ~ 2100 年精准公历/农历互相转换、闰月自动判断、大小月天数推算。
- **二十四节气精准推演**：包含从小寒到冬至的精确交节公历日期与时分（中国标准时间 UTC+8）。
- **干支四柱八字系统**：
  - 年柱：以立春交节换年；
  - 月柱：五虎遁年起月诀，以节气精确交节分界；
  - 日柱：百年干支顺排与推算；
  - 时柱：五鼠遁日起时诀，含全天 12 时辰干支对照表。
- **真太阳时与天文时空**：
  - 基于地理经纬度与时差方程（Equation of Time）校准真太阳时；
  - 提供日出、日落、日上中天、天亮（拂晓）、天黑（昏影）时刻推算；
  - 内置中国主要省会及全球核心城市经纬度字典库。
- **生肖与十二星座**：精准匹配生肖与十二星座划分（按节气/分割日界定）。
- **极速与零依赖**：
  - 纯 C#（Pure Managed Code），无任何 C++ 原生 DLL 依赖；
  - 零磁盘缓存污染（无 WebView2 / Chromium 进程负担）；
  - 多目标框架支持：`netstandard2.0`、`net8.0`、`net10.0`，可在 WPF、Avalonia、MAUI、ASP.NET Core、Unity、Mono 等任意 .NET 环境中通用。

---

## 📦 安装与引用

### 项目引用 (Project Reference)
```xml
<ProjectReference Include="..\..\maifeipin.ChineseCalendar\src\maifeipin.ChineseCalendar\maifeipin.ChineseCalendar.csproj" />
```

---

## 🚀 快速使用指南

### 1. 公历转农历与历法数据
```csharp
using maifeipin.ChineseCalendar;

DateTime today = DateTime.Today;
LunarDate lunar = ChineseCalendarEngine.GetLunarDate(today);

Console.WriteLine($"农历日期: {lunar.FullChineseDate}");         // 例: 农历 十月廿九
Console.WriteLine($"年干支: {lunar.YearGanZhi}年【{lunar.ZodiacName}】"); // 例: 丙午年【马】
Console.WriteLine($"月干支: {lunar.MonthGanZhi}月");              // 例: 己亥月
Console.WriteLine($"日干支: {lunar.DayGanZhi}日");                // 例: 丙子日
Console.WriteLine($"所属星座: {lunar.ConstellationName}");         // 例: 天蝎座
Console.WriteLine($"节气/节日: {lunar.SolarTerm?.Name} {lunar.Festivals}");
```

### 2. 四柱八字排盘
```csharp
DateTime birthTime = new DateTime(2026, 11, 8, 14, 30, 0);
FourPillars bazi = ChineseCalendarEngine.GetFourPillars(birthTime);

Console.WriteLine($"八字: {bazi.FullString}");
// 输出: 丙午年 己亥月 丙子日 乙未时
```

### 3. 获取某月节气精确交节时刻
```csharp
var (t1, t2) = ChineseCalendarEngine.GetMonthSolarTerms(2026, 2);
Console.WriteLine($"节气1: {t1.Name} - {t1.ExactTime:yyyy-MM-dd HH:mm}"); // 立春
Console.WriteLine($"节气2: {t2.Name} - {t2.ExactTime:yyyy-MM-dd HH:mm}"); // 雨水
```

### 4. 获取十二时辰对照表
```csharp
var hourTable = ChineseCalendarEngine.GetDayHourTable(DateTime.Today);
foreach (var h in hourTable)
{
    Console.WriteLine($"{h.TimeRange} -> {h.GanZhi}时 {(h.IsCurrentHour ? "[当前时辰]" : "")}");
}
```

### 5. 计算真太阳时与日出日落
```csharp
// 北京经纬度 (东经 116.4°, 北纬 39.9°)
var sun = ChineseCalendarEngine.GetSunTimes(DateTime.Today, 39.9, 116.4);
Console.WriteLine($"日出时刻: {sun.Sunrise}");
Console.WriteLine($"日落时刻: {sun.Sunset}");
Console.WriteLine($"日上中天: {sun.Transit}");
Console.WriteLine($"昼长: {sun.DayLength}");
Console.WriteLine($"时差方程校准: {sun.EquationOfTimeMinutes} 分钟");
```

---

## 🏛 架构与目录

```text
maifeipin.ChineseCalendar/
├── src/maifeipin.ChineseCalendar/
│   ├── Enums/                 # 天干、地支、五行、生肖、二十四节气、十二星座枚举
│   ├── Models/                # 农历日期、四柱八字、节气详情、真太阳时模型
│   ├── Data/                  # 1900-2100 年农历数据表、城市经纬度字典
│   └── ChineseCalendarEngine.cs # 核心门面 API
└── tests/maifeipin.ChineseCalendar.Tests/ # 单元测试套件
```

## 📄 开源许可
MIT License.

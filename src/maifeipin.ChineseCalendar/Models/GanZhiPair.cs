using maifeipin.ChineseCalendar.Enums;

namespace maifeipin.ChineseCalendar.Models
{
    /// <summary>
    /// 干支组合（如：甲子、丙寅）
    /// </summary>
    public readonly struct GanZhiPair : System.IEquatable<GanZhiPair>
    {
        public HeavenlyStem Stem { get; }
        public EarthlyBranch Branch { get; }

        public string StemName => ChineseCalendarConstants.GanNames[(int)Stem];
        public string BranchName => ChineseCalendarConstants.ZhiNames[(int)Branch];
        public string Name => $"{StemName}{BranchName}";

        public ZodiacAnimal Zodiac => (ZodiacAnimal)Branch;
        public string ZodiacName => ChineseCalendarConstants.AnimalNames[(int)Zodiac];

        public GanZhiPair(HeavenlyStem stem, EarthlyBranch branch)
        {
            Stem = stem;
            Branch = branch;
        }

        public GanZhiPair(int stemIndex, int branchIndex)
        {
            Stem = (HeavenlyStem)(stemIndex % 10);
            Branch = (EarthlyBranch)(branchIndex % 12);
        }

        public override string ToString() => Name;

        public bool Equals(GanZhiPair other) => Stem == other.Stem && Branch == other.Branch;
        public override bool Equals(object? obj) => obj is GanZhiPair other && Equals(other);
        public override int GetHashCode() => ((int)Stem * 397) ^ (int)Branch;
        public static bool operator ==(GanZhiPair left, GanZhiPair right) => left.Equals(right);
        public static bool operator !=(GanZhiPair left, GanZhiPair right) => !left.Equals(right);
    }
}

using System;
using System.Drawing;
using RepositoryManager;

namespace ATray.Tools
{
    public enum OverallStatusType
    {
        Ok=0,
        WarnAhead=1,
        WarnBehind=2,
        CodeRed=3
    }

    public static class Extensions
    {
        public static OverallStatusType ToOverallStatus(this RepoStatus status)
        {
            switch (status)
            {
                case RepoStatus.Conflict:
                    return OverallStatusType.CodeRed;
                case RepoStatus.Behind:
                    return OverallStatusType.WarnBehind;
                case RepoStatus.Dirty:
                    return OverallStatusType.WarnAhead;
                default:
                    return OverallStatusType.Ok;
            }
        }

        public static OverallStatusType WorstOf(this OverallStatusType overallStatus,RepoStatus status)
        {
            var converted = status.ToOverallStatus();
            return converted > overallStatus ? converted : overallStatus;
        }

        public static Color ToColor(OverallStatusType overallStatus)
        {
            switch (overallStatus)
            {
                case OverallStatusType.Ok: return Color.Transparent;
                case OverallStatusType.WarnAhead: return Color.FromArgb(0x80, 0xB1, 0xB1, 0xFF);
                case OverallStatusType.CodeRed: return Color.FromArgb(0x80, Color.Red);
                case OverallStatusType.WarnBehind: return Color.FromArgb(0x80, Color.Yellow);
                default: throw new ArgumentOutOfRangeException(nameof(overallStatus), overallStatus, null);
            }
        }

        public static (string, string) Divide(this string main, char dividor)
        {
            var parts = main.Split(new[] {dividor}, 2);
            return (parts[0], parts.Length == 1 ? null : parts[1]);
        }
    }
}

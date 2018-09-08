using System;
using System.Drawing;
using RepositoryManager;

namespace ATray.Tools
{
    public static class Extensions
    {
        public static OverallStatusType ToOverallStatus(this RepoStatus status)
        {
            if (status >= RepoStatus.Mergeable) return OverallStatusType.CodeRed;
            if (status >= RepoStatus.Behind) return OverallStatusType.WarnBehind;
            if (status >= RepoStatus.Dirty) return OverallStatusType.WarnAhead;
            return OverallStatusType.Ok;
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

        /// <summary>
        /// Converts DateTime to an integer representing the month in a readable format, e.g. 2018-03-01 becomes 201803
        /// </summary>
        public static int IntegerMonth(this DateTime date)
        {
           return date.Year * 100 + date.Month;
        }
    }
}
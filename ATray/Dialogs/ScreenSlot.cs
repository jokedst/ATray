namespace ATray
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using Activity;

    public class ScreenSlot
    {
        public ScreenSlot(uint startX, uint endX, int y, IEnumerable<ActivitySpan> activities)
        {
            StartX = (int)startX;
            EndX = (int)endX;
            Activities = new List<ActivitySpan>(activities);

            WasActive = Activities.Any(x => x.WasActive);
            var drawBox = new Rectangle(ActivityHistoryForm.GraphStartPixel + StartX, y, EndX - StartX,
                ActivityHistoryForm.GraphHeight - 1);
            if (!WasActive)
            {
                drawBox.Y += 15;
                drawBox.Height -= 30;
            }
            DrawBox = drawBox;
        }

        public int StartX { get; }
        public int EndX { get; }
        public List<ActivitySpan> Activities { get; }
        public bool WasActive { get; }
        public Rectangle DrawBox { get; }

        public string Describe(bool includeComputerName)
        {
            var sb = new StringBuilder();
            var first = !includeComputerName;
            foreach (var activity in Activities)
            {
                if (first) first = false; else sb.Append(Environment.NewLine);
                if (includeComputerName) sb.Append(activity.Owner.ComputerName).Append(": ");
                sb.Append(activity.ApplicationName());

                var title = activity.WindowTitle();
                if (!string.IsNullOrWhiteSpace(title)) sb.Append(" - ").Append(title);
            }
            return sb.ToString();
        }
    }
}
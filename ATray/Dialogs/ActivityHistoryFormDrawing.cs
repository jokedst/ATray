namespace ATray
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Activity;
    using NuGet;

    public partial class ActivityHistoryForm
    {
        private Bitmap _historyGraph;
        private readonly Dictionary<string, Brush> _textureBrushes = new Dictionary<string, Brush>();
        private readonly SolidBrush[] _programBrushes = { new SolidBrush(Color.BlueViolet), new SolidBrush(Color.CornflowerBlue), new SolidBrush(Color.Green), new SolidBrush(Color.DarkRed), new SolidBrush(Color.Olive)};
        private readonly SolidBrush[] _workBrushes = {new SolidBrush(Color.BlueViolet), new SolidBrush(Color.CornflowerBlue), new SolidBrush(Color.Green), new SolidBrush(Color.DarkRed)};

        private SolidBrush ActivityBrush(ActivitySpan activity, Dictionary<string, SolidBrush> brushLookup)
        {
            if (showWork.Checked)
            {
                return _workBrushes[(int)activity.Classification];
            }

            return brushLookup.GetValueOrDefault(activity.ApplicationName()) ?? _programBrushes.Last();
        }

        private void DrawHistory(Bitmap target, Dictionary<string, MonthActivities> history)
        {
            // Figure out when first activity started and when last activity ended (for the whole month)
            var firstTime = history.MinOrDefault(c => c.Value.Days.MinOrDefault(d => d.Value.First().StartSecond));
            var lastTime = history.MaxOrDefault(c => c.Value.Days.MaxOrDefault(d => d.Value.Last().EndSecond));

            // Create list of programs used, the most used first
            var programs = history.SelectMany(c => c.Value.Days.Values.SelectMany(x => x.Where(a => a.WasActive)
                    .Select(a => new
                    {
                        Id = c.Value.ApplicationNames[a.ApplicationNameIndex],
                        Seconds = a.EndSecond - a.StartSecond
                    })))
                .GroupBy(z => z.Id).ToDictionary(x => x.Key, a => a.Sum(s => s.Seconds))
                .OrderByDescending(x => x.Value);

            // Assign unique colors to the most used programs
            //var colors = new[] {Color.BlueViolet, Color.CornflowerBlue, Color.Green, Color.DarkRed};
            //var brushes = colors.Select(x => new SolidBrush(x)).ToArray();
            var brushLookup = programs.Zip(_programBrushes, (program, color) => new {program, color})
                .ToDictionary(x => x.program.Key, x => x.color);
            var idleBrush = new SolidBrush(Color.Gray);

            _graphFirstSecond = firstTime;

            var graphicsObj = Graphics.FromImage(_historyGraph);
            var pen = new Pen(Color.Olive, 1);
            var brush = new SolidBrush(Color.Olive);

            _graphWidth = (uint) (target.Width - 80);
            _graphSeconds = lastTime - firstTime;
            if (_graphSeconds == 0) _graphSeconds = 1;
            var daylineHeight = 2;

            // Draw hour lines
            var greypen = new Pen(Color.LightGray, 1);
            var firstHour = (int) Math.Ceiling(firstTime / 3600.0);
            var lastHour = (int) Math.Floor(lastTime / 3600.0);
            for (int x = firstHour; x <= lastHour; x++)
            {
                var xpixel = (((x * 3600) - firstTime) * _graphWidth) / _graphSeconds + GraphStartPixel;
                graphicsObj.DrawLine(greypen, xpixel, 10, xpixel, target.Height);
            }

            // Draw each day
            DaySlots.Clear();
            int currentY = GraphSpacing;
            foreach (var dayNumber in _indexToDaynumber)
            {
                DaySlots.Add(dayNumber, new List<ScreenSlot>());
                var dayHistory = history.Where(x => x.Value.Days.ContainsKey(dayNumber))
                    .ToDictionary(x => x.Key, x => x.Value.Days[dayNumber]);

                // At what pixel is this day's first and last activities?
                var todaysFirstSecond = dayHistory.Min(x => x.Value.FirstActiveSecond);
                var todaysLastSecond = dayHistory.Max(x => x.Value.LastActiveSecond);

                var startpixel = (todaysFirstSecond - firstTime) * _graphWidth / _graphSeconds;
                var endPixel = (todaysLastSecond - firstTime) * _graphWidth / _graphSeconds;

                // Draw a small line representing the whole day
                var graphbox = new Rectangle((int) startpixel + GraphStartPixel,
                    currentY + GraphHeight / 2 - daylineHeight / 2, (int) (endPixel - startpixel) + 1, daylineHeight);
                graphicsObj.DrawRectangle(pen, graphbox);

                var startz = dayHistory.SelectMany(x => x.Value)
                    .GroupBy(a => (a.StartSecond - firstTime) * _graphWidth / _graphSeconds)
                    .ToDictionary(x => x.Key, x => x.ToList());
                var endz = dayHistory.SelectMany(x => x.Value)
                    .GroupBy(a => (a.EndSecond - firstTime) * _graphWidth / _graphSeconds)
                    .ToDictionary(x => x.Key, x => x.ToList());
                var allPixels = startz.Keys.Concat(endz.Keys).Distinct()
                    .OrderBy(x => x).ToList();

                var activeActivities = new HashSet<ActivitySpan>();
                for (var index = 0; index < allPixels.Count-1; index++)
                {
                    startpixel = allPixels[index];
                    endPixel = allPixels[index + 1] -0;
                    // Add new activities
                    if (startz.ContainsKey(startpixel))
                        activeActivities.AddRange(startz[startpixel]);
                    // Remove ending activities
                    if (endz.ContainsKey(startpixel))
                        foreach (var endedSpan in endz[startpixel])
                            activeActivities.Remove(endedSpan);

                    var span = new ScreenSlot(startpixel, endPixel, currentY, activeActivities);
                    DaySlots[dayNumber].Add(span);

                    // Select brush
                    Brush thisBrush;
                    if (!span.WasActive)
                        thisBrush = idleBrush;
                    else if (activeActivities.Count(x => x.WasActive) == 1)
                    {
                        thisBrush = ActivityBrush(activeActivities.Single(x => x.WasActive), brushLookup);
                    }
                    else
                    {
                        var colorList = activeActivities.Select(a => ActivityBrush(a, brushLookup).Color)
                            .Distinct().OrderBy(x => x.Name).ToList();
                        var key = string.Join("|", colorList.Select(x => x.Name));
                        if (_textureBrushes.ContainsKey(key))
                            thisBrush = _textureBrushes[key];
                        else
                        {
                            var scale = 3;
                            var img = new Bitmap(1, colorList.Count * scale);
                            var ix = 0;
                            foreach (var color in colorList)
                            {
                                for (int i = 0; i < scale; i++)
                                {
                                    img.SetPixel(0, ix++, color);
                                }
                            }
                            thisBrush = new TextureBrush(img);
                            _textureBrushes[key] = thisBrush;
                        }
                    }

                    // Draw
                    graphicsObj.FillRectangle(thisBrush, span.DrawBox);
                }

                currentY += GraphSpacing + GraphHeight;
            }
            graphicsObj.Dispose();
        }

        public Dictionary<int, List<ScreenSlot>> DaySlots=new Dictionary<int, List<ScreenSlot>>();
    }
}

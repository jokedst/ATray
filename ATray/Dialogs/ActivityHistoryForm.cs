using System.Diagnostics;
using System.Drawing.Imaging;

namespace ATray
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using Activity;

    public partial class ActivityHistoryForm : Form
    {
        // 40 pixels margins
        private const int GraphStartPixel = 40;
        private const int GraphHeight = 50;
        private const int GraphSpacing = 20;
        private const int TimeLabelWidth = 40;

        private readonly List<Label> _timeLabels = new List<Label>();
        private readonly FloatingLabel _tipLabel = new FloatingLabel();
        private Bitmap _historyGraph;
        private int _lastWindowWidth;
        private DateTime _lastHistoryRedraw = DateTime.MinValue;

        private Point _lastPosition;
        private int _lastScrollPositionY;

        private int _currentMonth = (DateTime.Now.Year * 100) + DateTime.Now.Month;

        private bool _forceRedraw;

        private uint _graphWidth;
        private uint _graphSeconds;
        private uint _graphFirstSecond;
        private MonthActivities _shownHistory;
        private byte[] _indexToDaynumber;

        public ActivityHistoryForm()
        {
            InitializeComponent();
            ResizeRedraw = true;
            historyPicture.MouseMove += HistoryPictureOnMouseMove;
            historyPicture.MouseLeave += HistoryPictureOnMouseLeave;
            
            _tipLabel.Text = "hello";
            _tipLabel.AutoSize = true;
            _tipLabel.Hide();
#if DEBUG
            Icon = new Icon(GetType(), "debug.ico");
#endif
            InitHistoryDropDown();
        }

        private void InitHistoryDropDown()
        {
            // Check what files exists
            var rawMonths = ActivityManager.ListAvailableMonths();
            if (rawMonths.Count == 0)
            {
                MessageBox.Show("No history to show!");
                Close();
                return;
            }

            var months = rawMonths.Select(x => Tuple.Create(x.Key, new DateTime(x.Key / 100, x.Key % 100, 1).ToString("MMMM yyyy"))).ToList();
            monthDropDown.ValueMember = "item1";
            monthDropDown.DisplayMember = "item2";
            monthDropDown.DataSource = months;
            monthDropDown.SelectedValue = rawMonths.Keys.LastOrDefault();
#if DEBUG
            monthDropDown.SelectedValue = rawMonths.Keys.LastOrDefault() - 1;
#endif

            nextMonthButton.Enabled = false;
            lastMonthButton.Enabled = rawMonths.Count > 1;
        }

        private void HistoryPictureOnMouseLeave(object sender, EventArgs eventArgs)
        {
            if (_tipLabel.Visible) _tipLabel.Hide();
        }

        private void HistoryPictureOnMouseMove(object sender, MouseEventArgs e)
        {
            var p = Cursor.Position;
            var x = p.X;
            var y = p.Y + 20;

            if (_lastPosition.X == x && _lastPosition.Y == y && _lastScrollPositionY == e.Y) return;
            if (e.X < GraphStartPixel) return;

            _lastPosition = new Point(x, y);
            var second = (((uint)e.X - GraphStartPixel) * _graphSeconds / _graphWidth) + _graphFirstSecond;

            var absoluteY = y - historyPicture.Location.Y - Location.Y;
            var dayIndex = (e.Y - GraphSpacing / 2) / (GraphHeight + GraphSpacing);
            if (dayIndex < 0) dayIndex = 0;
            if (dayIndex >= _indexToDaynumber.Length) dayIndex = _indexToDaynumber.Length - 1;
            var dayNumber = _indexToDaynumber[dayIndex];
            var activity = _shownHistory.Days[dayNumber].FirstOrDefault(a => a.EndSecond >= second);
            if (activity?.StartSecond > second) activity = null;

            if (activity == null)
            {
                _tipLabel.Text = SecondToTime(second) + " (no activity)";
            }
            else
            {
                _tipLabel.Text = $"{SecondToTime(second)} {_shownHistory.ApplicationNames[activity.ApplicationNameIndex]}\r{_shownHistory.WindowTitles[activity.WindowTitleIndex]}";
            }
            //var astr = $"Act: {activity?.WasActive.ToString() ?? "unknown"}";
            //var astr = new DateTime(_shownHistory.Year, _shownHistory.Month, dayNumber).DayOfWeek + " " + dayNumber +"/" + _shownHistory.Month;

            // TODO: Look up activity item
            //_tipLabel.Text = SecondToTime(second);
            //_tipLabel.Text = $"{(e.Y - GraphSpacing / 2)} ({absoluteY}) day {dayIndex}";
            //_tipLabel.Text = astr;

            _tipLabel.Location = _lastPosition;
            _lastScrollPositionY = e.Y;

            if (!_tipLabel.Visible)
                _tipLabel.ShowFloating();
        }

        private void btnHistoryOk_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private string SecondToTime(uint second)
        {
            var hour = second / (60 * 60);
            var rest = second % (60 * 60);
            var minute = rest / 60;
            return $"{hour}:{minute:00}";
        }

        private void DrawHistory(Bitmap target, MonthActivities history)
        {
            // Figure out when first activity started and when last activity ended (for the whole month)
            var firstTime = (uint)60 * 60 * 24;
            var lastTime = (uint)0;
            foreach (List<ActivitySpan> activities in history.Days.Values)
            {
                firstTime = firstTime < activities.First().StartSecond ? firstTime : activities.First().StartSecond;
                lastTime = lastTime > activities.Last().EndSecond ? lastTime : activities.Last().EndSecond;
            }

            // Create list of programs used, the most used firts
            var programs = history.Days.Values.SelectMany(x => x.Where(a => a.WasActive))
                .GroupBy(x => x.ApplicationNameIndex).ToDictionary(x => x.Key,
                    spans => spans.Select(x => (int) x.EndSecond - x.StartSecond).Sum()).OrderByDescending(x=>x.Value);
            // Assign unique colors to the most used programs
            var colors = new[] {Color.BlueViolet,Color.CornflowerBlue, Color.Green, Color.DarkRed};
            var brushes = colors.Select(x => new SolidBrush(x)).ToArray();
            var brushLookup = programs.Zip(brushes, (program, color) => new {program, color}).ToDictionary(x=>x.program.Key,x=>x.color);

            _graphFirstSecond = firstTime;

            var graphicsObj = Graphics.FromImage(_historyGraph);
            var pen = new Pen(Color.Olive, 1);
            var brush = new SolidBrush(Color.Olive);

            _graphWidth = (uint) (target.Width - 80);
            _graphSeconds = lastTime - firstTime;
            var daylineHeight = 2;

            // Draw hour lines
            var greypen = new Pen(Color.LightGray, 1);
            var firstHour = (int)Math.Ceiling(firstTime / 3600.0);
            var lastHour = (int)Math.Floor(lastTime / 3600.0);
            for (int x = firstHour; x <= lastHour; x++)
            {
                var xpixel = (((x * 3600) - firstTime) * _graphWidth) / _graphSeconds + GraphStartPixel;
                graphicsObj.DrawLine(greypen, xpixel, 10, xpixel,target.Height);
            }

            // Draw each day
            int currentY = GraphSpacing;
            foreach (var dayNumber in history.Days.Keys.OrderBy(x => x))
            {
                // at what pixel is this day first and last activities?
                var todaysFirstSecond = 0u;
                var todaysLastSecond = 0u;
                if (history.Days[dayNumber].Any(x => x.WasActive))
                {
                    todaysFirstSecond = history.Days[dayNumber].First(x => x.WasActive).StartSecond;
                    todaysLastSecond = history.Days[dayNumber].Last(x => x.WasActive).EndSecond;
                }

                var startpixel = ((todaysFirstSecond - firstTime) * _graphWidth) / _graphSeconds;
                var endPixel = ((todaysLastSecond - firstTime) * _graphWidth) / _graphSeconds;

                // Draw a small line representing the whole day
                var graphbox = new Rectangle((int)startpixel + GraphStartPixel, currentY + GraphHeight / 2 - daylineHeight / 2, (int)(endPixel - startpixel) + 1, daylineHeight);
                graphicsObj.DrawRectangle(pen, graphbox);

                foreach (var span in history.Days[dayNumber])
                {
                    startpixel = ((span.StartSecond - firstTime) * _graphWidth) / _graphSeconds;
                    endPixel = ((span.EndSecond - firstTime) * _graphWidth) / _graphSeconds;

                    var top = currentY + (span.WasActive ? 0 : 15);
                    var boxheight = span.WasActive ? GraphHeight - 1 : GraphHeight - 31;

                    graphbox = new Rectangle((int)startpixel + GraphStartPixel, top, (int)(endPixel - startpixel) + 1, boxheight);

                    //graphicsObj.DrawRectangle(pen, graphbox);
                    graphicsObj.FillRectangle(brushLookup.GetValueOrDefault(span.ApplicationNameIndex,  brush), graphbox);
                }
                currentY += GraphSpacing + GraphHeight;
            }
            graphicsObj.Dispose();
        }

        private void ActivityHistoryForm_Paint(object sender, PaintEventArgs e)
        {
            // If we're showing the current month, redraw regularly so the image don't get stale
            var monthNow = DateTime.Now.Year * 100 + DateTime.Now.Month;
            var imageAgeInMinutes = DateTime.Now.Subtract(_lastHistoryRedraw).TotalMinutes;
            var imageGettingOld = _currentMonth == monthNow && imageAgeInMinutes > Program.Configuration.HistoryRedrawTimeout;

            if (_historyGraph != null 
                && ClientRectangle.Width == _lastWindowWidth
                && !imageGettingOld
                && !_forceRedraw)
            {
                return;
            }

            // get activity for selected year/month
            var history = ActivityManager.GetMonthActivity((short)(_currentMonth / 100), (byte)(_currentMonth % 100));
            _shownHistory = history;
            _indexToDaynumber = _shownHistory.Days.Keys.OrderBy(x => x).ToArray();

            // Create a new bitmap that is as wide as the windows and as high as it needs to be to fit all days
            var width = ClientRectangle.Width - SystemInformation.VerticalScrollBarWidth;
            var height = history.Days.Count * (GraphHeight + GraphSpacing) + GraphSpacing;

            // Only create a new Bitmap if needed
            Bitmap lastHistoryGraph = null;
            if (_historyGraph == null || width != _historyGraph.Width || height != _historyGraph.Height)
            {
                lastHistoryGraph = _historyGraph;
                _historyGraph = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                _historyGraph.MakeTransparent();
            }

            // Make sure we have as many labels as we need
            for (int i = _timeLabels.Count; i < history.Days.Count * 4; i++)
            {
                var label = new Label();
                label.AutoSize = true;
                label.BackColor = Color.Transparent;
                label.BringToFront();
                historyPicture.Controls.Add(label);
                var pos = i % 4;
                int y = GraphSpacing + (i/4)*(GraphSpacing + GraphHeight);
                label.Location = new Point(pos < 2 ? 0 : width - TimeLabelWidth, y + (pos == 0 || pos == 3 ? 0 : 20));
                _timeLabels.Add(label);
            }

            // Put correct text on labels
            int index = 0;
            foreach (var dayNumber in history.Days.Keys.OrderBy(x => x))
            {
                var todaysFirstSecond = history.Days[dayNumber].FirstOrDefault(x => x.WasActive)?.StartSecond??0;
                var todaysLastSecond = history.Days[dayNumber].LastOrDefault(x => x.WasActive)?.EndSecond??0;

                _timeLabels[index++].Text = new DateTime(history.Year, history.Month, dayNumber).DayOfWeek + " " + dayNumber + "/" + history.Month;
                _timeLabels[index++].Text = SecondToTime(todaysFirstSecond);
                _timeLabels[index].Location = new Point(width - TimeLabelWidth, _timeLabels[index].Location.Y);
                _timeLabels[index++].Text = SecondToTime(todaysLastSecond);
                _timeLabels[index].Location = new Point(width - TimeLabelWidth, _timeLabels[index].Location.Y);
                _timeLabels[index++].Text = "("+ SecondToTime(todaysLastSecond - todaysFirstSecond)+")";
            }

            DrawHistory(_historyGraph,history);

            // Replace old Bitmap if we had to create a new
            if (lastHistoryGraph != null || historyPicture.Image == null)
            {
                historyPicture.Image = _historyGraph;
                lastHistoryGraph?.Dispose();
            }

            _lastWindowWidth = ClientRectangle.Width;
            _lastHistoryRedraw = DateTime.Now;
            _forceRedraw = false;
        }

        private void ActivityHistoryForm_Resize(object sender, EventArgs e)
        {
            if (ClientRectangle.Width != _lastWindowWidth)
                Refresh();
        }

        private void historyPicture_MouseEnter(object sender, EventArgs e)
        {
            // To make the mouse weel scroll work
            historyPicture.Focus();
        }

        private void monthDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentMonth = (int)monthDropDown.SelectedValue;

            var availableMonths = monthDropDown.Items.Cast<Tuple<int, string>>().Select(x => x.Item1);
            lastMonthButton.Enabled = availableMonths.Any(x => x < _currentMonth);
            nextMonthButton.Enabled = availableMonths.Any(x => x > _currentMonth);

            _forceRedraw = true;
            Refresh();
        }

        private void lastMonthButton_Click(object sender, EventArgs e)
        {
            var earlierMonths =
                monthDropDown.Items.Cast<Tuple<int, string>>()
                             .Select(x => x.Item1)
                             .Where(x => x < _currentMonth)
                             .OrderByDescending(x => x);
            var previousMonth = earlierMonths.FirstOrDefault();

            monthDropDown.SelectedValue = previousMonth;
        }

        private void nextMonthButton_Click(object sender, EventArgs e)
        {
            var laterMonths =
                monthDropDown.Items.Cast<Tuple<int, string>>()
                             .Select(x => x.Item1)
                             .Where(x => x > _currentMonth)
                             .OrderBy(x => x);
            var nextMonth = laterMonths.FirstOrDefault();

            monthDropDown.SelectedValue = nextMonth;
        }

        protected override Point ScrollToControl(Control activeControl)
        {
            // Returning the current location prevents the panel from
            // scrolling to the active control when the panel loses and regains focus
            //return this.DisplayRectangle.Location;

            return base.ScrollToControl(activeControl);
        }
    }
}

using System.Diagnostics;
using ATray.Tools;

namespace ATray
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;
    using System.Windows.Forms;
    using Activity;

    public partial class ActivityHistoryForm : Form
    {
        // 40 pixels margins
        public const int GraphStartPixel = 40;
        public const int GraphHeight = 50;
        private const int GraphSpacing = 20;
        private const int TimeLabelWidth = 40;

        private readonly List<Label> _timeLabels = new List<Label>();
        private readonly FloatingLabel _tipLabel = new FloatingLabel();
        private int _lastWindowWidth;
        private DateTime _lastHistoryRedraw = DateTime.MinValue;

        private Point _lastPosition;
        private int _lastScrollPositionY;

        private int _currentMonth = DateTime.Now.IntegerMonth();

        private bool _forceRedraw;
        private bool _ignoreEvents;

        private uint _graphWidth;
        private uint _graphSeconds;
        private uint _graphFirstSecond;
        private byte[] _indexToDaynumber;

        private bool _showSharedHistory;
        private const string AllComputers = "*";

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
            InitComputerDropDown();
            InitHistoryDropDown();
        }

        private void InitComputerDropDown()
        {
            // Two magic strings: Empty string means current computer, '*' means all computers
            var computers = ActivityManager.GetSharedHistoryComputers().Select(x =>new Tuple<string,string> (x, x)).ToList();
            computers.RemoveAll(x => x.Item1 == Environment.MachineName);
            computers.Add(new Tuple<string, string>(AllComputers, "All"));
            computers.Insert(0, new Tuple<string, string>(string.Empty,$"{Environment.MachineName} (this computer)"));

            computerDropDown.ValueMember = "item1";
            computerDropDown.DisplayMember = "item2";
            computerDropDown.DataSource = computers;
            computerDropDown.SelectedValue = string.Empty;
            computerDropDown.SelectedValueChanged += computerDropDown_SelectedValueChanged;
        }

        private void InitHistoryDropDown()
        {
            // Check what files exists
            var rawMonths = ActivityManager.ListAvailableMonths((string)computerDropDown.SelectedValue);
            if (rawMonths.Count == 0)
            {
                MessageBox.Show("No history to show!", "ATray History");
                Close();
                return;
            }

            try
            {
                _ignoreEvents = true;
                var selectedMonth = monthDropDown.SelectedValue as int?;

                var months = rawMonths
                    .Select(x => Tuple.Create(x, new DateTime(x / 100, x % 100, 1).ToString("MMMM yyyy"))).ToList();
                monthDropDown.ValueMember = "item1";
                monthDropDown.DisplayMember = "item2";
                monthDropDown.DataSource = months;

                var month = selectedMonth.HasValue && rawMonths.Contains(selectedMonth.Value)
                    ? selectedMonth.Value
                    : rawMonths.LastOrDefault();
                monthDropDown.SelectedValue = month;

                nextMonthButton.Enabled = rawMonths.Any(x => x > month);
                lastMonthButton.Enabled = rawMonths.Any(x => x < month);
            }
            finally
            {
                _ignoreEvents = false;
            }
        }

        private void HistoryPictureOnMouseLeave(object sender, EventArgs eventArgs) => _tipLabel.Hide();

        /// <summary>
        /// Update tooltip when mouse moves over picture
        /// </summary>
        private void HistoryPictureOnMouseMove(object sender, MouseEventArgs e)
        {
            var p = Cursor.Position;
            var x = p.X;
            var y = p.Y + 20;

            if (_lastPosition.X == x && _lastPosition.Y == y && _lastScrollPositionY == e.Y) return;
            if (e.X < GraphStartPixel) return;
            if (_indexToDaynumber.Length == 0) return;

            _lastPosition = new Point(x, y);

           // var absoluteY = y - historyPicture.Location.Y - Location.Y;
            var dayIndex = (e.Y - GraphSpacing / 2) / (GraphHeight + GraphSpacing);
            if (dayIndex < 0) dayIndex = 0;
            if (dayIndex >= _indexToDaynumber.Length) dayIndex = _indexToDaynumber.Length - 1;
            var dayNumber = _indexToDaynumber[dayIndex];

            var second = (((uint)e.X - GraphStartPixel) * _graphSeconds / _graphWidth) + _graphFirstSecond;

            var graphPixel = e.X - GraphStartPixel;
            var slot = DaySlots.GetValueOrDefault(dayNumber).FirstOrDefault(d => d.EndX >= graphPixel);
            if (slot == null || slot.StartX > graphPixel)
                _tipLabel.Text = $"{SecondToTime(second)} (no activity)";
            else
                _tipLabel.Text = $"{SecondToTime(second)} {slot.Describe(_showSharedHistory)}";
            
            _tipLabel.Location = _lastPosition;
            _lastScrollPositionY = e.Y;

            if (!_tipLabel.Visible)
                _tipLabel.ShowFloating();
        }

        private void btnHistoryOk_Click(object sender, EventArgs e) => Hide();

        private string SecondToTime(uint second)
        {
            var hour = second / (60 * 60);
            var rest = second % (60 * 60);
            var minute = rest / 60;
            return $"{hour}:{minute:00}";
        }

        private void ActivityHistoryForm_Paint(object sender, PaintEventArgs e)
        {
            // If we're showing the current month, redraw regularly so the image don't get stale
            var now = DateTime.Now;
            var monthNow = now.Year * 100 + now.Month;
            var imageAgeInMinutes = now.Subtract(_lastHistoryRedraw).TotalMinutes;
            var imageGettingOld = _currentMonth == monthNow && imageAgeInMinutes > Program.Configuration.HistoryRedrawTimeout;

            if (_historyGraph != null 
                && ClientRectangle.Width == _lastWindowWidth
                && !imageGettingOld
                && !_forceRedraw)
            {
                return;
            }
            Log.Info(this, "Redrawing history because " + (_forceRedraw?"forced to":(ClientRectangle.Width != _lastWindowWidth)?"width changed": imageGettingOld?"it's getting stale":(_historyGraph==null)?"there is none yet":"aliens"));

            // Get activity for selected year/month
            var year = (short)(_currentMonth / 100);
            var month = (byte)(_currentMonth % 100);
            Dictionary<string, MonthActivities> history;

            var computer = (string) computerDropDown.SelectedValue;
            if (string.IsNullOrEmpty(computer))
            {
                history = new Dictionary<string, MonthActivities>
                {
                    [string.Empty] = ActivityManager.GetMonthActivity(year, month)
                };
            }
            else
            {
                if (computer == AllComputers) computer = null;
                history = ActivityManager.GetSharedMonthActivities(year, month, computer);
            }
            
            _indexToDaynumber = history.SelectMany(x=>x.Value.Days.Keys).Distinct().OrderBy(x => x).ToArray();
         
            // Create a new bitmap that is as wide as the windows and as high as it needs to be to fit all days
            var width = ClientRectangle.Width - SystemInformation.VerticalScrollBarWidth;
            var height = _indexToDaynumber.Length * (GraphHeight + GraphSpacing) + GraphSpacing;

            // Only create a new Bitmap if needed
            Bitmap lastHistoryGraph = null;
            if (_historyGraph == null || width != _historyGraph.Width || height != _historyGraph.Height)
            {
                lastHistoryGraph = _historyGraph;
                _historyGraph = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                _historyGraph.MakeTransparent();
            }

            // Make sure we have as many labels as we need
            for (int i = _timeLabels.Count; i < _indexToDaynumber.Length * 4; i++)
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
            foreach (var dayNumber in _indexToDaynumber)
            {
                var todaysFirstSecond = history.MinOrDefault(c => c.Value.Days.GetValueOrDefault(dayNumber)?
                                            .FirstOrDefault(x => x.WasActive)?.StartSecond) ?? 0;
                var todaysLastSecond = history.MaxOrDefault(c => c.Value.Days.GetValueOrDefault(dayNumber)?
                                           .LastOrDefault(x => x.WasActive)?.EndSecond) ?? 0;

                _timeLabels[index++].Text = new DateTime(year, month, dayNumber).DayOfWeek + " " + dayNumber + "/" + month;
                _timeLabels[index++].Text = SecondToTime(todaysFirstSecond);
                _timeLabels[index].Location = new Point(width - TimeLabelWidth, _timeLabels[index].Location.Y);
                _timeLabels[index++].Text = SecondToTime(todaysLastSecond);
                _timeLabels[index].Location = new Point(width - TimeLabelWidth, _timeLabels[index].Location.Y);
                _timeLabels[index++].Text = "("+ SecondToTime(todaysLastSecond - todaysFirstSecond)+")";
            }

            DrawHistory(_historyGraph, history);

            // Replace old Bitmap if we had to create a new
            if (lastHistoryGraph != null || historyPicture.Image == null)
            {
                historyPicture.Image = _historyGraph;
                lastHistoryGraph?.Dispose();
            }

            _lastWindowWidth = ClientRectangle.Width;
            _lastHistoryRedraw = now;
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
            if (_ignoreEvents) return;
            _currentMonth = (int)monthDropDown.SelectedValue;

            var availableMonths = monthDropDown.Items.Cast<Tuple<int, string>>().Select(x => x.Item1);
            lastMonthButton.Enabled = availableMonths.Any(x => x < _currentMonth);
            nextMonthButton.Enabled = availableMonths.Any(x => x > _currentMonth);

            _forceRedraw = true;
            Refresh();
        }

        private void lastMonthButton_Click(object sender, EventArgs e)
        {
            monthDropDown.SelectedValue = monthDropDown.Items.Cast<Tuple<int, string>>()
                .Where(x => x.Item1 < _currentMonth)
                .MaxOrDefault(x => x.Item1, monthDropDown.SelectedValue);
        }

        private void nextMonthButton_Click(object sender, EventArgs e)
        {
            monthDropDown.SelectedValue = monthDropDown.Items.Cast<Tuple<int, string>>()
                .Where(x => x.Item1 > _currentMonth)
                .MinOrDefault(x => x.Item1, monthDropDown.SelectedValue);
        }

        private void computerDropDown_SelectedValueChanged(object sender, EventArgs e)
        {
            var value = (string)computerDropDown.SelectedValue;
            _showSharedHistory = !string.IsNullOrEmpty(value);

            InitHistoryDropDown();

            _forceRedraw = true;
            Refresh();
        }

        private void OnShowWorkCheckboxChange(object sender, EventArgs e)
        {
            Trace.TraceInformation("work checkbox changed: " + showWork.Checked);
            ForceRedraw();
        }

        private void OnBlurCheckboxChange(object sender, EventArgs e)
        {
            ForceRedraw();
        }

        private void ForceRedraw()
        {
            _forceRedraw = true;
            Refresh();
        }
    }
}

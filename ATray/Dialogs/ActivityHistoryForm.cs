namespace ATray
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using Activity;
    using System.Diagnostics;

    public partial class ActivityHistoryForm : Form
    {
        // 40 pixels margins
        private const int GraphStartPixel = 40;
        private const int GraphHeight = 50;
        private const int GraphSpacing = 20;

        private readonly List<Label> timeLabels = new List<Label>();
        private readonly FloatingLabel tipLabel = new FloatingLabel();
        private Bitmap historyGraph = null;
        private int lastWindowWidth = 0;
        private DateTime lastHistoryRedraw = DateTime.MinValue;

        private Point lastPosition;
        private int lastScrollPositionY;

        private int currentMonth = (DateTime.Now.Year * 100) + DateTime.Now.Month;

        private bool forceRedraw = false;

        private uint graphWidth;
        private uint graphSeconds;
        private uint graphFirstSecond;
        
        public ActivityHistoryForm()
        {
            InitializeComponent();
            ResizeRedraw = true;
            historyPicture.MouseMove += HistoryPictureOnMouseMove;
            historyPicture.MouseLeave += HistoryPictureOnMouseLeave;
            
            tipLabel.Text = "hello";
            tipLabel.AutoSize = true;
            tipLabel.Hide();
#if DEBUG
            Icon = new Icon(GetType(), "debug.ico");
#endif

            InitHistoryDropDown();
        }

        private void InitHistoryDropDown()
        {
            // Check what files exists
            var rawMonths = ActivityManager.ListAvailableMonths();
            ////var months = rawMonths.Select(x => new { Id = x.Key, Name = new DateTime(x.Key / 100, x.Key % 100, 1).ToString("MMMM yyyy") }).ToList();

            ////monthDropDown.ValueMember = "Id";
            ////monthDropDown.DisplayMember = "Name";
            var months = rawMonths.Select(x => Tuple.Create(x.Key, new DateTime(x.Key / 100, x.Key % 100, 1).ToString("MMMM yyyy"))).ToList();

            monthDropDown.ValueMember = "item1";
            monthDropDown.DisplayMember = "item2";
            monthDropDown.DataSource = months;

            monthDropDown.SelectedValue = rawMonths.Keys.Last();

            nextMonthButton.Enabled = false;
            lastMonthButton.Enabled = rawMonths.Count > 1;
        }

        private void HistoryPictureOnMouseLeave(object sender, EventArgs eventArgs)
        {
            if (tipLabel.Visible) tipLabel.Hide();
        }

        private void HistoryPictureOnMouseMove(object sender, MouseEventArgs e)
        {
            var p = Cursor.Position;

            var x = p.X;
            var y = p.Y + 20;

            if (lastPosition.X != x || lastPosition.Y != y || lastScrollPositionY != e.Y)
            {
                lastPosition = new Point(x, y);

                if (e.X >= GraphStartPixel)
                {
                    tipLabel.Text = SecondToTime((((uint)e.X - GraphStartPixel) * graphSeconds / graphWidth) + graphFirstSecond);
                }
                else return;
                
                tipLabel.Location = lastPosition;
                lastScrollPositionY = e.Y;

                if(!tipLabel.Visible)
                    tipLabel.ShowFloating();
            }
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

        private void ActivityHistoryForm_Paint(object sender, PaintEventArgs e)
        {
            if (this.historyGraph != null 
                && this.ClientRectangle.Width == this.lastWindowWidth
                && !(DateTime.Now.Subtract(this.lastHistoryRedraw).TotalMinutes > Program.Configuration.HistoryRedrawTimeout)
                && !this.forceRedraw)
            {
                return;
            }

            var oldScroll = historyPanel.VerticalScroll.Value;

            // Allow user to select year/month
            var history = ActivityManager.GetMonthActivity((short)(this.currentMonth / 100), (byte)(this.currentMonth % 100));

            // Figure out when first activity started and when last activity ended (for the whole month)
            var firstTime = (uint)60 * 60 * 24;
            var lastTime = (uint)0;
            foreach (List<ActivitySpan> activities in history.Days.Values)
            {
                firstTime = firstTime < activities.First().StartSecond ? firstTime : activities.First().StartSecond;
                lastTime = lastTime > activities.Last().EndSecond ? lastTime : activities.Last().EndSecond;
            }

            this.graphFirstSecond = firstTime;

            // Create a new bitmap that is as wide as the windows and as high as it needs to be to fit all days
            var width = this.ClientRectangle.Width - SystemInformation.VerticalScrollBarWidth;
            var height = history.Days.Count * (GraphHeight + GraphSpacing);
            var lastHistoryGraph = this.historyGraph;
            this.historyGraph = new Bitmap(width: width,
                height: history.Days.Count * (GraphHeight + GraphSpacing),
                format: System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            this.historyGraph.MakeTransparent();

            // Make sure we have as many labels as we need
            for (int i = this.timeLabels.Count; i < history.Days.Count * 4; i++)
            {
                var label = new Label();
                label.AutoSize = true;
                label.BackColor = Color.Transparent;
                label.BringToFront();
                this.historyPicture.Controls.Add(label);
                this.timeLabels.Add(label);
            }

            var graphicsObj = Graphics.FromImage(this.historyGraph);
            var pen = new Pen(Color.Olive, 1);
            var brush = new SolidBrush(Color.Olive);

            this.graphWidth = (uint) width - 80;
            this.graphSeconds = lastTime - firstTime;
            var daylineHeight = 2;

            // Draw hour lines
            var greypen = new Pen(Color.LightGray, 1);
            var firstHour = (int)Math.Ceiling(firstTime / 3600.0);
            var lastHour = (int)Math.Floor(lastTime / 3600.0);
            for (int x = firstHour; x <= lastHour; x++)
            {
                var xpixel = (((x * 3600) - firstTime) * this.graphWidth) / this.graphSeconds + GraphStartPixel;
                graphicsObj.DrawLine(greypen, xpixel, 10, xpixel, height);
            }


            // Put labels
            int index = 0;
            int currentY = GraphSpacing;
            foreach (var dayNumber in history.Days.Keys.OrderBy(x => x))
            {
                var todaysFirstSecond = 0u;
                var todaysLastSecond = 0u;
                if (history.Days[dayNumber].Any(x => x.WasActive))
                {
                    todaysFirstSecond = history.Days[dayNumber].First(x => x.WasActive).StartSecond;
                    todaysLastSecond = history.Days[dayNumber].Last(x => x.WasActive).EndSecond;
                }

                var title = this.timeLabels[index++];
                title.Location = new Point(0, currentY);
                title.Text = new DateTime(history.Year, history.Month, dayNumber).DayOfWeek + " " + dayNumber + "/" + history.Month;

                var startTime = this.timeLabels[index++];
                startTime.Location = new Point(0, currentY + 20);
                startTime.Text = this.SecondToTime(todaysFirstSecond);

                var endTime = this.timeLabels[index++];
                endTime.Text = this.SecondToTime(todaysLastSecond);
                endTime.Location = new Point(width - endTime.Width, currentY + 20);

                var totalTime = this.timeLabels[index++];
                totalTime.Text = "("+this.SecondToTime(todaysLastSecond - todaysFirstSecond)+")";
                totalTime.Location = new Point(width - endTime.Width, currentY);

                var startpixel = ((todaysFirstSecond - firstTime) * this.graphWidth) / this.graphSeconds;
                var endPixel = ((todaysLastSecond - firstTime) * this.graphWidth) / this.graphSeconds;
                var graphbox = new Rectangle((int)startpixel + GraphStartPixel, currentY + GraphHeight / 2 - daylineHeight / 2, (int)(endPixel - startpixel) + 1, daylineHeight);

                graphicsObj.DrawRectangle(pen, graphbox);

                foreach (var span in history.Days[dayNumber])
                {
                    startpixel = ((span.StartSecond - firstTime) * this.graphWidth) / this.graphSeconds;
                    endPixel = ((span.EndSecond - firstTime) * this.graphWidth) / this.graphSeconds;

                    var top = currentY + (span.WasActive ? 0 : 15);
                    var boxheight = span.WasActive ? GraphHeight - 1 : GraphHeight - 31;

                    graphbox = new Rectangle((int)startpixel + GraphStartPixel, top, (int)(endPixel - startpixel) + 1, boxheight);

                    //graphicsObj.DrawRectangle(pen, graphbox);
                    graphicsObj.FillRectangle(brush, graphbox);
                }

                currentY += GraphSpacing + GraphHeight;
            }

            graphicsObj.Dispose();
            this.historyPicture.Image = this.historyGraph;
            lastHistoryGraph?.Dispose();

            //if (timeLabels.Any())
            //{
            //    foreach (var timeLabel in timeLabels)
            //    {
            //        historyPanel.Controls.Remove(timeLabel);
            //    }
            //    timeLabels.Clear();
            //}
            //{
            //    var label = new Label();
            //    label.Name = "label1";
            //    label.Text = "HEJ 13:4" + new Random().Next(10);
            //    label.AutoSize = true;
            //    label.Location = new Point(10, 15);
            //    label.BackColor = Color.Transparent;
            //    label.BringToFront();
            //    //label.Parent = historyPanel;
            //    historyPicture.Controls.Add(label);
            //    timeLabels.Add(label);
            //}
            this.lastWindowWidth = this.ClientRectangle.Width;
            this.lastHistoryRedraw = DateTime.Now;
            this.forceRedraw = false;
        }

        private void ActivityHistoryForm_Resize(object sender, EventArgs e)
        {
            if (this.ClientRectangle.Width != lastWindowWidth)
                this.Refresh();
        }

        private void historyPicture_MouseEnter(object sender, EventArgs e)
        {
            // To make the mouse weel scroll work
            historyPicture.Focus();
        }

        private void monthDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentMonth = (int)monthDropDown.SelectedValue;

            var availableMonths = monthDropDown.Items.Cast<Tuple<int, string>>().Select(x => x.Item1);
            lastMonthButton.Enabled = availableMonths.Any(x => x < currentMonth);
            nextMonthButton.Enabled = availableMonths.Any(x => x > currentMonth);

            forceRedraw = true;
            this.Refresh();
        }

        private void lastMonthButton_Click(object sender, EventArgs e)
        {
            var earlierMonths =
                monthDropDown.Items.Cast<Tuple<int, string>>()
                             .Select(x => x.Item1)
                             .Where(x => x < currentMonth)
                             .OrderByDescending(x => x);
            var previousMonth = earlierMonths.FirstOrDefault();

            monthDropDown.SelectedValue = previousMonth;
        }

        private void nextMonthButton_Click(object sender, EventArgs e)
        {
            var laterMonths =
                monthDropDown.Items.Cast<Tuple<int, string>>()
                             .Select(x => x.Item1)
                             .Where(x => x > currentMonth)
                             .OrderBy(x => x);
            var nextMonth = laterMonths.FirstOrDefault();

            monthDropDown.SelectedValue = nextMonth;
        }
    }
}

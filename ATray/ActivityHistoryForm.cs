using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace ATray
{
    public partial class ActivityHistoryForm : Form
    {
        private Bitmap historyGraph = null;
        private int lastWindowWidth = 0;
        private DateTime lastHistoryRedraw = DateTime.MinValue;
        private readonly List<Label> timeLabels = new List<Label>();

        private FloatingLabel tipLabel = new FloatingLabel();
        private Point lastPosition;
        private int lastScrollPositionY;

        uint graphWidth;
        uint graphSeconds;
        
        // 40 pixels margins
        private const int graphStartPixel = 40;

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
                //tipLabel.Text = "Mouse at " + e.X + ", " + e.Y;
                if (e.X >= graphStartPixel)
                {
                    tipLabel.Text = SecondToTime(((uint)e.X - graphStartPixel) * graphSeconds / graphWidth);
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
            var minute = rest/60;
            return string.Format("{0}:{1:00}", hour, minute);
        }

        private void ActivityHistoryForm_Paint(object sender, PaintEventArgs e)
        {
            if (historyGraph == null || this.ClientRectangle.Width != lastWindowWidth || DateTime.Now.Subtract(lastHistoryRedraw).TotalMinutes > Config.HistoryRedrawTimeout)
            {
                // TODO Allow user to select year/month
                var date = DateTime.Now;
                var history = ActivityManager.GetMonthActivity((short)date.Year, (byte)date.Month);

                // Figure out when first activity started and when last activity ended (for the whole month)
                var firstTime = (uint)60 * 60 * 24;
                var lastTime = (uint)0;
                foreach (List<ActivitySpan> activities in history.Days.Values)
                {
                    firstTime = firstTime < activities.First().StartSecond ? firstTime : activities.First().StartSecond;
                    lastTime = lastTime > activities.Last().EndSecond ? lastTime : activities.Last().EndSecond;
                }

                // Create a new bitmap that is as wide as the windows and as high as it needs to be to fit all days
                var width = this.ClientRectangle.Width - SystemInformation.VerticalScrollBarWidth;
                var height = history.Days.Count * (Config.GraphHeight + Config.GraphSpacing);
                historyGraph = new Bitmap(width: width,
                                          height: history.Days.Count * (Config.GraphHeight + Config.GraphSpacing),
                                          format: System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                historyGraph.MakeTransparent();

                // Make sure we have as many labels as we need
                for (int i = timeLabels.Count; i < history.Days.Count * 4; i++)
                {
                    var label = new Label();
                    label.AutoSize = true;
                    label.BackColor = Color.Transparent;
                    label.BringToFront();
                    historyPicture.Controls.Add(label);
                    timeLabels.Add(label);
                }

                var graphicsObj = Graphics.FromImage(historyGraph);
                Pen pen = new Pen(Color.Olive, 1);
                var brush = new SolidBrush(Color.Olive);

                graphWidth = (uint) width - 80;
                graphSeconds = lastTime - firstTime;
                var daylineHeight = 2;

                // Draw hour lines
                var greypen = new Pen(Color.LightGray, 1);
                var firstHour = (int)Math.Ceiling(firstTime / 3600.0);
                var lastHour = (int)Math.Floor(lastTime / 3600.0);
                for (int x = firstHour; x <= lastHour; x++)
                {
                    var xpixel = (x * 3600 * graphWidth) / graphSeconds + graphStartPixel;
                    graphicsObj.DrawLine(greypen, xpixel, 10, xpixel, height);
                }


                // Put labels
                int index = 0;
                int currentY = Config.GraphSpacing;
                foreach (var dayNumber in history.Days.Keys.OrderBy(x => x))
                {
                    var todaysFirstSecond = history.Days[dayNumber].First(x => x.WasActive).StartSecond;
                    var todaysLastSecond = history.Days[dayNumber].Last(x => x.WasActive).EndSecond;

                    var title = timeLabels[index++];
                    title.Location = new Point(0, currentY);
                    title.Text = (new DateTime(history.Year, history.Month, dayNumber).DayOfWeek) + " " + dayNumber + "/" + history.Month;

                    var startTime = timeLabels[index++];
                    startTime.Location = new Point(0, currentY + 20);
                    startTime.Text = SecondToTime(todaysFirstSecond);

                    var endTime = timeLabels[index++];
                    endTime.Text = SecondToTime(todaysLastSecond);
                    endTime.Location = new Point(width - endTime.Width, currentY + 20);

                    var totalTime = timeLabels[index++];
                    totalTime.Text = "("+SecondToTime(todaysLastSecond - todaysFirstSecond)+")";
                    totalTime.Location = new Point(width - endTime.Width, currentY);

                    var startpixel = (todaysFirstSecond * graphWidth) / graphSeconds;
                    var endPixel = (todaysLastSecond * graphWidth) / graphSeconds;
                    var graphbox = new Rectangle((int)startpixel + graphStartPixel, currentY + Config.GraphHeight / 2 - daylineHeight / 2, (int)(endPixel - startpixel)+1, daylineHeight);

                    graphicsObj.DrawRectangle(pen, graphbox);

                    foreach (var span in history.Days[dayNumber])
                    {
                        startpixel = (span.StartSecond * graphWidth) / graphSeconds;
                        endPixel = (span.EndSecond * graphWidth) / graphSeconds;

                        var top = currentY + (span.WasActive ? 0 : 15);
                        var boxheight = span.WasActive ? Config.GraphHeight - 1 : Config.GraphHeight - 31;

                        graphbox = new Rectangle((int)startpixel + graphStartPixel, top, (int)(endPixel - startpixel)+1, boxheight);

                        //graphicsObj.DrawRectangle(pen, graphbox);
                        graphicsObj.FillRectangle(brush, graphbox);
                    }

                    currentY += Config.GraphSpacing + Config.GraphHeight;
                }

                graphicsObj.Dispose();
                historyPicture.Image = historyGraph;

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
                lastWindowWidth = this.ClientRectangle.Width;
                lastHistoryRedraw = DateTime.Now;
            }
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
    }
}

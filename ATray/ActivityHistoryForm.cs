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
                tipLabel.Text = "Mouse at " + e.X + ", " + e.Y;
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
                historyGraph = new Bitmap(width: this.ClientRectangle.Width - SystemInformation.VerticalScrollBarWidth,
                                          height: history.Days.Count * (Config.GraphHeight + Config.GraphSpacing),
                                          format: System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                historyGraph.MakeTransparent();

                // Make sure we have as many labels as we need
                for (int i = timeLabels.Count; i < history.Days.Count*3; i++)
                {
                    var label = new Label();
                    label.AutoSize = true;
                    label.BackColor = Color.Transparent;
                    label.BringToFront();
                    historyPicture.Controls.Add(label);
                    timeLabels.Add(label);
                }


                // Put labels
                int index = 0;
                int currentY = Config.GraphSpacing;
                foreach (var dayNumber in history.Days.Keys.OrderBy(x => x))
                {
                    var title = timeLabels[index++];
                    title.Location = new Point(0, currentY);
                    title.Text = "Day " + dayNumber;

                    var startTime = timeLabels[index++];
                    startTime.Location = new Point(0, currentY + 20);
                    startTime.Text = history.Days[dayNumber].First().StartSecond.ToString();

                    var endTime = timeLabels[index++];
                    endTime.Location = new Point(200, currentY + 20);
                    endTime.Text = history.Days[dayNumber].Last().EndSecond.ToString();

                    currentY += Config.GraphSpacing + Config.GraphHeight;
                }

                var graphicsObj = Graphics.FromImage(historyGraph);

                // DEBUG just draw some shit
                Pen myPen = new Pen(Color.Plum, 3);
                Rectangle rectangleObj = new Rectangle(10, 10, 200, 200);

                graphicsObj.DrawEllipse(myPen, rectangleObj);
                graphicsObj.Dispose();
                historyPicture.Image = historyGraph;

                if (timeLabels.Any())
                {
                    foreach (var timeLabel in timeLabels)
                    {
                        historyPanel.Controls.Remove(timeLabel);
                    }
                    timeLabels.Clear();
                }
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

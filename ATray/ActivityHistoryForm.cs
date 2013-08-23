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
        private readonly List<Label> timeLabels = new List<Label>();

        public ActivityHistoryForm()
        {
            InitializeComponent();
            ResizeRedraw = true;
#if DEBUG
            Icon = new Icon(GetType(), "debug.ico");
#endif
        }

        private void btnHistoryOk_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void ActivityHistoryForm_Paint(object sender, PaintEventArgs e)
        {
            if (historyGraph == null || this.ClientRectangle.Width != lastWindowWidth)
            {
                Graphics graphicsObj;
                historyGraph = new Bitmap(this.ClientRectangle.Width - SystemInformation.VerticalScrollBarWidth, 400, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                historyGraph.MakeTransparent();
                graphicsObj = Graphics.FromImage(historyGraph);
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

                var label = new Label();
                label.Name = "label1";
                label.Text = "HEJ 13:4" + new Random().Next(10);
                label.AutoSize = true;
                label.Location = new Point(10, 15);
                label.BackColor = Color.Transparent;
                label.BringToFront();
                //label.Parent = historyPanel;
                historyPicture.Controls.Add(label);
                timeLabels.Add(label);

                lastWindowWidth = this.ClientRectangle.Width;
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

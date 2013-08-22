using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace ATray
{
    public partial class ActivityHistoryForm : Form
    {
        private Bitmap historyGraph = null;

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
            if (historyGraph == null)
            {
                Graphics graphicsObj;
                historyGraph = new Bitmap(this.ClientRectangle.Width,
                   this.ClientRectangle.Height,
                   System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                graphicsObj = Graphics.FromImage(historyGraph);
                Pen myPen = new Pen(System.Drawing.Color.Plum, 3);
                Rectangle rectangleObj = new Rectangle(10, 10, 200, 200);
                graphicsObj.DrawEllipse(myPen, rectangleObj);
                graphicsObj.Dispose();

                historyPicture.Image = historyGraph;
            }
            Graphics g = e.Graphics;

            //g.PageUnit = GraphicsUnit.Inch;

            var red1 = new Pen(Color.Green, 0.03f);

            g.DrawLine(red1, 10, 20, 30, 20);

            g.DrawLine(red1, 10, 30, 30, 30);

            g.DrawLine(red1, 30, 20, 30, 30);


            g.DrawLine(red1, 30, 20, 30, 30);

            


            var myFont = new Font("Helvetica", 40, FontStyle.Italic);

            Brush myBrush = new SolidBrush(Color.Red);

            g.DrawString(this.Width.ToString() + "/" + this.ClientRectangle.Width, myFont, myBrush, 30, 230);
        }
    }
}

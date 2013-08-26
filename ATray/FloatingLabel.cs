namespace ATray
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    /// <summary>
    /// A floating label that is shown over the borders of the actual program. 
    /// Also doesn't capture mouse events, they go to the window below it.
    /// </summary>
    public class FloatingLabel : Label
    {
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TOPMOST = 0x00000008;

        /// <summary>
        /// Get the <see cref="System.Windows.Forms.CreateParams"/>
        /// used to create the control.  This override adds the
        /// <code>WS_EX_NOACTIVATE</code>, <code>WS_EX_TOOLWINDOW</code>
        /// and <code>WS_EX_TOPMOST</code> extended styles to make
        /// the Window float on top.
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams p = base.CreateParams;
                p.ExStyle |= (WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);
                p.Parent = IntPtr.Zero;
                return p;
            }
        }

        [DllImport("user32")]
        private static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32")]
        private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// Shows the control as a floating Window child 
        /// of the desktop.  To hide the control again,
        /// use the <see cref="Visible"/> property.
        /// </summary>
        public void ShowFloating()
        {
            if (this.Handle == IntPtr.Zero)
            {
                base.CreateControl();
            }
            SetParent(base.Handle, IntPtr.Zero);
            ShowWindow(base.Handle, 1);
        }

        private const int WM_NCHITTEST = 0x0084;
        private const int HTTRANSPARENT = (-1);

        /// <summary>
        /// Overrides the standard Window Procedure to ensure the
        /// window is transparent to all mouse events.
        /// </summary>
        /// <param name="m">Windows message to process.</param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCHITTEST)
            {
                m.Result = (IntPtr)HTTRANSPARENT;
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }
}

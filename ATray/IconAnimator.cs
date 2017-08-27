namespace ATray
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    ///     Animates the notification icon
    /// </summary>
    public class IconAnimator
    {
        private readonly Icon[] _animationFrames;
        private readonly Icon _defaultIcon;
        private readonly NotifyIcon _notifyIcon;
        private readonly Timer _timer;
        private int _currentFrame;
        private int _repetitionsLeft;

        public IconAnimator(NotifyIcon notifyIcon, Bitmap animationFrames)
        {
            if (animationFrames == null)
                throw new ArgumentNullException(nameof(animationFrames), "No animation frames given");

            _notifyIcon = notifyIcon;
            _defaultIcon = _notifyIcon.Icon;
            _timer = new Timer();
            _timer.Tick += TimerTick;
            _animationFrames = new Icon[animationFrames.Width / 16];
            for (var i = 0; i < _animationFrames.Length; i++)
            {
                var rect = new Rectangle(i * 16, 0, 16, 16);
                var bmp = animationFrames.Clone(rect, animationFrames.PixelFormat);
                _animationFrames[i] = Icon.FromHandle(bmp.GetHicon());
            }
        }

        /// <summary> Starts or resets the animation </summary>
        /// <param name="loopCount"> How many loops to play</param>
        /// <param name="interval">Interval in millisecond in between each frame. Typicall 100</param>
        public void StartAnimation(int loopCount = 1, int interval = 100)
        {
            _repetitionsLeft = loopCount;
            _currentFrame = 0;
            _notifyIcon.Icon = _animationFrames[0];
            _timer.Interval = interval;
            _timer.Start();
        }

        /// <summary> Stops animation and resets icon </summary>
        public void StopAnimation()
        {
            _timer.Stop();
        }

        /// <summary>
        /// Shows the next frame
        /// </summary>
        private void TimerTick(object sender, EventArgs e)
        {
            if (++_currentFrame < _animationFrames.Length)
            {
                _notifyIcon.Icon = _animationFrames[_currentFrame];
            }
            else
            {
                _currentFrame = 0;
                if (--_repetitionsLeft <= 0)
                {
                    _timer.Stop();
                    _notifyIcon.Icon = _defaultIcon;
                }
            }
        }
    }
}
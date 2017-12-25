namespace RepositoryManager
{
    using System;
    using System.Threading;
    
    /// <summary>
    /// Represents a callback that will be called after a delay, unless it's scheduled again during that delay (in which case the new delay is used)
    /// </summary><inheritdoc />
    public class PostponedEvent : IDisposable
    {
        private readonly int _millisecondDelay;
        private readonly Action _callbackAction;
        private readonly int _maxUpdates;
        private readonly Timer _timer;
        private int _updatesSinceLastCallback;

        /// <summary>
        /// Creates a new callback that will be called after a delay once started
        /// </summary>
        /// <param name="millisecondDelay"> How long the delay should be between starting and firing the event </param>
        /// <param name="callbackAction"> Callback to call after the delay has ended </param>
        /// <param name="maxUpdates"> Max number of updates until the event should be fired even if the delay time isn't up </param>
        public PostponedEvent(int millisecondDelay, Action callbackAction, int maxUpdates = 0)
        {
            _millisecondDelay = millisecondDelay;
            _callbackAction = callbackAction;
            _maxUpdates = maxUpdates;

            _timer = new Timer(state =>
            {
                _updatesSinceLastCallback = 0;
                callbackAction();
            }, null, -1, -1);
        }

        /// <summary>
        /// Starts the delay countdown, or resets it if already active
        /// </summary>
        public void StartOrUpdate()
        {
            if (++_updatesSinceLastCallback >= _maxUpdates && _maxUpdates > 0)
            {
                _timer.Change(-1, -1);
                _callbackAction();
            }
            else
                _timer.Change(_millisecondDelay, -1);
        }

        /// <summary> Releases all resources used by the current instance of <see cref="PostponedEvent" />. </summary><inheritdoc />
        public void Dispose() => _timer.Dispose();
    }
}
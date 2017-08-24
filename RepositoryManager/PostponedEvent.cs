namespace RepositoryManager
{
    using System;
    using System.Threading;
    using System.Runtime.Caching;


    /// <summary>
    /// Represents a callback that will be called after a delay, unless it's scheduled again during that delay (in which case the new delay is used)
    /// </summary>
    public class PostponedEvent
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
        /// <param name="callbackAction"> Callback to call after teh delay has ended </param>
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
    }



    /// <summary>
    /// BAD! The memoryCache only evics stuff when new stuff is put in! :( crap
    /// Represents a callback that will be called after a delay, unless it's scheduled again during that delay (in which case the new delay is used)
    /// </summary>
    public class PostponedEventMemoryCache
    {
        private static readonly MemoryCache EventsToTrigger = new MemoryCache(nameof(PostponedEventMemoryCache));
        private readonly int _maxUpdates;
        private readonly CacheItemPolicy _cacheItemPolicy;
        private readonly CacheItem _cacheItem;
        private int _updatesSinceLastCallback;
        private readonly string _key;

        /// <summary>
        /// Creates a new callback that will be called after a delay once started
        /// </summary>
        /// <param name="millisecondDelay"> How long the delay should be between starting and firing the event </param>
        /// <param name="callbackAction"> Callback to call after teh delay has ended </param>
        /// <param name="maxUpdates"> Max number of updates until the event should be fired even if the delay time isn't up </param>
        public PostponedEventMemoryCache(int millisecondDelay, Action callbackAction, int maxUpdates = 0)
        {
            _maxUpdates = maxUpdates;
            _cacheItemPolicy = new CacheItemPolicy {
                SlidingExpiration = TimeSpan.FromMilliseconds(millisecondDelay),
                RemovedCallback = a =>
                {
                    _updatesSinceLastCallback = 0;
                    callbackAction();
                }
            };
            _key = Guid.NewGuid().ToString();
            _cacheItem = new CacheItem(_key, _key);
        }

        /// <summary>
        /// Starts the delay countdown, or resets it if already active
        /// </summary>
        public void StartOrUpdate()
        {
            if (++_updatesSinceLastCallback >= _maxUpdates && _maxUpdates > 0)
                EventsToTrigger.Remove(_cacheItem.Key);
            else
                //EventsToTrigger.Add(_cacheItem, _cacheItemPolicy);


            EventsToTrigger.AddOrGetExisting(_key, _key, _cacheItemPolicy);
        }
    }
}
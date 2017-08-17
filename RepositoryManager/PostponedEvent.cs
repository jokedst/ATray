namespace RepositoryManager
{
    using System;
    using System.Runtime.Caching;

    /// <summary>
    /// Represents a callback that will be called after a delay, unless it's scheduled again during that delay (in which case the new delay is used)
    /// </summary>
    public class PostponedEvent
    {
        private static readonly MemoryCache EventsToTrigger = new MemoryCache(nameof(PostponedEvent));
        private readonly int _maxUpdates;
        private readonly CacheItemPolicy _cacheItemPolicy;
        private readonly CacheItem _cacheItem;
        private int updatesSinceLastCallback;
        private string _key;

        /// <summary>
        /// Creates a new callback that will be called after a delay once started
        /// </summary>
        /// <param name="millisecondDelay"> How long the delay should be between starting and firing the event </param>
        /// <param name="callbackAction"> Callback to call after teh delay has ended </param>
        /// <param name="maxUpdates"> Max number of updates until the event should be fired even if the delay time isn't up </param>
        public PostponedEvent(int millisecondDelay, Action callbackAction, int maxUpdates = 0)
        {
            _maxUpdates = maxUpdates;
            _cacheItemPolicy = new CacheItemPolicy {
                SlidingExpiration = TimeSpan.FromMilliseconds(millisecondDelay),
                RemovedCallback = a =>
                {
                    updatesSinceLastCallback = 0;
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
            if (++updatesSinceLastCallback >= _maxUpdates && _maxUpdates > 0)
                EventsToTrigger.Remove(_cacheItem.Key);
            else
                //EventsToTrigger.Add(_cacheItem, _cacheItemPolicy);


            EventsToTrigger.AddOrGetExisting(_key, _key, _cacheItemPolicy);
        }
    }
}
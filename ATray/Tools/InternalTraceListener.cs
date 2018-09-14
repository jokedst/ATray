namespace ATray.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using Annotations;

    public interface IHaveLogs
    {
        string RegisterCallback(object id, Action<string> logListener);
        void Unregister(object id);
    }

    public class InternalTraceListener : TraceListener, IHaveLogs
    {
        private readonly Dictionary<object, Action<string>> _listeners = new Dictionary<object, Action<string>>();
        private readonly object _lockObject = new object();

        public InternalTraceListener(string name) : base(name)
        {
        }

        public List<string> Messages { get; } = new List<string>();

        /// <summary>
        /// Register a callback that gets all future log messages (the "feed") and returns what's currently in the log (the "seed")
        /// </summary>
        /// <param name="id"> object to use when unregistering </param>
        /// <param name="logListener"> Callback that will be called with every log message until unregistered </param>
        public string RegisterCallback(object id, Action<string> logListener)
        {
            lock (_lockObject)
            {
                _listeners[id] = logListener;
                return string.Join(Environment.NewLine, Messages);
            }
        }

        /// <summary>
        ///     Unregister a previously registered callback
        /// </summary>
        /// <param name="id"> object used when registering </param>
        public void Unregister(object id)
        {
            lock (_lockObject)
            {
                _listeners.Remove(id);
            }
        }

        public override void Write(string message)
        {
            lock (_lockObject)
            {
                Messages.Add(message);
                while (Messages.Count > 50)
                    Messages.RemoveAt(0);

                foreach (var callback in _listeners.Values) callback(message);
            }
        }

        public override void WriteLine(string message)
        {
            Write(message);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
            string message)
        {
            if (Filter?.ShouldTrace(eventCache, source, eventType, id, message, null, null, null) == false)
                return;

            WriteLine(message);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
            string format, [CanBeNull] params object[] args)
        {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, format, args, null, null))
                return;

            if (args != null)
                WriteLine(string.Format(CultureInfo.InvariantCulture, format, args));
            else
                WriteLine(format);
        }
    }
}
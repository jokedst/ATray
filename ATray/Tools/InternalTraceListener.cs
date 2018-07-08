using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace ATray.Tools
{
    public class InternalTraceListener : TraceListener
    {
        public List<string> Messages { get; } = new List<string>();

        public override void Write(string message)
        {
            Messages.Add(message);
            if (Messages.Count > 50)
                Messages.RemoveAt(0);
        }

        public override void WriteLine(string message)
        {
            this.Write(message);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
            string message)
        {
            if (Filter?.ShouldTrace(eventCache, source, eventType, id, message, null, null, null) == false)
                return;

            WriteLine(message);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
            string format, params object[] args)
        {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, format, args, null, null))
                return;

            //   WriteHeader(source, eventType, id);

            if (args != null)
                WriteLine(string.Format(CultureInfo.InvariantCulture, format, args));
            else
                WriteLine(format);
        }
    }
}

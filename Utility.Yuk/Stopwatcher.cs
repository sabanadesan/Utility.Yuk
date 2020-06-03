using System;
using System.Collections.Concurrent;
using System.Diagnostics;

using AspectInjector.Broker;

namespace Utility.Yuk
{
    public class StopwatcherData
    {

        // Auto-implemented readonly property:
        public string Message { get; set; }

        // Auto-implemented readonly property:
        public TimeSpan Elapsed { get; set; }

        public StopwatcherData(TimeSpan elapsed, string message)
        {
            Elapsed = elapsed;
            Message = message;
        }
    }

    [Aspect(Scope.Global)]
    [Injection(typeof(Stopwatcher))]
    public class Stopwatcher : Attribute
    {
        private static readonly ConcurrentBag<StopwatcherData> _data = new ConcurrentBag<StopwatcherData>();
        private Stopwatch _w;
        private Log _log;


        [Advice(Kind.Before)]
        public void LogEnter()
        {
            Log.Instance.WriteToConsole("Entering ...");
            Log.Instance.WriteToFile("Entering ...");

            _w = Stopwatch.StartNew();
        }

        [Advice(Kind.After)]
        public void LogExit([Argument(Source.Name)] string message)
        {
            Log.Instance.WriteToConsole("Leaving ...");
            Log.Instance.WriteToFile("Leaving ...");

            _w.Stop();
            _data.Add(new StopwatcherData(_w.Elapsed, message));

            Show();
        }

        public static void Track(Action action, string message)
        {
            var w = Stopwatch.StartNew();
            try
            {
                action();
            }
            finally
            {
                w.Stop();
                _data.Add(new StopwatcherData(w.Elapsed, message));
            }
        }

        public static T Track<T>(Func<T> func, string message)
        {
            var w = Stopwatch.StartNew();
            try
            {
                return func();
            }
            finally
            {
                w.Stop();
                _data.Add(new StopwatcherData(w.Elapsed, message));
            }
        }

        public static void Show()
        {
            if (_data.Count > 0)
            {
                int i = _data.Count - 1;

                StopwatcherData dt = _data.ToArray()[i];

                TimeSpan ts = dt.Elapsed;

                // Format and display the TimeSpan value.
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                
                Log.Instance.WriteToConsole(dt.Message + ":RunTime " + elapsedTime);
                Log.Instance.WriteToFile(dt.Message + ":RunTime " + elapsedTime);
            }
        }
    }
}

using System.Reflection;
[assembly: AssemblyTitle("Command line utility for ATray")]
[assembly: AssemblyProduct("atraycmd")]
[assembly: AssemblyCopyright("Copyright © 2017")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace atraycmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ATray.Activity;
    using Newtonsoft.Json;

    class Program
    {
        static void Main(string[] args)
        {
            Opts.Init(args);
          //  var filename = Opts.Parameter();
            var filename = Arg.Parameter();
            

            var acts = new MonthActivities(filename);

            Console.WriteLine($"ACTS {acts.Year}-{acts.Month} ({acts.Days.Count} days)");
            foreach (var day in acts.Days)
            {
                Console.WriteLine($"Day {day.Key} ({day.Value.Count} activities)");
                var first = day.Value.FirstOrDefault();
                Console.WriteLine($" first: {JsonConvert.SerializeObject(first)}");
                Console.WriteLine($" last: {JsonConvert.SerializeObject(day.Value.LastOrDefault())}");
            }
        }
    }

    internal class Opts
    {
        private static List<string> arguments;

        public static void Init(string[] args)
        {
            arguments = new List<string>( args);
        }

        public static LazyImplicit<string> Parameter()
        {
            return new LazyImplicit<string>(()=> arguments.First(x => !x.StartsWith("-")));
        }

        public class LazyImplicit<T> : Lazy<T>
        {
            public LazyImplicit(Func<T> valueFactory) : base(valueFactory) { }
            public static implicit operator T(LazyImplicit<T> lazy) => lazy.Value;
        }
    }

    /// <summary> A hands-off implementation of GetOpt. You just give you local values the right parameter </summary>
    public class Arg
    {
        private static readonly List<string> Arguments;
        private static readonly string ExeFileName;

        static Arg()
        {
            Arguments = Environment.GetCommandLineArgs().ToList();
            ExeFileName = Arguments[0];
            Arguments.RemoveAt(0);
        }

        public static LazyImplicit<string> Parameter()
        {
            return new LazyImplicit<string>(() => Arguments.First(x => !x.StartsWith("-")));
        }

        /// <summary> Lazy value that is implicitly converted to target type when used </summary>
        public class LazyImplicit<T> : Lazy<T>
        {
            public LazyImplicit(Func<T> valueFactory) : base(valueFactory) { }
            public static implicit operator T(LazyImplicit<T> lazy) => lazy.Value;
        }
    }
}

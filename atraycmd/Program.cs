using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace atraycmd
{
    using System.Reflection;
    using ATray.Activity;
    using Newtonsoft.Json;

    class Program
    {
        static void Main(string[] args)
        {
            Opts.Init(args);
            var filename = Opts.Parameter();

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
        public static string Parameter()
        {
            return arguments.First(x => !x.StartsWith("-"));
        }
    }
}

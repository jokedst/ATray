using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DiskUsage
{
    internal class Param
    {
        private static readonly List<string> Args;
        private static readonly HashSet<int> Claimed = new HashSet<int>();

        static Param() => Param.Args = ((IEnumerable<string>)Environment.GetCommandLineArgs()).ToList<string>();

        public static bool Flag(char flagChar, bool defaultValue = false) => Param.Args.Contains("-" + flagChar.ToString()) ? !defaultValue : defaultValue;

        public static T Get<T>(char parameterChar, T defaultValue = default)// where T : class
        {
            int num = Param.Args.IndexOf("-" + parameterChar.ToString());
            if (num == -1)
                return defaultValue;
            int index = num + 1;
            if (Param.Args.Count == index)
                Param.Error<T>(string.Format("Missing parameter value after '-{0}'", (object)parameterChar));
            Param.Claimed.Add(index - 1);
            Param.Claimed.Add(index);
            return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(Param.Args[index]);
        }

        private static void Error<T>(string message)
        {
            Console.Error.WriteLine(message);
            Environment.Exit(1);
        }
    }
}
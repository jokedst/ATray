﻿using System.Linq;
using System.Text.RegularExpressions;

namespace ATray
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Forms;

    internal static class Helpers
    {
        public static void Write(this BinaryWriter binaryWriter, IList<string> strings)
        {
            binaryWriter.Write(strings.Count);
            foreach (var str in strings)
            {
                binaryWriter.Write(str);
            }
        }

        public static IEnumerable<string> ReadStrings(this BinaryReader binaryReader)
        {
            var count = binaryReader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                yield return binaryReader.ReadString();
            }
        }

        public static int IndexOfOrAdd<T>(this IList<T> list, T item)
        {
            if (item == null) return -1;
            var index = list.IndexOf(item);
            if (index != -1) return index;

            list.Add(item);
            return list.Count - 1;
        }

        /// <summary>
        /// Checks if a given program (e.g. hej.exe) exists, using the PATH env. variable
        /// </summary>
        /// <param name="programName"></param>
        /// <returns></returns>
        public static bool ProgramExists(string programName)
        {
            if (programName.IndexOf('.') == -1) programName += ".*";
            var paths = Environment.GetEnvironmentVariable("PATH")?.Split(';');
            if (paths == null) return false;

            var reSearchPattern = new Regex(@"\.exe$|\.com$|\.bat$", RegexOptions.IgnoreCase);
            return paths
                .Where(Directory.Exists)
                .SelectMany(apath => Directory
                    .EnumerateFiles(apath, programName)
                    .Where(file => reSearchPattern.IsMatch(file)))
                .Any();
        }
    }

    internal static class ControlExtensions
    {
        /// <summary>
        /// Executes the Action asynchronously on the UI thread, does not block execution on the calling thread.
        /// </summary>
        public static void UIThread(this Control @this, Action code)
        {
            if (@this.InvokeRequired)
            {
                @this.BeginInvoke(code);
            }
            else
            {
                code.Invoke();
            }
        }
    }
}

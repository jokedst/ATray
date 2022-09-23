using System;
using System.IO;
using System.IO.Filesystem.Ntfs;
using System.IO.Pipes;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

// Attempt to rewrite DiskUsage, since I lost my source files
// This program has to be run as Admin! So a bit tricky to debug.
namespace DiskUsage
{
    class Program
    {
        static void Main(string[] args)
        {
            int num = 0;
            TextWriter textWriter = Console.Out;
            char driveName = 'c';
            bool verbose = Param.Flag('v');
            int maxDepth = Param.Get('D', int.MaxValue);
            bool compact = Param.Flag('c');
            PipeStream pipeStream = null;
            NamedPipeClientStream pipeClientStream = null;
            while (args.Length > num)
            {
                string input = args[num++];
                if (input == "-o")
                    textWriter = new StreamWriter(args[num++]);
                else if (input.Length == 1)
                    driveName = input[0];
                else if (Regex.IsMatch(input, "[a-z](:(\\\\))"))
                    driveName = input[0];
                else if (input == "-A")
                {
                    string pipeHandleAsString = args[num++];
                    Console.WriteLine("Opening anonymous pipe " + pipeHandleAsString);
                    pipeStream = new AnonymousPipeClientStream(PipeDirection.Out, pipeHandleAsString);
                    Console.WriteLine("Opening pipe for writing");
                    textWriter = new StreamWriter(pipeStream);
                }
                else if (input == "-N")
                {
                    string pipeName = args[num++];
                    Console.WriteLine("Opening named pipe " + pipeName);
                    pipeClientStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
                    pipeClientStream.Connect();
                    textWriter = new StreamWriter(pipeClientStream);
                }
            }
            DiagnosticTimer diagnosticTimer = new DiagnosticTimer();
            DriveInfo driveInfo = new DriveInfo(driveName.ToString());
            diagnosticTimer.Checkpoint("drive info");
            NtfsReader ntfsReader = new NtfsReader(driveInfo, RetrieveMode.Minimal, (done, total) => Console.Error.WriteLine("{0:#.#}% done ({1} of {2} nodes)", 100.0 * done / total, done, total));
            diagnosticTimer.Checkpoint("constructor");
            TreeNode tree = ntfsReader.GetTree();
            diagnosticTimer.Checkpoint("get tree");
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new TreeNodeConverter(maxDepth, compact));
            string str = JsonConvert.SerializeObject(tree, settings);
            textWriter.WriteLine(str);
            pipeStream?.WaitForPipeDrain();
            textWriter.Dispose();
            pipeStream?.Dispose();
            pipeClientStream?.Dispose();
            if (!verbose)
                return;
            Console.Error.WriteLine("\n=== Statistics:\n" + diagnosticTimer.LastCheckpoint("Exit"));
        }
    }
}

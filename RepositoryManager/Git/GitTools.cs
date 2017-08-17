namespace RepositoryManager.Git
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// A collection of git helper tools
    /// </summary>
    public class GitTools
    {
        /// <summary>
        /// This method uses git.exe, which must be present and in the user PATH
        /// </summary>
        public static string RunGitCommand(string command, string workingDir=null, int timeout = 60000)
        {
            return RunShit("git.exe", command, workingDir, timeout);
        }

        /// <summary>
        /// Runs a command as an external process
        /// </summary>
        protected static string RunShit(string program, string arguments, string workingDir=null, int timeout = 60000)
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = program;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                if (!string.IsNullOrWhiteSpace(workingDir)) process.StartInfo.WorkingDirectory = workingDir;

                var output = new StringBuilder();
                var error = new StringBuilder();

                using (var outputWaitHandle = new AutoResetEvent(false))
                using (var errorWaitHandle = new AutoResetEvent(false))
                {
                    process.OutputDataReceived += (sender, e) => {
                        if (e.Data == null)
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            output.AppendLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            error.AppendLine(e.Data);
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (process.WaitForExit(timeout) &&
                        outputWaitHandle.WaitOne(timeout) &&
                        errorWaitHandle.WaitOne(timeout))
                    {
                        // Process completed. Check process.ExitCode here.
                        if (process.ExitCode != 0) 
                            throw new Exception($"External program return with code {process.ExitCode}:\n{error}");
                        return output.ToString();
                    }
                    // Timed out.
                    throw new TimeoutException($"Timout when running external command '{program}'");
                }
            }

        }
    }
}
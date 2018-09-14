namespace ATray.Tools
{
    using System.Diagnostics;
    using System.Windows.Forms;

    public class Log
    {
        /// <summary>
        /// Shows an error message to the user (and logs it)
        /// </summary>
        public static void ShowError(object sender, string message)
        {
            Trace.TraceError("ERROR in {0}: {1}", sender.GetType().Name, message);
            MessageBox.Show(message, "ATray Error");
        }

        public static void Info(object sender, string message)
        {
            Trace.TraceInformation("INFO from {0}: {1}", sender.GetType().Name, message);
        }

        public static void Warning(object sender, string message)
        {
            Trace.TraceWarning("WARN from {0}: {1}", sender.GetType().Name, message);
        }
    }
}
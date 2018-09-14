namespace ATray.Tools
{
    using System.Diagnostics;
    using System.Windows.Forms;

    public class LogHelper
    {
        public static void LogError(object sender, string message)
        {
            Debug.WriteLine(message);
            MessageBox.Show(message, "Error in ATray");
        }

        public static void LogInfo(object sender, string message)
        {
            Debug.WriteLine(message);
        }
    }
}
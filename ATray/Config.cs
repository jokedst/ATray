using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATray
{
    /// <summary>
    /// All configurable values should go here
    /// </summary>
    public static class Config
    {
        public static int HistoryRedrawTimeout
        {
            get { return 10; }
        }

        public static int GraphHeight
        {
            get { return 50; }
        }

        public static int GraphSpacing
        {
            get { return 20; }
        }

        public static bool StoreActiveApp
        {
            get { return true; }
        }

        public static bool StoreActiveAppTitle
        {
            get { return true; }
        }
    }
}

using System.Drawing;
using System.Windows.Forms;

namespace ATray.Dialogs
{
    /// <summary>
    /// Helper class to preserve scroll position in a panel with AutoScroll and large content 
    /// </summary>
    public class AutoScrollPanel : Panel
    {
        protected override Point ScrollToControl(Control activeControl)
        {
            return AutoScrollPosition;
        }
    }
}
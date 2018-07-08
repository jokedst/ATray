using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace ATray.Activity
{
    public interface IActivityMonitor
    {
        string CurrentlyActiveWindow { get; }
        bool HasTakenBreak { get; }
        bool HasWorkedTooLong { get; }
        string IdleTime { get; }
        string WorkingTime { get; }

        event PropertyChangedEventHandler PropertyChanged;
        event Action<object, EventArgs> UserHasTakenBreak;
        event Action<object, EventArgs> UserIsBackFromAbsense;
        event Action<object, EventArgs> UserWorkedTooLong;

        void HandleWindowsMessage(Message m);
    }
}
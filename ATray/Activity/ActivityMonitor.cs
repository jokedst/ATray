using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ATray.Annotations;
using Microsoft.Win32;

namespace ATray.Activity
{
    /// <summary>
    ///     Monitors if the user is active or not, and for how long
    /// </summary>
    public class ActivityMonitor : INotifyPropertyChanged, IActivityMonitor
    {
        private uint _workingtime;
        private string _workingTime;
        private string _currentlyActiveWindow;
        private string _idleTime;
        private DateTime _lastSave = DateTime.MinValue;
        private DateTime _lastTimerEvent = DateTime.MinValue;
        private DateTime _startTime = DateTime.Now;
        private readonly Timer _mainTimer = new Timer {Interval = 1000};

        public ActivityMonitor()
        {
            _mainTimer.Tick += OnMainTimerTick;
            _mainTimer.Start();

            SystemEvents.SessionSwitch += SystemEventsOnSessionSwitch;
        }

        public bool InWarnState { get; set; }

        public bool HasWorkedTooLong { get; private set; }
        public bool HasTakenBreak { get; private set; }

        public event Action<object, EventArgs> UserWorkedTooLong;
        public event Action<object, EventArgs> UserHasTakenBreak;
        public event Action<object, EventArgs> UserIsBackFromAbsense;
        public event PropertyChangedEventHandler PropertyChanged;

        public string IdleTime
        {
            get => _idleTime;
            private set => SetProperty(ref _idleTime, value);
        }

        public string WorkingTime
        {
            get => _workingTime;
            private set => SetProperty(ref _workingTime, value);
        }

        public string CurrentlyActiveWindow
        {
            get => _currentlyActiveWindow;
            private set => SetProperty(ref _currentlyActiveWindow, value);
        }

        [NotifyPropertyChangedInvocator]
        private void SetProperty<T>(ref T underlyingField, T newValue, [CallerMemberName] string propertyName = null)
        {
            // someone: if (EqualityComparer<T>.Default.Equals(underlyingField, newValue)) return;
            if (newValue.Equals(underlyingField)) return;
            underlyingField = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private MonitorState LastMonitorState = MonitorState.On;

        public void HandleWindowsMessage( Message m)
        {
            // Detect closing/opening of lid
            if (m.Msg == NativeMethods.WM_POWERBROADCAST && m.WParam.ToInt32() == NativeMethods.PBT_POWERSETTINGCHANGE)
            {
                var pData = (IntPtr) (m.LParam.ToInt32() + Marshal.SizeOf<NativeMethods.POWERBROADCAST_SETTING>());
                var iData = (int) Marshal.PtrToStructure(pData, typeof(int));
                var monitorState = (MonitorState) iData;

                if (monitorState != LastMonitorState)
                {
                    // Technically this could happen when only turning off ONE screen in a multiscreen setup, but meh
                    Trace.TraceInformation("Monitor changed to " + monitorState);
                    if (monitorState == MonitorState.Off && Program.Configuration.LockOnMonitorOff)
                        NativeMethods.LockWorkStation();
                    LastMonitorState = monitorState;
                }
            }
        }

        private void SystemEventsOnSessionSwitch(object sender, SessionSwitchEventArgs sessionSwitchEventArgs)
        {
            // When logging in or unlocking we want to update immediatly
            if (sessionSwitchEventArgs.Reason == SessionSwitchReason.SessionLogon ||
                sessionSwitchEventArgs.Reason == SessionSwitchReason.SessionUnlock)
                //_repositoryCollection.TriggerUpdate(r => r.UpdateSchedule != Schedule.Never);
                UserIsBackFromAbsense?.Invoke(this, EventArgs.Empty);

            Trace.TraceInformation("Session changed ({0})", sessionSwitchEventArgs.Reason);
        }

        private void OnMainTimerTick(object sender, EventArgs e)
        {
            var idleMilliseconds = NativeMethods.GetIdleMilliseconds();
            // Only call "Now" once to avoid annoying bugs
            var now = DateTime.Now;

            var unpoweredMilliseconds = (uint) Math.Min(now.Subtract(_lastTimerEvent).TotalMilliseconds, uint.MaxValue);
            if (unpoweredMilliseconds > 100_000)
            {
                Trace.TraceInformation("No timer events for {0} ms - unpowered?", unpoweredMilliseconds);
                idleMilliseconds = Math.Max(idleMilliseconds, unpoweredMilliseconds - 2000);
            }

            //lblSmall.Text =Helpers.MillisecondsToString(idle);
            IdleTime = Helpers.MillisecondsToString(idleMilliseconds);

            if (idleMilliseconds > Program.Configuration.MinimumBrakeLength * 1000)
            {
                _workingtime = 0;
                _startTime = now;
                if (InWarnState)
                {
                    UserHasTakenBreak?.Invoke(this, EventArgs.Empty);
                    InWarnState = false;
                }
            }
            else
            {
                _workingtime += (uint) now.Subtract(_startTime).TotalMilliseconds;
                _startTime = now;

                if (_workingtime > Program.Configuration.MaximumWorkTime * 1000 && !InWarnState)
                {
                    InWarnState = true;
                    UserWorkedTooLong?.Invoke(this, EventArgs.Empty);
                }
            }

            NativeMethods.GetForegroundProcessInfo(out var foregroundApp, out var foregroundTitle);

            if (now.Subtract(_lastSave).TotalSeconds > Program.Configuration.SaveInterval)
            {
                // Time to save
                var wasActive = idleMilliseconds < Program.Configuration.SaveInterval * 1000;
                ActivityManager.SaveActivity(now, (uint) Program.Configuration.SaveInterval, wasActive, foregroundApp,
                    foregroundTitle);
                _lastSave = now;
            }

            WorkingTime = Helpers.MillisecondsToString(_workingtime);
            CurrentlyActiveWindow = foregroundApp + " : " + foregroundTitle;
            _lastTimerEvent = now;
        }
    }
}
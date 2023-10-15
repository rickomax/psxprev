using System;

namespace PSXPrev.Forms.Utils
{
    // Helper class to delay refreshing controls to reduce lag
    public class RefreshDelayTimer
    {
        // Time is in seconds
        public bool NeedsRefresh { get; private set; }
        public double ElapsedSeconds { get; private set; }
        public double Interval { get; set; }
        public bool AutoReset { get; set; }

        public event Action Elapsed;

        public RefreshDelayTimer(double intervalSeconds = 1d / 1000d)
        {
            Interval = intervalSeconds;
        }

        // Start the timer but keep the current elapsed time
        public void Start()
        {
            NeedsRefresh = true;
        }

        // Stop the timer but keep the current elapsed time
        public void Stop()
        {
            NeedsRefresh = false;
        }

        // Stop the timer and reset the elapsed time
        public void Reset()
        {
            NeedsRefresh = false;
            ElapsedSeconds = 0d;
        }

        // Start the timer and reset the elapsed time
        public void Restart()
        {
            NeedsRefresh = true;
            ElapsedSeconds = 0d;
        }

        // Finish the timer and raise the event if NeedsRefresh is true
        public bool Finish() => AddTime(Interval);

        // Update the timer if NeedsRefresh is true, and raise the event if finished
        public bool AddTime(double seconds)
        {
            if (NeedsRefresh)
            {
                ElapsedSeconds += seconds;
                if (ElapsedSeconds >= Interval)
                {
                    NeedsRefresh = AutoReset;
                    ElapsedSeconds = 0d;

                    Elapsed?.Invoke();
                    return true;
                }
            }
            return false;
        }
    }
}

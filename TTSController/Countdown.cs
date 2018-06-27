using System;

namespace Controller
{
    public class CountDownEventArgs : EventArgs
    {
        public int Duration { get; private set; }

        public CountDownEventArgs(int duration)
        {
            Duration = duration;
        }
    }

    public class CountDown
    {
        public int Duration { get; private set; } = 0;
        public int Current { get; private set; } = 0;

        public bool IsComplete => Current == 0;

        public EventHandler<CountDownEventArgs> Completed;

        public CountDown() { }

        public void Reset(int duration)
        {
            // if (duration <= 0) throw new ArgumentOutOfRangeException("Duration must be positive and greater than zero");

            Duration = duration;
            Current = duration;
        }

        public void Decrement(int amount = 1)
        {
            if (Current > 0) // throw new InvalidOperationException("CountDown has already completed");
            {
                Current -= amount;

                if (Current <= 0)
                {
                    Current = 0;

                    if (Completed != null)
                    {
                        Completed.Invoke(this, new CountDownEventArgs(Duration));
                    }
                }
            }
        }
    }
}

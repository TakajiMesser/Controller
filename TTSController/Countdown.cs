using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTSController
{
    public class CountDownEventArgs : EventArgs
    {
        private int _duration;

        public int Duration { get { return _duration; } }

        public CountDownEventArgs(int duration)
        {
            _duration = duration;
        }
    }

    public class CountDown
    {
        private int _duration = 0;
        private int _currentValue = 0;

        public int Duration { get { return _duration; } }
        public int Current { get { return _currentValue; } }

        public bool IsComplete { get { return _currentValue == 0; } }

        public EventHandler<CountDownEventArgs> Completed;

        public CountDown() { }

        public void Reset(int duration)
        {
            if (duration <= 0) throw new ArgumentOutOfRangeException("Duration must be positive and greater than zero");

            _duration = duration;
            _currentValue = duration;
        }

        public void Decrement(int amount = 1)
        {
            if (_currentValue > 0) // throw new InvalidOperationException("CountDown has already completed");
            {
                _currentValue -= amount;

                if (_currentValue <= 0)
                {
                    _currentValue = 0;
                    if (Completed != null)
                    {
                        Completed.Invoke(this, new CountDownEventArgs(_duration));
                    }
                }
            }
        }
    }
}

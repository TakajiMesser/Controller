using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTSController.Phasing
{
    public enum PhaseStates
    {
        Green,
        Yellow,
        Red
    }

    public class Phase
    {
        private int _id;
        private PhaseStates _state = PhaseStates.Red;
        private CountDown _forceOffTimer = new CountDown();
        private CountDown _maxGreenTimer = new CountDown();
        private CountDown _minGreenTimer = new CountDown();
        private CountDown _yellowTimer = new CountDown();
        private CountDown _redClearanceTimer = new CountDown();
        private bool _hasCall = false;
        private bool _hasOpposingCall = false;
        private bool _isCoordinated = false;
        private bool _floatingForceOff = false;
        private HashSet<int> _conflictPhases = new HashSet<int>();

        public int ID { get { return _id; } }
        public PhaseStates State { get { return _state; } }
        public int Split { get; set; }
        public int MinGreen { get; set; }
        public double MaxGreen { get; set; }
        public double Yellow { get; set; }
        public double RedClearance { get; set; }
        public bool IsCoordinated { get { return _isCoordinated; } set { _isCoordinated = value; } }
        public bool FloatingForceOff { get { return _floatingForceOff; } set { _floatingForceOff = value; } }
        public bool HasCall { get { return _hasCall; } }
        public bool HasOpposingCall { get { return _hasOpposingCall; } }

        internal bool IsZero
        {
            get
            {
                return (_state == PhaseStates.Red)
                        && (_forceOffTimer != null) && (_forceOffTimer.IsComplete)
                        && (_maxGreenTimer != null) && (_maxGreenTimer.IsComplete)
                        && (_minGreenTimer != null) && (_minGreenTimer.IsComplete)
                        && (_yellowTimer != null) && (_yellowTimer.IsComplete)
                        && (_redClearanceTimer != null) && (_redClearanceTimer.IsComplete);
            }
        }
        internal int ForceOffPoint { get; set; }
        internal HashSet<int> ConflictPhases { get { return _conflictPhases; } }

        public Phase(int id)
        {
            if (id < 0) throw new ArgumentOutOfRangeException("Phase ID must be positive");
            _id = id;
        }
        public Phase(int id, int split)
        {
            if (id < 0) throw new ArgumentOutOfRangeException("Phase ID must be positive");
            if (split < 0) throw new ArgumentOutOfRangeException("Split must be positive");

            _id = id;
            Split = split;
        }

        internal void PlaceCall()
        {
            _hasCall = true;
        }

        internal void PlaceOpposingCall()
        {
            _hasOpposingCall = true;
        }

        internal void Advance(int nSeconds)
        {
            switch (_state)
            {
                case PhaseStates.Green:
                    _minGreenTimer.Decrement(nSeconds);
                    _forceOffTimer.Decrement(nSeconds);
                    _maxGreenTimer.Decrement(nSeconds);

                    if (_minGreenTimer.IsComplete)
                    {
                        if (_isCoordinated)
                        {
                            if (_forceOffTimer.IsComplete && _hasOpposingCall) TransitionToYellow();
                        }
                        else
                        {
                            if (_forceOffTimer.IsComplete) TransitionToYellow();
                            if (_maxGreenTimer.IsComplete) TransitionToYellow();
                        }
                    }
                    break;
                case PhaseStates.Yellow:
                    _yellowTimer.Decrement(nSeconds);
                    if (_yellowTimer.IsComplete) TransitionToRed();
                    break;
                case PhaseStates.Red:
                    _redClearanceTimer.Decrement(nSeconds);
                    if (_redClearanceTimer.IsComplete && _hasCall) TransitionToGreen();
                    break;
            }
        }

        private void TransitionToGreen()
        {
            _state = PhaseStates.Green;
            _hasCall = false;
            int forceOff = (ForceOffPoint <= 0) ? Split - (int)Yellow - (int)RedClearance : Math.Min(Split - (int)Yellow - (int)RedClearance, ForceOffPoint);
            if (_forceOffTimer.IsComplete) _forceOffTimer.Reset(forceOff);
            if (_maxGreenTimer.IsComplete) _maxGreenTimer.Reset((int)MaxGreen);
            if (_minGreenTimer.IsComplete) _minGreenTimer.Reset(MinGreen);

            _redClearanceTimer = new CountDown();
        }

        private void TransitionToYellow()
        {
            _state = PhaseStates.Yellow;
            _hasOpposingCall = false;
            if (_yellowTimer.IsComplete) _yellowTimer.Reset((int)Yellow);

            _forceOffTimer = new CountDown();
            _maxGreenTimer = new CountDown();
            _minGreenTimer = new CountDown();
        }

        private void TransitionToRed()
        {
            _state = PhaseStates.Red;
            if (_redClearanceTimer.IsComplete) _redClearanceTimer.Reset((int)RedClearance);

            _yellowTimer = new CountDown();
        }

        internal void Zero()
        {
            _state = PhaseStates.Red;
            _forceOffTimer = new CountDown();
            _maxGreenTimer = new CountDown();
            _minGreenTimer = new CountDown();
            _yellowTimer = new CountDown();
            _redClearanceTimer = new CountDown();
            _hasCall = false;
            _hasOpposingCall = false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        internal int ForceOff { get; set; }
        internal Dictionary<int, bool> _callByConflictPhase = new Dictionary<int, bool>();
        internal List<int> ConflictPhases { get { return _callByConflictPhase.Keys.ToList(); } }
        internal bool IsZero
        {
            get
            {
                return (_state == PhaseStates.Red)
                        && (_forceOffTimer.IsComplete)
                        && (_maxGreenTimer.IsComplete)
                        && (_minGreenTimer.IsComplete)
                        && (_yellowTimer.IsComplete)
                        && (_redClearanceTimer.IsComplete);
            }
        }

        public int ID { get { return _id; } }
        public PhaseStates State { get { return _state; } }
        public int Split { get; set; }
        public int MinGreen { get; set; }
        public double MaxGreen { get; set; }
        public double Yellow { get; set; }
        public double RedClearance { get; set; }
        public bool IsCoordinated { get; set; }
        public bool FloatingForceOff { get; set; }
        public bool HasCall { get; set; }
        public bool HasOpposingCall { get { return _callByConflictPhase.Any(kvp => kvp.Value); } }

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

        internal void Advance(int nSeconds)
        {
            switch (_state)
            {
                case PhaseStates.Green:
                    _minGreenTimer.Decrement(nSeconds);
                    _forceOffTimer.Decrement(nSeconds);

                    // If this phase has an opposing call, decrement the max green timer
                    // Otherwise, reset the max green timer
                    if (HasOpposingCall)
                    {
                        _maxGreenTimer.Decrement(nSeconds);
                    }
                    else
                    {
                        _maxGreenTimer.Reset((int)MaxGreen);
                    }

                    if (_minGreenTimer.IsComplete)
                    {
                        if (IsCoordinated)
                        {
                            if (_forceOffTimer.IsComplete && HasOpposingCall) TransitionToYellow();
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
                    if (_redClearanceTimer.IsComplete && HasCall) TransitionToGreen();
                    break;
            }
        }

        private void TransitionToGreen()
        {
            _state = PhaseStates.Green;

            if (_forceOffTimer.IsComplete) _forceOffTimer.Reset(ForceOff);
            if (_maxGreenTimer.IsComplete) _maxGreenTimer.Reset((int)MaxGreen);
            if (_minGreenTimer.IsComplete) _minGreenTimer.Reset(MinGreen);
            _redClearanceTimer = new CountDown();

            HasCall = false;
        }

        private void TransitionToYellow()
        {
            _state = PhaseStates.Yellow;

            if (_yellowTimer.IsComplete) _yellowTimer.Reset((int)Yellow);
            _forceOffTimer = new CountDown();
            _maxGreenTimer = new CountDown();
            _minGreenTimer = new CountDown();

            foreach (var phase in _callByConflictPhase.Keys.ToList())
            {
                _callByConflictPhase[phase] = false;
            }
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

            HasCall = false;
            foreach (var phase in _callByConflictPhase.Keys.ToList())
            {
                _callByConflictPhase[phase] = false;
            }
        }
    }
}

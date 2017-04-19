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
        private PhaseStates _state = PhaseStates.Red;
        private int _forceOffTimer = 0;
        private int _maxGreenTimer = 0;
        private int _minGreenTimer = 0;
        private int _yellowTimer = 0;
        private int _redClearanceTimer = 0;

        private bool _hasCall = false;
        private bool _hasOpposingCall = false;

        private bool _isCoordinated = false;
        private bool _floatingForceOff = true;
        private int _forceOff = 0;

        public int ID { get; set; }
        public PhaseStates State { get { return _state; } }
        public int Split { get; set; }
        public bool HasCall { get { return _hasCall; } }

        public int MinGreen { get; set; }
        public double MaxGreen { get; set; }
        public double Yellow { get; set; }
        public double RedClearance { get; set; }
        public bool IsCoordinated { get { return _isCoordinated; } set { _isCoordinated = value; } }
        public bool FloatingForceOff { get { return _floatingForceOff; } set { _floatingForceOff = value; } }
        public int ForceOffPoint { get { return _forceOff; } set { _forceOff = value; } }

        public Phase(int id)
        {
            if (id < 0) throw new ArgumentOutOfRangeException("Phase ID must be positive");
            ID = id;
        }
        public Phase(int id, int split)
        {
            if (id < 0) throw new ArgumentOutOfRangeException("Phase ID must be positive");
            if (split < 0) throw new ArgumentOutOfRangeException("Split must be positive");

            ID = id;
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

        internal bool IsZero()
        {
            if (_state != PhaseStates.Red) return false;
            if (_forceOffTimer > 0) return false;
            if (_yellowTimer > 0) return false;
            if (_redClearanceTimer > 0) return false;

            return true;
        }

        private void TransitionToGreen()
        {
            _state = PhaseStates.Green;
            _hasCall = false;

            _forceOffTimer = Split - (int)Yellow - (int)RedClearance;
            if (!_floatingForceOff && _forceOff < _forceOffTimer) _forceOffTimer = _forceOff;

            _maxGreenTimer = (int)MaxGreen;
            _minGreenTimer = MinGreen;
        }

        private void TransitionToYellow()
        {
            _state = PhaseStates.Yellow;
            _hasOpposingCall = false;

            _yellowTimer = (int)Yellow;
            _forceOffTimer = 0;
            _forceOff = 0;
            _maxGreenTimer = 0;
        }

        private void TransitionToRed()
        {
            _state = PhaseStates.Red;

            _redClearanceTimer = (int)RedClearance;
        }

        internal void Advance(int nSeconds)
        {
            switch (_state)
            {
                case PhaseStates.Green:
                    if (_minGreenTimer > 0) _minGreenTimer--;
                    if (_forceOffTimer > 0) _forceOffTimer--;
                    if (_maxGreenTimer > 0) _maxGreenTimer--;

                    if (_minGreenTimer == 0)
                    {
                        if (_isCoordinated)
                        {
                            if (_forceOffTimer == 0 && _hasOpposingCall) TransitionToYellow();
                        }
                        else
                        {
                            if (_forceOffTimer == 0) TransitionToYellow();
                            if (_maxGreenTimer == 0) TransitionToYellow();
                        }
                    }
                    break;
                case PhaseStates.Yellow:
                    if (_yellowTimer > 0) _yellowTimer--;
                    if (_yellowTimer == 0) TransitionToRed();
                    break;
                case PhaseStates.Red:
                    if (_redClearanceTimer > 0) _redClearanceTimer--;
                    if (_redClearanceTimer == 0 && _hasCall) TransitionToGreen();
                    break;
            }
        }

        internal void Zero()
        {
            _state = PhaseStates.Red;
            _forceOffTimer = 0;
            _maxGreenTimer = 0;
            _yellowTimer = 0;
            _redClearanceTimer = 0;
        }
    }
}

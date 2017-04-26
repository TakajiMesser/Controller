using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTSController.Phasing
{
    public enum VehiclePhaseStates
    {
        Green,
        Yellow,
        Red
    }

    public class VehiclePhase
    {
        private VehiclePhaseStates _state = VehiclePhaseStates.Red;
        private CountDown _forceOffTimer = new CountDown();
        private CountDown _maxGreenTimer = new CountDown();
        private CountDown _minGreenTimer = new CountDown();
        private CountDown _yellowTimer = new CountDown();
        private CountDown _redClearanceTimer = new CountDown();

        internal int ForceOff { get; set; }
        internal bool IsZero
        {
            get
            {
                return (_state == VehiclePhaseStates.Red)
                        && (_forceOffTimer.IsComplete)
                        && (_maxGreenTimer.IsComplete)
                        && (_minGreenTimer.IsComplete)
                        && (_yellowTimer.IsComplete)
                        && (_redClearanceTimer.IsComplete);
            }
        }

        public VehiclePhaseStates State { get { return _state; } }
        public int Split { get; set; }
        public int MinGreen { get; set; }
        public double MaxGreen { get; set; }
        public double Yellow { get; set; }
        public double RedClearance { get; set; }
        public bool HasCall { get; set; }

        public VehiclePhase()
        {
        }

        internal void Advance(int nSeconds, bool isCoordinated, bool hasOpposingCall)
        {
            switch (_state)
            {
                case VehiclePhaseStates.Green:
                    _minGreenTimer.Decrement(nSeconds);
                    _forceOffTimer.Decrement(nSeconds);

                    // If this phase has an opposing call, decrement the max green timer
                    // Otherwise, reset the max green timer
                    if (hasOpposingCall)
                    {
                        _maxGreenTimer.Decrement(nSeconds);
                    }
                    else
                    {
                        _maxGreenTimer.Reset((int)MaxGreen);
                    }

                    if (_minGreenTimer.IsComplete)
                    {
                        if (isCoordinated)
                        {
                            if (_forceOffTimer.IsComplete && hasOpposingCall) TransitionToYellow();
                        }
                        else
                        {
                            if (_forceOffTimer.IsComplete) TransitionToYellow();
                            if (_maxGreenTimer.IsComplete) TransitionToYellow();
                        }
                    }
                    break;
                case VehiclePhaseStates.Yellow:
                    _yellowTimer.Decrement(nSeconds);
                    if (_yellowTimer.IsComplete) TransitionToRed();
                    break;
                case VehiclePhaseStates.Red:
                    _redClearanceTimer.Decrement(nSeconds);
                    if (_redClearanceTimer.IsComplete && HasCall) TransitionToGreen();
                    break;
            }
        }

        private void TransitionToGreen()
        {
            _state = VehiclePhaseStates.Green;

            if (_forceOffTimer.IsComplete) _forceOffTimer.Reset(ForceOff);
            if (_maxGreenTimer.IsComplete) _maxGreenTimer.Reset((int)MaxGreen);
            if (_minGreenTimer.IsComplete) _minGreenTimer.Reset(MinGreen);
            _redClearanceTimer = new CountDown();

            HasCall = false;
        }

        private void TransitionToYellow()
        {
            _state = VehiclePhaseStates.Yellow;

            if (_yellowTimer.IsComplete) _yellowTimer.Reset((int)Yellow);
            _forceOffTimer = new CountDown();
            _maxGreenTimer = new CountDown();
            _minGreenTimer = new CountDown();
        }

        private void TransitionToRed()
        {
            _state = VehiclePhaseStates.Red;

            if (_redClearanceTimer.IsComplete) _redClearanceTimer.Reset((int)RedClearance);
            _yellowTimer = new CountDown();
        }

        internal void Zero()
        {
            _state = VehiclePhaseStates.Red;

            _forceOffTimer = new CountDown();
            _maxGreenTimer = new CountDown();
            _minGreenTimer = new CountDown();
            _yellowTimer = new CountDown();
            _redClearanceTimer = new CountDown();

            HasCall = false;
        }
    }
}

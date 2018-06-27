namespace Controller.Phasing
{
    public enum VehiclePhaseStates
    {
        Green,
        Yellow,
        Red
    }

    public class VehiclePhase
    {
        private CountDown _forceOffTimer = new CountDown();
        private CountDown _gapTimer = new CountDown();
        private CountDown _maxGreenTimer = new CountDown();
        private CountDown _minGreenTimer = new CountDown();
        private CountDown _yellowTimer = new CountDown();
        private CountDown _redClearanceTimer = new CountDown();

        internal int ForceOff { get; set; }
        internal bool IsZero => (State == VehiclePhaseStates.Red)
            && _forceOffTimer.IsComplete && _maxGreenTimer.IsComplete && _minGreenTimer.IsComplete && _yellowTimer.IsComplete && _redClearanceTimer.IsComplete;

        public VehiclePhaseStates State { get; private set; } = VehiclePhaseStates.Red;
        public double GapTime { get; set; }
        public double MaxGreen { get; set; }
        public int MinGreen { get; set; }
        public double Yellow { get; set; }
        public double RedClearance { get; set; }
        public bool HasCall { get; set; }

        public VehiclePhase() { }

        internal void Advance(int nSeconds, bool isCoordinated, bool hasOpposingCall)
        {
            switch (State)
            {
                case VehiclePhaseStates.Green:
                    _minGreenTimer.Decrement(nSeconds);
                    _forceOffTimer.Decrement(nSeconds);
                    _gapTimer.Decrement(nSeconds);

                    if (HasCall)
                    {
                        _gapTimer.Reset((int)GapTime);
                    }

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
                            if (_gapTimer.IsComplete) TransitionToYellow();
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
                    if (_redClearanceTimer.IsComplete && (HasCall)) TransitionToGreen();
                    break;
            }
        }

        private void TransitionToGreen()
        {
            State = VehiclePhaseStates.Green;

            if (ForceOff > 0) _forceOffTimer.Reset(ForceOff);
            if (MaxGreen > 0.0) _maxGreenTimer.Reset((int)MaxGreen);
            if (MinGreen > 0) _minGreenTimer.Reset(MinGreen);
            _redClearanceTimer = new CountDown();

            HasCall = false;
        }

        private void TransitionToYellow()
        {
            State = VehiclePhaseStates.Yellow;

            if (Yellow > 0.0) _yellowTimer.Reset((int)Yellow);
            _forceOffTimer = new CountDown();
            _maxGreenTimer = new CountDown();
            _minGreenTimer = new CountDown();
        }

        private void TransitionToRed()
        {
            State = VehiclePhaseStates.Red;

            if (RedClearance > 0.0) _redClearanceTimer.Reset((int)RedClearance);
            _yellowTimer = new CountDown();
        }

        internal void Zero()
        {
            State = VehiclePhaseStates.Red;

            _forceOffTimer = new CountDown();
            _maxGreenTimer = new CountDown();
            _minGreenTimer = new CountDown();
            _yellowTimer = new CountDown();
            _redClearanceTimer = new CountDown();

            HasCall = false;
        }
    }
}

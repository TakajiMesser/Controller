namespace Controller.Phasing
{
    public enum PedestrianPhaseStates
    {
        Walk,
        FlashingDoNotWalk,
        DoNotWalk
    }

    public class PedestrianPhase
    {
        public PedestrianPhaseStates State { get; private set; } = PedestrianPhaseStates.DoNotWalk;
        public int Walk { get; set; }
        public int PedClearance { get; set; }
        public double DoNotWalkClearance { get; set; }
        public bool HasCall { get; set; }

        internal bool IsZero => (State == PedestrianPhaseStates.DoNotWalk)
            && _walkTimer.IsComplete && _pedClearanceTimer.IsComplete && _doNotWalkClearanceTimer.IsComplete;

        private CountDown _walkTimer = new CountDown();
        private CountDown _pedClearanceTimer = new CountDown();
        private CountDown _doNotWalkClearanceTimer = new CountDown();

        public PedestrianPhase() { }

        internal void Advance(int nSeconds)
        {
            switch (State)
            {
                case PedestrianPhaseStates.Walk:
                    _walkTimer.Decrement(nSeconds);
                    if (_walkTimer.IsComplete) TransitionToFlashingDoNotWalk();
                    break;
                case PedestrianPhaseStates.FlashingDoNotWalk:
                    _pedClearanceTimer.Decrement(nSeconds);
                    if (_pedClearanceTimer.IsComplete) TransitionToDoNotWalk();
                    break;
                case PedestrianPhaseStates.DoNotWalk:
                    _doNotWalkClearanceTimer.Decrement(nSeconds);
                    if (_doNotWalkClearanceTimer.IsComplete && HasCall) TransitionToWalk();
                    break;
            }
        }

        private void TransitionToWalk()
        {
            State = PedestrianPhaseStates.Walk;

            if (Walk > 0) _walkTimer.Reset(Walk);
            _doNotWalkClearanceTimer = new CountDown();
        }

        private void TransitionToFlashingDoNotWalk()
        {
            State = PedestrianPhaseStates.FlashingDoNotWalk;

            if (PedClearance > 0) _pedClearanceTimer.Reset(PedClearance);
            _walkTimer = new CountDown();
        }

        private void TransitionToDoNotWalk()
        {
            State = PedestrianPhaseStates.DoNotWalk;

            if (DoNotWalkClearance > 0.0) _doNotWalkClearanceTimer.Reset((int)DoNotWalkClearance);
            _pedClearanceTimer = new CountDown();
        }

        internal void Zero()
        {
            State = PedestrianPhaseStates.DoNotWalk;

            _walkTimer = new CountDown();
            _pedClearanceTimer = new CountDown();
            _doNotWalkClearanceTimer = new CountDown();

            HasCall = false;
        }
    }
}

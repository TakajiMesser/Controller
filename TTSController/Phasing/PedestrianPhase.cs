using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTSController.Phasing
{
    public enum PedestrianPhaseStates
    {
        Walk,
        FlashingDoNotWalk,
        DoNotWalk
    }

    public class PedestrianPhase
    {
        private PedestrianPhaseStates _state = PedestrianPhaseStates.DoNotWalk;
        private CountDown _walkTimer = new CountDown();
        private CountDown _pedClearanceTimer = new CountDown();
        private CountDown _doNotWalkClearanceTimer = new CountDown();

        internal bool IsZero
        {
            get
            {
                return (_state == PedestrianPhaseStates.DoNotWalk)
                    && _walkTimer.IsComplete
                    && _pedClearanceTimer.IsComplete
                    && _doNotWalkClearanceTimer.IsComplete;
            }
        }

        public PedestrianPhaseStates State { get { return _state; } }
        public int Walk { get; set; }
        public int PedClearance { get; set; }
        public double DoNotWalkClearance { get; set; }
        public bool HasCall { get; set; }

        public PedestrianPhase() { }

        internal void Advance(int nSeconds)
        {
            switch (_state)
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
            _state = PedestrianPhaseStates.Walk;

            if (Walk > 0) _walkTimer.Reset(Walk);
            _doNotWalkClearanceTimer = new CountDown();
        }

        private void TransitionToFlashingDoNotWalk()
        {
            _state = PedestrianPhaseStates.FlashingDoNotWalk;

            if (PedClearance > 0) _pedClearanceTimer.Reset(PedClearance);
            _walkTimer = new CountDown();
        }

        private void TransitionToDoNotWalk()
        {
            _state = PedestrianPhaseStates.DoNotWalk;

            if (DoNotWalkClearance > 0.0) _doNotWalkClearanceTimer.Reset((int)DoNotWalkClearance);
            _pedClearanceTimer = new CountDown();
        }

        internal void Zero()
        {
            _state = PedestrianPhaseStates.DoNotWalk;

            _walkTimer = new CountDown();
            _pedClearanceTimer = new CountDown();
            _doNotWalkClearanceTimer = new CountDown();

            HasCall = false;
        }
    }
}

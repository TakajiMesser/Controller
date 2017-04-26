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

        public PedestrianPhaseStates State { get { return _state; } }
        public bool IsZero { get { return _state == PedestrianPhaseStates.DoNotWalk; } }
        public bool HasCall { get; set; }

        public PedestrianPhase()
        {
        }

        internal void Advance(int nSeconds)
        {
            switch (_state)
            {
                case PedestrianPhaseStates.Walk:
                    break;
                case PedestrianPhaseStates.FlashingDoNotWalk:
                    break;
                case PedestrianPhaseStates.DoNotWalk:
                    break;
            }
        }

        /*private void TransitionToGreen()
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
        }*/

        internal void Zero()
        {
            _state = PedestrianPhaseStates.DoNotWalk;

            HasCall = false;
        }
    }
}

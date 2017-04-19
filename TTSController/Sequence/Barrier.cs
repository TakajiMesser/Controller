using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTSController.Phasing;

namespace TTSController.Sequence
{
    public class Barrier
    {
        private int _servicePhaseRedCount = 0;
        private int _servicePhaseIndex = 0;
        private List<Phase> _phases = new List<Phase>();

        public List<Phase> Phases { get { return _phases; } }
        public int Duration { get { return _phases.Select(p => p.Split).Sum(); } }

        public Barrier(IEnumerable<Phase> phases)
        {
            _phases.AddRange(phases);
        }

        internal void Advance(int nSeconds, int barrierSecond, Phase coordinatedPhase)
        {
            // Check all phases in this barrier to see if we have any calls(and iterate _servicePhaseIndex up)
            // If we don't find any calls, keep the _servicePhaseIndex where it is, and attempt to run the coordinated phase
            for (var i = 0; (i + _servicePhaseIndex) < Phases.Count; i++)
            {
                Phase phase = Phases[_servicePhaseIndex + i];
                if (phase.HasCall || !phase.IsZero())
                {
                    _servicePhaseIndex = _servicePhaseIndex + i;

                    if (!phase.FloatingForceOff && phase.ForceOffPoint == 0)
                    {
                        phase.ForceOffPoint = Duration - barrierSecond - (int)phase.Yellow - (int)phase.RedClearance;
                    }

                    phase.Advance(nSeconds);
                    if (phase.State == PhaseStates.Red)
                    {
                        _servicePhaseRedCount++;
                        if (_servicePhaseRedCount >= (int)phase.RedClearance)
                        {
                            _servicePhaseIndex = (_servicePhaseIndex + 1) % Phases.Count;
                        }
                    }
                    return;
                }
            }

            // If we make it this far, run the coordinated phase
            if (coordinatedPhase != null)
            {
                coordinatedPhase.PlaceCall();
                coordinatedPhase.Advance(nSeconds);
            }
        }

        internal void Zero()
        {
            foreach (var phase in Phases)
            {
                phase.Zero();
            }
        }
    }
}

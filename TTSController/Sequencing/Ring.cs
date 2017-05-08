using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTSController.Phasing;

namespace TTSController.Sequencing
{
    public class Ring
    {
        private List<Phase> _phases = new List<Phase>();
        private int _servicePhaseIndex = 0;
        private SortedSet<int> _barrierIndices = new SortedSet<int>();

        public List<Phase> Phases { get { return _phases; } }
        public SortedSet<int> BarrierIndices { get { return _barrierIndices; } }
        public int Duration { get { return _phases.Select(p => p.Split).Sum(); } }
        public Phase CoordinatedPhase { get { return Phases.FirstOrDefault(p => p.IsCoordinated); } }

        public Ring() { }

        internal void Advance(int nSeconds, int ringSecond)
        {
            if (ringSecond == 0) _servicePhaseIndex = 0;

            // Is there a phase that is already running?
            Phase phase = _phases.FirstOrDefault(p => !p.IsZero);
            if (phase != null)
            {
                // If so, advance this phase forward
                phase.Advance(nSeconds);

                // If this phase just ended, start the next phase as well (since this should coincide in the same step)
                if (phase.IsZero)
                {
                    Phase nextPhase = GetNextPhaseToService();
                    if (nextPhase != null)
                    {
                        nextPhase.Advance(nSeconds);
                    }
                }
            }
            else
            {
                // If no phase is running, run the next available phase
                Phase nextPhase = GetNextPhaseToService();
                if (nextPhase != null)
                {
                    nextPhase.Advance(nSeconds);
                }
            }
        }

        private Phase GetNextPhaseToService()
        {
            for (var i = _servicePhaseIndex; i < _phases.Count; i++)
            {
                var phase = _phases[_servicePhaseIndex];
                if (phase.HasCall)
                {
                    phase.VehiclePhase.ForceOff = phase.Split - (int)phase.VehiclePhase.Yellow - (int)phase.VehiclePhase.RedClearance;
                    _servicePhaseIndex++;
                    return phase;
                }
            }

            // If no phase is found, return the coordinated phase
            // Ensure that the coordinated phase gets a call so that it can begin timing
            var coordinatedPhase = CoordinatedPhase;
            if (coordinatedPhase != null)
            {
                coordinatedPhase.VehiclePhase.HasCall = true;
            }

            return CoordinatedPhase;
        }

        internal void Zero()
        {
            foreach (var phase in _phases)
            {
                phase.Zero();
            }
        }
    }
}

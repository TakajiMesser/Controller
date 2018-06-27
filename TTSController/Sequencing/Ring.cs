using Controller.Phasing;
using System.Collections.Generic;
using System.Linq;

namespace Controller.Sequencing
{
    public class Ring
    {
        public List<Phase> Phases { get; } = new List<Phase>();
        public SortedSet<int> BarrierIndices { get; } = new SortedSet<int>();

        public int Duration => Phases.Select(p => p.Split).Sum();
        public Phase CoordinatedPhase => Phases.FirstOrDefault(p => p.IsCoordinated);

        private int _servicePhaseIndex = 0;

        public Ring() { }

        internal void Advance(int nSeconds, int ringSecond)
        {
            if (ringSecond == 0)
            {
                _servicePhaseIndex = 0;
            }

            // Is there a phase that is already running?
            var phase = Phases.FirstOrDefault(p => !p.IsZero);
            if (phase != null)
            {
                // If so, advance this phase forward
                phase.Advance(nSeconds);

                // If this phase just ended, start the next phase as well (since this should coincide in the same step)
                if (phase.IsZero)
                {
                    var nextPhase = GetNextPhaseToService();
                    if (nextPhase != null)
                    {
                        nextPhase.Advance(nSeconds);
                    }
                }
            }
            else
            {
                // If no phase is running, run the next available phase
                var nextPhase = GetNextPhaseToService();
                if (nextPhase != null)
                {
                    nextPhase.Advance(nSeconds);
                }
            }
        }

        private Phase GetNextPhaseToService()
        {
            for (var i = _servicePhaseIndex; i < Phases.Count; i++)
            {
                var phase = Phases[_servicePhaseIndex];
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
            foreach (var phase in Phases)
            {
                phase.Zero();
            }
        }
    }
}

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
        private HashSet<int> _servicedPhaseIDs = new HashSet<int>();
        private SortedSet<int> _barrierIndices = new SortedSet<int>();

        public List<Phase> Phases { get { return _phases; } }
        public SortedSet<int> BarrierIndices { get { return _barrierIndices; } }
        public int Duration { get { return _phases.Select(p => p.Split).Sum(); } }
        public Phase CoordinatedPhase { get { return Phases.FirstOrDefault(p => p.IsCoordinated); } }

        public Ring() { }

        internal void Advance(int nSeconds, int ringSecond)
        {
            if (_phases.Count == _servicedPhaseIDs.Count) _servicedPhaseIDs.Clear();

            //  Possible Extra Check -> Any time a new phase is starting, check all of its conflicting phases to ensure that none of them are already running?

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
                        _servicedPhaseIDs.Add(nextPhase.ID);
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
                    _servicedPhaseIDs.Add(nextPhase.ID);
                    nextPhase.Advance(nSeconds);
                }
            }
        }

        internal void Zero()
        {
            foreach (var phase in _phases)
            {
                phase.Zero();
            }
        }

        private Phase GetNextPhaseToService()
        {
            // If no phase is found, return the coordinated phase
            Phase phase = _phases.FirstOrDefault(p => p.HasCall && !_servicedPhaseIDs.Contains(p.ID));
            if (phase != null)
            {
                phase.VehiclePhase.ForceOff = phase.Split - (int)phase.VehiclePhase.Yellow - (int)phase.VehiclePhase.RedClearance;
                return phase;
            }
            else
            {
                // Ensure that the coordinated phase gets a call so that it can begin timing
                var coordinatedPhase = CoordinatedPhase;
                if (coordinatedPhase != null)
                {
                    coordinatedPhase.VehiclePhase.HasCall = true;
                }

                return CoordinatedPhase;
            }
        }
    }
}

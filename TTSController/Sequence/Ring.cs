using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTSController.Phasing;

namespace TTSController.Sequence
{
    public class Ring
    {
        private List<Phase> _phases = new List<Phase>();
        private List<Phase> _servicedPhases = new List<Phase>();
        private SortedSet<int> _barrierIndices = new SortedSet<int>();

        public List<Phase> Phases { get { return _phases; } }
        public SortedSet<int> BarrierIndices { get { return _barrierIndices; } }
        public int Duration { get { return _phases.Select(p => p.Split).Sum(); } }
        public Phase CoordinatedPhase { get { return Phases.FirstOrDefault(p => p.IsCoordinated); } }

        public Ring(IEnumerable<Phase> phases)
        {
            _phases.AddRange(phases);
        }

        internal void Advance(int nSeconds, int ringSecond)
        {
            if (_phases.Count == _servicedPhases.Count) _servicedPhases.Clear();
            var remainingPhases = _phases.Where(p => !_servicedPhases.Any(s => p.ID == s.ID));

            Phase coordinatedPhase = CoordinatedPhase;

            // Find phases that are either being serviced or need servicing
            foreach (var phase in remainingPhases)
            {
                // Do not advance this phase if the coordinated phase is already running, and the coordinated phase conflicts with this one
                if (coordinatedPhase != null && phase.ConflictPhases.Contains(coordinatedPhase.ID) && !coordinatedPhase.IsZero)
                {
                    coordinatedPhase.ForceOffPoint = coordinatedPhase.MinGreen;
                    coordinatedPhase.Advance(nSeconds);

                    // Is this phase completed?
                    if (coordinatedPhase.IsZero)
                    {
                        // We still need to iterate the next phase forward as well
                        _servicedPhases.Add(phase);
                    }
                    else
                    {
                        return;
                    }
                }

                if (phase.HasCall || !phase.IsZero)
                {
                    // Set the ForceOffPoint to the end of the barrier for fixed force-off phases
                    if (!phase.FloatingForceOff && phase.ForceOffPoint == 0)
                    {
                        phase.ForceOffPoint = Duration - (int)phase.Yellow - (int)phase.RedClearance;
                    }

                    phase.Advance(nSeconds);

                    // Is this phase completed?
                    if (phase.IsZero)
                    {
                        // We still need to iterate the next phase forward as well
                        _servicedPhases.Add(phase);
                    }
                    else
                    {
                        // Iterate the coordinated phase forward as well, if it is already running
                        if (coordinatedPhase != null && phase.ID != coordinatedPhase.ID && !coordinatedPhase.IsZero)
                        {
                            coordinatedPhase.Advance(nSeconds);
                        }
                        return;
                    }
                }
            }

            // If no remaining phases are found that need servicing, run the coordinated phase instead (if it exists in this ring, and doesn't have any opposing calls or is already running)
            if (coordinatedPhase != null && !coordinatedPhase.HasOpposingCall)
            {
                coordinatedPhase.PlaceCall();
                coordinatedPhase.Advance(nSeconds);
            }
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

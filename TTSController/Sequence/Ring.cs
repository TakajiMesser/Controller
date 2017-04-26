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
        public int Duration { get { return _phases.Select(p => p.Vehicle.Split).Sum(); } }
        public Phase CoordinatedPhase { get { return Phases.FirstOrDefault(p => p.IsCoordinated); } }

        public Ring(IEnumerable<Phase> phases)
        {
            _phases.AddRange(phases);
        }

        internal void Advance(int nSeconds, int ringSecond)
        {
            if (_phases.Count == _servicedPhases.Count) _servicedPhases.Clear();
            Phase coordinatedPhase = CoordinatedPhase;

            // Find any remaining phases (including phases that are currently being serviced)
            foreach (var phase in _phases.Where(p => !_servicedPhases.Any(s => p.ID == s.ID)))
            {
                // Do not advance this phase if the coordinated phase is already running, and the coordinated phase conflicts with this one
                if (coordinatedPhase != null && phase.ConflictPhases.Contains(coordinatedPhase.ID) && !coordinatedPhase.IsZero)
                {
                    // Unclear line...
                    coordinatedPhase.Vehicle.ForceOff = coordinatedPhase.Vehicle.MinGreen;

                    coordinatedPhase.Advance(nSeconds);

                    // If this phase isn't completed yet, we are done advancing for this iteration
                    if (!coordinatedPhase.IsZero)
                    {
                        return;
                    }

                    _servicedPhases.Add(coordinatedPhase);
                }

                if (phase.HasCall || !phase.IsZero)
                {
                    if (phase.FloatingForceOff)
                    {
                        // For floating force-off, just use the calculated green duration
                        phase.Vehicle.ForceOff = phase.Vehicle.Split - (int)phase.Vehicle.Yellow - (int)phase.Vehicle.RedClearance;
                    }
                    else
                    {
                        // For fixed force-offs, use calculated force-off points in the sequence
                        // WRONG FOR NOW
                        phase.Vehicle.ForceOff = phase.Vehicle.Split - (int)phase.Vehicle.Yellow - (int)phase.Vehicle.RedClearance;
                    }

                    // ADD SAFETY CHECK -> Don't advance this phase if any conflicting phases are running
                    phase.Advance(nSeconds);

                    // If this phase isn't completed yet, we are done advancing for this iteration
                    if (!phase.IsZero)
                    {
                        // Iterate the coordinated phase forward as well, if it is already running
                        if (coordinatedPhase != null && phase.ID != coordinatedPhase.ID && !coordinatedPhase.IsZero)
                        {
                            coordinatedPhase.Advance(nSeconds);
                        }
                        return;
                    }

                    _servicedPhases.Add(coordinatedPhase);
                }
            }

            // If no remaining phases are found that need servicing, run the coordinated phase instead (if it exists in this ring, and doesn't have any opposing calls or is already running)
            if (coordinatedPhase != null && !coordinatedPhase.HasOpposingCall)
            {
                coordinatedPhase.Vehicle.HasCall = true;
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

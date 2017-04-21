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
        private List<int> _barrierIndices = new List<int>();

        public List<Phase> Phases { get { return _phases; } }
        public List<int> BarrierIndices { get { return _barrierIndices; } }
        public int Duration { get { return _phases.Select(p => p.Split).Sum(); } }
        public Phase CoordinatedPhase { get { return Phases.FirstOrDefault(p => p.IsCoordinated); } }

        public Ring(IEnumerable<Phase> phases)
        {
            _phases.AddRange(phases);
        }

        public void AddBarrier(int index)
        {
            // From this new barrier index position, find the closest preceding and succeeding barriers
            int precedingBarrier = _barrierIndices.LastOrDefault(i => i < index);

            int succeedingBarrier = _barrierIndices.FirstOrDefault(i => i > index);
            if (succeedingBarrier == 0) succeedingBarrier = _phases.Count;
            _barrierIndices.Add(index);

            // Using these, find the set of phases between them
            List<Phase> phases = _phases.GetRange(precedingBarrier, succeedingBarrier - precedingBarrier);

            // Now, find out how the new barrier splits this phase set
            HashSet<Phase> precedingSet = new HashSet<Phase>();
            for (var i = precedingBarrier; i < index; i++)
            {
                precedingSet.Add(_phases[i]);
            }
            
            HashSet<Phase> succeedingSet = new HashSet<Phase>();
            for (var i = index; i < succeedingBarrier; i++)
            {
                succeedingSet.Add(_phases[i]);
            }

            // Now, add all phases from set1 to set2's conflict phases, and vice versa!
            foreach (var phase in precedingSet)
            {
                phase.ConflictPhases.UnionWith(succeedingSet.Select(p => p.ID));
            }

            foreach (var phase in succeedingSet)
            {
                phase.ConflictPhases.UnionWith(precedingSet.Select(p => p.ID));
            }
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
                    return;
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

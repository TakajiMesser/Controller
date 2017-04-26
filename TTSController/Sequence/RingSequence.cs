using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTSController.Phasing;

namespace TTSController.Sequence
{
    public class RingSequence
    {
        private List<Ring> _rings = new List<Ring>();

        public List<Ring> Rings { get { return _rings; } }
        public int Duration { get { return Rings.Select(r => r.Duration).Max(); } }

        public RingSequence(IEnumerable<Ring> rings)
        {
            _rings.AddRange(rings);

            foreach (var ring in Rings)
            {
                foreach (var phase in ring.Phases)
                {
                    // Any phases on the same ring are conflicts
                    var conflictPhases = new HashSet<int>();

                    conflictPhases.UnionWith(Rings
                        .Where(
                            r => Rings.IndexOf(r) == Rings.IndexOf(ring)
                            )
                        .SelectMany(
                            r => r.Phases
                            )
                        .Select(
                            p => p.ID
                            )
                        .Where(
                            p => p != phase.ID
                            ));

                    // Any phases in a different barrier are conflicts
                    conflictPhases.UnionWith(Rings
                        .Where(
                            r => Rings.IndexOf(r) != Rings.IndexOf(ring)
                            )
                        .SelectMany(
                            r => r.Phases
                            )
                        .Where(
                            p => GetBarrierIndexForPhase(p.ID) != GetBarrierIndexForPhase(phase.ID)
                            )
                        .Select(
                            p => p.ID)
                            );

                    foreach (var phaseID in conflictPhases)
                    {
                        phase._callByConflictPhase[phaseID] = false;
                    }
                }
            }
        }

        private int GetBarrierIndexForPhase(int phaseID)
        {
            foreach (var ring in Rings)
            {
                Phase phase = ring.Phases.FirstOrDefault(p => p.ID == phaseID);
                if (phase != null)
                {
                    int phaseIndex = ring.Phases.IndexOf(phase);
                    return ring.BarrierIndices.FirstOrDefault(b => b > phaseIndex);
                }
            }

            throw new ArgumentOutOfRangeException("Phase ID " + phaseID + " not found in sequence");
        }

        internal void Advance(int nSeconds, int cycleSecond)
        {
            foreach (var ring in Rings)
            {
                ring.Advance(nSeconds, cycleSecond);
            }
        }

        internal void Zero()
        {
            foreach (var ring in Rings)
            {
                ring.Zero();
            }
        }

        public void PlaceCall(int phaseID)
        {
            Phase callPhase = GetPhase(phaseID);
            callPhase.Vehicle.HasCall = true;

            foreach (var ring in Rings)
            {
                foreach (var phase in ring.Phases)
                {
                    if (phase._callByConflictPhase.ContainsKey(callPhase.ID)) phase._callByConflictPhase[callPhase.ID] = true;
                }
            }
        }

        public void RemoveCall(int phaseID)
        {
            Phase callPhase = GetPhase(phaseID);
            callPhase.Vehicle.HasCall = false;

            foreach (var ring in Rings)
            {
                foreach (var phase in ring.Phases)
                {
                    if (phase._callByConflictPhase.ContainsKey(callPhase.ID)) phase._callByConflictPhase[callPhase.ID] = false;
                }
            }
        }

        public Phase GetPhase(int phaseID)
        {
            foreach (var ring in Rings)
            {
                Phase phase = ring.Phases.FirstOrDefault(p => p.ID == phaseID);
                if (phase != null)
                {
                    return phase;
                }
            }

            throw new ArgumentOutOfRangeException("Phase " + phaseID + " not found in sequence");
        }

        public List<Phase> GetPhases()
        {
            List<Phase> phases = new List<Phase>();

            foreach (var ring in Rings)
            {
                phases.AddRange(ring.Phases);
            }

            return phases;
        }
    }
}

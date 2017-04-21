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

            for (var i = 0; i < Rings.Count; i++)
            {
                foreach (var phase in Rings[i].Phases)
                {
                    for (var j = 0; j < Rings.Count; j++)
                    {
                        foreach (var conflictPhase in Rings[j].Phases)
                        {
                            // Add conflict phases for any phases on the same ring
                            if (phase.ID != conflictPhase.ID && i == j) phase.ConflictPhases.Add(conflictPhase.ID);

                            // Also need to add conflict phases for any phases on a different ring, in a different barrier

                        }
                    }
                }
            }
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
            callPhase.PlaceCall();

            foreach (var ring in Rings)
            {
                foreach (var phase in ring.Phases)
                {
                    if (phase.ConflictPhases.Contains(callPhase.ID)) phase.PlaceOpposingCall();
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

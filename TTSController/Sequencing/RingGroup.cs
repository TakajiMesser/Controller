using Controller.Phasing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Controller.Sequencing
{
    public class RingGroup
    {
        public BindingList<Ring> Rings { get; } = new BindingList<Ring>();
        public int Duration => Rings.Select(r => r.Duration).Max();

        public RingGroup()
        {
            Rings.ListChanged += (s, e) =>
            {
                foreach (var ring in Rings)
                {
                    foreach (var phase in ring.Phases)
                    {
                        // Any phases on the same ring are conflicts
                        var conflictPhases = new HashSet<int>();

                        conflictPhases.UnionWith(Rings
                            .Where(r => Rings.IndexOf(r) == Rings.IndexOf(ring))
                            .SelectMany(r => r.Phases)
                            .Select(p => p.ID)
                            .Where(p => p != phase.ID));

                        // Any phases in a different barrier are conflicts
                        conflictPhases.UnionWith(Rings
                            .Where(r => Rings.IndexOf(r) != Rings.IndexOf(ring))
                            .SelectMany(r => r.Phases)
                            .Where(p => GetBarrierIndexForPhase(p.ID) != GetBarrierIndexForPhase(phase.ID))
                            .Select(p => p.ID));

                        foreach (var phaseID in conflictPhases)
                        {
                            phase._callByConflictPhase[phaseID] = false;
                        }
                    }
                }
            };
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
    }
}

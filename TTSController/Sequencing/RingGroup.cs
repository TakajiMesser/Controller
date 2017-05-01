using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTSController.Phasing;

namespace TTSController.Sequencing
{
    public class RingGroup
    {
        private BindingList<Ring> _rings = new BindingList<Ring>();

        public BindingList<Ring> Rings { get { return _rings; } }
        public int Duration { get { return Rings.Select(r => r.Duration).Max(); } }

        public RingGroup()
        {
            _rings.ListChanged += (s, e) =>
            {
                foreach (var ring in _rings)
                {
                    foreach (var phase in ring.Phases)
                    {
                        // Any phases on the same ring are conflicts
                        var conflictPhases = new HashSet<int>();

                        conflictPhases.UnionWith(_rings
                            .Where(
                                r => _rings.IndexOf(r) == _rings.IndexOf(ring)
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
                                r => _rings.IndexOf(r) != _rings.IndexOf(ring)
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
            foreach (var ring in _rings)
            {
                ring.Advance(nSeconds, cycleSecond);
            }
        }

        internal void Zero()
        {
            foreach (var ring in _rings)
            {
                ring.Zero();
            }
        }
    }
}

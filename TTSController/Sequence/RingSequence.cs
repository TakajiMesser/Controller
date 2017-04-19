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

        public RingSequence() { }

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
            for (var i = 0; i < Rings.Count; i++) {
                var ring = Rings[i];

                for (var j = 0; j < ring.Barriers.Count; j++)
                {
                    var barrier = ring.Barriers[j];

                    for (var k = 0; k < barrier.Phases.Count; k++)
                    {
                        var phase = barrier.Phases[k];

                        if (phase.ID == phaseID)
                        {
                            phase.PlaceCall();

                            // Place opposing calls on any phases in the same ring, or in a different barrier
                            PlaceOpposingCalls(phaseID, j, i);
                            return;
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException("Phase " + phaseID + " not found in sequence");
        }

        private void PlaceOpposingCalls(int calledPhaseID, int calledBarrierIndex, int calledRingIndex)
        {
            for (var i = 0; i < Rings.Count; i++)
            {
                var ring = Rings[i];

                for (var j = 0; j < ring.Barriers.Count; j++)
                {
                    var barrier = ring.Barriers[j];

                    for (var k = 0; k < barrier.Phases.Count; k++)
                    {
                        Phase phase = barrier.Phases[k];

                        if (phase.ID != calledPhaseID && (i == calledRingIndex || j != calledBarrierIndex))
                        {
                            phase.PlaceOpposingCall();
                        }
                    }
                }
            }
        }

        public Phase GetPhase(int phaseID)
        {
            foreach (var ring in Rings)
            {
                foreach (var barrier in ring.Barriers)
                {
                    if (barrier.Phases.Exists(p => p.ID == phaseID)) return barrier.Phases[phaseID];
                }
            }

            throw new ArgumentOutOfRangeException("Phase " + phaseID + " not found in sequence");
        }

        public List<Phase> GetPhases()
        {
            List<Phase> phases = new List<Phase>();

            foreach (var ring in Rings)
            {
                foreach (var barrier in ring.Barriers)
                {
                    phases.AddRange(barrier.Phases);
                }
            }

            return phases;
        }
    }
}

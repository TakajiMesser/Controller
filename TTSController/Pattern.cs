using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTSController.Phasing;
using TTSController.Sequence;

namespace TTSController
{
    public class Pattern
    {
        private int _offsetReferencePoint;

        public int ID { get; set; }
        public int CycleLength { get { return Sequence.Duration; } }
        public int OffsetReferencePoint { get { return _offsetReferencePoint; } }

        public RingSequence Sequence { get; set; }

        public Pattern(int id)
        {
            if (id < 0) throw new ArgumentOutOfRangeException("Pattern ID must be positive");

            ID = id;
            _offsetReferencePoint = 0;
        }

        public Phase GetPhase(int id)
        {
            foreach (var ring in Sequence.Rings)
            {
                foreach (var phase in ring.Phases)
                {
                    if (phase.ID == id) return phase;
                }
            }

            throw new KeyNotFoundException("Phase with id " + id + " was not found in Sequence");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTSController.Phasing;
using TTSController.Sequencing;

namespace TTSController
{
    public class Pattern
    {
        private int _offsetReferencePoint;
        private Sequence _sequence = new Sequence();

        public int ID { get; set; }
        public int CycleLength { get { return Sequence.Duration; } }
        public int OffsetReferencePoint { get { return _offsetReferencePoint; } }

        public Sequence Sequence { get { return _sequence; } }

        public Pattern(int id)
        {
            if (id < 0) throw new ArgumentOutOfRangeException("Pattern ID must be positive");

            ID = id;
            _offsetReferencePoint = 0;
        }
    }
}

using Controller.Sequencing;
using System;

namespace Controller
{
    public class Pattern
    {
        public int ID { get; private set; }
        public Sequence Sequence { get; private set; } = new Sequence();
        public int CycleLength => Sequence.Duration;
        public int OffsetReferencePoint { get; private set; }

        public Pattern(int id)
        {
            if (id < 0) throw new ArgumentOutOfRangeException("Pattern ID must be positive");

            ID = id;
            OffsetReferencePoint = 0;
        }
    }
}

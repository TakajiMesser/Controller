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
        private List<Barrier> _barriers = new List<Barrier>();

        public List<Barrier> Barriers { get { return _barriers; } }
        public int Duration { get { return Barriers.Select(b => b.Duration).Sum(); } }
        public Phase CoordinatedPhase { get { return Barriers.SelectMany(b => b.Phases).Where(p => p.IsCoordinated).FirstOrDefault(); } }

        public Ring(IEnumerable<Barrier> barriers)
        {
            _barriers.AddRange(barriers);
        }

        internal void Advance(int nSeconds, int ringSecond)
        {
            // Find out which barrier we are in, based on the ringSecond parameter
            List<int> durations = Barriers.Select(b => b.Duration).ToList();
            var durationSums = Enumerable.Range(0, durations.Count).Select(d => (d == 0) ? durations[d] : durations[d] += durations[d - 1]).ToList();

            // Using the constructed list of barrier duration ranges, find the barrier that this ringSecond should correspond to
            int barrierIndex = durationSums.FindIndex(s => ringSecond < s);
            if (barrierIndex < 0) barrierIndex = 0;

            var barrier = Barriers[barrierIndex];

            // Find out "how far" we are into this barrier
            int barrierSecond = (barrierIndex == 0) ? ringSecond : ringSecond - durationSums[barrierIndex - 1];
            if (barrierSecond < 0) barrierSecond = 0;

            barrier.Advance(nSeconds, barrierSecond, CoordinatedPhase);
        }

        internal void Zero()
        {
            foreach (var barrier in Barriers)
            {
                barrier.Zero();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTSController.Phasing;

namespace TTSController.Sequencing
{
    public class Sequence
    {
        private List<RingGroup> _ringGroups = new List<RingGroup>();

        public List<RingGroup> RingGroups { get { return _ringGroups; } }
        public int Duration { get { return RingGroups.Select(r => r.Duration).Max(); } }

        public Sequence() { }

        internal void Advance(int nSeconds, int cycleSecond)
        {
            foreach (var ringGroup in _ringGroups)
            {
                ringGroup.Advance(nSeconds, cycleSecond);
            }
        }

        internal void Zero()
        {
            foreach (var ringGroup in _ringGroups)
            {
                ringGroup.Zero();
            }
        }

        public void PlaceVehicleCall(int phaseID)
        {
            Phase callPhase = GetPhase(phaseID);
            callPhase.VehiclePhase.HasCall = true;

            foreach (var ringGroup in RingGroups)
            {
                foreach (var ring in ringGroup.Rings)
                {
                    foreach (var phase in ring.Phases)
                    {
                        if (phase._callByConflictPhase.ContainsKey(callPhase.ID)) phase._callByConflictPhase[callPhase.ID] = true;
                    }
                }
            }
        }

        public void PlacePedestrianCall(int phaseID)
        {
            Phase callPhase = GetPhase(phaseID);
            callPhase.PedestrianPhase.HasCall = true;

            foreach (var ringGroup in RingGroups)
            {
                foreach (var ring in ringGroup.Rings)
                {
                    foreach (var phase in ring.Phases)
                    {
                        if (phase._callByConflictPhase.ContainsKey(callPhase.ID)) phase._callByConflictPhase[callPhase.ID] = true;
                    }
                }
            }
        }

        public void RemoveVehicleCall(int phaseID)
        {
            Phase callPhase = GetPhase(phaseID);
            callPhase.VehiclePhase.HasCall = false;

            foreach (var ringGroup in RingGroups)
            {
                foreach (var ring in ringGroup.Rings)
                {
                    foreach (var phase in ring.Phases)
                    {
                        if (phase._callByConflictPhase.ContainsKey(callPhase.ID)) phase._callByConflictPhase[callPhase.ID] = false;
                    }
                }
            }
        }

        public void RemovePedestrianCall(int phaseID)
        {
            Phase callPhase = GetPhase(phaseID);
            callPhase.PedestrianPhase.HasCall = false;
        }

        public Phase GetPhase(int phaseID)
        {
            foreach (var ringGroup in RingGroups)
            {
                foreach (var ring in ringGroup.Rings)
                {
                    Phase phase = ring.Phases.FirstOrDefault(p => p.ID == phaseID);
                    if (phase != null)
                    {
                        return phase;
                    }
                }
            }

            throw new ArgumentOutOfRangeException("Phase " + phaseID + " not found in sequence");
        }

        public List<Phase> GetPhases()
        {
            List<Phase> phases = new List<Phase>();

            foreach (var ringGroup in RingGroups)
            {
                foreach (var ring in ringGroup.Rings)
                {
                    phases.AddRange(ring.Phases);
                }
            }

            return phases;
        }
    }
}

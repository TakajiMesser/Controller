using Controller.Phasing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Controller.Sequencing
{
    public class Sequence
    {
        public List<RingGroup> RingGroups { get; } = new List<RingGroup>();
        public int Duration => RingGroups.Select(r => r.Duration).Max();

        public Sequence() { }

        internal void Advance(int nSeconds, int cycleSecond)
        {
            foreach (var ringGroup in RingGroups)
            {
                ringGroup.Advance(nSeconds, cycleSecond);
            }
        }

        internal void Zero()
        {
            foreach (var ringGroup in RingGroups)
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

        public IEnumerable<Phase> GetPhases()
        {
            foreach (var ringGroup in RingGroups)
            {
                foreach (var ring in ringGroup.Rings)
                {
                    foreach (var phase in ring.Phases)
                    {
                        yield return phase;
                    }
                }
            }
        }
    }
}

using Controller.Phasing;
using System;
using System.Collections.Generic;

namespace Controller
{
    public class TimingPlan
    {
        public int CycleSecond { get; private set; }
        public Dictionary<int, Pattern> Patterns { get; private set; } = new Dictionary<int, Pattern>();
        public Dictionary<int, Phase> DefaultPhases { get; set; } = new Dictionary<int, Phase>();
        public Pattern DefaultPattern { get; set; } = new Pattern(1);

        public int CurrentPattern
        {
            get => _currentPattern;
            set
            {
                if (!Patterns.ContainsKey(value)) throw new ArgumentOutOfRangeException("Pattern ID was not found in Timing Plan");
                _currentPattern = value;
            }
        }

        private int _currentPattern = 0;

        public TimingPlan() { }

        public void AddPattern(Pattern pattern) => Patterns.Add(pattern.ID, pattern);

        public void AdvanceController(int nSeconds)
        {
            Pattern pattern = Patterns[CurrentPattern];

            int cycleLength = pattern.CycleLength;
            CycleSecond += nSeconds;

            pattern.Sequence.Advance(nSeconds, CycleSecond);
            CycleSecond %= cycleLength;
        }

        public void Zero()
        {
            foreach (var pattern in Patterns)
            {
                pattern.Value.Sequence.Zero();
            }
        }

        public VehiclePhaseStates GetVehiclePhaseState(int phaseID) => Patterns[_currentPattern].Sequence.GetPhase(phaseID).VehiclePhase.State;

        public PedestrianPhaseStates GetPedestrianPhaseState(int phaseID) => Patterns[_currentPattern].Sequence.GetPhase(phaseID).PedestrianPhase.State;

        public Dictionary<int, VehiclePhaseStates> GetVehiclePhaseStates()
        {
            Dictionary<int, VehiclePhaseStates> states = new Dictionary<int, VehiclePhaseStates>();

            foreach (var pattern in Patterns)
            {
                foreach (var ringGroup in pattern.Value.Sequence.RingGroups)
                {
                    foreach (var ring in ringGroup.Rings)
                    {
                        foreach (var phase in ring.Phases)
                        {
                            states.Add(phase.ID, phase.VehiclePhase.State);
                        }
                    }
                }
            }

            return states;
        }

        public Dictionary<int, PedestrianPhaseStates> GetPedestrianPhaseStates()
        {
            Dictionary<int, PedestrianPhaseStates> states = new Dictionary<int, PedestrianPhaseStates>();

            foreach (var pattern in Patterns)
            {
                foreach (var ringGroup in pattern.Value.Sequence.RingGroups)
                {
                    foreach (var ring in ringGroup.Rings)
                    {
                        foreach (var phase in ring.Phases)
                        {
                            states.Add(phase.ID, phase.PedestrianPhase.State);
                        }
                    }
                }
            }

            return states;
        }
    }
}

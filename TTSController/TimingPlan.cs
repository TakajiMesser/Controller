﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTSController.Phasing;

namespace TTSController
{
    public class TimingPlan
    {
        private int _cycleSecond = 0;
        private int _currentPattern = 0;
        private Dictionary<int, Pattern> _patterns = new Dictionary<int, Pattern>();

        public int CycleSecond { get { return _cycleSecond; } }
        public int CurrentPattern {
            get { return _currentPattern; }
            set {
                if (!_patterns.ContainsKey(value)) throw new ArgumentOutOfRangeException("Pattern ID was not found in Timing Plan");
                _currentPattern = value;
            }
        }
        public Dictionary<int, Pattern> Patterns { get { return _patterns; } }

        public Dictionary<int, Phase> DefaultPhases { get; set; }
        public Pattern DefaultPattern { get; set; }

        public TimingPlan()
        {
            DefaultPhases = new Dictionary<int, Phase>();
            DefaultPattern = new Pattern(1);
        }

        public void AddPattern(Pattern pattern)
        {
            _patterns.Add(pattern.ID, pattern);
        }

        public void AdvanceController(int nSeconds)
        {
            Pattern pattern = _patterns[CurrentPattern];

            int cycleLength = pattern.CycleLength;
            _cycleSecond = _cycleSecond + nSeconds;

            pattern.Sequence.Advance(nSeconds, _cycleSecond);
            _cycleSecond %= cycleLength;
        }

        public void Zero()
        {
            foreach (var pattern in Patterns)
            {
                pattern.Value.Sequence.Zero();
            }
        }

        public VehiclePhaseStates GetVehiclePhaseState(int phaseID)
        {
            Phase phase = _patterns[_currentPattern].Sequence.GetPhase(phaseID);
            return phase.VehiclePhase.State;
        }

        public PedestrianPhaseStates GetPedestrianPhaseState(int phaseID)
        {
            Phase phase = _patterns[_currentPattern].Sequence.GetPhase(phaseID);
            return phase.PedestrianPhase.State;
        }

        public Dictionary<int, VehiclePhaseStates> GetVehiclePhaseStates()
        {
            Dictionary<int, VehiclePhaseStates> states = new Dictionary<int, VehiclePhaseStates>();

            foreach (var pattern in _patterns)
            {
                foreach (var ring in pattern.Value.Sequence.Rings)
                {
                    foreach (var phase in ring.Phases)
                    {
                        states.Add(phase.ID, phase.VehiclePhase.State);
                    }
                }
            }

            return states;
        }

        public Dictionary<int, PedestrianPhaseStates> GetPedestrianPhaseStates()
        {
            Dictionary<int, PedestrianPhaseStates> states = new Dictionary<int, PedestrianPhaseStates>();

            foreach (var pattern in _patterns)
            {
                foreach (var ring in pattern.Value.Sequence.Rings)
                {
                    foreach (var phase in ring.Phases)
                    {
                        states.Add(phase.ID, phase.PedestrianPhase.State);
                    }
                }
            }

            return states;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Controller.Phasing
{
    public class Phase
    {
        public int ID { get; private set; }
        public int Split { get; private set; }
        public VehiclePhase VehiclePhase { get; private set; } = new VehiclePhase();
        public PedestrianPhase PedestrianPhase { get; private set; } = new PedestrianPhase();
        public bool IsCoordinated { get; set; }
        public bool FloatingForceOff { get; set; }

        public bool HasCall => VehiclePhase.HasCall || PedestrianPhase.HasCall;
        public bool HasOpposingCall => _callByConflictPhase.Any(kvp => kvp.Value);

        internal Dictionary<int, bool> _callByConflictPhase = new Dictionary<int, bool>();
        internal List<int> ConflictPhases { get { return _callByConflictPhase.Keys.ToList(); } }
        internal bool IsZero => VehiclePhase.IsZero && PedestrianPhase.IsZero;

        public Phase(int id, int split)
        {
            if (id < 0) throw new ArgumentOutOfRangeException("Phase ID must be positive");
            if (split < 0) throw new ArgumentOutOfRangeException("Split must be positive");

            ID = id;
            Split = split;
        }

        internal void Advance(int nSeconds)
        {
            VehiclePhase.Advance(nSeconds, IsCoordinated, HasOpposingCall);
            PedestrianPhase.Advance(nSeconds);

            // Clear out the opposing phase calls once this phase enters clearance
            if (VehiclePhase.State == VehiclePhaseStates.Yellow)
            {
                foreach (var phase in _callByConflictPhase.Keys.ToList())
                {
                    _callByConflictPhase[phase] = false;
                }
            }
        }

        internal void Zero()
        {
            VehiclePhase.Zero();
            PedestrianPhase.Zero();

            foreach (var phase in _callByConflictPhase.Keys.ToList())
            {
                _callByConflictPhase[phase] = false;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTSController.Phasing
{
    public class Phase
    {
        private int _id;
        private int _split;
        private VehiclePhase _vehiclePhase = new VehiclePhase();
        private PedestrianPhase _pedestrianPhase = new PedestrianPhase();

        internal Dictionary<int, bool> _callByConflictPhase = new Dictionary<int, bool>();
        internal List<int> ConflictPhases { get { return _callByConflictPhase.Keys.ToList(); } }
        internal bool IsZero { get { return _vehiclePhase.IsZero && _pedestrianPhase.IsZero; } }

        public int ID { get { return _id; } }
        public int Split { get { return _split; } }
        public VehiclePhase VehiclePhase { get { return _vehiclePhase; } }
        public PedestrianPhase PedestrianPhase { get { return _pedestrianPhase; } }
        public bool IsCoordinated { get; set; }
        public bool FloatingForceOff { get; set; }
        public bool HasCall { get { return _vehiclePhase.HasCall || _pedestrianPhase.HasCall; } }
        public bool HasOpposingCall { get { return _callByConflictPhase.Any(kvp => kvp.Value); } }

        public Phase(int id)
        {
            if (id < 0) throw new ArgumentOutOfRangeException("Phase ID must be positive");
            _id = id;
        }

        public Phase(int id, int split)
        {
            if (id < 0) throw new ArgumentOutOfRangeException("Phase ID must be positive");
            if (split < 0) throw new ArgumentOutOfRangeException("Split must be positive");

            _id = id;
            _split = split;
        }

        internal void Advance(int nSeconds)
        {
            _vehiclePhase.Advance(nSeconds, IsCoordinated, HasOpposingCall);
            _pedestrianPhase.Advance(nSeconds);

            // Clear out the opposing phase calls once this phase enters clearance
            if (_vehiclePhase.State == VehiclePhaseStates.Yellow)
            {
                foreach (var phase in _callByConflictPhase.Keys.ToList())
                {
                    _callByConflictPhase[phase] = false;
                }
            }
        }

        internal void Zero()
        {
            _vehiclePhase.Zero();
            _pedestrianPhase.Zero();

            foreach (var phase in _callByConflictPhase.Keys.ToList())
            {
                _callByConflictPhase[phase] = false;
            }
        }
    }
}

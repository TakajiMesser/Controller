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
        private VehiclePhase _vehiclePhase = new VehiclePhase();
        private PedestrianPhase _pedestrianPhase = new PedestrianPhase();

        internal Dictionary<int, bool> _callByConflictPhase = new Dictionary<int, bool>();
        internal List<int> ConflictPhases { get { return _callByConflictPhase.Keys.ToList(); } }
        internal bool IsZero { get { return _vehiclePhase.IsZero && _pedestrianPhase.IsZero; } }

        public int ID { get { return _id; } }
        public VehiclePhase Vehicle { get { return _vehiclePhase; } }
        public PedestrianPhase Pedestrian { get { return _pedestrianPhase; } }
        public bool IsCoordinated { get; set; }
        public bool FloatingForceOff { get; set; }
        public bool HasCall { get { return _vehiclePhase.HasCall || _pedestrianPhase.HasCall; } }
        public bool HasOpposingCall { get { return _callByConflictPhase.Any(kvp => kvp.Value); } }

        public Phase(int id)
        {
            if (id < 0) throw new ArgumentOutOfRangeException("Phase ID must be positive");
            _id = id;
        }

        internal void Advance(int nSeconds)
        {
            _vehiclePhase.Advance(nSeconds, IsCoordinated, HasOpposingCall);
            _pedestrianPhase.Advance(nSeconds);

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

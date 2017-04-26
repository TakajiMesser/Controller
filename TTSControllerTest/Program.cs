using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TTSController;
using TTSController.Phasing;
using TTSController.Sequence;

namespace TTSControllerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TTSControllerTest Console Application");

            string option;
            do
            {
                Console.WriteLine("t - Test");
                Console.WriteLine("q - Quit");

                Console.Write("Please select an option: ");
                option = Console.ReadLine();
                Console.WriteLine();

                TimingPlan controller = GenerateTimingPlan();

                switch (option)
                {
                    case "t":
                        controller.CurrentPattern = 1;
                        IterateAndAdvanceController(controller, 100);
                        controller.Zero();
                        break;
                }

                Console.WriteLine();
            } while (option != "q");

            Console.WriteLine("Closing application...");
        }

        private static void IterateAndAdvanceController(TimingPlan timingPlan, int nAdvances)
        {
            Pattern pattern = timingPlan.Patterns[timingPlan.CurrentPattern];

            WriteRingSequence(pattern.Sequence);
            WritePhases(pattern.Sequence);

            WriteHeaders(pattern.Sequence);

            for (var i = 1; i <= nAdvances; i++)
            {
                ReportValues(timingPlan, i);
                PlaceCalls(pattern.Sequence, i);
                timingPlan.AdvanceController(1);
            }
        }

        private static TimingPlan GenerateTimingPlan()
        {
            var controller = new TimingPlan();
            var pattern = new Pattern(1);

            var phases = new List<Phase>();

            for (var i = 1; i <= 8; i++)
            {
                var phase = new Phase(i);

                phase.Vehicle.Split = 25;
                phase.Vehicle.MinGreen = 10;
                phase.Vehicle.Yellow = 3.0;
                phase.Vehicle.RedClearance = 2.0;
                phase.Vehicle.MaxGreen = 15;

                phases.Add(phase);
            }

            phases.First().IsCoordinated = true;

            Ring ring1 = new Ring(phases.GetRange(0, 4));
            ring1.BarrierIndices.Add(2);

            Ring ring2 = new Ring(phases.GetRange(4, 4));
            ring2.BarrierIndices.Add(2);

            var rings = new List<Ring>() { ring1, ring2 };
            pattern.Sequence = new RingSequence(rings);

            controller.AddPattern(pattern);
            return controller;
        }

        public static void PlaceCalls(RingSequence sequence, int iteration)
        {
            // Place calls all the time, for now
            if (iteration == 1)
            {
                sequence.PlaceCall(1);
                sequence.PlaceCall(2);
            }

            if (iteration == 49 || iteration == 127)
            {
                sequence.PlaceCall(3);
                sequence.PlaceCall(4);
            }
        }

        private static void WriteRingSequence(RingSequence sequence)
        {
            Console.WriteLine("Ring Sequence:");
            Console.WriteLine();

            foreach (var ring in sequence.Rings)
            {
                List<string> items = new List<string>();

                for (var i = 0; i < ring.Phases.Count; i++)
                {
                    if (ring.BarrierIndices.Contains(i))
                    {
                        items.Add("|");
                    }

                    Phase phase = ring.Phases[i];
                    items.Add(phase.ID.ToString());
                }

                Console.WriteLine(String.Join(" ", items));
            }

            Console.WriteLine();
        }

        private static void WritePhases(RingSequence sequence)
        {
            foreach (var ring in sequence.Rings)
            {
                foreach (var phase in ring.Phases)
                {
                    Console.WriteLine("Phase " + phase.ID + "| Coordinated: " + phase.IsCoordinated + ", MaxGreen: " + phase.Vehicle.MaxGreen + ", MinGreen: " + phase.Vehicle.MinGreen + ", Yellow: " + phase.Vehicle.Yellow + ", RedClearance: " + phase.Vehicle.RedClearance);
                }
            }

            Console.WriteLine();
        }

        private static void WriteHeaders(RingSequence sequence)
        {
            List<string> headers = new List<string>() { "CA", "CS" };
            headers.AddRange(sequence.GetPhases().Select(p => "P" + p.ID));

            Console.WriteLine(String.Join("\t| ", headers));
            Console.WriteLine(new String('-', headers.Count * 8));
        }

        private static void ReportValues(TimingPlan controller, int iteration)
        {
            List<string> values = new List<string>();

            values.Add(iteration.ToString());
            values.Add(controller.CycleSecond.ToString());

            foreach (var phaseStates in controller.GetVehiclePhaseStates())
            {
                values.Add(GetPhaseStateString(phaseStates.Value));
            }

            Console.WriteLine(String.Join("\t| ", values));
        }

        private static string GetPhaseStateString(VehiclePhaseStates state)
        {
            switch (state)
            {
                case VehiclePhaseStates.Green:
                    return "G";
                case VehiclePhaseStates.Yellow:
                    return "Y";
                case VehiclePhaseStates.Red:
                    return "R";
                default:
                    return "";
            }
        }
    }
}

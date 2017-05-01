using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TTSController;
using TTSController.Phasing;
using TTSController.Sequencing;

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
                var phase = new Phase(i, 25);

                if (i == 1)
                {
                    phase.IsCoordinated = true;
                }

                phase.VehiclePhase.MinGreen = 10;
                phase.VehiclePhase.Yellow = 3.0;
                phase.VehiclePhase.RedClearance = 2.0;
                phase.VehiclePhase.MaxGreen = 15;

                phases.Add(phase);
            }

            Ring ring1 = new Ring();
            ring1.Phases.AddRange(phases.GetRange(0, 4));
            ring1.BarrierIndices.Add(2);

            Ring ring2 = new Ring();
            ring2.Phases.AddRange(phases.GetRange(4, 4));
            ring2.BarrierIndices.Add(2);

            RingGroup ringGroup = new RingGroup();
            ringGroup.Rings.Add(ring1);
            ringGroup.Rings.Add(ring2);

            pattern.Sequence.RingGroups.Add(ringGroup);

            controller.AddPattern(pattern);
            return controller;
        }

        public static void PlaceCalls(Sequence sequence, int iteration)
        {
            // Place calls all the time, for now
            if (iteration == 1)
            {
                sequence.PlaceVehicleCall(1);
                sequence.PlaceVehicleCall(2);
            }

            if (iteration == 49 || iteration == 127)
            {
                sequence.PlaceVehicleCall(3);
                sequence.PlaceVehicleCall(4);
            }
        }

        private static void WriteRingSequence(Sequence sequence)
        {
            Console.WriteLine("Ring Sequence:");
            Console.WriteLine();

            foreach (var ringGroup in sequence.RingGroups)
            {
                foreach (var ring in ringGroup.Rings)
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
            }

            Console.WriteLine();
        }

        private static void WritePhases(Sequence sequence)
        {
            foreach (var ringGroup in sequence.RingGroups)
            {
                foreach (var ring in ringGroup.Rings)
                {
                    foreach (var phase in ring.Phases)
                    {
                        Console.WriteLine("Phase " + phase.ID + "| Coordinated: " + phase.IsCoordinated + ", MaxGreen: " + phase.VehiclePhase.MaxGreen + ", MinGreen: " + phase.VehiclePhase.MinGreen + ", Yellow: " + phase.VehiclePhase.Yellow + ", RedClearance: " + phase.VehiclePhase.RedClearance);
                    }
                }
            }

            Console.WriteLine();
        }

        private static void WriteHeaders(Sequence sequence)
        {
            List<string> headers = new List<string>() { "CA", "CS" };
            headers.AddRange(sequence.GetPhases().Select(p => "P" + p.ID));

            Console.WriteLine(String.Join("\t| ", headers));
            Console.WriteLine(new String('-', headers.Count * 8));
        }

        private static void ReportValues(TimingPlan controller, int iteration)
        {
            Console.Write(iteration.ToString() + "\t|");
            Console.Write(controller.CycleSecond.ToString() + "\t|");

            var vehicleStates = controller.GetVehiclePhaseStates();
            var pedestrianStates = controller.GetPedestrianPhaseStates();

            SortedSet<int> phaseIDs = new SortedSet<int>(vehicleStates.Keys.Union(pedestrianStates.Keys));

            foreach (var phaseID in phaseIDs)
            {
                if (vehicleStates.ContainsKey(phaseID))
                {
                    switch (vehicleStates[phaseID])
                    {
                        case VehiclePhaseStates.Green:
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(" G");
                            Console.ResetColor();
                            break;
                        case VehiclePhaseStates.Yellow:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write(" Y");
                            Console.ResetColor();
                            break;
                        case VehiclePhaseStates.Red:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(" R");
                            Console.ResetColor();
                            break;
                    }
                }

                if (pedestrianStates.ContainsKey(phaseID))
                {
                    switch (pedestrianStates[phaseID])
                    {
                        case PedestrianPhaseStates.Walk:
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(" W");
                            Console.ResetColor();
                            break;
                        case PedestrianPhaseStates.FlashingDoNotWalk:
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.Write(" F");
                            Console.ResetColor();
                            break;
                        case PedestrianPhaseStates.DoNotWalk:
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.Write(" D");
                            Console.ResetColor();
                            break;
                    }
                }

                Console.Write("\t| ");
            }

            Console.Write("\n");
        }
    }
}

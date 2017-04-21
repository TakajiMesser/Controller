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

            var phases1 = new List<Phase>();
            phases1.Add(new Phase(1, 25) { MinGreen = 10, Yellow = 3.0, RedClearance = 2.0, MaxGreen = 15, IsCoordinated = true });
            phases1.Add(new Phase(2, 25) { MinGreen = 10, Yellow = 3.0, RedClearance = 2.0, MaxGreen = 15 });
            phases1.Add(new Phase(3, 25) { MinGreen = 10, Yellow = 3.0, RedClearance = 2.0, MaxGreen = 15 });
            phases1.Add(new Phase(4, 25) { MinGreen = 10, Yellow = 3.0, RedClearance = 2.0, MaxGreen = 15 });

            Ring ring1 = new Ring(phases1);
            ring1.BarrierIndices.Add(2);

            var phases2 = new List<Phase>();
            phases2.Add(new Phase(5, 25) { MinGreen = 10, Yellow = 3.0, RedClearance = 2.0, MaxGreen = 15 });
            phases2.Add(new Phase(6, 25) { MinGreen = 10, Yellow = 3.0, RedClearance = 2.0, MaxGreen = 15 });
            phases2.Add(new Phase(7, 25) { MinGreen = 10, Yellow = 3.0, RedClearance = 2.0, MaxGreen = 15 });
            phases2.Add(new Phase(8, 25) { MinGreen = 10, Yellow = 3.0, RedClearance = 2.0, MaxGreen = 15 });

            Ring ring2 = new Ring(phases2);
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
                    Console.WriteLine("Phase " + phase.ID + "| Coordinated: " + phase.IsCoordinated + ", MaxGreen: " + phase.MaxGreen + ", MinGreen: " + phase.MinGreen + ", Yellow: " + phase.Yellow + ", RedClearance: " + phase.RedClearance);
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

            foreach (var phaseStates in controller.GetPhaseStates())
            {
                values.Add(GetPhaseStateString(phaseStates.Value));
            }

            Console.WriteLine(String.Join("\t| ", values));
        }

        private static string GetPhaseStateString(PhaseStates state)
        {
            switch (state)
            {
                case PhaseStates.Green:
                    return "G";
                case PhaseStates.Yellow:
                    return "Y";
                case PhaseStates.Red:
                    return "R";
                default:
                    return "";
            }
        }
    }
}

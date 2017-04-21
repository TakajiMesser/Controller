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
            var rings = new List<Ring>();
            var barriers = new List<Barrier>();
            var phases = new List<Phase>();

            phases.Add(new Phase(1, 25) { MinGreen = 10, Yellow = 3.0, RedClearance = 2.0, MaxGreen = 15, IsCoordinated = true });
            phases.Add(new Phase(2, 25) { MinGreen = 10, Yellow = 3.0, RedClearance = 2.0, MaxGreen = 15 });
            phases.Add(new Phase(3, 25) { MinGreen = 10, Yellow = 3.0, RedClearance = 2.0, MaxGreen = 15 });
            phases.Add(new Phase(4, 25) { MinGreen = 10, Yellow = 3.0, RedClearance = 2.0, MaxGreen = 15 });
            phases.Add(new Phase(5, 25) { MinGreen = 10, Yellow = 3.0, RedClearance = 2.0, MaxGreen = 15 });
            phases.Add(new Phase(6, 25) { MinGreen = 10, Yellow = 3.0, RedClearance = 2.0, MaxGreen = 15 });
            phases.Add(new Phase(7, 25) { MinGreen = 10, Yellow = 3.0, RedClearance = 2.0, MaxGreen = 15 });
            phases.Add(new Phase(8, 25) { MinGreen = 10, Yellow = 3.0, RedClearance = 2.0, MaxGreen = 15 });

            barriers.Add(new Barrier(phases.GetRange(0, 2)));
            barriers.Add(new Barrier(phases.GetRange(2, 2)));
            barriers.Add(new Barrier(phases.GetRange(4, 2)));
            barriers.Add(new Barrier(phases.GetRange(6, 2)));

            rings.Add(new Ring(barriers.GetRange(0, 2)));
            rings.Add(new Ring(barriers.GetRange(2, 2)));

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

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
                        IterateAndAdvanceController(controller, 200);
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

            List<string> headers = new List<string>() { "CS", "CA" };
            headers.AddRange(pattern.Sequence.GetPhases().Select(p => "P" + p.ID));

            Console.WriteLine(String.Join("\t| ", headers));
            Console.WriteLine(new String('-', headers.Count * 8));

            for (var i = 0; i <= nAdvances; i++)
            {
                List<string> values = new List<string>();

                values.Add(timingPlan.CycleSecond.ToString());
                values.Add(i.ToString());

                foreach (var phaseStates in timingPlan.GetPhaseStates())
                {
                    values.Add(GetPhaseStateString(phaseStates.Value));
                }

                Console.WriteLine(String.Join("\t| ", values));

                // Place calls all the time, for now
                foreach (var ring in pattern.Sequence.Rings)
                {
                    if (i == 0)
                    {
                        pattern.Sequence.PlaceCall(1);
                        //pattern.Sequence.PlaceCall(2);
                    }

                    if (i == 27 || i == 127)
                    {
                        pattern.Sequence.PlaceCall(2);
                    }
                }

                timingPlan.AdvanceController(1);
            }
        }

        private static TimingPlan GenerateTimingPlan()
        {
            var controller = new TimingPlan();

            var pattern = new Pattern(1);

            int nRings = 2;
            int nBarriersPerRing = 2;
            int nPhasesPerBarrier = 2;
            int coordPhaseID = 1;

            for (var i = 0; i < nRings; i++)
            {
                var ring = new Ring();

                for (var j = 0; j < nBarriersPerRing; j++)
                {
                    var phases = new List<Phase>();

                    for (var k = 1; k <= nPhasesPerBarrier; k++)
                    {
                        int id = (i * nBarriersPerRing * nPhasesPerBarrier) + (j * nPhasesPerBarrier) + k;

                        var phase = new Phase(id, 25);

                        phase.MinGreen = 10;
                        phase.Yellow = 2.0;
                        phase.RedClearance = 1.0;

                        if (id == coordPhaseID)
                        {
                            phase.IsCoordinated = true;
                        }
                        else
                        {
                            phase.FloatingForceOff = false;
                            phase.MaxGreen = 18;
                        }

                        phases.Add(phase);
                    }

                    var barrier = new Barrier(phases);
                    ring.Barriers.Add(barrier);
                }

                pattern.Sequence.Rings.Add(ring);
            }

            controller.AddPattern(pattern);

            return controller;
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

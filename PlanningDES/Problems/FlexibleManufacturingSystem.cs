using System.Collections.Generic;
using System.Linq;
using UltraDES;
using DFA = UltraDES.DeterministicFiniteAutomaton;

namespace PlanningDES.Problems
{
    public class FlexibleManufacturingSystem : ISchedulingProblem
    {
        public readonly Dictionary<int, AbstractEvent> _e;

        public FlexibleManufacturingSystem()
        {
            _e = new[]
            {
                11, 12, 21, 22, 41, 42, 51, 52, 53, 54, 31, 32, 33, 34, 35, 36, 37, 38, 39, 30, 61, 63, 65, 64, 66,
                71, 72, 73, 74, 81, 82
            }.ToDictionary(alias => alias,
                alias => (AbstractEvent) new Event($"{alias}", alias % 2 == 0 ? Controllability.Uncontrollable : Controllability.Controllable));


            var s = Enumerable.Range(0, 6).ToDictionary(i => i,
                i => new ExpandedState($"{i}", i == 0 ? 0u : 1u, i == 0 ? Marking.Marked : Marking.Unmarked));

            // C1
            var c1 = new DFA(new Transition[] {(s[0], _e[11], s[1]), (s[1], _e[12], s[0])}, s[0], "C1");

            // C2
            var c2 = new DFA(new Transition[] {(s[0], _e[21], s[1]), (s[1], _e[22], s[0])}, s[0], "C2");

            // Lathe
            var lathe = new DFA(new Transition[] {(s[0], _e[41], s[1]), (s[1], _e[42], s[0])}, s[0], "Fresa");

            // Paint Device
            var pd = new DFA(new Transition[] {(s[0], _e[81], s[1]), (s[1], _e[82], s[0])}, s[0], "MP");

            // Mill
            var mill = new DFA(
                new Transition[]
                {
                    (s[0], _e[51], s[1]), (s[1], _e[52], s[0]), (s[0], _e[53], s[2]), (s[2], _e[54], s[0])
                }, s[0], "Torno");

            // C3
            var c3 = new DFA(
                new Transition[]
                {
                    (s[0], _e[71], s[1]), (s[1], _e[72], s[0]), (s[0], _e[73], s[2]), (s[2], _e[74], s[0])
                }, s[0], "C3");

            // Robot
            var robot = new DFA(
                new Transition[]
                {
                    (s[0], _e[31], s[1]), (s[1], _e[32], s[0]), (s[0], _e[33], s[2]), (s[2], _e[34], s[0]),
                    (s[0], _e[35], s[3]), (s[3], _e[36], s[0]), (s[0], _e[37], s[4]), (s[4], _e[38], s[0]),
                    (s[0], _e[39], s[5]), (s[5], _e[30], s[0])
                }, s[0], "Robot");

            // Assembly Machine
            var am = new DFA(
                new Transition[]
                {
                    (s[0], _e[61], s[1]), (s[1], _e[63], s[2]), (s[1], _e[65], s[3]), (s[2], _e[64], s[0]),
                    (s[3], _e[66], s[0])
                }, s[0], "MM");

            // Specifications
            s = Enumerable.Range(0, 6).ToDictionary(i => i,
                i => new ExpandedState($"{i}", 0, i == 0 ? Marking.Marked : Marking.Unmarked));

            // E1
            var e1 = new DFA(new Transition[] {(s[0], _e[12], s[1]), (s[1], _e[31], s[0])}, s[0], "E1");

            // E2
            var e2 = new DFA(new Transition[] {(s[0], _e[22], s[1]), (s[1], _e[33], s[0])}, s[0], "E2");

            // E5
            var e5 = new DFA(new Transition[] {(s[0], _e[36], s[1]), (s[1], _e[61], s[0])}, s[0], "E5");

            // E6
            var e6 = new DFA(new Transition[] {(s[0], _e[38], s[1]), (s[1], _e[63], s[0])}, s[0], "E6");

            // E3
            var e3 = new DFA(
                new Transition[]
                {
                    (s[0], _e[32], s[1]), (s[1], _e[41], s[0]), (s[0], _e[42], s[2]), (s[2], _e[35], s[0])
                }, s[0], "E3");

            // E7
            var e7 = new DFA(
                new Transition[]
                {
                    (s[0], _e[30], s[1]), (s[1], _e[71], s[0]), (s[0], _e[74], s[2]), (s[2], _e[65], s[0])
                }, s[0], "E7");

            // E8
            var e8 = new DFA(
                new Transition[]
                {
                    (s[0], _e[72], s[1]), (s[1], _e[81], s[0]), (s[0], _e[82], s[2]), (s[2], _e[73], s[0])
                }, s[0], "E8");

            // E4
            var e4 = new DFA(
                new Transition[]
                {
                    (s[0], _e[34], s[1]), (s[1], _e[51], s[0]), (s[1], _e[53], s[0]), (s[0], _e[52], s[2]),
                    (s[2], _e[37], s[0]), (s[0], _e[54], s[3]), (s[3], _e[39], s[0])
                }, s[0], "E4");

            Supervisor = DFA.MonolithicSupervisor(new[] {c1, c2, lathe, mill, robot, am, c3, pd},
                new[] {e1, e2, e3, e4, e5, e6, e7, e8}, true);

            Events = _e.Values.ToList();

            Transitions = Supervisor.Transitions.GroupBy(t => t.Origin)
                .ToDictionary(g => g.Key, g => g.ToDictionary(t => t.Trigger, t => t.Destination));

        }

        public DFA Supervisor { get; }
        public IEnumerable<AbstractEvent> Events { get; }
        public Dictionary<AbstractState, Dictionary<AbstractEvent, AbstractState>> Transitions { get; }

        public int Depth => 44;

        public AbstractState InitialState => Supervisor.InitialState;

        public AbstractState TargetState => Supervisor.InitialState;

        public Restriction InitialRestrition(int products)
        {
            return new Restriction(new[]
            {
                (_e[11], (uint) (2 * products)), (_e[21], (uint) (2 * products)), (_e[31], (uint) (2 * products)),
                (_e[33], (uint) (2 * products)), (_e[35], (uint) (2 * products)), (_e[37], (uint) (1 * products)),
                (_e[39], (uint) (1 * products)), (_e[41], (uint) (2 * products)), (_e[51], (uint) (1 * products)),
                (_e[53], (uint) (1 * products)), (_e[61], (uint) (2 * products)), (_e[63], (uint) (1 * products)),
                (_e[65], (uint) (1 * products)), (_e[71], (uint) (1 * products)), (_e[73], (uint) (1 * products)),
                (_e[81], (uint) (1 * products))
            });
        }

        public Scheduler InitialScheduler =>
            new Scheduler(_e.Select(kvp => (kvp.Value, kvp.Value.IsControllable ? 0.0f : float.PositiveInfinity)),
                new[]
                {
                    (_e[11], _e[12], 25f), (_e[21], _e[22], 25f), (_e[31], _e[32], 21f), (_e[33], _e[34], 19f),
                    (_e[35], _e[36], 16f), (_e[37], _e[38], 24f), (_e[39], _e[30], 20f), (_e[41], _e[42], 30f),
                    (_e[51], _e[52], 38f), (_e[53], _e[54], 32f), (_e[61], _e[63], 15f), (_e[61], _e[65], 15f),
                    (_e[63], _e[64], 25f), (_e[65], _e[66], 25f), (_e[71], _e[72], 25f), (_e[73], _e[74], 25f),
                    (_e[81], _e[82], 24f)
                });

    }
}

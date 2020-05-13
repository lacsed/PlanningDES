using System.Collections.Generic;
using System.Linq;
using UltraDES;
using DFA = UltraDES.DeterministicFiniteAutomaton;

namespace PlanningDES.Problems
{
    public class IndustrialTransferLine : ISchedulingProblem
    {
        private readonly Dictionary<int, AbstractEvent> _e;

        public IndustrialTransferLine()
        {
            _e = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12}.ToDictionary(alias => alias,
                alias => (AbstractEvent) new Event($"{alias}", alias % 2 == 0 ? Controllability.Uncontrollable : Controllability.Controllable));

            var s = new[] {new ExpandedState("I", 0, Marking.Marked), new ExpandedState("W", 1, Marking.Unmarked)};

            var m1 = new DFA(new[] {new Transition(s[0], _e[1], s[1]), new Transition(s[1], _e[2], s[0])}, s[0], "M1");

            var m2 = new DFA(
                new[] {new Transition(s[0], _e[3], s[1]), new Transition(s[1], _e[4], s[0])}, s[0], "M2");

            var m3 = new DFA(
                new[] {new Transition(s[0], _e[5], s[1]), new Transition(s[1], _e[6], s[0])}, s[0], "M3");

            var m4 = new DFA(
                new[] {new Transition(s[0], _e[7], s[1]), new Transition(s[1], _e[8], s[0])}, s[0], "M4");

            var m5 = new DFA(
                new[] {new Transition(s[0], _e[9], s[1]), new Transition(s[1], _e[10], s[0])}, s[0], "M5");

            var m6 = new DFA(
                new[] {new Transition(s[0], _e[11], s[1]), new Transition(s[1], _e[12], s[0])}, s[0], "M6");

            s = Enumerable.Range(0, 4)
                .Select(i => new ExpandedState(i.ToString(), 0, i == 0 ? Marking.Marked : Marking.Unmarked)).ToArray();

            var e1 = new DFA(
                new[] {new Transition(s[0], _e[2], s[1]), new Transition(s[1], _e[3], s[0])}, s[0], "E1");

            var e2 = new DFA(
                new[] {new Transition(s[0], _e[6], s[1]), new Transition(s[1], _e[7], s[0])}, s[0], "E2");

            var e3 = new DFA(
                new[]
                {
                    new Transition(s[0], _e[4], s[1]), new Transition(s[1], _e[8], s[2]),
                    new Transition(s[0], _e[8], s[3]), new Transition(s[3], _e[4], s[2]),
                    new Transition(s[2], _e[9], s[0])
                }, s[0], "E3");

            var e4 = new DFA(
                new[] {new Transition(s[0], _e[10], s[1]), new Transition(s[1], _e[11], s[0])}, s[0], "E4");

            Supervisor = DFA.MonolithicSupervisor(new[] {m1, m2, m3, m4, m5, m6}, new[] {e1, e2, e3, e4}, true);

            Events = _e.Values.ToList();

            Transitions = Supervisor.Transitions.GroupBy(t => t.Origin)
                .ToDictionary(g => g.Key, g => g.ToDictionary(t => t.Trigger, t => t.Destination));

        }
        public DFA Supervisor { get; }
        public IEnumerable<AbstractEvent> Events { get; }
        public Dictionary<AbstractState, Dictionary<AbstractEvent, AbstractState>> Transitions { get; }
        public int Depth => 12;

        public Scheduler InitialScheduler =>
            new Scheduler(_e.Select(kvp => (kvp.Value, kvp.Value.IsControllable ? 0.0f : float.PositiveInfinity)),
                new[]
                {
                    (_e[1], _e[2], 25f), (_e[3], _e[4], 25f), (_e[5], _e[6], 38f), (_e[7], _e[8], 21f),
                    (_e[9], _e[10], 19f), (_e[11], _e[12], 24f),
                });
        public AbstractState InitialState => Supervisor.InitialState;
        public AbstractState TargetState => Supervisor.InitialState;

        public Restriction InitialRestrition(int products) =>
            new Restriction(new[]
            {
                (_e[01], (uint) (1 * products)), (_e[03], (uint) (1 * products)), (_e[05], (uint) (1 * products)),
                (_e[07], (uint) (1 * products)), (_e[09], (uint) (1 * products)), (_e[11], (uint) (1 * products)),
            });
    }
}

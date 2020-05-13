using System.Collections.Generic;
using System.Linq;
using UltraDES;
using DFA = UltraDES.DeterministicFiniteAutomaton;

namespace PlanningDES.Problems
{
    public class SmallFactory : ISchedulingProblem
    {
        private readonly Dictionary<int, AbstractEvent> _e;

        public SmallFactory()
        {
            var s = new[] {new ExpandedState("I", 0, Marking.Marked), new ExpandedState("W", 1, Marking.Unmarked)};

            _e = new[] {1, 2, 3, 4}.ToDictionary(alias => alias,
                alias => (AbstractEvent) new Event($"{alias}",
                    alias % 2 == 0 ? Controllability.Uncontrollable : Controllability.Controllable));

            var m1 = new DFA(new Transition[] {(s[0], _e[1], s[1]), (s[1], _e[2], s[0])}, s[0], "M1");

            var m2 = new DFA(new Transition[] {(s[0], _e[3], s[1]), (s[1], _e[4], s[0])}, s[0], "M2");

            s = new[] {new ExpandedState("E", 0, Marking.Marked), new ExpandedState("F", 0, Marking.Unmarked)};

            var e1 = new DFA(new Transition[] {(s[0], _e[2], s[1]), (s[1], _e[3], s[0])}, s[0], "E");

            Supervisor = DFA.MonolithicSupervisor(new[] {m1, m2}, new[] {e1}, true);

            Events = _e.Values.ToList();

            Transitions = Supervisor.Transitions.GroupBy(t => t.Origin)
                .ToDictionary(g => g.Key, g => g.ToDictionary(t => t.Trigger, t => t.Destination));

        }

        public int Depth => 4;
        public DFA Supervisor { get; }
        public IEnumerable<AbstractEvent> Events { get; }
        public Dictionary<AbstractState, Dictionary<AbstractEvent, AbstractState>> Transitions { get; }

        public Scheduler InitialScheduler =>
            new Scheduler(_e.Select(kvp => (kvp.Value, kvp.Value.IsControllable ? 0.0f : float.PositiveInfinity)),
                new[] {(_e[1], _e[2], 10f), (_e[3], _e[4], 5f)});

        public AbstractState InitialState => Supervisor.InitialState;
        public AbstractState TargetState => Supervisor.InitialState;

        public Restriction InitialRestrition(int products) =>
            new Restriction(new[] {(_e[1], (uint) (1 * products)), (_e[3], (uint) (1 * products))});
    }
}

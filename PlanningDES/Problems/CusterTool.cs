using System.Collections.Generic;
using System.Linq;
using UltraDES;
using DFA = UltraDES.DeterministicFiniteAutomaton;

namespace PlanningDES.Problems
{
    public class LinearCusterTool : ISchedulingProblem
    {
        private readonly int _clusters;

        private readonly List<(AbstractEvent, AbstractEvent, float)> _times =
            new List<(AbstractEvent, AbstractEvent, float)>();

        public LinearCusterTool(int clusters = 2, float robotTime = 2f, float chamberTime = 12f)
        {
            _clusters = clusters;
            var splant = Enumerable.Range(0, 4).Select(k =>
                new ExpandedState($"{k}", k == 0 ? 0u : 1u, k == 0 ? Marking.Marked : Marking.Unmarked)).ToArray();
            var sspec = Enumerable.Range(0, 4)
                .Select(k => new ExpandedState($"{k}", 0u, k == 0 ? Marking.Marked : Marking.Unmarked)).ToArray();

            var plants = new List<DFA>();
            var specs = new List<DFA>();

            var events = new HashSet<AbstractEvent>();

            for (var i = 1; i <= clusters; i++)
            {
                var e = Enumerable.Range(0, 9).Select(k => new Event($"{i}|{k}",
                    k % 2 == 0 ? Controllability.Uncontrollable : Controllability.Controllable)).ToArray();

                DFA ri;

                if (i != clusters)
                {
                    events.UnionWith(e);
                    ri = new DFA(
                        new Transition[]
                        {
                            (splant[0], e[1], splant[1]), (splant[1], e[2], splant[0]),
                            (splant[0], e[3], splant[2]), (splant[2], e[4], splant[0]),
                            (splant[0], e[5], splant[3]), (splant[3], e[6], splant[0])
                        }, splant[0], $"R{i}");

                    _times.Add((e[1], e[2], robotTime));
                    _times.Add((e[3], e[4], robotTime));
                    _times.Add((e[5], e[6], robotTime));
                }
                else
                {
                    events.UnionWith(new[] {e[1], e[2], e[5], e[4], e[7], e[8]});
                    ri = new DFA(
                        new Transition[]
                        {
                            (splant[0], e[1], splant[1]), (splant[1], e[2], splant[0]),
                            (splant[0], e[5], splant[2]), (splant[2], e[4], splant[0]),
                        }, splant[0], $"R{i}");

                    _times.Add((e[1], e[2], robotTime));
                    _times.Add((e[5], e[4], robotTime));
                }

                var ci = new DFA(new Transition[] {(splant[0], e[7], splant[1]), (splant[1], e[8], splant[0]),},
                    splant[0], $"C{i}");

                _times.Add((e[7], e[8], chamberTime));

                var ei = new DFA(
                    new Transition[]
                    {
                        (sspec[0], e[2], sspec[1]), (sspec[1], e[7], sspec[0]), (sspec[0], e[8], sspec[2]),
                        (sspec[2], e[5], sspec[0])
                    }, sspec[0], $"E{i}");

                plants.Add(ri);
                plants.Add(ci);
                specs.Add(ei);
            }

            for (var i = 1; i < clusters; i++)
            {
                var e61 = events.First(e => e.ToString() == $"{i}|6");
                var e31 = events.First(e => e.ToString() == $"{i}|3");
                var e12 = events.First(e => e.ToString() == $"{i + 1}|1");
                var e42 = events.First(e => e.ToString() == $"{i + 1}|4");

                var Eij = new DFA(
                    new Transition[]
                    {
                        (sspec[0], e61, sspec[1]), (sspec[1], e12, sspec[0]), (sspec[0], e42, sspec[2]),
                        (sspec[2], e31, sspec[0])
                    }, sspec[0], $"E{i}_{i + 1}");

                specs.Add(Eij);
            }

            Supervisor = DFA.MonolithicSupervisor(plants, specs, true);

            Events = events;

            Transitions = Supervisor.Transitions.GroupBy(t => t.Origin)
                .ToDictionary(g => g.Key, g => g.ToDictionary(t => t.Trigger, t => t.Destination));

        }

        public DeterministicFiniteAutomaton Supervisor { get; }
        public IEnumerable<AbstractEvent> Events { get; }
        public Dictionary<AbstractState, Dictionary<AbstractEvent, AbstractState>> Transitions { get; }
        public int Depth => 8 * _clusters - 2;

        public Scheduler InitialScheduler =>
            new Scheduler(Events.Select(e => (e, e.IsControllable ? 0f : float.PositiveInfinity)), _times);

        public AbstractState InitialState => Supervisor.InitialState;
        public AbstractState TargetState => Supervisor.InitialState;

        public Restriction InitialRestrition(int products) =>
            new Restriction(Events.Where(e => e.IsControllable).Select(e => (e, (uint) products)));
    }
}

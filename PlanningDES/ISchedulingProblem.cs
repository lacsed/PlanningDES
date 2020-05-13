using System.Collections.Generic;
using UltraDES;

namespace PlanningDES
{
    public interface ISchedulingProblem
    {
        DeterministicFiniteAutomaton Supervisor { get; }
        IEnumerable<AbstractEvent> Events { get; }
        Dictionary<AbstractState, Dictionary<AbstractEvent, AbstractState>> Transitions { get; }
        int Depth { get; }
        Scheduler InitialScheduler { get; }
        AbstractState InitialState { get; }
        AbstractState TargetState { get; }
        Restriction InitialRestrition(int products);
    }

}

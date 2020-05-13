using System;
using UltraDES;
using Marking = UltraDES.Marking;

namespace PlanningDES.Problems
{
    [Serializable]
    internal class ExpandedState : State
    {
        public uint Tasks { get; private set; }

        public ExpandedState(string alias, uint tasks, Marking marking = Marking.Unmarked) : base(alias, marking)
        {
            Tasks = tasks;
        }

        public override AbstractState ToMarked => IsMarked ? this : new ExpandedState(Alias, Tasks, Marking.Marked);

        public override AbstractState ToUnmarked => !IsMarked ? this : new ExpandedState(Alias, Tasks, Marking.Unmarked);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (ReferenceEquals(null, obj)) return false;

            var p = obj as State;
            return !ReferenceEquals(p, null) && (Alias == p.Alias && Marking == p.Marking);
        }

        public override int GetHashCode() => Alias.GetHashCode();

        public override string ToString() => Alias;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UltraDES;

namespace PlanningDES
{
    public class Scheduler
    {
        private readonly Dictionary<AbstractEvent, float> _internal;
        private readonly Dictionary<AbstractEvent, (AbstractEvent end, float dt)[]> _timeTable;
        private readonly float _elapsedTime;
        

        private Scheduler(Dictionary<AbstractEvent, float> scheduler, Dictionary<AbstractEvent, (AbstractEvent end, float dt)[]> timeTable, float elapsedTime)
        {
            _internal = scheduler.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            _timeTable = timeTable;
            _elapsedTime = elapsedTime;

        }

        public Scheduler(IEnumerable<(AbstractEvent key, float value)> scheduler, IEnumerable<(AbstractEvent start, AbstractEvent end, float dt)> timeTable, float elapsedTime = 0f)
        {
            _internal = scheduler.ToDictionary(kvp => kvp.key, kvp => kvp.value);
            _elapsedTime = elapsedTime;
            _timeTable = timeTable.GroupBy(t => t.start)
                .ToDictionary(g => g.Key, g => g.Select(t => (t.end, t.dt)).ToArray());
        }

        public float ElapsedTime => _elapsedTime;

        public float this[AbstractEvent e] => _internal.ContainsKey(e) ? _internal[e] : float.PositiveInfinity;

        public ISet<AbstractEvent> Enabled
        {
            get
            {
                var min = _internal.Where(kvp => !float.IsInfinity(kvp.Value) && !kvp.Key.IsControllable)
                    .Select(kvp => kvp.Value).Append(float.PositiveInfinity).Min();

                return _internal.Where(kvp => kvp.Value <= min && !float.IsInfinity(kvp.Value)).Select(kvp => kvp.Key).ToSet();
            }
        }

        public Scheduler Update(AbstractEvent e)
        {
            if(!_internal.ContainsKey(e) || float.IsInfinity(_internal[e])) throw new Exception($"Event {e} not allowed!");
            var time = _internal[e];

            var scheduler = _internal.ToDictionary(kvp => kvp.Key, kvp => kvp.Value > time ? kvp.Value - time : 0f);
            scheduler[e] = e.IsControllable ? 0.0f : float.PositiveInfinity;
            
            if (!_timeTable.ContainsKey(e)) return new Scheduler(scheduler, _timeTable, _elapsedTime + time);
            
            foreach (var (end, dt) in _timeTable[e]) scheduler[end] = dt;

            return new Scheduler(scheduler, _timeTable, _elapsedTime + time);
        
        }
    }
}

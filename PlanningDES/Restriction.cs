using System;
using System.Collections.Generic;
using System.Linq;
using UltraDES;

namespace PlanningDES
{
    public class Restriction
    {
        private readonly Dictionary<AbstractEvent, uint> _internal;

        public Restriction(Dictionary<AbstractEvent, uint> restriction)
        {
            _internal = restriction.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public Restriction(IEnumerable<(AbstractEvent key, uint value)> restriction)
        {
            _internal = restriction.ToDictionary(kvp => kvp.key, kvp => kvp.value);
        }

        public Restriction(IEnumerable<AbstractEvent> sequence, bool onlyControllable = false)
        {
            _internal = (onlyControllable ? sequence.Where(s => s.IsControllable) : sequence).GroupBy(s => s)
                .ToDictionary(g => g.Key, g => (uint) g.Count());
        }

        public ISet<AbstractEvent> Enabled => _internal.Where(kvp => kvp.Value > 0).Select(kvp => kvp.Key).ToSet();

        public uint this[AbstractEvent e] => _internal.ContainsKey(e) ? _internal[e] : 0;

        public Restriction Update(AbstractEvent e)
        {
            if(!_internal.ContainsKey(e) || _internal[e] == 0) throw new Exception($"Event {e} not allowed");
            return new Restriction(_internal.Select(kvp => (kvp.Key, kvp.Key == e ? kvp.Value - 1 : kvp.Value)));
        }

    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PlanningDES.Problems;
using UltraDES;

namespace PlanningDES
{
    public static class Tools
    {
        public static HashSet<T> ToSet<T>(this IEnumerable<T> seq) => new HashSet<T>(seq);

        public static (float, AbstractEvent[]) TimeEvaluationControllable(this ISchedulingProblem problem, AbstractEvent[] sequence, AbstractState target = null)
        {
            target = target ?? problem.TargetState;
            var state = problem.InitialState;
            var transitions = problem.Transitions;
            var events = new List<AbstractEvent>();
            var sch = problem.InitialScheduler;
            var k = 0;

            float time = 0;

            while (true)
            {
                Transition trans = null;
                if (k < sequence.Length)
                {
                    if (transitions[state].ContainsKey(sequence[k]))
                    {
                        trans = (state, sequence[k], transitions[state][sequence[k]]);
                        k++;
                    }
                }
                if(trans == null)
                {
                    var enable = sch.Enabled;
                    enable.IntersectWith(transitions[state].Keys);

                    if (enable.All(e => e.IsControllable)) break;

                    trans = (state, enable.First(e => !e.IsControllable),
                        transitions[state][enable.First(e => !e.IsControllable)]);

                    if (trans == null) break;

                }

                var ev = trans.Trigger;
                state = trans.Destination;

                events.Add(ev);
                time += sch[ev];
                sch = sch.Update(ev);
                //Debug.WriteLine($"{state}; {ev}; {time}");
            }

            if(state != target) throw new Exception($"The target state ({target}) was not reached!");

            return (time, events.ToArray());
        }

        public static (float, AbstractEvent[]) TimeEvaluationControllableRandom(this ISchedulingProblem problem, AbstractEvent[] sequence, Random rnd, AbstractState target, double stdDeviation)
        {
            target = target ?? problem.TargetState;
            var state = problem.InitialState;
            var transitions = problem.Transitions;
            var events = new List<AbstractEvent>();
            var sch = problem.InitialScheduler;
            var k = 0;

            float time = 0;

            while (true)
            {
                Transition trans = null;
                if (k < sequence.Length)
                {
                    if (transitions[state].ContainsKey(sequence[k]))
                    {
                        trans = (state, sequence[k], transitions[state][sequence[k]]);
                        k++;
                    }
                }
                if (trans == null)
                {
                    var enable = sch.Enabled;
                    enable.IntersectWith(transitions[state].Keys);

                    if (enable.All(e => e.IsControllable)) break;

                    trans = (state, enable.First(e => !e.IsControllable),
                        transitions[state][enable.First(e => !e.IsControllable)]);

                    if (trans == null) break;

                }

                var ev = trans.Trigger;
                state = trans.Destination;

                events.Add(ev);
                time += sch[ev];
                sch = sch.Update(ev, (float) NormalSample(rnd, 0, stdDeviation));

            }

            if (state != target) throw new Exception($"The target state ({target}) was not reached!");

            return (time, events.ToArray());
        }

        private static double NormalSample(Random rand, double mean = 0, double stdDev = 1)
        {
            var u1 = 1.0 - rand.NextDouble();
            var u2 = 1.0 - rand.NextDouble();
            var stdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return mean + stdDev * stdNormal;
        }

        public static float TimeEvaluation(this ISchedulingProblem problem, AbstractEvent[] sequence, AbstractState target = null)
        {
            target = target ?? problem.TargetState;
            var state = problem.InitialState;
            var transitions = problem.Transitions;
            var events = new List<AbstractEvent>();
            var sch = problem.InitialScheduler;

            float time = 0;

            foreach (var ev in sequence)
            {
                state = transitions[state][ev];

                events.Add(ev);
                time += sch[ev];
                sch = sch.Update(ev);
                
                //Debug.WriteLine($"{state}; {ev}; {time}");
            }

            if (state != target) throw new Exception($"The target state ({target}) was not reached!");

            return time;
        }

        public static float MetricEvaluation(this ISchedulingProblem problem, AbstractEvent[] sequence, Func<(AbstractState origin, AbstractEvent trigger, AbstractState destination), float> value, AbstractState target = null)
        {
            target = target ?? problem.TargetState;
            var state = problem.InitialState;
            var transitions = problem.Transitions;
            var events = new List<AbstractEvent>();
            var sch = problem.InitialScheduler;

            float metric = 0;

            foreach (var ev in sequence)
            {
                metric += value((state, ev, transitions[state][ev]));
                state = transitions[state][ev];

                events.Add(ev);
                sch = sch.Update(ev);

                //Debug.WriteLine($"{state}; {ev}; {metric}");
            }

            if (state != target) throw new Exception($"The target state ({target}) was not reached!");

            return metric;
        }

        public static uint ActiveTasks(this AbstractState state)
        {
            if (state is ExpandedState expandedState) return expandedState.Tasks;
            if (state is AbstractCompoundState compoundState) return (uint)compoundState.S.Sum(s => s.ActiveTasks());
            return 0u;
        }

        public static (double time, T result) Timming<T>(this Func<T> f)
        {
            var timer = new Stopwatch();
            timer.Start();
            var result = f();
            timer.Stop();

            return (timer.ElapsedMilliseconds / 1000.0, result);
        }

        public static double Timming<T>(this Action f)
        {
            var timer = new Stopwatch();
            timer.Start();
            f();
            timer.Stop();

            return timer.ElapsedMilliseconds / 1000.0;
        }
    }


}

using System;
using UnityEngine;

namespace Topacai.StatsSystem
{
    /// <summary>
    /// Generic class for handling stats, use a class or struct for the baseStats
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StatsHandler<T>
    {
        private T baseStats;
        private StatsBroker statsMiddleware;

        public T BaseStats => baseStats;
        public StatsBroker StatsMiddleware => statsMiddleware;

        private object GetBaseStat(string statName)
        {
            return typeof(T).GetField(statName).GetValue(baseStats);
        }

        public object GetStat(Type type, string statName)
        {
            var q = new Query(GetBaseStat(statName), type, statName);
            statsMiddleware.PerformQuery(this, q);
            return q.Value;
        }

        public ExpectedType GetStat<ExpectedType>(string statName)
        {
            var q = new Query(GetBaseStat(statName), typeof(ExpectedType), statName);

            statsMiddleware.PerformQuery(this, q);

            try
            {
                var result = (ExpectedType)q.Value;
                return result;
            }
            catch (InvalidCastException)
            {
                throw new Exception($"Stat {statName} is not of type {typeof(ExpectedType)}");
            }
        }

        public StatsHandler(T baseStats = default(T))
        {
            statsMiddleware = new StatsBroker();
            this.baseStats = baseStats;
        }

        public T GetCloneWithModifiers()
        {
            object copy = Activator.CreateInstance<T>();
            foreach (var field in typeof(T).GetFields())
            {
                var val = GetStat(field.FieldType, field.Name);
                field.SetValue(copy, val);
            }

            return (T)copy;
        }

        public void PasteStats(out T target) => target = GetCloneWithModifiers();
        public void PasteStatsAsReference(ref T target) => target = GetCloneWithModifiers();
    }
}

using System;
using System.Collections.Generic;
using Topacai.TDebug;
using UnityEngine;

namespace Topacai.StatsSystem
{
    public class Query
    {
        public Type ValueType { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }

        public Query(object value, Type type, string name)
        {
            ValueType = type;
            Value = value;
            Name = name;
        }
    }

    public class BasicStatModifier<T> : StatModifier
    {
        protected Type statType;
        protected string statName;
        protected Func<T, T> operation;

        public BasicStatModifier(float duration, Func<T, T> operation, string statName) : base(duration)
        {
            this.statType = typeof(T);
            this.operation = operation;
            this.statName = statName;
        }

        public override void HandleQuery(object sender, Query query)
        {
            if (query.ValueType != statType || query.Name != statName) return;

            var newValue = operation((T)query.Value);
#if UNITY_EDITOR
            if(debugLogs)
                Debug.Log($"Query handled, ({statName}) prev value: {query.Value}, new value: {newValue}");
#endif
            query.Value = newValue;
        }

        public override string ToString()
        {
            return $"{statName} with type {statType})";
        }
    }

    public abstract class StatModifier : IDisposable
    {
        public event EventHandler<StatModifier> OnRemoved;
        public bool Expired => TimeLeft <= 0 && Duration > 0;

        public float Duration { get; set; }
        public float TimeLeft { get; set; }

#if UNITY_EDITOR
        public bool debugLogs = false;
#endif

        public StatModifier(float duration = 0)
        {
            Duration = duration;
            TimeLeft = duration;
        }

        public void Update(float deltaTime)
        {
#if UNITY_EDITOR
            if (debugLogs)
                Debugcanvas.Instance.AddTextToDebugLog(this.ToString(), $"Time left: {TimeLeft} / {Duration}");
#endif
            if (Duration == 0) return;

            TimeLeft -= deltaTime;
        }

        public void Remove(object sender)
        {
            OnRemoved?.Invoke(sender, this);
            Dispose();
        }

        public abstract void HandleQuery(object sender, Query query);

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Generic class for handling stats, use a class or struct for the baseStats
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StatsHandler<T>
    {
        private T baseStats;
        private StatsBroker statsMiddleware;

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
    }

    public class StatsBroker
    {
        private LinkedList<StatModifier> modifiers = new();
        private bool debugLogs = false;

        public event EventHandler<Query> Queries;
        public void PerformQuery(object sender, Query query) => Queries?.Invoke(sender, query);

        public void AddModifier(StatModifier modifier)
        {
            modifiers.AddLast(modifier);

            Queries += modifier.HandleQuery;

            modifier.OnRemoved += (sender, modifier) =>
            {
                modifiers.Remove(modifier);
                Queries -= modifier.HandleQuery;
#if UNITY_EDITOR
                if(debugLogs)
                    Debug.Log($"[StatsBroker] modifier {modifier.ToString()} removed");
#endif
            };
        }

        public void RemoveModifier(StatModifier modifier) => modifiers.Remove(modifier);

        public void Update(float deltaTime)
        {
            var modifierNode = modifiers.First;
            while (modifierNode != null)
            {
#if UNITY_EDITOR
                modifierNode.Value.debugLogs = debugLogs;
#endif
                modifierNode.Value.Update(deltaTime);
                modifierNode = modifierNode.Next;
            }

            foreach (var modifier in modifiers)
            {
                if (modifier.Expired)
                {
                    modifier.Remove(this);
#if UNITY_EDITOR
                    if (debugLogs)
                        Debug.Log($"[StatsBroker] modifier {modifier.ToString()} removed");
#endif
                }
            }
        }

        public void SetDebug(bool value) => debugLogs = value;
    }
}

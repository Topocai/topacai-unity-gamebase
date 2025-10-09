using System;
using System.Collections.Generic;
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
        private Type statType;
        private Func<T, T> operation;

        public BasicStatModifier(float duration, Func<T, T> operation) : base(duration)
        {
            this.statType = typeof(T);
            this.operation = operation;
        }

        public override void HandleQuery(object sender, Query query)
        {
            if (query.ValueType != statType) return;

            query.Value = operation((T)query.Value);
        }
    }

    public abstract class StatModifier
    {
        public event EventHandler<StatModifier> OnRemoved;
        public bool Expired => TimeLeft <= 0 && Duration > 0;

        public float Duration { get; set; }
        public float TimeLeft { get; set; }

        public StatModifier(float duration = 0)
        {
            Duration = duration;
            TimeLeft = duration;
        }

        public void Update(float deltaTime)
        {
            if (Duration == 0) return;

            TimeLeft -= deltaTime;
        }

        public void Remove(object sender) => OnRemoved?.Invoke(sender, this);

        public abstract void HandleQuery(object sender, Query query);
    }

    /// <summary>
    /// Generic class for handling stats, use a class or struct for the baseStats
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StatsHandler<T>
    {
        private T baseStats;
        private StatsBroker statsMiddleware;

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
            };
        }

        public void RemoveModifier(StatModifier modifier) => modifiers.Remove(modifier);

        public void Update(float deltaTime)
        {
            var modifierNode = modifiers.First;
            while (modifierNode != null)
            {
                modifierNode.Value.Update(deltaTime);
                modifierNode = modifierNode.Next;
            }

            foreach (var modifier in modifiers)
            {
                if (modifier.Expired)
                {
                    modifier.Remove(this);
                }
            }
        }
    }
}

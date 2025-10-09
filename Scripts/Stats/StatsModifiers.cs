using System;
using Topacai.TDebug;
using UnityEngine;

namespace Topacai.StatsSystem
{
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
            query.Value = operation((T)query.Value);
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
                Debugcanvas.Instance.AddTextToDebugLog(this.ToString(), $"Time left: {TimeLeft} / {Duration}", 0.5f);
#endif
            if (Duration == 0) return;

            TimeLeft -= deltaTime;
        }

        public void Remove(object sender) => OnRemoved?.Invoke(sender, this);

        public abstract void HandleQuery(object sender, Query query);

        public void Dispose() => GC.SuppressFinalize(this);
    }
}

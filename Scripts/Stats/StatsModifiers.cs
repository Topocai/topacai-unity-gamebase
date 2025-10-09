using System;
using Topacai.TDebug;
using UnityEngine;

namespace Topacai.StatsSystem
{
    public enum ModifierOperation
    {
        Add,
        Multiply,
        Subtract
    }

    public abstract class ConfigurableModifier<T> : BaseStatModifier<T>
    {
        protected ModifierOperation operationType;

        protected abstract Func<T, T> GetOperation();

        public ConfigurableModifier(float duration, string statName, ModifierOperation operationType) : base(duration, null, statName)
        {
            this.operationType = operationType;
            SetOperation(GetOperation());
        }
    }

    public class IntModifier : ConfigurableModifier<int>
    {
        protected int Amount { get; set; }

        public IntModifier(float duration, string statName, ModifierOperation operationType, int modifierAmount) : base(duration, statName, operationType)
        {
            Amount = modifierAmount;
        }

        protected override Func<int, int> GetOperation()
        {
            switch (operationType)
            {
                case ModifierOperation.Add:
                    return (int value) => (int)(value + Amount);
                case ModifierOperation.Multiply:
                    return (int value) => (int)(value * Amount);
                case ModifierOperation.Subtract:
                    return (int value) => (int)(value - Amount);
                default:
                    return (int value) => (int)(value + Amount);
            }
        }

        public override string ToString()
        {
            var op = operationType == ModifierOperation.Add ? "+" : operationType == ModifierOperation.Multiply ? "*" : "-";
            return $"Modifier of {statName} ( {operationType}{Amount} ))";
        }
    }

    public class FloatModifier : ConfigurableModifier<float>
    {
        protected float Amount { get; set; }

        public FloatModifier(float duration, string statName, ModifierOperation operationType, float modifierAmount) : base(duration, statName, operationType)
        {
            Amount = modifierAmount;
        }

        protected override Func<float, float> GetOperation()
        {
            switch (operationType)
            {
                case ModifierOperation.Add:
                    return (float value) => (float)(value + Amount);
                case ModifierOperation.Multiply:
                    return (float value) => (float)(value * Amount);
                case ModifierOperation.Subtract:
                    return (float value) => (float)(value - Amount);
                default:
                    return (float value) => (float)(value + Amount);
            }
        }

        public override string ToString()
        {
            var op = operationType == ModifierOperation.Add ? "+" : operationType == ModifierOperation.Multiply ? "*" : "-";
            return $"Modifier of {statName} ( {operationType}{Amount} ))";
        }
    }

    public class BoolModifier : ConfigurableModifier<bool>
    {
        protected bool ModifierValue { get; set; }

        public BoolModifier(float duration, string statName, ModifierOperation operationType, bool modifierAmount) : base(duration, statName, operationType)
        {
            ModifierValue = modifierAmount;
        }

        protected override Func<bool, bool> GetOperation()
        {
            switch (operationType)
            {
                case ModifierOperation.Add:
                    return (bool value) => (bool)(value || ModifierValue);
                case ModifierOperation.Multiply:
                    return (bool value) => (bool)(value && ModifierValue);
                case ModifierOperation.Subtract:
                    return (bool value) => (bool)(value && !ModifierValue);
                default:
                    return (bool value) => (bool)(value && ModifierValue);
            }
        }

        public override string ToString()
        {
            var op = operationType == ModifierOperation.Add ? "|" : operationType == ModifierOperation.Multiply ? "&" : "!";
            return $"Modifier of {statName} ( {operationType}{ModifierValue} ))";
        }
    }

    public class BaseStatModifier<T> : StatModifier
    {
        protected Type statType;
        protected string statName;
        protected Func<T, T> operation;

        public BaseStatModifier(float duration, Func<T, T> operation, string statName) : base(duration)
        {
            this.statType = typeof(T);
            this.operation = operation;
            this.statName = statName;
        }

        protected void SetOperation(Func<T, T> operation) => this.operation = operation;

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

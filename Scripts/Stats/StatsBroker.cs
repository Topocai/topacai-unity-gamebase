using System;
using System.Collections.Generic;
using UnityEngine;

/// 
/// Generic class for handling stats inspired by https://github.com/adammyhre/Unity-Stats-and-Modifiers/commit/2815f8767
///
/// Prototype version by Topacai

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

    public class StatsBroker
    {
        public event EventHandler OnStatsChanged;

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
                if (debugLogs)
                    Debug.Log($"[StatsBroker] modifier {modifier.ToString()} removed");
#endif
                modifier.Dispose();
                OnStatsChanged?.Invoke(this, new EventArgs());
            };

            OnStatsChanged?.Invoke(this, new EventArgs());
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

            modifierNode = modifiers.First;

            while (modifierNode != null)
            {
                if (modifierNode.Value.Expired)
                {
                    modifierNode.Value.Remove(this);
#if UNITY_EDITOR
                    if (debugLogs)
                        Debug.Log($"[StatsBroker] modifier {modifierNode.Value.ToString()} removed");
#endif
                }
                modifierNode = modifierNode.Next;
            }
        }

        public void SetDebug(bool value) => debugLogs = value;
    }
}

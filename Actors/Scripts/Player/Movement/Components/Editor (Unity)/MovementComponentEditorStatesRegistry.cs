#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Topacai.Player.Movement.Components.Editor
{
    [CreateAssetMenu(fileName = "MovementStateRegistry", menuName = "ScriptableObjects/Movement/StatesRegistry")]
    /// <summary>
    /// A scriptable object to manage the states of the movement component and made them persistent across recompile and sessions
    /// This is used only during development/unity editor
    /// </summary>
    public class MovementStateRegistry : ScriptableObject
    {
        [SerializeField] public List<string> registeredStates = new() { "dash", "grip" };

        public void AddState(string nameState)
        {
            registeredStates.Add(nameState);
            EditorUtility.SetDirty(this);
        }

        public void RemoveState(string nameState)
        {
            registeredStates.Remove(nameState);
            EditorUtility.SetDirty(this);
        }
    }
}

#endif

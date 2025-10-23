#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

using System.Collections.Generic;
using System.Linq;

namespace Topacai.Player.Movement.Components.Editor
{
    [CustomEditor(typeof(MovementComponent), true)]
    public class MovementComponentEditor : UnityEditor.Editor
    {
        [SerializeField] private static List<string> _registerStates = new() { "dash", "grip" };

        private string newStateR;

        private Dictionary<string, bool> _currentStateFlags = new();

        private void DrawStates(IEnumerable<string> states)
        {
            foreach(var state in states)
            {
                GUILayout.Label(state);
                if (GUILayout.Button("Remove"))
                {
                    _registerStates.Remove(state);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Space(7);
            /// [ component state flags section ]

            GUILayout.Label("States Manager", EditorStyles.boldLabel);

            DrawStates(_registerStates);

            newStateR = GUILayout.TextField(newStateR);

            if (GUILayout.Button("Register"))
            {
                _registerStates.Add(newStateR);
            }

            GUILayout.Space(10);

            // end - add a toggle view for this section

            /// [ imcompatible states setter section ]

            GUILayout.Label("Component incompatible states", EditorStyles.boldLabel);

            var mComponent = (MovementComponent)target;

            var incompatibleStates = mComponent.GetIncompatibleStates();

            foreach (var name in _registerStates)
            {
                _currentStateFlags[name] = incompatibleStates.Contains(name);

                _currentStateFlags[name] = GUILayout.Toggle(_currentStateFlags[name], name);

            }

            var currentStateFlags = _currentStateFlags.Where(x => x.Value).Select(x => x.Key).ToArray();

            mComponent.SetIncompatibleStates(currentStateFlags);

            GUILayout.Space(20);

            base.OnInspectorGUI();
        }
    }
}

#endif
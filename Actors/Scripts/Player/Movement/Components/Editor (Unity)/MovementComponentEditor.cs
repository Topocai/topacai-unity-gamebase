#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

using System.Collections.Generic;
using System.Linq;

using Topacai.Utils.Files;

namespace Topacai.Player.Movement.Components.Editor
{
    [CustomEditor(typeof(MovementComponent), true)]
    /// <summary>
    /// Custom inspector for the <see cref="MovementComponent"/> class.
    /// That displays a section to manage component states, registering global strings for states
    /// and setting incompatible states for the specific component, ensuring the states are unique and consistent across components.
    /// </summary>
    public class MovementComponentEditor : UnityEditor.Editor
    {
        #region Static Registry

        private const string REGISTRY_RESOURCE_PATH = "MovementComponents";
        private const string REGISTRY_RESOURCE_FILE_NAME = "StatesRegistry";

        private static MovementStateRegistry _statesRegistry;

        /// <summary>
        /// The registry for movement component states, if it doesn't exists
        /// it will be created as an asset in resources folder (don't create on another path)
        /// </summary>
        private static MovementStateRegistry StatesRegistry
        {
            get
            {
                if (_statesRegistry == null)
                {
                    var data = Resources.Load<MovementStateRegistry>($"{REGISTRY_RESOURCE_PATH}/{REGISTRY_RESOURCE_FILE_NAME}");

                    if (data == null)
                    {
                        string path = $"Assets/Resources/{REGISTRY_RESOURCE_PATH}";
                        FileManager.CreateDirectory(path);

                        data = ScriptableObject.CreateInstance<MovementStateRegistry>();

                        AssetDatabase.CreateAsset(data, $"{path}/{REGISTRY_RESOURCE_FILE_NAME}.asset");
                        AssetDatabase.SaveAssets();
                    }

                    _statesRegistry = data;
                }

                return _statesRegistry;
            }
        }

        private static IEnumerable<string> _RegisterStates => StatesRegistry.registeredStates;

        #endregion

        private string newStateR;

        private Dictionary<string, bool> _currentStateFlags = new();

        private void DrawStates(IEnumerable<string> states)
        {
            foreach(var state in states)
            {
                GUILayout.Label(state);
                if (GUILayout.Button("Remove"))
                {
                    StatesRegistry.RemoveState(state);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Space(7);

            /// [ component state flags section ]

            GUILayout.Label("States Manager", EditorStyles.boldLabel);

            DrawStates(_RegisterStates);

            newStateR = GUILayout.TextField(newStateR);

            if (GUILayout.Button("Register"))
            {
                StatesRegistry.AddState(newStateR);
            }

            GUILayout.Space(10);

            // end - add a toggle view for this section

            /// [ imcompatible states setter section ]

            GUILayout.Label("Component incompatible states", EditorStyles.boldLabel);

            // Provides states flags
            var mComponent = (MovementComponent)target;
            var incompatibleStates = mComponent.GetIncompatibleStates();

            foreach (var name in _RegisterStates)
            {
                _currentStateFlags[name] = incompatibleStates.Contains(name);
                _currentStateFlags[name] = GUILayout.Toggle(_currentStateFlags[name], name);
            }

            // Updates incompatible states list in component
            var currentStateFlags = _currentStateFlags.Where(x => x.Value).Select(x => x.Key).ToArray();
            mComponent.SetIncompatibleStates(currentStateFlags);

            GUILayout.Space(20);

            base.OnInspectorGUI();
        }
    }
}

#endif
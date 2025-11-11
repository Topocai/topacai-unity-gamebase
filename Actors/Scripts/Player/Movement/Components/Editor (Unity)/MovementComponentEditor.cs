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
        /// <summary>
        /// This code was modified by IA, it follows the main logic code from the previous commit, and the
        /// new code is just to decorate the inspector, but it is the same logic
        /// </summary>

        #region Static Registry

        private const string REGISTRY_RESOURCE_PATH = "MovementComponents";
        private const string REGISTRY_RESOURCE_FILE_NAME = "StatesRegistry";

        private static MovementStateRegistry _statesRegistry;

        /// <summary>
        /// Lazy-loads or creates the global movement state registry asset in Resources.
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
                        AssetDatabase.Refresh();
                    }

                    _statesRegistry = data;
                }

                return _statesRegistry;
            }
        }

        private static IEnumerable<string> _RegisteredStates => StatesRegistry.registeredStates;

        #endregion

        #region Foldout State

        private bool _showRegistrySection = true;
        private bool _showIncompatibleSection = true;

        #endregion

        private string _newState = string.Empty;
        private readonly Dictionary<string, bool> _currentStateFlags = new();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.Space(5);
            EditorGUILayout.LabelField("Movement Component Inspector", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use this panel to manage global movement states and define incompatible states per component.", MessageType.Info);

            DrawRegistrySection();
            EditorGUILayout.Space(12);
            DrawIncompatibleStatesSection();

            GUILayout.Space(15);
            EditorGUILayout.LabelField("Component Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Below are the default fields from the MovementComponent.", MessageType.None);
            GUILayout.Space(5);

            base.OnInspectorGUI();
        }

        // ------------------------------
        // [ GLOBAL REGISTRY MANAGEMENT ]
        // ------------------------------
        private void DrawRegistrySection()
        {
            EditorGUILayout.BeginVertical("box");
            _showRegistrySection = EditorGUILayout.Foldout(_showRegistrySection, "Global States Registry", true, EditorStyles.foldoutHeader);

            if (_showRegistrySection)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.HelpBox("These states are shared globally across all MovementComponents. " +
                                        "Adding or removing them will affect every component.", MessageType.Warning);

                GUILayout.Space(5);
                EditorGUILayout.LabelField("Registered States:", EditorStyles.boldLabel);

                if (_RegisteredStates.Any())
                {
                    foreach (var state in _RegisteredStates.ToArray())
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(state, GUILayout.MinWidth(100));

                        GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
                        if (GUILayout.Button("Remove", GUILayout.Width(70)))
                        {
                            StatesRegistry.RemoveState(state);
                            EditorUtility.SetDirty(StatesRegistry);
                            AssetDatabase.SaveAssets();
                        }
                        GUI.backgroundColor = Color.white;
                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No states registered yet.", MessageType.Info);
                }

                GUILayout.Space(10);
                EditorGUILayout.LabelField("Add New State", EditorStyles.boldLabel);

                _newState = EditorGUILayout.TextField("State Name", _newState);

                GUI.backgroundColor = new Color(0.4f, 0.9f, 0.5f);
                if (GUILayout.Button("Register New State"))
                {
                    if (!string.IsNullOrEmpty(_newState))
                    {
                        StatesRegistry.AddState(_newState);
                        EditorUtility.SetDirty(StatesRegistry);
                        AssetDatabase.SaveAssets();
                        _newState = string.Empty;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid State Name", "Please enter a valid state name.", "OK");
                    }
                }
                GUI.backgroundColor = Color.white;

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        // -------------------------------------
        // [ INCOMPATIBLE STATES FOR COMPONENT ]
        // -------------------------------------
        private void DrawIncompatibleStatesSection()
        {
            EditorGUILayout.BeginVertical("box");
            _showIncompatibleSection = EditorGUILayout.Foldout(_showIncompatibleSection, "Incompatible States", true, EditorStyles.foldoutHeader);

            if (_showIncompatibleSection)
            {
                EditorGUI.indentLevel++;

                var mComponent = (MovementComponent)target;
                var incompatibleStates = mComponent.GetIncompatibleStates();

                EditorGUILayout.HelpBox("Mark which global states are incompatible with this component. " +
                                        "Multiple components may share or differ in their incompatibility rules.", MessageType.None);

                GUILayout.Space(5);

                foreach (var name in _RegisteredStates)
                {
                    bool currentValue = incompatibleStates.Contains(name);
                    bool newValue = EditorGUILayout.ToggleLeft(name, currentValue);
                    _currentStateFlags[name] = newValue;
                }

                // Apply changes if any toggles changed serializing it
                var selectedStates = _currentStateFlags.Where(x => x.Value).Select(x => x.Key).ToArray();
                SerializedProperty incompatibleStatesProp = serializedObject.FindProperty("_incompatibleStates");

                incompatibleStatesProp.ClearArray();
                for (int i = 0; i < selectedStates.Length; i++)
                {
                    incompatibleStatesProp.InsertArrayElementAtIndex(i);
                    incompatibleStatesProp.GetArrayElementAtIndex(i).stringValue = selectedStates[i];
                }

                serializedObject.ApplyModifiedProperties();

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }
    }
}

#endif

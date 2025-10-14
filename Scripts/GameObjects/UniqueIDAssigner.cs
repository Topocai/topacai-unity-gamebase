using EditorAttributes;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Topacai.Utils.GameObjects.Unique
{
    /// <summary>
    /// Provides a unique and persistent identifier to each GameObject in the scene.
    /// This identifier is assigned automatically during editing and stored as a serialized string,
    /// ensuring that objects can be reliably referenced across play sessions, saves, and loads.
    /// 
    /// This component runs in Edit Mode using [ExecuteAlways], allowing it to assign and validate IDs
    /// even outside Play Mode. It avoids ID duplication via a local HashSet in the Editor.
    /// 
    /// Used for save persistend data during sessions in each GameObject
    /// </summary>

#if UNITY_EDITOR
    [ExecuteAlways]
#endif
    public class UniqueIDAssigner : MonoBehaviour
    {
#if UNITY_EDITOR
        public static bool showLogs { get; protected set; } = false;

        public static void ToggleLogs() => showLogs = !showLogs;
#endif

        /// <summary>
        /// The unique identifier assigned to this object.
        /// serialized to ensure persistence.
        /// </summary>
        [SerializeField, HideInInspector] private string uniqueID;

#if UNITY_EDITOR
        /// <summary>
        /// Temporary storage of all assigned IDs during edit-time to prevent duplication.
        /// Only active in the Unity Editor.
        /// </summary>
        private static Dictionary<string, UniqueIDAssigner> usedIDs = new();
#endif
        /// <summary>
        /// Automatically called by Unity when the component's values are changed in the Inspector.
        /// Used here to ensure a valid and unique ID is always assigned during editing.
        /// </summary>
        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            CheckAndCreateID();
#endif
        }

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            CheckAndCreateID();
#endif
        }

        public void CheckAndCreateID()
        {
            if (string.IsNullOrEmpty(uniqueID))
            {
#if UNITY_EDITOR
                if (showLogs)
                    Debug.Log("Generating new ID: it was null or empty.", this);
#endif
                GenerateUniqueID();
            }
#if UNITY_EDITOR
            else if (usedIDs.TryGetValue(uniqueID, out var register))
            {
                if (register != this && register != null)
                {
                    if (showLogs)
                        Debug.Log("Duplicate ID found: " + uniqueID + ". Generating a new ID.", this);
                    GenerateUniqueID();
                }
            }
            else
            {
                usedIDs[uniqueID] = this;
            }
#endif
        }


        public void GenerateUniqueID()
        {
            uniqueID = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
            usedIDs[uniqueID] = this;
#endif
        }


        /// <summary>
        /// Ensures the ID is registered at runtime.
        /// Logs a warning if a duplicate is detected, which should not happen.
        /// </summary>
        protected virtual void Awake()
        {
            CheckAndCreateID();
        }

        public string GetUniqueID()
        {
            return uniqueID;
        }
    }

#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UniqueIDAssigner), true)]
    public class UniqueIDAssignerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(10);

            var idObject = (UniqueIDAssigner)target;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Unique ID", idObject.GetUniqueID());
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Generate ID"))
            {
                idObject.GenerateUniqueID();
            }

            if (GUILayout.Button("Check duplicate (and regenerate)"))
            {
                idObject.CheckAndCreateID();
            }

            if (GUILayout.Button(UniqueIDAssigner.showLogs ? "Hide logs" : "Show logs"))
            {
                UniqueIDAssigner.ToggleLogs();
            }

            GUILayout.Space(15);
        }
    }

#endif
}

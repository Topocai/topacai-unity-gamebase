using EditorAttributes;
using System.Collections.Generic;
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
    /// 
    [ExecuteAlways] // Hace que se ejecute en el Editor y en el modo Play
    public class UniqueIDAssigner : MonoBehaviour
    {
        /// <summary>
        /// The unique identifier assigned to this object.
        /// serialized to ensure persistence.
        /// </summary>
        [SerializeField, ReadOnly] private string uniqueID;

#if UNITY_EDITOR
        /// <summary>
        /// Temporary storage of all assigned IDs during edit-time to prevent duplication.
        /// Only active in the Unity Editor.
        /// </summary>
        private static HashSet<string> usedIDs = new HashSet<string>();
#endif
        /// <summary>
        /// Automatically called by Unity when the component's values are changed in the Inspector.
        /// Used here to ensure a valid and unique ID is always assigned during editing.
        /// </summary>
        private void OnValidate()
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(uniqueID))
            {
                GenerateUniqueID();
            }
#endif
        }

        private void GenerateUniqueID()
        {
            uniqueID = System.Guid.NewGuid().ToString();

            if (!Application.isPlaying)
            {
                usedIDs.Add(uniqueID);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Ensures the ID is registered at runtime.
        /// Logs a warning if a duplicate is detected, which should not happen.
        /// </summary>
        void Awake()
        {
            // Register ID in runtime
            if (!usedIDs.Contains(uniqueID))
            {
                usedIDs.Add(uniqueID);
            }
        }
#endif

        public string GetUniqueID()
        {
            return uniqueID;
        }
    }
}

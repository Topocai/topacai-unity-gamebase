using UnityEngine;
using UnityEditor;

namespace Topacai.Utils.SaveSystem
{
    /// <summary>
    /// A ScriptableObject class that would save the data between sessions
    /// use as base class to made a scriptable object persistent
    /// </summary>
    public class PersistentDataSO : ScriptableObject
    {
        private bool isSuscribed = false;
        protected virtual void OnValidate()
        {
            if (!isSuscribed)
            {
                SaveSystem.OnSaveGameEvent.AddListener(SaveData);
                isSuscribed = true;
            }
        }
        public virtual void SaveData()
        {
            EditorUtility.SetDirty(this);
        }
    }
}

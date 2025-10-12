using System;
using System.IO;
using UnityEngine;
using Topacai.Utils.SaveSystem;

namespace Topacai.Utils.PersistentData
{
    /// <summary>
    /// Base scriptableObject class that saves all the posible serializable fields
    /// into JSON inside the current profile folder. and also recovers it data
    /// </summary>
    public class PersistentProfileDataSO : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The name of the file where the data will be saved, it must be unique and ends with .json")]
        private string fileName = "persistent_data.json";

        protected string FileName => fileName;

        private string SavePath
        {
            get
            {
                // Usa tu sistema de perfiles existente
                var profile = SaveSystemClass.GetCurrentProfile();
                if (profile.ID == null)
                {
                    Debug.LogWarning("[PersistentProfileDataSO] No current profile found.");
                    return null;
                }

                string dir = SaveDataManager.GetProfilePath(profile);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                return Path.Combine(dir, fileName);
            }
        }

        /// <summary>
        /// Save serialized data into JSON
        /// </summary>
        public virtual void SaveData()
        {
            try
            {
                string path = SavePath;
                if (string.IsNullOrEmpty(path)) return;

                string json = JsonUtility.ToJson(this, true);
                File.WriteAllText(path, json);

            }
            catch (Exception ex)
            {
                Debug.LogError($"[PersistentProfileDataSO] Error saving data: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets and loads into the saved data into the object fields.
        /// </summary>
        public virtual void LoadData()
        {
            try
            {
                string path = SavePath;
                if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

                string json = File.ReadAllText(path);
                JsonUtility.FromJsonOverwrite(json, this);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PersistentProfileDataSO] Error loading data: {ex.Message}");
            }
        }

        public virtual void OnProfileLoaded()
        {
            LoadData();
        }

        public virtual void OnProfileSaved()
        {
            SaveData();
        }
    }
}

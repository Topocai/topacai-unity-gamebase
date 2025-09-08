using System.Collections.Generic;
using Topacai.Inputs;
using Topacai.Utils.GameObjects;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Topacai.Utils.SaveSystem
{
    public class SaveController : Singleton<SaveController>
    {
        public static UnityEvent OnSaveGameEvent = new UnityEvent();

        [SerializeField] private string _savePath = "/SaveData";
        [SerializeField] private string _profilesFileName = "profiles.json";
        [Space(5)]
        [SerializeField] private string _levelsPath = "/Levels";

        List<UserProfile> _profiles = new List<UserProfile>();
        UserProfile _currentProfile;

        protected override void Awake()
        {
            base.Awake();

            RecoverProfiles();
            SetDebugProfile();
        }

        private void SetDebugProfile()
        {
            _currentProfile = _profiles.Count > 0 ? _profiles[0] : SaveManager.CreateProfile("Debug Profile");
        }

        public UserProfile[] GetProfiles() => _profiles.ToArray();

        public void SetProfile(UserProfile profile) => _currentProfile = profile;
        public void SetProfile(int index) => _currentProfile = _profiles[index % _profiles.Count];

        public void RecoverProfiles()
        {
            SaveManager.SetPaths(_savePath, _profilesFileName);
            SaveManager.RecoverProfiles();

            _profiles = SaveManager.GetProfiles();
        }

        private void ProfileExists()
        {
            if(_currentProfile.Equals(null))
            {
                Debug.LogWarning("No profile selected");
                SetDebugProfile();
            }
        }

        /// <summary>
        /// Save the current profile data and emits `OnSaveGameEvent` in order to notify custom saves
        /// </summary>
        public void SaveGame()
        {
            ProfileExists();
            OnSaveGameEvent?.Invoke();
            SaveManager.SaveProfile(_currentProfile);
        }

        /// <summary>
        /// Save data in current profile with a custom data type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="profile"></param>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <param name="subFolder"></param>
        public void SaveDataToProfile<T>(T data, string fileName, string subFolder = "")
        {
            ProfileExists();
            SaveManager.SaveProfileData(_currentProfile, fileName, data, subFolder);
        }

        /// <summary>
        /// Save data in current profile and current level/scene with a custom data type, util for persistentObjects
        /// or states in a scene or a specific gameobject
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="fileName"></param>
        /// <param name="subFolder"></param>
        public void SaveLevelDataToProfile<T>(T data, string fileName, string subFolder = "") 
        {
            ProfileExists();
            string levelName = SceneManager.GetActiveScene().name;
            SaveManager.SaveProfileData(_currentProfile, fileName + ".json", data, $"{_levelsPath}/{levelName}/{subFolder}");
        }

        /// <summary>
        /// Get a specific data saved in current profile searching by an *unique* name and stores the recovered data in out parameter.
        /// </summary>
        /// <typeparam name="T">Expected data type to get</typeparam>
        /// <param name="fileName">Unique name of saved data</param>
        /// <param name="data">Variable to store the data</param>
        /// <param name="subFolder">(optional) subfolder path to search data</param>
        /// <returns>True flag if data was found</returns>
        public bool GetLevelData<T>(string fileName, out T data, string subFolder = "")
        {
            ProfileExists();
            string levelName = SceneManager.GetActiveScene().name;
            return SaveManager.GetProfileData<T>(_currentProfile, fileName + ".json", out data, $"{_levelsPath}/{levelName}/{subFolder}");
        }

        /// <summary>
        /// Get a specific data saved in current profile searching by an *unique* name and stores the recovered data in out parameter.
        /// </summary>
        /// <typeparam name="T">Expected data type to get</typeparam>
        /// <param name="fileName">Unique name of saved data</param>
        /// <param name="data">Variable to store the data</param>
        /// <param name="subFolder">(optional) subfolder path to search data</param>
        /// <returns>True flag if data was found</returns>
        public bool GetProfileData<T>(string fileName, out T data, string subFolder = "")
        {
            ProfileExists();
            return SaveManager.GetProfileData<T>(_currentProfile, fileName, out data, subFolder);
        }

        private void Update()
        {
            if (InputHandler.CrouchPressed)
            {
                SaveGame();
            }
        }
    }
}

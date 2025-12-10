using System;
using System.Collections.Generic;

using Topacai.Utils.PersistentData;
using Topacai.Utils.Files;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR

using UnityEditor;
using System.IO;

#endif

namespace Topacai.Utils.SaveSystem
{
    [ExecuteAlways]
    public static class SaveSystemClass
    {
        public static event EventHandler OnSaveGameEvent;

        public static UnityEvent<UserProfile> OnProfileChanged = new ();
        public static UnityEvent<List<UserProfile>> OnProfilesFetched = new ();

        public static string SavePath { get; private set; } = "/SaveData";
        public static string ProfilesFileName { get; private set; } = "profiles.json";
        public static string LevelsPath { get; private set; } = "/Levels";
        public static void SetPaths(string savePath, string profileFileName, string levelsPath) { SavePath = savePath; ProfilesFileName = profileFileName; LevelsPath = levelsPath; }

        public static string SaveFullPath => $"{Application.dataPath}/{SavePath}";

        private static List<UserProfile> _profiles => SaveDataManager.GetProfiles();
        
        private static UserProfile _currentProfile;
        public static UserProfile GetCurrentProfile() => _currentProfile;
        public static ref UserProfile GetCurrentProfileRef() => ref _currentProfile;

        public static void CallSaveGameEvent(object sender = null, System.EventArgs e = null)
        {
            SaveGame();
            OnSaveGameEvent?.Invoke(sender, new());
        }

        public static void CallProfileChanged(UserProfile profile)
        {
            OnProfileChanged?.Invoke(profile);

            foreach (var data in Resources.FindObjectsOfTypeAll<PersistentProfileDataSO>())
                data.OnProfileLoaded();
        }

        #region Profile management

        private static UserProfile GetDebugProfile()
        {
            var debugProfile = _profiles.Find(x => x.Name == "Debug Profile");

            if (debugProfile.ID == null) debugProfile = SaveDataManager.CreateProfile("Debug Profile");

            return debugProfile;
        }

        private static void SetDebugProfile()
        {
            SetProfile(GetDebugProfile());
        }

        public static void SetProfile(UserProfile profile)
        {
            _currentProfile = profile;
            CallProfileChanged(_currentProfile);
        }

        public static void SetProfile(int index) => SetProfile(_profiles[index % _profiles.Count]);

#if UNITY_EDITOR
        /// 
        /// This code block makes sure that the selected profile persists between playmode, editmode and after recompilation
        /// 

        static SaveSystemClass()
        {
            EditorApplication.playModeStateChanged += OnStateModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += SaveLastProfile;
            AssemblyReloadEvents.afterAssemblyReload += LoadLastProfile;
        }

        [InitializeOnLoadMethod]
        private static void RegisterStateModeChanged() => EditorApplication.playModeStateChanged += OnStateModeChanged;

        private static void SaveLastProfile()
        {
            if (_currentProfile.ID != null)
                FileManager.WriteFile(SaveFullPath, "last_used_profile.sp", _currentProfile.ID);
        }

        private static void LoadLastProfile()
        {
            var lastProfile = File.Exists($"{SaveFullPath}/last_used_profile.sp") ? File.ReadAllText($"{SaveFullPath}/last_used_profile.sp") : "";
            if (lastProfile != "")
                SetProfile(_profiles.Find(x => x.ID == lastProfile));
        }


        private static void OnStateModeChanged(PlayModeStateChange playModeState)
        {
            switch(playModeState)
            {
                case PlayModeStateChange.EnteredEditMode:
                case PlayModeStateChange.EnteredPlayMode:
                    LoadLastProfile();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                case PlayModeStateChange.ExitingPlayMode:
                    SaveLastProfile();
                    break;
            }
        }
#endif

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        public static void RecoverProfiles()
        {
            SaveDataManager.SetPaths(SavePath, ProfilesFileName);
            SaveDataManager.RecoverProfiles();

            OnProfilesFetched?.Invoke(_profiles);
        }

        private static void ProfileExists()
        {
            SaveDataManager.SetPaths(SavePath, ProfilesFileName);

            if (_currentProfile.ID == null)
            {
                Debug.LogWarning("No profile selected, using debug profile");
                SetDebugProfile();
            }
        }

#endregion

        #region Save and Recovery Data methods

        /// <summary>
        /// Save the current profile data and emits `OnSaveGameEvent` in order to notify custom saves
        /// </summary>
        public static void SaveGame()
        {
            ProfileExists();

            SaveDataManager.SaveProfile(_currentProfile);

            foreach (var data in Resources.FindObjectsOfTypeAll<PersistentProfileDataSO>())
                data.OnProfileSaved();
        }

        /// <summary>
        /// Save data in current profile with a custom data type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="profile"></param>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <param name="subFolder"></param>
        public static void SaveDataToProfile<T>(T data, string fileName, string subFolder = "")
        {
            ProfileExists();
            SaveDataManager.SaveProfileData(_currentProfile, fileName, data, subFolder);
        }

        /// <summary>
        /// Save data in current profile and current level/scene with a custom data type, util for persistentObjects
        /// or states in a scene or a specific gameobject
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="fileName"></param>
        /// <param name="subFolder"></param>
        public static void SaveLevelDataToProfile<T>(T data, string fileName, string subFolder = "") 
        {
            ProfileExists();
            string levelName = SceneManager.GetActiveScene().name;
            SaveDataManager.SaveProfileData(_currentProfile, fileName + ".json", data, $"{LevelsPath}/{levelName}/{subFolder}");
        }

        /// <summary>
        /// Get a specific data saved in current profile searching by an *unique* name and stores the recovered data in out parameter.
        /// </summary>
        /// <typeparam name="T">Expected data type to get</typeparam>
        /// <param name="fileName">Unique name of saved data</param>
        /// <param name="data">Variable to store the data</param>
        /// <param name="subFolder">(optional) subfolder path to search data</param>
        /// <returns>True flag if data was found</returns>
        public static bool GetLevelData<T>(string fileName, out T data, string subFolder = "")
        {
            ProfileExists();
            string levelName = SceneManager.GetActiveScene().name;
            return SaveDataManager.GetProfileData<T>(_currentProfile, fileName + ".json", out data, $"{LevelsPath}/{levelName}/{subFolder}");
        }

        /// <summary>
        /// Get a specific data saved in current profile searching by an *unique* name and stores the recovered data in out parameter.
        /// </summary>
        /// <typeparam name="T">Expected data type to get</typeparam>
        /// <param name="fileName">Unique name of saved data</param>
        /// <param name="data">Variable to store the data</param>
        /// <param name="subFolder">(optional) subfolder path to search data</param>
        /// <returns>True flag if data was found</returns>
        public static bool GetProfileData<T>(string fileName, out T data, string subFolder = "")
        {
            ProfileExists();
            return SaveDataManager.GetProfileData<T>(_currentProfile, fileName, out data, subFolder);
        }

#endregion
    }
}

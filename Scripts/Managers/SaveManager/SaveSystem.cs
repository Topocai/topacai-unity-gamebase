using System.Collections.Generic;

using Topacai.Utils.GameObjects;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

namespace Topacai.Utils.SaveSystem
{/*
    public class SaveSystemWindow : EditorWindow
    {
        [MenuItem("TopacaiTools/Save System")]
        public static void ShowWindow()
        {
            GetWindow<SaveSystemWindow>("Save System");
        }

        private void OnGUI()
        {
            GUILayout.Label("Current Profile");
            
        }

        private UserProfile DrawProfile(UserProfile profile)
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("ID: ");
            GUILayout.Label(profile.ID);

            GUILayout.Label("Name: ");
            GUILayout.Label(profile.ID);
            EditorGUILayout.EndVertical();

            return profile;
        }
    }*/

    public class SaveSystemClass
    {
        public static UnityEvent OnSaveGameEvent = new UnityEvent();

        public static UnityEvent<UserProfile> OnProfileChanged = new ();
        public static UnityEvent<List<UserProfile>> OnProfilesFetched = new ();

        public static string SavePath { get; private set; } = "/SaveData";
        public static string ProfilesFileName { get; private set; } = "profiles.json";
        public static string LevelsPath { get; private set; } = "/Levels";
        public static void SetPaths(string savePath, string profileFileName, string levelsPath) { SavePath = savePath; ProfilesFileName = profileFileName; LevelsPath = levelsPath; }

        private static List<UserProfile> _profiles => SaveDataManager.GetProfiles();
        
        private static UserProfile _currentProfile;
        public static UserProfile GetCurrentProfile() => _currentProfile;

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
            OnProfileChanged?.Invoke(_currentProfile);
        }

        public static void SetProfile(int index) => SetProfile(_profiles[index % _profiles.Count]);

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
            OnSaveGameEvent?.Invoke();
            SaveDataManager.SaveProfile(_currentProfile);
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

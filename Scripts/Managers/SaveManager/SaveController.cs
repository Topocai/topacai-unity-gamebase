using System.Collections.Generic;
using Topacai.Inputs;
using Topacai.Utils.GameObjects.Unique;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Topacai.Utils.SaveSystem
{
    public class SaveController : MonoBehaviour
    {
        public static UnityEvent OnSaveGameEvent = new UnityEvent();
        public static SaveController Instance { get; private set; }

        [SerializeField] private string _savePath = "/SaveData";
        [SerializeField] private string _profilesFileName = "profiles.json";
        [Space(5)]
        [SerializeField] private string _levelsPath = "/Levels";

        UserProfile currentProfile;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
            GetProfile();
        }

        private void GetProfile()
        {
            SaveManager.SetPaths(_savePath, _profilesFileName);
            SaveManager.RecoverProfiles();

            var profiles = SaveManager.GetProfiles();

            currentProfile = profiles.Count > 0 ? profiles[0] : SaveManager.CreateProfile("Debug Profile");
        }

        private void ProfileExists()
        {
            if(currentProfile.Equals(null))
            {
                Debug.LogWarning("No profile selected");
                GetProfile();
            }
        }

        public void SaveGame()
        {
            ProfileExists();
            OnSaveGameEvent?.Invoke();
            SaveManager.SaveProfile(currentProfile);
        }

        public void SaveDataToProfile<T>(T data, string fileName, string subFolder = "")
        {
            ProfileExists();
            SaveManager.SaveProfileData(currentProfile, fileName, data, subFolder);
        }

        public void SaveLevelDataToProfile<T>(T data, string fileName, string subFolder = "") 
        {
            ProfileExists();
            string levelName = SceneManager.GetActiveScene().name;
            SaveManager.SaveProfileData(currentProfile, fileName + ".json", data, $"{_levelsPath}/{levelName}/{subFolder}");
        }

        public bool GetLevelData<T>(string fileName, out T data, string subFolder = "")
        {
            ProfileExists();
            string levelName = SceneManager.GetActiveScene().name;
            return SaveManager.GetProfileData<T>(currentProfile, fileName + ".json", out data, $"{_levelsPath}/{levelName}/{subFolder}");
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

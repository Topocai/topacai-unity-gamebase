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

        [SerializeField] private string _savePath = "SaveData";
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
        }

        void Start()
        {
            SaveManager.SetPaths(_savePath, _profilesFileName);
            SaveManager.RecoverProfiles();

            var profiles = SaveManager.GetProfiles();

            if (profiles.Count == 0)
            {
                currentProfile = SaveManager.CreateProfile("Debug Profile");
            }
            else currentProfile = profiles[0];
        }

        public void SaveGame()
        {
            OnSaveGameEvent.Invoke();
            SaveManager.SaveProfile(currentProfile);
        }

        public void SaveDataToProfile(object data, string fileName, string subFolder = "")
        {
            SaveManager.SaveProfileData(currentProfile, fileName, data, subFolder);
        }

        public void SaveLevelDataToProfile(object data, string fileName, string subFolder = "") 
        {
            string levelName = SceneManager.GetActiveScene().name;
            SaveManager.SaveProfileData(currentProfile, fileName, data, _levelsPath + "/" + levelName + subFolder);
        }

        public void GetLevelData<T>(string fileName, out T data, string subFolder = "")
        {
            string levelName = SceneManager.GetActiveScene().name;
            SaveManager.GetProfileData<T>(currentProfile, fileName, out data, _levelsPath + "/" + levelName + subFolder);
        }
    }
}

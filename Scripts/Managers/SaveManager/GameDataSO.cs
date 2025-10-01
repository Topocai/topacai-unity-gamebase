using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Topacai.Utils.SaveSystem
{
    [CreateAssetMenu(fileName = "GameDataSO", menuName = "ScriptableObjects/SaveSystem/GameDataSO")]
    public class GameDataSO : ScriptableObject
    {
        public UserProfile CurrentProfile;
        public List<UserProfile> Profiles;

        private void OnValidate()
        {
            SaveController.OnProfileChanged.RemoveListener(SetCurrentProfile);
            SaveController.OnProfilesFetched.RemoveListener(SetProfiles);

            SaveController.OnProfileChanged.AddListener(SetCurrentProfile);
            SaveController.OnProfilesFetched.AddListener(SetProfiles);
        }

        public void SetProfiles(List<UserProfile> profiles) => Profiles = profiles;

        public void SetCurrentProfile(UserProfile profile) => CurrentProfile = profile;
    }

    /// <summary>
    /// Register a converter for time in seconds to hours:minutes:seconds in data binding of unity ui toolkit
    /// </summary>
    public static class TimePlayedConverter
    {
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        static void Initialize() => Register();

        public static void Register()
        {
            RegisterTimeConverter();
        }

        static void RegisterTimeConverter()
        {
            var timeConverter = new ConverterGroup("TimePlayed");

            timeConverter.AddConverter((ref float timePlayed) =>
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds((double)timePlayed);
                return timeSpan.ToString(@"hh\:mm\:ss");
            });

            ConverterGroups.RegisterConverterGroup(timeConverter);
        }
    }
}

using System.Collections.Generic;

using Topacai.Utils.GameObjects;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

namespace Topacai.Utils.SaveSystem
{
    public class SaveSystemWindow : EditorWindow
    {
        #region Style

        #region Style fields
        private GUIStyle headerStyle;
        private GUIStyle boxStyle;
        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle smallLabelStyle;
        private GUIStyle sectionBackgroundStyle;
        #endregion

        #region Style configuration
        private void OnEnable()
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleCenter
            };

            boxStyle = new GUIStyle("box")
            {
                padding = new RectOffset(10, 10, 8, 8),
            };

            titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                normal = { textColor = new Color(0.8f, 0.9f, 1f) }
            };

            labelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11
            };

            smallLabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = Color.gray }
            };

            sectionBackgroundStyle = new GUIStyle("HelpBox")
            {
                padding = new RectOffset(10, 10, 8, 8),
                margin = new RectOffset(4, 4, 4, 4)
            };
        }
        #endregion

        #endregion

        #region Fields
        private string newProfileName = "";
        private bool reloadAssetsAtSelect = false;
        private bool reloadAssetsAtAdd = false;
        #endregion

        [MenuItem("TopacaiTools/Save System")]
        public static void ShowWindow()
        {
            GetWindow<SaveSystemWindow>("Save System");
        }

        private void OnGUI()
        {
            // Principal header decorator
            GUILayout.Space(10);
            GUILayout.Label("🗂  SAVE SYSTEM MANAGER", headerStyle);
            GUILayout.Space(8);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            /// Current profile section
            GUILayout.Label("📌 Current Profile", titleStyle);
            GUILayout.Space(5);

            var currentProfile = SaveSystemClass.GetCurrentProfile();
            if (currentProfile.ID != null)
                DrawProfile(currentProfile, selectable: false, highlight: true);
            else
                EditorGUILayout.HelpBox("No profile currently selected.", MessageType.Info);

            GUILayout.Space(15);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            /// Configuration flags
            GUILayout.Label("⚙️  Configuration", titleStyle);
            GUILayout.Space(4);

            EditorGUILayout.BeginVertical(sectionBackgroundStyle);
            reloadAssetsAtSelect = EditorGUILayout.ToggleLeft("Reload assets when selecting a profile", reloadAssetsAtSelect);
            reloadAssetsAtAdd = EditorGUILayout.ToggleLeft("Reload assets when adding a new profile", reloadAssetsAtAdd);

            GUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "If enabled, the Asset Database will refresh after the action, useful when profiles affect asset loading.",
                MessageType.None);
            EditorGUILayout.EndVertical();

            GUILayout.Space(15);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            /// All profiles list section
            GUILayout.Label("👥  All Profiles", titleStyle);
            GUILayout.Space(5);

            var profiles = SaveDataManager.GetProfiles();

            if (profiles == null || profiles.Count == 0)
            {
                EditorGUILayout.HelpBox("No profiles found in SaveDataManager.", MessageType.Warning);
            }
            else
            {
                foreach (var profile in profiles)
                {
                    bool selectable = profile.ID != currentProfile.ID;
                    DrawProfile(profile, selectable, !selectable);
                    GUILayout.Space(6);
                }
            }

            GUILayout.Space(15);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            /// Create new profile section
            GUILayout.Label("🆕  Create New Profile", titleStyle);
            GUILayout.Space(4);

            EditorGUILayout.BeginVertical(sectionBackgroundStyle);
            GUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            newProfileName = EditorGUILayout.TextField("Profile Name", newProfileName);
            if (GUILayout.Button("Create", GUILayout.Width(80)))
            {
                if (string.IsNullOrEmpty(newProfileName))
                {
                    EditorUtility.DisplayDialog("Invalid name", "Profile name cannot be empty.", "OK");
                }
                else
                {
                    SaveDataManager.CreateProfile(newProfileName);
                    if (reloadAssetsAtAdd)
                        AssetDatabase.Refresh();
                    newProfileName = "";
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            EditorGUILayout.HelpBox("Creates a new user profile. The name must be unique.", MessageType.Info);
            EditorGUILayout.EndVertical();

            /// Footer section
            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Topacai Tools © " + System.DateTime.Now.Year, smallLabelStyle);
        }

        /// <summary>
        /// Draws an UserProfile struct on the editor with a button to select the profile
        /// </summary>
        /// <param name="profile">The profile to show</param>
        /// <param name="selectable">Disables the select button</param>
        /// <param name="highlight">Highlights the profile</param>
        private void DrawProfile(UserProfile profile, bool selectable = false, bool highlight = false)
        {
            Color originalColor = GUI.backgroundColor;

            if (highlight)
                GUI.backgroundColor = new Color(0.3f, 0.5f, 0.9f, 0.3f);

            EditorGUILayout.BeginVertical(boxStyle);

            GUILayout.Label(profile.Name, EditorStyles.boldLabel);
            GUILayout.Label("ID: " + profile.ID, smallLabelStyle);
            GUILayout.Space(5);

            GUI.enabled = selectable;
            if (GUILayout.Button("Select", GUILayout.Height(22)))
            {
                SaveSystemClass.SetProfile(profile);
                if (reloadAssetsAtSelect)
                    AssetDatabase.Refresh();
            }
            GUI.enabled = true;

            EditorGUILayout.EndVertical();

            GUI.backgroundColor = originalColor;
        }
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
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

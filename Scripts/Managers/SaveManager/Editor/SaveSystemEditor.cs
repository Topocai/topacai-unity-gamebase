using UnityEditor;
using UnityEngine;

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

}

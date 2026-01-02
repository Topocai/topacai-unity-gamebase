// Original code from : https://discussions.unity.com/t/can-i-stop-auto-compile-after-edit-create-or-remove-a-script/878449/8

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Topacai.Utils.Editor
{
    /// <summary>
    /// Avoids unity auto-refreshing when creating new scripts
    /// you could also disable the auto refresh in TopacaiTools/Editor menu
    /// Also it adds a shortcut for refreshing the scripts correctly, make sure to remove the original one.
    /// </summary>
    public static class AutorefreshSettings
    {

        private const string KEY_NAME = "T-AutoRefresh";

        private static bool _isAutoRefreshEnabled
        {
            get
            {
                if (!EditorPrefs.HasKey(KEY_NAME))
                {
                    EditorPrefs.SetInt(KEY_NAME, 1);
                }

                return EditorPrefs.GetInt(KEY_NAME) == 1;
            }
            set
            {
                EditorPrefs.SetInt(KEY_NAME, value ? 1 : 0);
            }
        }

        private static void SetAutoRefresh(bool val)
        {
            _isAutoRefreshEnabled = val;
            EditorPrefs.SetInt("kAutoRefresh", _isAutoRefreshEnabled ? 1 : 0);

            if (!_isAutoRefreshEnabled)
                EditorApplication.LockReloadAssemblies();
            else
                EditorApplication.UnlockReloadAssemblies();
        }

        [InitializeOnLoadMethod]
        private static void InitAutoRefresh()
        {
            SetAutoRefresh(_isAutoRefreshEnabled);
        }

        [MenuItem("TopacaiTools/Editor/Auto Refresh")]
        private static void AutoRefreshToggle()
        {
            SetAutoRefresh(!_isAutoRefreshEnabled);
        }

        [MenuItem("TopacaiTools/Editor/Auto Refresh", true)]
        private static bool AutoRefreshToggleValidation()
        {
            var status = EditorPrefs.GetInt(KEY_NAME);

            Menu.SetChecked("TopacaiTools/Editor/Auto Refresh", status == 1);

            return true;
        }

        [MenuItem("TopacaiTools/Editor/Refresh %r")]
        private static void Refresh()
        {
            Debug.Log("Request script reload.");

            EditorApplication.UnlockReloadAssemblies();

            AssetDatabase.Refresh();

            EditorApplication.delayCall += () =>
            {
                Debug.Log("Reloading assemblies...");
                EditorUtility.RequestScriptReload();
            };
        }
    }
}

#endif
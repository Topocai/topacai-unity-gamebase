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
        [MenuItem("TopacaiTools/Editor/Auto Refresh")]
        private static void AutoRefreshToggle()
        {
            var status = EditorPrefs.GetInt("kAutoRefresh");

            EditorPrefs.SetInt("kAutoRefresh", status == 1 ? 0 : 1);
        }

        [MenuItem("TopacaiTools/Editor/Auto Refresh", true)]
        private static bool AutoRefreshToggleValidation()
        {
            var status = EditorPrefs.GetInt("kAutoRefresh");

            Menu.SetChecked("TopacaiTools/Editor/Auto Refresh", status == 1);

            return true;
        }

        [MenuItem("TopacaiTools/Editor/Refresh %r")]
        private static void Refresh()
        {
            Debug.Log("Request script reload.");

            EditorApplication.UnlockReloadAssemblies();
            AssetDatabase.Refresh();
            EditorUtility.RequestScriptReload();
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            AssetDatabase.SaveAssets();
            EditorApplication.LockReloadAssemblies();
        }
    }
}

#endif
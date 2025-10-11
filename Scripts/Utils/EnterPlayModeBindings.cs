// Original code from: https://forum.unity.com/threads/unwanted-editor-hotkeys-in-game-mode.182073/#post-6886781
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ShortcutManagement;


namespace Topacai.Utils.Editor
{
    // Sets a specific shortcut profile when entering play mode
    // To avoid save with "ctrl+s" and another issues during playmode
    //
    // Make sure to include and config the shortcut profile on Edit -> Shortcuts..
    [InitializeOnLoad]
    public static class EnterPlayModeBindings
    {
        static EnterPlayModeBindings()
        {
            EditorApplication.playModeStateChanged += ModeChanged;
            EditorApplication.quitting += Quitting;
        }

        static void ModeChanged(PlayModeStateChange playModeState)
        {
            if (playModeState == PlayModeStateChange.EnteredPlayMode)
                ShortcutManager.instance.activeProfileId = "Play";
            else if (playModeState == PlayModeStateChange.EnteredEditMode)
                ShortcutManager.instance.activeProfileId = "Default";
        }

        static void Quitting()
        {
            ShortcutManager.instance.activeProfileId = "Default";
        }
    }
}
#endif


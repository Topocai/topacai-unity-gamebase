#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;

namespace Topacai.Utils.Editor
{
    [InitializeOnLoad]
    public static class ModeStateWatcher
    {
        public static event EventHandler<PlayModeStateChange> OnPlayModeStateChangedEvent;

        static ModeStateWatcher()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            OnPlayModeStateChangedEvent?.Invoke(null, state);
        }
    }
}

#endif

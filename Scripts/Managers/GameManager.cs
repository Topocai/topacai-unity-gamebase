using EditorAttributes;

using System;
using System.Collections;
using System.Collections.Generic;

using Topacai.Utils.GameObjects;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Topacai.Managers.GameManager
{
    public partial class GameManager : Singleton<GameManager>
    {
        public partial class OnSceneArgs : System.EventArgs
        {
            public Scene ActualScene;
            public Scene NewScene;

            public LoadSceneMode NewSceneType;

            public Scene[] NonActiveScenes;
        }

        public static UnityEvent<object, OnSceneArgs> OnLoadingScene = new();
        public static UnityEvent<object, OnSceneArgs> OnSwitchingScene = new();

        public static UnityEvent<OnSceneArgs> OnSceneLoaded = new();
        public static UnityEvent<OnSceneArgs> OnSceneSwitched = new();

        public static UnityEvent<object, OnSceneArgs> OnSceneUnloaded = new();

#if UNITY_EDITOR
        public void DebugLog(string message) => Debug.Log(message);
        public void ShowLogOnScreen(object sender, string msg, float duration= 0.1f) => Topacai.TDebug.Debugcanvas.Instance.AddTextToDebugLog(sender!=null ? $"{sender.ToString()}" : "GM Log", msg, duration);
#endif

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private IEnumerator WaitingForSceneLoad(string sceneName, AsyncOperation op, Action<string> action)
        {
            yield return new WaitUntil(() => op.isDone);

            var scene = SceneManager.GetSceneByName(sceneName);

            if (scene.isLoaded) action.Invoke(sceneName);
        }

        private void OnAdditiveLoaded(string sceneName)
        {
            OnSceneLoaded?.Invoke(new OnSceneArgs() { NewScene = SceneManager.GetSceneByName(sceneName) });
        }

        private void OnSwitchLoaded(string sceneName)
        {
            OnSceneSwitched?.Invoke(new OnSceneArgs() { NewScene = SceneManager.GetSceneByName(sceneName) });
        }

        public void LoadScene(object sender, string sceneName, LoadSceneMode mode = LoadSceneMode.Additive)
        {
            var scene = SceneManager.GetSceneByName(sceneName);

            var args = new OnSceneArgs()
            {
                ActualScene = SceneManager.GetActiveScene(),
                NewScene = scene,
                NewSceneType = mode
            };

            if (mode == LoadSceneMode.Single)
            {
                OnSwitchingScene?.Invoke(sender, args);
            }
            else
            {
                OnLoadingScene?.Invoke(sender, args);
            }

            var asyncOp = SceneManager.LoadSceneAsync(sceneName, mode);
            StartCoroutine(WaitingForSceneLoad(sceneName, asyncOp, mode == LoadSceneMode.Additive ? OnAdditiveLoaded : OnSwitchLoaded));
        }
    }
}


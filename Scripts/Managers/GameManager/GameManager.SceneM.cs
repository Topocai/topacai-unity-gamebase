using System;
using System.Collections;

using Topacai.Utils.GameObjects;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Topacai.Managers
{
    public partial class GameManager : Singleton<GameManager>
    {
        public partial class OnSceneArgs : System.EventArgs
        {
            public Scene ActualScene;
            public Scene TargetScene;

            public LoadSceneMode NewSceneType;

            public string[] NonActiveScenes;
        }

        public static UnityEvent<object, OnSceneArgs> OnLoadingScene = new();
        public static UnityEvent<object, OnSceneArgs> OnSwitchingScene = new();

        public static UnityEvent<OnSceneArgs> OnSceneLoaded = new();
        public static UnityEvent<OnSceneArgs> OnSceneSwitched = new();

        public static UnityEvent<object, OnSceneArgs> OnUnloadingScene = new();
        public static UnityEvent<OnSceneArgs> OnSceneUnloaded = new();

        private IEnumerator WaitForSceneLoad(string sceneName, AsyncOperation op, Action<string, OnSceneArgs> action, OnSceneArgs args = null)
        {
            yield return new WaitUntil(() => op.isDone);

            var scene = SceneManager.GetSceneByName(sceneName);

            if (scene.isLoaded) action.Invoke(sceneName, args);
        }

        private IEnumerator WaitForSceneUnload(string sceneName, AsyncOperation op, Action<string, OnSceneArgs> action, OnSceneArgs args = null)
        {
            yield return new WaitUntil(() => op.isDone);

            var scene = SceneManager.GetSceneByName(sceneName);

            if (!scene.isLoaded) action.Invoke(sceneName, args);
        }

        private void OnAdditiveLoaded(string sceneName, OnSceneArgs args)
        {
            OnSceneLoaded?.Invoke(args ?? new OnSceneArgs() { TargetScene = SceneManager.GetSceneByName(sceneName) });
        }

        private void OnSceneFinishUnload(string sceneName, OnSceneArgs args)
        {
            OnSceneUnloaded?.Invoke(args ?? new OnSceneArgs() { TargetScene = SceneManager.GetSceneByName(sceneName) });
        }

        private void OnSwitchLoaded(string sceneName, OnSceneArgs args)
        {
            OnSceneSwitched?.Invoke(args ?? new OnSceneArgs() { TargetScene = SceneManager.GetSceneByName(sceneName) });
        }

        public void LoadSetOfScenes(object sender, string[] scenes)
        {
            var args = new OnSceneArgs()
            {
                ActualScene = SceneManager.GetActiveScene(),

                NonActiveScenes = scenes
            };

            foreach (var scene in scenes)
            {
                args.TargetScene = SceneManager.GetSceneByName(scene);

                LoadScene(sender, scene, LoadSceneMode.Additive, args);
            }
        }

        public void UnloadSetOfScenes(object sender, string[] scenes)
        {
            var args = new OnSceneArgs()
            {
                ActualScene = SceneManager.GetActiveScene(),

                NonActiveScenes = scenes
            };

            foreach (var scene in scenes)
            {
                args.TargetScene = SceneManager.GetSceneByName(scene);

                UnloadScene(sender, scene, args);
            }
        }

        public void UnloadScene(object sender, string sceneName, OnSceneArgs args = null)
        {
            var scene = SceneManager.GetSceneByName(sceneName);

            if (!scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            var a = args ?? new OnSceneArgs() { ActualScene = SceneManager.GetActiveScene(), TargetScene = scene };
            OnUnloadingScene?.Invoke(sender, a);

            var asyncOp = SceneManager.UnloadSceneAsync(scene);

            StartCoroutine(WaitForSceneUnload(sceneName, asyncOp, OnSceneFinishUnload, a));
        }

        public void LoadScene(object sender, string sceneName, LoadSceneMode mode = LoadSceneMode.Additive, OnSceneArgs args = null)
        {
            var scene = SceneManager.GetSceneByName(sceneName);

            if (scene.isLoaded)
            {
                return;
            }

            var a = args ?? new OnSceneArgs()
            {
                ActualScene = SceneManager.GetActiveScene(),
                TargetScene = scene,
                NewSceneType = mode
            };

            if (mode == LoadSceneMode.Single)
            {
                OnSwitchingScene?.Invoke(sender, a);
            }
            else
            {
                OnLoadingScene?.Invoke(sender, a);
            }

            var asyncOp = SceneManager.LoadSceneAsync(sceneName, mode);
            StartCoroutine(WaitForSceneLoad(sceneName, asyncOp, mode == LoadSceneMode.Additive ? OnAdditiveLoaded : OnSwitchLoaded, args));
        }
    }
}

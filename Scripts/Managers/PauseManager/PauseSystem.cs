using Topacai.Utils.GameObjects;
using Topacai.Inputs;

using UnityEngine;
using UnityEngine.Events;

namespace Topacai.Managers.GM.PauseSystem
{
    public class PauseManager : Singleton<PauseManager>
    {
        public static UnityEvent<object, bool> OnGamePaused = new();

        protected static bool _isPaused;

        public static bool IsPaused => _isPaused;

        [Header("Pause settings")]
        [SerializeField] protected bool _pauseOnStart = false;

        protected void Start()
        {
            InputHandler._Pause.started += (_) => PauseGame(null, !IsPaused);
        }

        public void PauseGame(object sender, bool pause)
        {
            _isPaused = pause;

            Time.timeScale = pause ? 0 : 1;

#if UNITY_EDITOR
            Topacai.TDebug.Debugcanvas.Instance.AddTextToDebugLog("Pause", "", pause ? 0 : 0.1f);
#endif

            OnGamePaused?.Invoke(sender, pause);
        }

        protected void InitializePauseManager()
        {
            PauseGame(this, _pauseOnStart);
        }
    }
}

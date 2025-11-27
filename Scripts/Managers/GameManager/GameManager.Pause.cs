using System;

using Topacai.Utils.GameObjects;

using UnityEngine;
using UnityEngine.Events;

using Topacai.Utils.GameMenu;
using Topacai.Managers.GM.PauseMenu;

using UnityEngine.InputSystem;

namespace Topacai.Managers.GM
{
    public partial class GameManager : Singleton<GameManager>
    {
        public static UnityEvent<object, bool> OnGamePaused = new();

        private static bool _isPaused;

        public static bool IsPaused => _isPaused;

        [Header("Pause settings")]
        [SerializeField] private bool _usePauseMenu = false;
        private TGameMenu _pauseMenu;

        public void PauseGame(object sender, bool pause)
        {
            _isPaused = pause;

            Time.timeScale = pause ? 0 : 1;

#if UNITY_EDITOR
            Topacai.TDebug.Debugcanvas.Instance.AddTextToDebugLog("Pause", "", pause ? 0 : 0.1f);
#endif

            OnGamePaused?.Invoke(sender, pause);

            if (_usePauseMenu)
            {
                InitializePauseMenu();

                _pauseMenu.enabled = pause;
                SetCursor(pause);
            }
        }

        private void InitializePauseMenu()
        {
            if (_pauseMenu != null) return;

            if (TryGetComponent(out TGameMenu gameMenu))
            {
                _pauseMenu = gameMenu;
                gameMenu.Init();
            }
            else
            {
                Debug.LogWarning("Pause menu not found");
            }
        }
    }
}

using System;

using Topacai.Utils.GameObjects;

using UnityEngine;
using UnityEngine.Events;

using Topacai.Utils.MenuSystem;
using Topacai.Managers.GM.PauseMenu;

using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Topacai.Managers.GM
{
    public partial class GameManager : Singleton<GameManager>
    {
        public static UnityEvent<object, bool> OnGamePaused = new();
        public UnityEvent<ClickEvent> OnPauseMenuButtonClicked = new();

        private static bool _isPaused;

        public static bool IsPaused => _isPaused;

        [Header("Pause settings")]
        [SerializeField] private bool _usePauseMenu = false;
        [SerializeField] private bool _pauseOnStart = false;

        private const string PAUSE_RESUME_BUTTON = "gamepause-resume-button";
        private const string PAUSE_EXIT_BUTTON = "gamepause-exit-button";
        private const string PAUSE_CONFIG_BUTTON = "gamepause-config-button";

        private const string PAUSE_CONFIG_WINDOW_NAME = "PauseConfigMenu";
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
                InitializePauseMenu(false);

                _pauseMenu.enabled = pause;
                SetCursor(pause);
            }
        }

        private void InitializePauseManager()
        {
            InitializePauseMenu();
            PauseGame(this, _pauseOnStart);
        }

        private void InitializePauseMenu(bool hide = true)
        {
            if (_pauseMenu != null) return;

            if (TryGetComponent(out TGameMenu gameMenu))
            {
                _pauseMenu = gameMenu;
                gameMenu.Init();

                gameMenu.OnAnyViewButton.AddListener(MenuButtonListener);
                gameMenu.OnAnyPersistentButton.AddListener(MenuButtonListener);

                gameMenu.enabled = !hide;
            }
            else
            {
                Debug.LogWarning("Pause menu not found");
            }
        }

        private void MenuButtonListener(ClickEvent args)
        {
            OnPauseMenuButtonClicked?.Invoke(args);

            var b = args.target as Button;

            if (b == null) return;

            switch (b.name)
            {
                case PAUSE_CONFIG_BUTTON:
                    _pauseMenu.MainMenu.GoChildren(PAUSE_CONFIG_WINDOW_NAME);
                    return;
                case PAUSE_RESUME_BUTTON:
                    PauseGame(null, false);
                    break;
                case PAUSE_EXIT_BUTTON:
                    ExitGame();
                    break;
            }
        }
    }
}

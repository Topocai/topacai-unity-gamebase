using Topacai.Utils;
using Topacai.Utils.GameObjects;

using UnityEngine;

namespace Topacai.Managers.GM
{
    public struct ExitingGameBus { }
    public partial class GameManager : Singleton<GameManager>
    {
#if UNITY_EDITOR
        public void DebugLog(string message) => Debug.Log(message);
        public void ShowLogOnScreen(object sender, string msg, float duration = 0.1f) => Topacai.TDebug.Debugcanvas.Instance.AddTextToDebugLog(sender != null ? $"{sender.ToString()}" : "GM Log", msg, duration);
#endif

        public static bool IsCursorLocked => Cursor.lockState == CursorLockMode.Locked;

        protected void Start()
        {
            DisableCursor();
        }

        public static void EnableCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public static void DisableCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public static void SetCursor(bool free)
        {
            if (free)
                EnableCursor();
            else
                DisableCursor();
        }

        public async void ExitGame()
        {
#if UNITY_EDITOR
            Debug.Log("[GameManager] Exiting game, waiting for listeners to do their job");
#endif
            await EventBus.Publish<ExitingGameBus>(default);
#if UNITY_EDITOR
            Debug.Log("[GameManager] Exiting complete");
#endif

            Application.Quit();
        }

    }
}


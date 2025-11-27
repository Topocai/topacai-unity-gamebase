using EditorAttributes;

using System;
using System.Collections;
using System.Collections.Generic;
using Topacai.Inputs;
using Topacai.Utils.GameObjects;
using Topacai.Utils.SaveSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Topacai.Managers.GM
{
    public partial class GameManager : Singleton<GameManager>
    {
        
#if UNITY_EDITOR
        public void DebugLog(string message) => Debug.Log(message);
        public void ShowLogOnScreen(object sender, string msg, float duration= 0.1f) => Topacai.TDebug.Debugcanvas.Instance.AddTextToDebugLog(sender!=null ? $"{sender.ToString()}" : "GM Log", msg, duration);
#endif

        public static bool IsCursorLocked => Cursor.lockState == CursorLockMode.Locked;

        private void Start()
        {
            DisableCursor();
            InputHandler._Pause.started += (_) => PauseGame(null, !IsPaused);

            InitializePauseManager();
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

        public void ExitGame()
        {
            SaveSystemClass.CallSaveGameEvent();
            Application.Quit();
        }

    }
}


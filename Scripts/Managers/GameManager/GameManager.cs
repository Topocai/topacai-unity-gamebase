using EditorAttributes;

using System;
using System.Collections;
using System.Collections.Generic;

using Topacai.Utils.GameObjects;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Topacai.Managers
{
    public partial class GameManager : Singleton<GameManager>
    {
        
#if UNITY_EDITOR
        public void DebugLog(string message) => Debug.Log(message);
        public void ShowLogOnScreen(object sender, string msg, float duration= 0.1f) => Topacai.TDebug.Debugcanvas.Instance.AddTextToDebugLog(sender!=null ? $"{sender.ToString()}" : "GM Log", msg, duration);
#endif

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

    }
}


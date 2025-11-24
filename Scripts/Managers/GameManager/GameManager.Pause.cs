using System;

using Topacai.Utils.GameObjects;

using UnityEngine;
using UnityEngine.Events;

namespace Topacai.Managers
{
    public partial class GameManager : Singleton<GameManager>
    {
        public static UnityEvent<object, bool> OnGamePaused = new();

        private static bool _isPaused;

        public static bool IsPaused => _isPaused;

        public void PauseGame(object sender, bool pause)
        {
            _isPaused = pause;

            Time.timeScale = pause ? 0 : 1;

            OnGamePaused?.Invoke(sender, pause);
        }
    }
}

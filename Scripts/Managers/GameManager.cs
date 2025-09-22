using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Topacai.Managers.GameManager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField, OnValueChanged(nameof(OnFrameRateChanged))] private int frameRate = 60;

        private void OnFrameRateChanged() => SetFrameRate(frameRate);
        public void SetFrameRate(int frameRate)
        {
            Application.targetFrameRate = frameRate;
        }

        public void DebugLog(string message) => Debug.Log(message);

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}


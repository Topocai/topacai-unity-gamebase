using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Topacai.TDebug
{
    [System.Serializable]
    public struct ShortcutExecuter<T> where T : Delegate
    {
        [SerializeField] public InputAction HotKey;
        [SerializeField] public T ActionToExecute;

        public ShortcutExecuter(InputAction hotKey, T actionToExecute) 
        {
            HotKey = hotKey;
            ActionToExecute = actionToExecute;

            HotKey.Enable();
        }

        public void Update()
        {
            Debug.Log(HotKey.IsPressed());
            if (HotKey.IsPressed())
            {
                ActionToExecute?.DynamicInvoke();
            }
        }

        public void Enable()
        {
            HotKey.Enable();
        }

        public void Disable()
        {
            HotKey.Disable();
        }
    }
}

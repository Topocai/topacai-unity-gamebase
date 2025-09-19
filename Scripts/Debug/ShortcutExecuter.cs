using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Topacai.TDebug
{
    [System.Serializable]
    public struct ShortcutExecuter<T>
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
            if (HotKey.IsPressed())
            {
                Execute();
            }
        }

        private void Execute()
        {
            if (ActionToExecute is UnityEvent unityEvent)
                unityEvent.Invoke();
            else if (ActionToExecute is Action action)
                action();
            else if (ActionToExecute is UnityAction unityAction)
                unityAction();
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

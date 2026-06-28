using System;
using System.Collections.Generic;

using UnityEngine;

namespace Topacai.Player.Movement.Components
{
    public static class MovementRegistry
    {

        private static Dictionary<PlayerMovement, MovementData> _Movements = new();

        private static Dictionary<PlayerMovement, MovementData> _movements => _Movements;

        #region Register Components
        public static void RegisterComponent(MovementComponent component)
        {
            if (_Movements.TryGetValue(component.Movement, out MovementData data))
            {
                data.Components.Add(component);
                data.StateManager.RegisterState(component.name, component);
            }
            else if (component.Movement != null)
            {
                RegisterPlayerMovement(component.Movement);

                _Movements[component.Movement].Components.Add(component);
                _Movements[component.Movement].StateManager.RegisterState(component.ComponentStateName, component);
            }
            else
            {
                Debug.LogWarning("Movement instance in component is null");
            }
        }

        public static MovementStateManager GetStateManager(PlayerMovement movement)
        {
            if (_Movements.TryGetValue(movement, out MovementData data))
            {
                return data.StateManager;
            }
            else
            {
                Debug.LogWarning("Movement instance not found");
            }

            return null;
        }

        public static T GetRegisteredComponent<T>(PlayerMovement movement) where T : MovementComponent
        {
            if (_Movements.TryGetValue(movement, out MovementData data))
            {
                foreach (MovementComponent component in data.Components)
                {
                    if (component is T typedComponent)
                    {
                        return typedComponent;
                    }
                }
                Debug.LogWarning("Movement Component not found");
            }
            else
            {
                Debug.LogWarning("Movement instance not found");
            }

            return null;
        }

        public static object GetRegisteredComponent(PlayerMovement movement, MovementComponent searched)
        {
            if (_Movements.TryGetValue(movement, out MovementData data))
            {
                foreach (MovementComponent component in data.Components)
                {
                    if (component == searched)
                    {
                        return component;
                    }
                }
                Debug.LogWarning("Movement Component not found");
            }
            else
            {
                Debug.LogWarning("Movement instance not found");
            }

            return null;
        }

        public static object GetRegisteredComponentOfType(PlayerMovement movement, System.Type typeObject)
        {
            if (_Movements.TryGetValue(movement, out MovementData data))
            {
                foreach (MovementComponent component in data.Components)
                {
                    if (component.GetType() == typeObject)
                    {
                        return component;
                    }
                }
                Debug.LogWarning("Movement Component not found");
            }
            else
            {
                Debug.LogWarning("Movement instance not found");
            }

            return null;
        }

        public static void RegisterPlayerMovement(PlayerMovement movement)
        {
            if (!_Movements.ContainsKey(movement))
            {
                _Movements.Add(movement, new MovementData()
                {
                    StateManager = new MovementStateManager(),
                    Components = new HashSet<MovementComponent>()
                });
            }
            else
            {
                Debug.LogWarning("Movement instance already registered");
            }
        }
        #endregion

    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Plastic.Antlr3.Runtime;
using UnityEngine;

namespace Topacai.Player.Movement.Components
{
    /// <summary>
    /// Manage all components attached to a PlayerMovement, list them by their priority value on each event/callback
    /// make sure that each PlayerMovement has an MovementStateManager and each component is call in order
    /// by their priority.
    /// </summary>
    public static class MovementRegistry
    {

        private static Dictionary<PlayerMovement, MovementData> _Movements = new();

        public static Dictionary<PlayerMovement, MovementData> PlayerMovements => _Movements;

        #region Invoke Handler

        public static void InvokeFinallCalback(PlayerMovement movement, ref Vector3 finalForce, ref Vector3 moveDir)
        {
            if (!_Movements.TryGetValue(movement, out var data))
                return;

            foreach (var component in data.BeforeMove)
            {
                if (!component.IsEnabled)
                    continue;

                component.OnBeforeMoveInternal(ref finalForce, ref moveDir);
            }
        }

        public static void InvokeAfterAcceleration(PlayerMovement movement, ref Vector3 targetSpeed, ref float accelRate)
        {
            if (!_Movements.TryGetValue(movement, out var data))
                return;

            foreach (var component in data.BeforeMove)
            {
                if (!component.IsEnabled)
                    continue;

                component.OnMoveAfterAccelInternal(ref targetSpeed, ref accelRate);
            }
        }

        public static void InvokeBeforeAcceleration(PlayerMovement movement, ref Vector3 targetSpeed, ref Vector3 currentFlatVel, ref Vector3 moveDir)
        {
            if (!_Movements.TryGetValue(movement, out var data))
                return;

            foreach (var component in data.BeforeMove)
            {
                if (!component.IsEnabled)
                    continue;

                component.OnBeforeAccelerationInternal(ref targetSpeed, ref currentFlatVel, ref moveDir);
            }
        }

        public static void InvokeGroundNewData(PlayerMovement movement, RaycastHit groundData)
        {
            if (!_Movements.TryGetValue(movement, out var data))
                return;

            foreach (var component in data.BeforeMove)
            {
                if (!component.IsEnabled)
                    continue;

                component.OnGroundChangedInternal(groundData);
            }
        }

        #endregion

        #region Register Components

        private static void SortComponents(MovementData data)
        {
            data.AfterAcceleration.Sort(
                (a, b) => a.Priority.AfterAcceleration.CompareTo(b.Priority.AfterAcceleration)
            );

            data.BeforeMove.Sort(
                (a, b) => a.Priority.BeforeMove.CompareTo(b.Priority.BeforeMove)
            );

            data.BeforeAcceleration.Sort(
                (a, b) => a.Priority.BeforeAcceleration.CompareTo(b.Priority.BeforeAcceleration)
            );

            data.GroundChanged.Sort(
                (a, b) => a.Priority.GroundChanged.CompareTo(b.Priority.GroundChanged)
            );
        }

        /// <summary>
        /// Use this to register a MovementComponent that already has an PlayerMovement
        /// </summary>
        /// <param name="component"></param>
        public static void RegisterComponent(MovementComponent component)
        {
            MovementData data = null;

            if (_Movements.ContainsKey(component.Movement))
            {
                data = _Movements[component.Movement];
            }
            else if (component.Movement != null)
            {
                data = RegisterPlayerMovement(component.Movement);
            }
            else
            {
                Debug.LogWarning("Movement instance in component is null");
                return;
            }

            data.Components.Add(component);
            data.StateManager.RegisterState(component.ComponentStateName, component);

            data.AfterAcceleration.Add(component);
            data.BeforeMove.Add(component);
            data.BeforeAcceleration.Add(component);
            data.GroundChanged.Add(component);
            data.Components.Add(component);

            SortComponents(data);
        }

        /// <summary>
        /// Returns the StateManager related to PlayerMovement
        /// </summary>
        /// <param name="movement"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get a registered component on a PlayerMovement by their type as generic, first one found will be returned
        /// </summary>
        /// <param name="movement"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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

        /// <summary>
        /// Get a registered component on a PlayerMovement by their type passed as argument. First one found will be returned.
        /// </summary>
        /// <param name="movement"></param>
        /// <param name="typeObject"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Setups the data necessary to attach components to a PlayerMovement.
        /// </summary>
        /// <param name="movement"></param>
        /// <returns></returns>
        public static MovementData RegisterPlayerMovement(PlayerMovement movement)
        {
            if (!_Movements.ContainsKey(movement))
            {
                var data = new MovementData();
                _Movements.Add(movement, data);
                return data;
            }
            else
            {
                Debug.LogWarning("Movement instance already registered");
            }

            return null;
        }
        #endregion

    }
}

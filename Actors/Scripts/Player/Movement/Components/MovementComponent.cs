using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Topacai.Player.Movement.Components
{
    /// <summary>
    /// Manages state of movement components
    /// in order to check if a component is being used or not
    /// without having to know the class of the component just by name state
    /// </summary>
    public class MovementStateManager
    {
        private Dictionary<string, MovementComponent> _registeredStates = new Dictionary<string, MovementComponent>();

        /// <summary>
        /// Checks for the state of a component only when it is called.
        /// If the component is not registered, it will return false
        /// </summary>
        /// <param name="stateName">The registered name of the component</param>
        /// <returns></returns>
        public bool GetState(string stateName)
        {
            if (_registeredStates.TryGetValue(stateName.ToLower(), out MovementComponent state))
                /// To check if a component is being used correctly it has to
                /// override the IsUsing method from the base class
                return state.IsUsing();
            else
                return false;
        }

        public void RegisterState(string stateName, MovementComponent state)
        {
            _registeredStates.TryAdd(stateName.ToLower(), state);
        }
    }

    public struct MovementData
    {
        public MovementStateManager StateManager;
        public HashSet<MovementComponent> Components;
    }

    public class MovementComponent : MonoBehaviour
    {
        private static Dictionary<PlayerMovement, MovementData> _Movements = new();

        protected static Dictionary<PlayerMovement, MovementData> _movements => _Movements;

        #region Register Components
        protected static void RegisterComponent(MovementComponent component)
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
                _Movements[component.Movement].StateManager.RegisterState(component.name, component);
            }
            else
            {
                Debug.LogWarning("Movement instance in component is null");
            }
        }

        protected static object GetRegisteredComponentOfType(PlayerMovement movement, System.Type typeObject)
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

        protected static void RegisterPlayerMovement(PlayerMovement movement)
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

        #region Instance Data

        /// FIELDS 
        [Header("Component Setup")]
        [SerializeField] protected PlayerMovement _movement;
        [SerializeField] protected string _componentStateName;

        [SerializeField] protected string[] _incompatibleStates;

        /// PROPERTIES
        protected MovementStateManager _currentManager => _Movements[_movement].StateManager;
        protected bool CheckState(string stateName) => _currentManager.GetState(stateName);
        protected bool InConflict(string[] states)
        {
            if (_incompatibleStates.Length == 0) return false;

            foreach (string state in states)
            {
                if (CheckState(state))
                    return true;
            }
            return false;
        }

        public PlayerMovement Movement => _movement;
        public virtual bool IsEnabled => this.enabled;

        #endregion

        #region Movement Events

        private void OnMoveBeforeWallHandler(ref Vector3 moveDir, ref Vector3 flatVel, ref RaycastHit wallHitInfo)
        {
            if (!enabled) return;
            OnMoveBeforeWall(ref moveDir, ref flatVel, ref wallHitInfo);
        }

        protected virtual void OnMoveBeforeWall(ref Vector3 moveDir, ref Vector3 flatVel, ref RaycastHit wallHitInfo) { }

        private void OnMoveAfterAccelHandler(ref float accelRate)
        {
            if (!enabled) return;
            OnMoveAfterAccel(ref accelRate);
        }

        protected virtual void OnMoveAfterAccel(ref float accelRate) { }

        private void OnBeforeMoveHandler(ref Vector3 finalForce, ref Vector3 moveDir)
        {
            if (!enabled) return;
            OnBeforeMove(ref finalForce, ref moveDir);
        }

        protected virtual void OnBeforeMove(ref Vector3 finalForce, ref Vector3 moveDir) { }

        private void OnGroundChangedHandler(RaycastHit groundData)
        {
            if (!enabled) return;
            OnGroundChanged(groundData);
        }

        protected virtual void OnGroundChanged(RaycastHit groundData) { }
        #endregion

        #region Instance Methods

        #region Unity Callbacks

        protected virtual void Awake()
        {
            if (_movement == null) return;

            if(!_movements.ContainsKey(_movement))
                RegisterPlayerMovement(_movement);

            RegisterComponent(this);
        }

        protected virtual void OnEnable()
        {
            if (_movement == null)
            {
                Debug.LogError($"Component: {gameObject.name} not enabled");
                return;
            }

            _movement.OnBeforeMove += OnBeforeMoveHandler;
            _movement.OnMoveBeforeWall += OnMoveBeforeWallHandler;
            _movement.OnMoveAfterAccel += OnMoveAfterAccelHandler;
            _movement.OnGroundChangedEvent += OnGroundChangedHandler;

            if (string.IsNullOrEmpty(_componentStateName))
                _componentStateName = this.ToString();

            Debug.Log($"Component: {gameObject.name} enabled");
        }

        protected virtual void OnDisable()
        {
            _movement.OnBeforeMove -= OnBeforeMoveHandler;
            _movement.OnMoveBeforeWall -= OnMoveBeforeWallHandler;
            _movement.OnMoveAfterAccel -= OnMoveAfterAccelHandler;
            _movement.OnGroundChangedEvent -= OnGroundChangedHandler;
        }

        #endregion

        public virtual void Enable()
        {
            enabled = true;
        }

        public virtual void Disable()
        {
            enabled = false;
        }

        /// <summary>
        /// Override to share the component status in the state manager
        /// </summary>
        /// <returns></returns>
        public virtual bool IsUsing() => false;

        public override string ToString()
        {
            string name = GetType().Name;
            if (name.Contains("Component"))
                name = name[..^9];
            return name;
        }
        #endregion

        
    }
}



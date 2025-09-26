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
    public class MovementComponent : MonoBehaviour
    {
        protected static UnityEvent<MovementComponent> OnComponentRegistered = new UnityEvent<MovementComponent>();
        /// <summary>
        /// Keep register of all MovementComponents existing in the scene
        /// You can acces to them with the static method GetRegisteredComponentOfType if you know the type
        /// </summary>
        private static HashSet<MovementComponent> _registeredComponents = new HashSet<MovementComponent>();

        private static MovementStateManager _stateManager = new MovementStateManager();

        [Header("Component Setup")]
        [SerializeField] protected PlayerMovement _movement;
        [SerializeField] protected string _componentStateName;

        public PlayerMovement Movement => _movement;
        public bool IsEnabled => this.enabled;

        #region Register Components
        protected static void RegisterComponent(MovementComponent component)
        {
            _registeredComponents.Add(component);
        }

        protected static object GetRegisteredComponentOfType(System.Type typeObject)
        {
            foreach (MovementComponent component in _registeredComponents)
            {
                if (component.GetType() == typeObject)
                {
                    return component;
                }
            }
            return null;
        }
        #endregion

        #region Unity Callbacks

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

            _registeredComponents.Add(this);

            if (string.IsNullOrEmpty(_componentStateName))
                _componentStateName = this.ToString();

            _stateManager.RegisterState(_componentStateName, this);

            OnComponentRegistered.Invoke(this);

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

        #region Events

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

        #region Public Class-Instance Methods
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

        protected bool CheckState(string stateName) => _stateManager.GetState(stateName);
    }
}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Topacai.Player.Firstperson.Movement.Components
{
    public class MovementStateManager
    {
        private Dictionary<string, MovementComponent> _registeredStates = new Dictionary<string, MovementComponent>();

        public bool GetState(string stateName)
        {
            if (_registeredStates.TryGetValue(stateName.ToLower(), out MovementComponent state))
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
        private static HashSet<MovementComponent> _registeredComponents = new HashSet<MovementComponent>();

        private static MovementStateManager _stateManager = new MovementStateManager();

        [Header("Component Setup")]
        [SerializeField] protected FirstpersonMovement _movement;
        [SerializeField] protected string _componentStateName;

        public FirstpersonMovement Movement => _movement;
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
        #endregion

        #region Public Methods
        public virtual void Enable()
        {
            enabled = true;
        }

        public virtual void Disable()
        {
            enabled = false;
        }

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



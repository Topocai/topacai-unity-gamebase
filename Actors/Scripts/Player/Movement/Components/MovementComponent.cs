using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Topacai.Player.Movement.Components
{
    public class MovementComponent : MonoBehaviour
    {

        #region Instance Data

        /// FIELDS 
        [Header("Component Setup")]
        [SerializeField] protected PlayerMovement _movement;
        [SerializeField] protected string _componentStateName;
        [SerializeField] protected int _priority = 0;

        [HideInInspector, SerializeField] protected string[] _incompatibleStates = new string[0];

        public void SetIncompatibleStates(string[] states) => _incompatibleStates = states;
        public string[] GetIncompatibleStates() => _incompatibleStates;

        /// PROPERTIES
        protected virtual MovementStateManager _currentManager => MovementRegistry.GetStateManager(_movement);
        protected virtual bool CheckState(string stateName) => _currentManager.GetState(stateName);
        protected virtual bool InConflict(string[] states)
        {
            if (states.Length == 0) return false;

            foreach (string state in states)
            {
                if (CheckState(state))
                    return true;
            }
            return false;
        }

        public PlayerMovement Movement => _movement;
        public virtual bool IsEnabled => this.enabled;
        public int Priority => _priority;

        public string ComponentStateName => _componentStateName;

        #endregion

        #region Movement Events

        private void OnMoveBeforeWallHandler(ref Vector3 moveDir, ref Vector3 flatVel, ref RaycastHit wallHitInfo)
        {
            if (!enabled) return;
            OnMoveBeforeWall(ref moveDir, ref flatVel, ref wallHitInfo);
        }

        protected virtual void OnMoveBeforeWall(ref Vector3 moveDir, ref Vector3 flatVel, ref RaycastHit wallHitInfo) { }

        private void OnMoveAfterAccelHandler(ref Vector3 targetSpeed, ref float accelRate)
        {
            if (!enabled) return;
            OnMoveAfterAccel(ref targetSpeed, ref accelRate);
        }

        protected virtual void OnMoveAfterAccel(ref Vector3 targetSpeed, ref float accelRate) { }

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

        protected virtual void Start()
        {
            if (_movement == null) return;

            MovementRegistry.RegisterComponent(this);
        }

        protected virtual void OnEnable()
        {
            if (!Application.isPlaying) return;

            if (_movement == null)
            {
                Debug.LogError($"Component: {gameObject.name} not enabled");
                return;
            }

            _movement.FinalCallback += OnBeforeMoveHandler;
            _movement.WallDetectedCallback += OnMoveBeforeWallHandler;
            _movement.AccelerationCallback += OnMoveAfterAccelHandler;
            _movement.OnGroundNewData += OnGroundChangedHandler;

            if (string.IsNullOrEmpty(_componentStateName))
                _componentStateName = this.ToString();

            Debug.Log($"Component: {gameObject.name} enabled");
        }

        protected virtual void OnDisable()
        {
            _movement.FinalCallback -= OnBeforeMoveHandler;
            _movement.WallDetectedCallback -= OnMoveBeforeWallHandler;
            _movement.AccelerationCallback -= OnMoveAfterAccelHandler;
            _movement.OnGroundNewData -= OnGroundChangedHandler;
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



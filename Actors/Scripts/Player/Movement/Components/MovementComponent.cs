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
        [SerializeField, HideInInspector] protected string _componentStateName;
        [SerializeField] protected MovementPriority _prorityTable;

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
        public MovementPriority Priority => _prorityTable;

        public string ComponentStateName => _componentStateName;

        #endregion

        #region Movement Events

        /// <summary>
        /// Called before calculated dynamic values, calcualted target speed with moveDir and receibed inputs but before any other change
        /// any change to move direction or desired speed should be here
        /// </summary>
        /// <param name="finalForce">Final calculated force</param>
        /// <param name="moveDir">Final calculated direction</param> <summary>
        internal void OnFirstCallInternal(ref Vector3 moveDir, ref Vector3 flatVel, ref Vector3 targetSpeed)
        {
            OnFirstCall(ref moveDir, ref flatVel, ref targetSpeed);
        }

        /// <summary>
        /// Called just before the final calculated force will be applied by PlayerMovement
        /// </summary>
        /// <param name="finalForce">Final calculated force</param>
        /// <param name="moveDir">Final calculated direction</param> <summary>
        internal void OnBeforeMoveInternal(ref Vector3 finalForce, ref Vector3 moveDir)
        {
            OnBeforeMove(ref finalForce, ref moveDir);
        }

        /// <summary>
        /// Called before calculated acceleration and desaceleration rate
        /// </summary>
        /// <param name="targetSpeed">Current target speed</param>
        /// <param name="flatVel">Current velocity on movement rigidbody</param>
        /// <param name="moveDir">Calculated movement direction</param>
        internal void OnBeforeAccelerationInternal(ref Vector3 targetSpeed, ref Vector3 flatVel, ref Vector3 moveDir)
        {
            OnBeforeAcceleration(ref targetSpeed, ref flatVel, ref moveDir);
        }


        /// <summary>
        /// Called after acceleration/desaceleration were calculated
        /// </summary>
        /// <param name="targetSpeed">actual target speed</param>
        /// <param name="accelRate">accel rate calculated to be used on this frame</param>
        internal void OnMoveAfterAccelInternal(ref Vector3 targetSpeed, ref float accelRate)
        {
            OnMoveAfterAccel(ref targetSpeed, ref accelRate);
        }


        /// <summary>
        /// when any change on ground data, such as player is not anymore on ground or ground has changed this would be called
        /// </summary>
        /// <param name="groundData"></param>
        internal void OnGroundChangedInternal(RaycastHit groundData)
        {
            OnGroundChanged(groundData);
        }

        protected virtual void OnFirstCall(ref Vector3 moveDir, ref Vector3 flatVel, ref Vector3 targetSpeed) { }
        protected virtual void OnBeforeMove(ref Vector3 finalForce, ref Vector3 moveDir) { }
        protected virtual void OnBeforeAcceleration(ref Vector3 targetSpeed, ref Vector3 flatVel, ref Vector3 moveDir) { }
        protected virtual void OnMoveAfterAccel(ref Vector3 targetSpeed, ref float accelRate) { }
        protected virtual void OnGroundChanged(RaycastHit groundData) { }

        #endregion

        #region Instance Methods

        #region Unity Callbacks

        protected virtual void Start()
        {
            if (_movement == null) return;

            MovementRegistry.RegisterComponent(this);
        }

        #endregion

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



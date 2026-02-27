using System;

using UnityEngine;
using UnityEngine.InputSystem;

using Topacai.Player.Movement.Components;

using Topacai.Inputs;
using Topacai.Utils;

namespace Topacai.Player.Movement.Firstperson.Components.Crouch
{
    public class CrouchComponent : MovementComponent
    {

        [Tooltip("Use this configures input action on instance start or not (use SetInputAction() for custom input) ")]
        [SerializeField] private bool _useOwnInput;
        [Tooltip("Configure if input is switcheable or holdeable, not in-play changes only at start or on custom set of input")]
        [SerializeField] private bool _holdInput;
        [SerializeField] private InputAction _ownInputAction;

        private SwitchKey _switchKeyHolder;

        private bool _isCrouching = false;

        private bool _suscribed = false;

        private void Start()
        {
            if (_useOwnInput)
                _switchKeyHolder = new(_ownInputAction, !_holdInput);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (!Application.isPlaying) return;

            _switchKeyHolder?.Disable();
            DesuscribeToSwitcheable();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!Application.isPlaying) return;

            _switchKeyHolder?.Enable();
            SuscribeToSwitcheable();
        }

        #region Input 

        private void StartCrouch(InputAction.CallbackContext context)
        {
            Crouch(true);
        }

        private void StopCrouch(InputAction.CallbackContext context)
        {
            Crouch(false);
        }

        protected void SuscribeToSwitcheable()
        {
            if (_suscribed || _switchKeyHolder == null) return;

            _switchKeyHolder.OnStart += StartCrouch;
            _switchKeyHolder.OnStop += StopCrouch;

            _suscribed = true;
        }

        protected void DesuscribeToSwitcheable()
        {
            if (!_suscribed || _switchKeyHolder == null) return;

            _switchKeyHolder.OnStart -= StartCrouch;
            _switchKeyHolder.OnStop -= StopCrouch;

            _suscribed = false;
        }

        #endregion

        #region input-configuration

        public virtual void SetInputAction(InputAction action)
        {
            SetInputAction(action, !_holdInput);
        }

        public virtual void SetInputAction(InputAction action, bool switcheable)
        {
            DesuscribeToSwitcheable();

            _switchKeyHolder = new(action, switcheable);
            _switchKeyHolder.Enable();

            SuscribeToSwitcheable();
        }

        public virtual void SwitchMode(bool switcheable)
        {
            _switchKeyHolder.SetSwitch(switcheable);
        }

        public bool IsInputSwitch => _switchKeyHolder.IsSwitch;

        #endregion

        public virtual void Crouch()
        {
            Debug.Log("crouch log");
            RigidbodyInterpolation originalInter = _movement.Rigidbody.interpolation;
            CollisionDetectionMode originalDetection = _movement.Rigidbody.collisionDetectionMode;
            _movement.Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            _movement.Rigidbody.interpolation = RigidbodyInterpolation.None;

            Bounds bounds = _movement.PlayerBrain.GetComponent<Renderer>().bounds;
            Vector3 _crouchPivotPos = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

            Vector3 scaleFactor = _isCrouching ? new Vector3(1f, 2f, 1f) : new Vector3(1f, 0.5f, 1f);
            _isCrouching = !_isCrouching;
            Transforms.ScaleRelativeToPivot(_movement.transform, scaleFactor, _crouchPivotPos);

            _movement.Rigidbody.interpolation = originalInter;
            _movement.Rigidbody.collisionDetectionMode = originalDetection;
        }

        public virtual void Crouch(bool state)
        {
            Debug.Log($"crouch {state}");
            if (_isCrouching == state) return;

            Crouch();
        }

        private void Update()
        {

        }



        protected virtual void CrouchInputs()
        {

        }

    }
}


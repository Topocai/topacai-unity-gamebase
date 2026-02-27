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

        [Tooltip("Put here any child on player that has to conserve their scale when crouching, like visuals")]
        [SerializeField] private Transform[] _preserveScale;
        [SerializeField] private float crouchHeightRatio = 0.5f;

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
            /// detection and interpolation mode are handled because sometimes crouch is
            /// perform on air or not able to scale due to rigidbody physics
            RigidbodyInterpolation originalInter = _movement.Rigidbody.interpolation;
            CollisionDetectionMode originalDetection = _movement.Rigidbody.collisionDetectionMode;
            _movement.Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            _movement.Rigidbody.interpolation = RigidbodyInterpolation.None;

            Bounds bounds = _movement.PlayerBrain.GetComponent<Renderer>().bounds;
            Vector3 _crouchPivotPos = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

            float multiplier = _isCrouching ? (1f / crouchHeightRatio) : crouchHeightRatio;
            Vector3 scaleFactor = new Vector3(1f, multiplier, 1f);
            _isCrouching = !_isCrouching;
            Transforms.ScaleRelativeToPivot(_movement.transform, scaleFactor, _crouchPivotPos);

            _movement.Rigidbody.interpolation = originalInter;
            _movement.Rigidbody.collisionDetectionMode = originalDetection;

            /// calculate new scale for any child that are marked to preserve their original size
            foreach (Transform child in _preserveScale)
            {
                Vector3 currentScale = child.localScale;
                child.localScale = new Vector3(
                    currentScale.x / scaleFactor.x,
                    currentScale.y / scaleFactor.y,
                    currentScale.z / scaleFactor.z
                );
            }
        }

        public virtual void Crouch(bool state)
        {
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


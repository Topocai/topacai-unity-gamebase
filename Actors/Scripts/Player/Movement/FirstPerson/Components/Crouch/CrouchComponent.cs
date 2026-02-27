using System;

using UnityEngine;
using UnityEngine.InputSystem;

using Topacai.Player.Movement.Components;

using Topacai.Inputs;
using Topacai.Utils;
using Topacai.TDebug;

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

        private Vector3 _initialPlayerScale;
        private Vector3[] _initialChildrenScales;

        private void Start()
        {
            if (_useOwnInput)
                _switchKeyHolder = new(_ownInputAction, !_holdInput);

            _initialPlayerScale = _movement.transform.localScale;

            _initialChildrenScales = new Vector3[_preserveScale.Length];
            for (int i = 0; i < _preserveScale.Length; i++)
            {
                _initialChildrenScales[i] = _preserveScale[i].localScale;
            }
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



        public virtual void SetHeightScale(float ratio)
        {
            RigidbodyInterpolation originalInter = _movement.Rigidbody.interpolation;
            CollisionDetectionMode originalDetection = _movement.Rigidbody.collisionDetectionMode;
            _movement.Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            _movement.Rigidbody.interpolation = RigidbodyInterpolation.None;

            Bounds bounds = _movement.PlayerBrain.GetComponent<Renderer>().bounds;
            Vector3 _crouchPivotPos = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

            float targetY = _initialPlayerScale.y * ratio;
            Vector3 targetScale = new Vector3(_initialPlayerScale.x, targetY, _initialPlayerScale.z);

            Vector3 currentScale = _movement.transform.localScale;
            Vector3 scaleFactor = new Vector3(
                targetScale.x / currentScale.x,
                targetScale.y / currentScale.y,
                targetScale.z / currentScale.z
            );

            Transforms.ScaleRelativeToPivot(_movement.transform, scaleFactor, _crouchPivotPos);

            _movement.Rigidbody.interpolation = originalInter;
            _movement.Rigidbody.collisionDetectionMode = originalDetection;

            /// calculate new scale for any child that are marked to preserve their original size
            for (int i = 0; i < _preserveScale.Length; i++)
            {
                if (_preserveScale[i] != null)
                {
                    Vector3 newScale = _initialChildrenScales[i];
                    newScale.y = _initialChildrenScales[i].y / ratio;
                    _preserveScale[i].localScale = newScale;
                }
            }
        }

        public virtual void Crouch()
        {
            SetHeightScale(_isCrouching ? 1 : crouchHeightRatio);
            _isCrouching = !_isCrouching;

            Debugcanvas.Instance.AddTextToDebugLog("crouch", $"{_isCrouching}");


        }

        public virtual void Crouch(bool state)
        {
            if (_isCrouching == state) return;

            Crouch();
        }

    }
}


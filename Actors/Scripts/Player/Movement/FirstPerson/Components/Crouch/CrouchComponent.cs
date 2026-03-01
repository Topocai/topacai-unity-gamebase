using System;

using UnityEngine;
using UnityEngine.InputSystem;

using Topacai.Player.Movement.Components;

using Topacai.Inputs;
using Topacai.Utils;
using Topacai.TDebug;
using System.Collections;

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

        private Coroutine _heightRoutine;

        public void SetHeightScale(float ratio, float duration = 0.15f)
        {
            if (_heightRoutine != null)
                StopCoroutine(_heightRoutine);

            _heightRoutine = StartCoroutine(HeightScaleRoutine(ratio, duration));
        }

        private IEnumerator HeightScaleRoutine(float ratio, float duration)
        {
            /// detection and interpolation mode are handled because sometimes crouch is
            /// perform on air or not able to scale due to rigidbody physics
            RigidbodyInterpolation originalInter = _movement.Rigidbody.interpolation;
            CollisionDetectionMode originalDetection = _movement.Rigidbody.collisionDetectionMode;

            _movement.Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            _movement.Rigidbody.interpolation = RigidbodyInterpolation.None;

            // gets the pivot for scale from the bottom of the player using the bound of the capsule
            // and calculate the target new scale by ratio, and applying it doing an interpolation

            Bounds bounds = _movement.PlayerBrain.GetComponent<Renderer>().bounds;
            Vector3 pivot = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

            float targetY = _initialPlayerScale.y * ratio;
            Vector3 targetScale = new Vector3(_initialPlayerScale.x, targetY, _initialPlayerScale.z);

            Vector3 startScale = _movement.transform.localScale;

            float time = 0f;
            while (time < 1f)
            {
                time += Time.deltaTime / duration;
                float k = Mathf.SmoothStep(0f, 1f, time);

                Vector3 current = Vector3.Lerp(startScale, targetScale, k);

                Vector3 scaleFactor = new Vector3(
                    current.x / _movement.transform.localScale.x,
                    current.y / _movement.transform.localScale.y,
                    current.z / _movement.transform.localScale.z
                );

                Transforms.ScaleRelativeToPivot(_movement.transform, scaleFactor, pivot);

                // keep the size of childs that are marked to preserve their scale
                for (int i = 0; i < _preserveScale.Length; i++)
                {
                    if (_preserveScale[i] != null)
                    {
                        Vector3 newScale = _initialChildrenScales[i];
                        newScale.y = _initialChildrenScales[i].y / Mathf.Lerp(1f, ratio, k);
                        _preserveScale[i].localScale = newScale;
                    }
                }

                yield return null;
            }

            _movement.Rigidbody.interpolation = originalInter;
            _movement.Rigidbody.collisionDetectionMode = originalDetection;

            _heightRoutine = null;
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


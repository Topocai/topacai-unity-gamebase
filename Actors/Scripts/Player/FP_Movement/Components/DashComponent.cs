using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Topacai.Player.Firstperson.Camera;
using Topacai.TDebug;

namespace Topacai.Player.Firstperson.Movement.Components
{
    public class DashComponent : MovementComponent
    {
        [Header("Dash Settings")]
        [SerializeField] private float dashTimeToApex;
        [SerializeField] private float dashDistance;
        [SerializeField] private float cooldown;

        [Header("Physics")]
        [SerializeField] private float isInvertedThreshold;

        [Header("Inputs")]
        [SerializeField] private bool OwnInputs;

        [Header("Debug")]
        [field: SerializeField, ReadOnly] public static float LastDashUsage { get; private set; }
        [field: SerializeField, ReadOnly] public static Vector3 LastDashDir { get; private set; }
        [field: SerializeField, ReadOnly] public static float CooldownTimer { get; private set; }
        [field: SerializeField, ReadOnly] public static bool IsDashing { get; private set; }
        [field: SerializeField, ReadOnly] public static bool GroundBeforeDash { get; private set; }

        [SerializeField, ReadOnly] private float _accelForce;
        [SerializeField, ReadOnly] private float _deccelForce;

        private float _initialVel;
        private Coroutine _stopCoroutine;
        private bool needToApplyForce;

        #region Callbacks
        private void Start()
        {
            LastDashUsage = 0f;
            CooldownTimer = cooldown;
        }

        private void FixedUpdate()
        {
            LastDashUsage -= Time.deltaTime;

            if (IsDashing)
            {
                Vector3 velToComprobate = _movement.FlatVel;
                if (needToApplyForce)
                {
                    _movement.Rigidbody.AddForce(LastDashDir * _accelForce, ForceMode.Impulse);
                    velToComprobate = LastDashDir * _accelForce;
                    needToApplyForce = false;
                }

                if (GroundBeforeDash)
                    GroundBeforeDash = _movement.LastGroundTime > 0f;

                float angle = Vector3.Dot(velToComprobate.normalized, LastDashDir);
                Debugcanvas.Instance.AddTextToDebugLog("Angle", angle.ToString("0.000"));

                if (velToComprobate.sqrMagnitude >= _initialVel && !(angle < -isInvertedThreshold))
                {
                    _movement.Rigidbody.AddForce(-LastDashDir * _deccelForce * 9.81f, ForceMode.Acceleration);
                }
                else
                {
                    if (velToComprobate.sqrMagnitude > _initialVel)
                    {
                        UnityEngine.Debug.Log("HHH");
                        Vector3 newVel = _movement.FlatVel.normalized * _movement._maxSpeed;
                        newVel.y = _movement.Rigidbody.linearVelocity.y;
                        _movement.Rigidbody.linearVelocity = newVel;
                    }

                    SetDashing(false);
                }

                UnityEngine.Debug.DrawRay(transform.position, -LastDashDir * _deccelForce * 9.81f, Color.aquamarine);
            }
            else
            {
                if (!GroundBeforeDash)
                    GroundBeforeDash = _movement.LastGroundTime > 0f;
            }

            if (GroundBeforeDash)
                CooldownTimer += Time.deltaTime;
        }

        private void Update()
        {
            if (IsDashing)
            {
                _movement.Data.CanMove = false;
            }
            else
            {
                _movement.Data.CanMove = _movement.DefaultData.CanMove;
            }

            if (OwnInputs)
            {
                Dash(_movement.MoveDir.normalized);
            }
        }

        protected override void OnBeforeMove(ref Vector3 finalForce, ref Vector3 moveDir)
        {
            if (IsDashing)
                finalForce = Vector3.zero;
        }
        #endregion

        public override bool IsUsing() => IsDashing;

        private float GetAccelForce()
        {
            //return ((2 * dashDistance) / dashTimeToApex) * _movement.Rigidbody.mass;
            return Mathf.Abs(GetDeccelForce()) * dashTimeToApex;
        }
        private float GetDeccelForce()
        {
            return -(2 * dashDistance) / (dashTimeToApex * dashTimeToApex);
        }

        public void Dash(Vector3 dir, bool forceDash = false)
        {
            if (!forceDash && CooldownTimer < cooldown) return;

            _initialVel = _movement.FlatVel.sqrMagnitude;
            LastDashUsage = 0f;
            CooldownTimer = 0f;
            LastDashDir = dir;
            _accelForce = GetAccelForce();
            _deccelForce = GetDeccelForce() / -9.81f;

            Vector3 newVel = dir * _initialVel * Vector3.Dot(dir, _movement.FlatVel.normalized);
            _movement.Rigidbody.linearVelocity = new Vector3(0, _movement.Rigidbody.linearVelocity.y, 0);

            //_movement.Rigidbody.AddForce(dir * _accelForce, ForceMode.Impulse);
            needToApplyForce = true;
            SetDashing(true);
            _stopCoroutine = StartCoroutine(InverseForce());
            //Invoke(nameof(DeccelDash), dashTimeToApex);
        }

        private void SetDashing(bool value)
        {
            if (!value && IsDashing)
            {
                //_movement.Rigidbody.linearVelocity = LastDashDir * _movement._maxSpeed;
                StopCoroutine(_stopCoroutine);
            }

            IsDashing = value;
        }

        IEnumerator InverseForce()
        {
            yield return new WaitForSeconds(dashTimeToApex);
            SetDashing(false);
        }
    }
}

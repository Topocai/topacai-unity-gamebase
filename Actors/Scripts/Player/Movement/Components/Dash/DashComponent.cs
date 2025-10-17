using EditorAttributes;
using System.Collections;
using Topacai.Inputs;
using UnityEngine;

namespace Topacai.Player.Movement.Components
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
        [field: SerializeField, ReadOnly] public float LastDashUsage { get; private set; }
        [field: SerializeField, ReadOnly] public Vector3 LastDashDir { get; private set; }
        [field: SerializeField, ReadOnly] public float CooldownTimer { get; private set; }
        [field: SerializeField, ReadOnly] public bool IsDashing { get; private set; }
        [field: SerializeField, ReadOnly] public bool GroundBeforeDash { get; private set; }

        [SerializeField, ReadOnly] private float _accelForce;
        [SerializeField, ReadOnly] private float _deccelForce;

        public float Cooldown => cooldown;

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

                if (velToComprobate.sqrMagnitude >= _initialVel && !(angle < -isInvertedThreshold))
                {
                    _movement.Rigidbody.AddForce(-LastDashDir * _deccelForce * 9.81f, ForceMode.Acceleration);
                }
                else
                {
                    if (velToComprobate.sqrMagnitude > _initialVel)
                    {
                        Vector3 newVel = _movement.FlatVel.normalized * Mathf.Clamp(_movement.MaxSpeed, 0, _movement.FlatVel.magnitude);
                        newVel.y = _movement.Rigidbody.linearVelocity.y;
                        _movement.Rigidbody.linearVelocity = newVel;
                    }

                    SetDashing(false);
                }

                _movement.Rigidbody.linearVelocity = new Vector3(_movement.Rigidbody.linearVelocity.x, Mathf.Clamp(_movement.Rigidbody.linearVelocity.y, 0, _movement.Data.MaxFallSpeed), _movement.Rigidbody.linearVelocity.z);

                Debug.DrawRay(transform.position, -LastDashDir * _deccelForce * 9.81f, Color.aquamarine);
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
                _movement.Data.FreezeMove = false;
            }
            else
            {
                _movement.Data.FreezeMove = _movement.DefaultData.FreezeMove;
            }

            if (OwnInputs)
            {
                if (_movement.PlayerBrain.InputHandler.GetActionHandler(ActionName.Run).InstantPress)
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

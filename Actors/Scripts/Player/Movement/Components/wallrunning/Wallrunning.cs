using EditorAttributes;
using UnityEditor.SearchService;
using UnityEditorInternal;
using UnityEngine;

namespace Topacai.Player.Movement.Components.Wallrunning
{
    public class Wallrunning : MovementComponent
    {
        [SerializeField] private string _wallTag = "RunneableWall";
        [SerializeField] private LayerMask _wallMask;
        [SerializeField] private float _wallCheckDistance = 1f;
        [SerializeField] private bool _autoDetect = true;

        [Space(10)]
        [SerializeField] private float _duration = 2.5f;
        [SerializeField] private AnimationCurve _fallCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [SerializeField, ReadOnly] private bool _isWallRunning = false;

        private float _wallDelayTimer = 0f;

        RaycastHit _wallHit = new RaycastHit();

        Collider _lastWallWalked = null;

        private Vector3 runDirection = Vector3.zero;

        private void Update()
        {
            _wallDelayTimer -= Time.deltaTime;
        }

        private bool CheckWall(Vector3 dir)
        {
            if (!Physics.Raycast(Movement.Rigidbody.transform.position, dir, out _wallHit, _wallCheckDistance, _wallMask)) return false;

            return _wallHit.collider?.tag == _wallTag;
        }

        private bool CheckWall(Vector3 dir, out bool output)
        {
            output = CheckWall(dir);
            return output;
        }

        private void DetectWall(ref Vector3 moveDir)
        {
            if (_wallDelayTimer > 0f && !_isWallRunning)
            {
                return;
            }

            if (!_isWallRunning && Movement.LastGroundTime > 0)
            {
                _wallTime = 0f;
                _lastWallWalked = null;
                return;
            }

            var crossLeft = Vector3.Cross(moveDir, Vector3.up);
            bool isHittingWall = CheckWall(crossLeft);

#if UNITY_EDITOR
            Debug.DrawRay(Movement.Rigidbody.transform.position, crossLeft.normalized * _wallCheckDistance, isHittingWall ? Color.red : Color.green);
#endif

            if (!isHittingWall)
#if UNITY_EDITOR
            {
                var crossRight = Vector3.Cross(Vector3.up, moveDir);
                isHittingWall = CheckWall(crossRight);
                Debug.DrawRay(Movement.Rigidbody.transform.position, crossRight.normalized * _wallCheckDistance, isHittingWall ? Color.red : Color.green);
            }
#else
             CheckWall(Vector3.Cross(Vector3.up, moveDir), out wallHit);
#endif

            if (!_isWallRunning && _wallHit.collider == _lastWallWalked)
            {
                return;
            }
            else if (isHittingWall)
            {
                if (_lastWallWalked != _wallHit.collider)
                {
                    Movement.ResetFallSpeed();
                }
                _lastWallWalked = _wallHit.collider ?? _lastWallWalked;
            }

            _lastWallWalked = Movement.LastGroundTime > 0 ? null : _lastWallWalked;

            if (isHittingWall && !_isWallRunning)
            {
                runDirection = Vector3.ProjectOnPlane(moveDir, _wallHit.normal);
#if UNITY_EDITOR
                Debug.DrawRay(_wallHit.transform.position, runDirection.normalized, Color.blue, 1f);
#endif

                StartWallRunning(runDirection, _duration);
            }

        }

        public void StartWallRunning(Vector3 dir, float duration)
        {
            _isWallRunning = true;
            _wallTime = 0f;
            _duration = duration;
        }

        public void StopWallRunning()
        {
            _wallTime = 0;
            _isWallRunning = false;
        }

        float _wallTime = 0f;

        protected override void OnMoveAfterAccel(ref Vector3 targetSpeed, ref float accelRate)
        {
            if(!_isWallRunning) return;

            targetSpeed = runDirection.normalized * targetSpeed.magnitude;

            targetSpeed *= 1.5f;
            accelRate *= 2.33f;
        }

        protected override void OnBeforeMove(ref Vector3 finalForce, ref Vector3 moveDir)
        {
            if (_autoDetect) DetectWall(ref moveDir);

            Movement.UseGravity(!_isWallRunning);

            if (!_isWallRunning)
            {
                StopWallRunning();
                return;
            }

            _wallDelayTimer = 0.5f;

            float fixedTime = _fallCurve.Evaluate(_wallTime / _duration);
            float downForce = fixedTime * 15f;

            Movement.Rigidbody.AddForce(Vector3.down * downForce);

            _wallTime += Time.deltaTime;

            if (fixedTime >= 1f || Vector3.Dot(moveDir, runDirection) < 0.5f)
            {
                StopWallRunning();
                return;
            }

            bool wantJump = Movement.PlayerBrain.InputHandler.GetActionHandler(Inputs.ActionName.Jump).InstantPress;
            if (wantJump)
            {
                StopWallRunning();
                Movement.Jump(true);
            }

        }

        public override void Disable()
        {
            base.Disable();
            StopWallRunning();
        }

        public override bool IsUsing()
        {
            return _isWallRunning;
        }

    }
}

using UnityEngine;
using System.Collections;
using UnityEngine.Events;

#if UNITY_EDITOR
using Topacai.TDebug;
#endif

namespace Topacai.Player.Movement.Components.Wallrunning
{
    public class OnWallRunEventArgs : System.EventArgs { public Wallrunning Component; public bool Starting; public bool Jump; }

    public class Wallrunning : MovementComponent
    {
        public UnityEvent<OnWallRunEventArgs> OnWallRunAction = new();

        [Header("Wall Running wall detection")]
        [Tooltip("Enable auto detection of wall")]
        [SerializeField] private bool _autoDetect = true;
        [Tooltip("Tag to difference between runneable walls and unrunneable walls")]
        [SerializeField] private string _wallTag = "RunneableWall";
        [Tooltip("Layers to check for wall")]
        [SerializeField] private LayerMask _wallMask;
        [Tooltip("Distance to check for wall")]
        [SerializeField] private float _wallCheckDistance = 1f;
        [Tooltip("Minimum distance to ground to attach on wall")]
        [SerializeField] private float _distanceToGround = 1.5f;

        [Space(10)]
        [Header("Wall Running Fall settings")]
        [Tooltip("How long the player could stay on the wall")]
        [SerializeField] private float _duration = 2.5f;
        [Tooltip("Fall curve to apply down force")]
        [SerializeField] private AnimationCurve _fallCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Space(10)]
        [Header("Wall Running movement settings")]
        [Tooltip("During wall running, target speed will be multiplied by this value")]
        [SerializeField] private float _targetSpeedMultiplier = 1.5f;
        [Tooltip("During wall running, acceleration rate will be multiplied by this value")]
        [SerializeField] private float _accelRateMultiplier = 2.33f;
        [Tooltip("Buffer time before player is considered on a wall and still could jump")]
        [SerializeField, Range(0, 1f)] private float _coyoteJumpTime = 0.15f;
        [SerializeField, Range(0, 90f)] private float _maxWallJumpAngle = 45f;
        [Tooltip("Stick distance to wall while wall running (how far to wall the player will be)")]
        [SerializeField] private float _stickDistance = 1f;

#if UNITY_EDITOR
        [Space(10)]
        [Header("Debug")]
        [SerializeField] private bool SHOW_LOGS = false;
        [SerializeField] private bool SHOW_GIZMOS = false;
#endif

        [SerializeField] private bool _isWallRunning = false;

        /// <summary>
        /// Is the player currently running on the right side of the wall, false if on the left or not on a wall
        /// </summary>
        public bool IsRightSide { get => _isRightSide; }
        /// <summary>
        /// The multiplier amount used on the target speed
        /// </summary>
        public float TargetSpeedMultiplier { get => _targetSpeedMultiplier; }
        /// <summary>
        /// The multiplier amount used on the acceleration rate
        /// </summary>
        public float AccelerationRateMultiplier { get => _accelRateMultiplier; }
        /// <summary>
        /// The time the player has been on the wall, 0 if not on a wall
        /// </summary>
        public float WallTime { get => _wallTime; }

        private bool _isRightSide = false;

        private float _wallDelayTimer = 0f;
        private float _wallTime = 0f;
        private RaycastHit _wallHit = new RaycastHit();
        private Collider _lastWallWalked = null;
        private Vector3 runDirection = Vector3.zero;

        private float _lastTimeOnWall = 0f;

        private void Update()
        {
            _wallDelayTimer -= Time.deltaTime;
            if (!_isWallRunning) _lastTimeOnWall -= Time.deltaTime;

#if UNITY_EDITOR
            if (_lastTimeOnWall >= 0 && SHOW_LOGS)
                Debugcanvas.Instance.AddTextToDebugLog("wallrunning", $"jump {_lastTimeOnWall.ToString("0.00")}", 0.05f);
#endif

            bool wantJump = Movement.PlayerBrain.InputHandler.GetActionHandler(Inputs.ActionName.Jump).InstantPress;
            /// Jumps adding a force to the direction of player clamped by angle with the wall
            if (wantJump && _lastTimeOnWall >= 0 && Movement.LastGroundTime < 0)
            {
                StopWallRunning(0f);
                Movement.Jump(true);

                Vector3 oppositeDir = IsRightSide ? Vector3.Cross(runDirection, Vector3.up) : Vector3.Cross(Vector3.up, runDirection);

                Debug.DrawRay(_wallHit.point, oppositeDir * 2f, Color.purple, 1.5f);

                float dot = Vector3.Dot(oppositeDir.normalized, Movement.MoveDir.normalized);
                float angleDeg = Mathf.Acos(dot) * Mathf.Rad2Deg;

                Vector3 jumpDir = Movement.MoveDir;

                if (angleDeg > _maxWallJumpAngle)
                {
                    jumpDir = Quaternion.AngleAxis(IsRightSide ? _maxWallJumpAngle : -_maxWallJumpAngle, Vector3.up) * oppositeDir;
                }

#if UNITY_EDITOR
                if (SHOW_GIZMOS)
                {
                    Debug.DrawRay(_wallHit.point, jumpDir * 3f, Color.yellow, 3f);
                    Debug.DrawRay(_wallHit.point, Movement.MoveDir * 2f, Color.red, 3f);
                }
#endif
                Movement.Rigidbody.AddForce(jumpDir.normalized * Movement.FlatVel.magnitude * 0.33f, ForceMode.Impulse);

                OnWallRunAction?.Invoke(new OnWallRunEventArgs()
                {
                    Component = this,
                    Starting = false,
                    Jump = true
                });
            }
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

        /// <summary>
        /// Auto detection of the walls on right or left side
        /// and updates all fields for wall running
        /// </summary>
        /// <param name="moveDir"></param>
        private void DetectWall(ref Vector3 moveDir)
        {
            /// If the player was on a wall recently, don't detect a new wall
            /// also avoid detecting the previous wall if the player didn't touch the ground before
            /// so the player doesn't get stuck on the wall
            /// Checks first for the left side and then for the right side if on left side there is no wall
        
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

            // Checks for left and then for right side - only when player is not on ground
            bool isHittingWall = false;
            bool rightSide = false;
            if (Movement.LastGroundTime < 0)
            {
                bool groundDistance = Physics.Raycast(transform.position, Vector3.down, _distanceToGround, Movement.DataAsset.GroundLayer);
#if UNITY_EDITOR
                if (SHOW_GIZMOS)
                    Debug.DrawRay(transform.position, Vector3.down * _distanceToGround, groundDistance ? Color.red : Color.green);
#endif

                if (groundDistance)
                {
                    if (_isWallRunning)
                    {
                        StopWallRunning(-1f);
                    }
                    return;
                }

                var crossLeft = Vector3.Cross(moveDir, Vector3.up);
                isHittingWall = CheckWall(crossLeft);

                rightSide = !isHittingWall;
#if UNITY_EDITOR
                if (SHOW_GIZMOS)
                    Debug.DrawRay(Movement.Rigidbody.transform.position, crossLeft.normalized * _wallCheckDistance, isHittingWall ? Color.red : Color.green);
#endif

                if (!isHittingWall)
#if UNITY_EDITOR
                {
                    var crossRight = Vector3.Cross(Vector3.up, moveDir);
                    isHittingWall = CheckWall(crossRight);
                    if (SHOW_GIZMOS)
                        Debug.DrawRay(Movement.Rigidbody.transform.position, crossRight.normalized * _wallCheckDistance, isHittingWall ? Color.red : Color.green);
                }
#else
             CheckWall(Vector3.Cross(Vector3.up, moveDir), out isHittingWall);
#endif
            }


            // Makes sure to not use the previous wall
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

            // Resets lastWall value if the player is on ground
            _lastWallWalked = Movement.LastGroundTime > 0 ? null : _lastWallWalked;

            // Updates the direction and start wall running
            if (isHittingWall && !_isWallRunning)
            {
                Vector3 playerWallRunDir = Vector3.ProjectOnPlane(moveDir, _wallHit.normal);
#if UNITY_EDITOR
                if (SHOW_GIZMOS)
                    Debug.DrawRay(_wallHit.transform.position, playerWallRunDir.normalized, Color.blue, 1f);
#endif
                
                StartWallRunning(playerWallRunDir, _duration, rightSide);
            }
            else if (!isHittingWall && _isWallRunning)
            {
                StopWallRunning(_coyoteJumpTime);
            }

        }

        /// <summary>
        /// Starts the wall running using a direction, duration and optionally pass wall side information
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="duration"></param>
        /// <param name="rightSide"></param>
        public void StartWallRunning(Vector3 dir, float duration, bool rightSide = false)
        {
            _isWallRunning = true;
            _wallTime = 0f;
            _duration = duration;
            _isRightSide = rightSide;
            runDirection = dir;

            _lastTimeOnWall = 0f;

            Movement.Rigidbody.linearVelocity = dir.normalized * Mathf.Clamp(Movement.FlatVel.magnitude, 0, Movement.MaxSpeed * _targetSpeedMultiplier);

            OnWallRunAction?.Invoke(new OnWallRunEventArgs()
            {
                Component = this,
                Starting = true,
                Jump = false
            });
        }

        /// <summary>
        /// Stops the wall running
        /// </summary>
        public void StopWallRunning(float lastTimeBuffer = 0f)
        {
            if (!_isWallRunning) return;

            _wallTime = 0;
            _lastTimeOnWall = lastTimeBuffer;
            Movement.UseGravity(Movement.GravityOn && true);
            _isWallRunning = false;

            OnWallRunAction?.Invoke(new OnWallRunEventArgs()
            {
                Component = this,
                Starting = false,
                Jump = false
            });
        }

        protected override void OnMoveAfterAccel(ref Vector3 targetSpeed, ref float accelRate)
        {
            if(!_isWallRunning) return;

            targetSpeed = runDirection.normalized * targetSpeed.magnitude;

            targetSpeed *= _targetSpeedMultiplier;
            accelRate *= _accelRateMultiplier;
        }

        /// <summary>
        /// Controls the movement while wall running and allow the player to jump
        /// </summary>
        /// <param name="finalForce"></param>
        /// <param name="moveDir"></param>
        protected override void OnBeforeMove(ref Vector3 finalForce, ref Vector3 moveDir)
        {
            bool inConflict = InConflict(_incompatibleStates);

            if (_autoDetect && !inConflict) DetectWall(ref moveDir);

            if (!_isWallRunning || inConflict || Movement.LastGroundTime >= 0)
            {
                StopWallRunning();
                return;
            }

            /// Keep the player at a fixed distance from the wall
            /// defined by _stickDistance
            float distanceToWall = (_wallHit.point - Movement.transform.position).magnitude;
            if (distanceToWall > _stickDistance)
            {
                Vector3 toWallDir = !IsRightSide ? Vector3.Cross(runDirection, Vector3.up) : Vector3.Cross(Vector3.up, runDirection);
                Movement.Rigidbody.position = Vector3.Lerp(Movement.Rigidbody.position, Movement.Rigidbody.position + toWallDir.normalized * _stickDistance, 0.01f);
#if UNITY_EDITOR
                if(SHOW_LOGS)
                    Debugcanvas.Instance.AddTextToDebugLog("wall closing", "", 0.1f);
#endif
            }


#if UNITY_EDITOR
            if (SHOW_LOGS)
                Debugcanvas.Instance.AddTextToDebugLog("wallRunning", _isRightSide ? "Right" : "Left", 0.1f);
#endif

            Movement.UseGravity(!_isWallRunning);

            _wallDelayTimer = 0.5f;

            float fixedTime = _fallCurve.Evaluate(_wallTime / _duration);
            float downForce = fixedTime * 5f;

            Movement.Rigidbody.AddForce(Vector3.down * downForce);

            _wallTime += Time.deltaTime;

            if (fixedTime >= 1f || Vector3.Dot(moveDir, runDirection) < 0.5f)
            {
                StopWallRunning();
                return;
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

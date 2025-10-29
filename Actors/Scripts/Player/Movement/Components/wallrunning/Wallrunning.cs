using EditorAttributes;
using UnityEditor.SearchService;
using UnityEditorInternal;
using UnityEngine;

namespace Topacai.Player.Movement.Components.Wallrunning
{
    public class Wallrunning : MovementComponent
    {
        [Header("Wall Running wall detection")]
        [Tooltip("Enable auto detection of wall")]
        [SerializeField] private bool _autoDetect = true;
        [Tooltip("Tag to difference between runneable walls and unrunneable walls")]
        [SerializeField, EnableField(nameof(_autoDetect))] private string _wallTag = "RunneableWall";
        [Tooltip("Layers to check for wall")]
        [SerializeField, EnableField(nameof(_autoDetect))] private LayerMask _wallMask;
        [Tooltip("Distance to check for wall")]
        [SerializeField, EnableField(nameof(_autoDetect))] private float _wallCheckDistance = 1f;

        [Space(10)]
        [Header("Wall Running Fall settings")]
        [Tooltip("How long the player could stay on the wall")]
        [SerializeField] private float _duration = 2.5f;
        [Tooltip("Fall curve to apply down force")]
        [SerializeField] private AnimationCurve _fallCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Space(10)]
        [Header("Wall Running Run settings")]
        [Tooltip("During wall running, target speed will be multiplied by this value")]
        [SerializeField] private float _targetSpeedMultiplier = 1.5f;
        [Tooltip("During wall running, acceleration rate will be multiplied by this value")]
        [SerializeField] private float _accelRateMultiplier = 2.33f;

        [SerializeField, ReadOnly] private bool _isWallRunning = false;

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

            // Checks for left and then for right side
            var crossLeft = Vector3.Cross(moveDir, Vector3.up);
            bool isHittingWall = CheckWall(crossLeft);

            _isRightSide = !isHittingWall;
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
                Debug.DrawRay(_wallHit.transform.position, playerWallRunDir.normalized, Color.blue, 1f);
#endif

                StartWallRunning(playerWallRunDir, _duration);
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
        }

        /// <summary>
        /// Stops the wall running
        /// </summary>
        public void StopWallRunning()
        {
            _wallTime = 0;
            Movement.UseGravity(Movement.GravityOn && true);
            _isWallRunning = false;
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

            if (!_isWallRunning || inConflict)
            {
                StopWallRunning();
                return;
            }

            Movement.UseGravity(!_isWallRunning);

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

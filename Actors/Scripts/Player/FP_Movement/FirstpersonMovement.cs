using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;
using System.Reflection;
using Topacai.Utils;
using Topacai.CustomPhysics;
using Topacai.Inputs;
using Topacai.Player.Firstperson.Camera;
using Topacai.TDebug;

namespace Topacai.Player.Firstperson.Movement
{
    public class FirstpersonMovement : CustomRigidbody
    {
        public delegate void BeforeWallDetect(ref Vector3 moveDir, ref Vector3 flatVel, ref RaycastHit wallHitInfo);
        public delegate void AfterDefineAccel(ref float accelRate);
        public delegate void BeforeMove(ref Vector3 finalForce, ref Vector3 moveDir);

        public event BeforeWallDetect OnMoveBeforeWall;
        public event AfterDefineAccel OnMoveAfterAccel;
        public event BeforeMove OnBeforeMove;

        [Header("Data")]
        [SerializeField] private FP_MovementAsset _defaultData;

        [field: SerializeField] public FP_MovementAsset Data { private set; get; }

        [Header("Ground Check")]
        [SerializeField] private Transform _groundT;
        [SerializeField] private float _groundSize;

        [Header("Collision Checks")]
        [SerializeField] private Transform wallCheckMidPoint;
        [SerializeField] private float heightToCheckWall;
        [SerializeField] private float distanceFromWall;
        [SerializeField] private float wallSphereRadius;

        [Header("Slope")]
        [SerializeField] private float slopeDistance;
        [Range(0.1f, 0.5f), SerializeField] private float slopeBoxSize = 0.2f;
        [Range(0.001f, 0.1f), SerializeField] private float exitingSlopeTime;
        [SerializeField] private float slopeCrossThreshold = 0.0001f;

        [Header("StepClimb")]
        [SerializeField] private Transform stepStart;
        [SerializeField] private Transform stepHeight;

        [Header("Debug")]
        [SerializeField] private bool GIZMOS = false;
        [SerializeField] private bool ShowDebug = false;
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public bool isJumping { get; private set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public bool isJumpApex { get; private set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public bool isFalling { get; private set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public bool isCrouched { get; private set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public bool jumpCut { get; private set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public bool exitingSlope { get; private set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public bool WasJumpPressed { get; private set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public bool IsJumpPressed { get; private set; }
        /*[field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))]*/
        public float LastGroundTime { get; private set; }
        /*[field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))]*/
        public float LastPressedJump { get; private set; }
        /*[field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))]*/
        public float LastStepTime { get; private set; }
        /*[field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))]*/
        public float LastJumpApex { get; private set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public float _initialPlayerHeight { get; private set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public float _maxSpeed { get; private set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public Vector3 _targetSpeed { get; private set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public bool ClimbingStair { get; private set; }

        public Rigidbody Rigidbody => _rb;
        public Vector3 FlatVel { get { return new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z); } }
        public Vector3 MoveDir { get { return _moveDir; } private set { _moveDir = value; } }
        public Vector3 MoveDirNative { get { return GetMoveDirByCameraAndInput(); } }
        public FP_MovementAsset DataAsset { get { return Data; } private set { Data = value; } }
        public FP_MovementAsset DefaultData { get { return _defaultData; } }

        private Vector3 crouchPivotPos;
        private RaycastHit groundHit;
        private Vector3 _moveDir;

        private float GroundSize => _groundSize * transform.localScale.magnitude;
        private float PlayerHeight => _initialPlayerHeight * transform.localScale.y;
        private bool InGround => LastGroundTime >= 0;
        private Vector3 InputDir => InputHandler.MoveDir;
        private Vector3 WallStartPointUpper => wallCheckMidPoint.position + Vector3.down * (PlayerHeight * 0.5f + heightToCheckWall * transform.localScale.y);
        private Vector3 WallStartPointBottom => wallCheckMidPoint.position + Vector3.down * (PlayerHeight * 0.5f + -heightToCheckWall * transform.localScale.y);

        private void Awake()
        {
            Data = Instantiate(_defaultData);
        }

        void Start()
        {
            _rb = PlayerBrain.Instance.PlayerReferences.Rigidbody;

            _maxSpeed = Data.WalkSpeed;

            _defaultData.OnValuesChanged.AddListener(SyncValuesWithBaseDataMovement);
        }

        private void SyncValuesWithBaseDataMovement()
        {
            if (Data == null) return;

            FieldInfo[] fields = typeof(FP_MovementAsset).GetFields(BindingFlags.Instance | BindingFlags.Public);

            foreach (FieldInfo field in fields)
            {
                object baseValue = field.GetValue(_defaultData);
                field.SetValue(Data, baseValue);
            }

            UnityEngine.Debug.Log("Data changed");
        }
        private void FixedUpdate()
        {
            LastGroundTime -= Time.deltaTime;
            LastPressedJump -= Time.deltaTime;
            LastJumpApex -= Time.deltaTime;

            base.Gravity();

            CheckGround();
            DragControl();
            Movement();
            CrouchInputs();
            ControlGravityScale();
        }
        void Update()
        {
            _rb.mass = Data.RBMass;
            AirCheckers();
            InputHandlers();


            MoveDir = GetMoveDirByCameraAndInput();
        }

        #region Booleans

        private bool CanJump()
        {
            return LastGroundTime > 0 && !isJumping;
        }

        private bool CanJumpCut()
        {
            return isJumping && _rb.linearVelocity.y > 0;
        }

        private bool CanJumpHang()
        {
            return LastJumpApex > 0;
        }

        private bool CanStep(float angle, float dot)
        {
            return angle >= Data.StepMinAngle && dot >= Data.StepDotThreshold;
        }

        private bool ConserveMomentum()
        {
            return Mathf.Abs(_rb.linearVelocity.magnitude) > _targetSpeed.magnitude && (Vector3.Dot(_rb.linearVelocity.normalized, _moveDir) > 0.7f || Vector3.Equals(_moveDir, Vector3.zero)) && !InGround;
        }

        #endregion

        #region Checkers and Gravity

        private void AirCheckers()
        {
            if (isJumping && (_rb.linearVelocity.y < -Data.WhenCancelJumping || _rb.linearVelocity.y <= 0 && InGround))
            {
                isJumping = false;
                LastJumpApex = Data.JumpApexBuffer;
                isJumpApex = true;
            }

            if (isJumpApex && !CanJumpHang())
            {
                isJumpApex = false;
            }

            if (!isJumping && !InGround)
            {
                isFalling = true;
                jumpCut = false;
            }
            else
            {
                isFalling = false;
            }
        }

        private void CheckGround()
        {
            if (Physics.SphereCast(_groundT.position, GroundSize, Vector3.down, out groundHit, GroundSize, Data.GroundLayer))
            {
                LastGroundTime = isJumping ? 0.01f : Data.CoyoteTime;
            }
        }

        private void DragControl()
        {
            if (InGround)
            {
                _rb.linearDamping = Data.GroundDrag;
            }
            else if (!InGround && isFalling)
            {
                _rb.linearDamping = Data.FallDrag;
            }
            else if (!InGround && !isFalling)
            {
                _rb.linearDamping = Data.AirDrag;
            }
        }

        private void ControlGravityScale()
        {
            if (InGround)
            {
                SetGravityScale(Data.GravityScale);
            }
            else if (!InGround && jumpCut && Data.LargeJump)
            {
                SetGravityScale(Data.GravityScale * Data.JumpCutGravityMult);
            }
            else if (!InGround && isJumping)
            {
                SetGravityScale(Data.GravityScale * Data.JumpingGravityMult);
            }
            else if (!InGround && isFalling)
            {
                SetGravityScale(Data.GravityScale * Data.FallingGravityMult);
            }

            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, Mathf.Clamp(_rb.linearVelocity.y, -Data.MaxFallSpeed, Data.MaxFallSpeed), _rb.linearVelocity.z);
        }
        #endregion

        #region Inputs
        private Vector3 GetMoveDirByCameraAndInput()
        {
            Vector3 cameraDir = FirstpersonCamera.CameraDirFlat;

            Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraDir).normalized;
            Vector3 cameraForward = Vector3.Cross(cameraRight, Vector3.up).normalized;

            Vector3 moveDir = cameraForward * InputDir.y + cameraRight * InputDir.x;

            return moveDir.normalized;
        }

        private void JumpInput()
        {
            if (!WasJumpPressed)
            {
                if (InputHandler.IsJumping || InputHandler.JumpPressed || InputHandler.InstantJump)
                {
                    WasJumpPressed = true;
                    LastPressedJump = Data.JumpBufferInput;
                }
            }
            else
            {
                if (InputHandler.JumpPressed || InputHandler.IsJumping || InputHandler.InstantJump) return;

                WasJumpPressed = false;
            }

            IsJumpPressed = (InputHandler.JumpPressed || InputHandler.IsJumping || InputHandler.InstantJump);
        }

        private void RunInput()
        {
            if (Data.CanChangeSpeed)
                SwitchRun(InputHandler.IsRunning);
        }

        private void CrouchInputs()
        {
            if (InputHandler.IsCrouching && !isCrouched)
            {
                Crouch();
            }
            else if (!InputHandler.IsCrouching && isCrouched)
            {
                Crouch();
            }
        }

        private void InputHandlers()
        {
            JumpInput();
            RunInput();

            if (CanJump() && LastPressedJump > 0 && Data.CanJump)
            {
                isJumping = true;
                jumpCut = false;
                exitingSlope = true;
                Invoke(nameof(ResetExitingSlope), exitingSlopeTime);
                Jump();
            }

            if (!IsJumpPressed && CanJumpCut())
            {
                jumpCut = true;
            }
            else if (IsJumpPressed && CanJumpCut())
            {
                jumpCut = false;
            }
        }

        private void ResetExitingSlope() => exitingSlope = false;
        #endregion

        #region Actions
        public void Jump(bool forceJump, float height = -999f, float timeToApex = -999f)
        {
            if (forceJump || CanJump())
            {
                isJumping = true;
                jumpCut = false;
                exitingSlope = true;
                Invoke(nameof(ResetExitingSlope), exitingSlopeTime);
                Jump(height, timeToApex);
            }
        }
        private void Jump(float height = -999f, float timeToApex = -999f)
        {
            if (height < 0)
            {
                height = Data.JumpHeight;
                timeToApex = Data.JumpTimeToApex;
            }
            LastPressedJump = 0;
            LastGroundTime = 0;

            Data.CalculateJumpForce(height, timeToApex);

            float force = Data.JumpForce;
            /*
            if (_rb.velocity.y > 0)
            {
                force -= _rb.velocity.y;
            }
            else if (_rb.velocity.y < 0)
            {
                force += _rb.velocity.y;
            }*/
            ResetFallSpeed();

            _rb.AddForce(Vector3.up * force, ForceMode.Impulse);
        }

        private void SwitchRun(bool run)
        {
            if (run)
            {
                _maxSpeed = Data.RunSpeed;
            }
            else
            {
                _maxSpeed = Data.WalkSpeed;
            }
        }

        private void Crouch()
        {
            RigidbodyInterpolation originalInter = _rb.interpolation;
            CollisionDetectionMode originalDetection = _rb.collisionDetectionMode;
            _rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            _rb.interpolation = RigidbodyInterpolation.None;

            Bounds bounds = GetComponent<Renderer>().bounds;
            crouchPivotPos = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            Vector3 scaleFactor = Vector3.one;

            if (!isCrouched)
            {
                isCrouched = true;
                scaleFactor = new Vector3(1f, 0.5f, 1f);
            }
            else
            {
                isCrouched = false;
                scaleFactor = new Vector3(1f, 2f, 1f);
            }

            Transforms.ScaleRelativeToPivot(transform, scaleFactor, crouchPivotPos);
            _rb.interpolation = originalInter;
            _rb.collisionDetectionMode = originalDetection;
        }
        #endregion

        #region Movement
        private void AntiSlideHandler(Vector3 flatVel)
        {
            if (!Data.CanMove || !(_moveDir.sqrMagnitude > 0.001f)) return;

            float alignment = Vector3.Dot(flatVel.normalized, _moveDir);

            if (alignment < Data.SlideAlignmentThreshold1 && flatVel.magnitude > 0.001f)
            {
                bool isInverted = alignment < -0.9f;

                if (!isInverted)
                {
                    Vector3 lateralVel = flatVel - _moveDir;
                    _rb.AddForce(-lateralVel * Data.BrakeForceMultiplier, ForceMode.Force);
                }
                else
                {
                    _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
                }
            }
        }

        private void AntiSlideHandler2(Vector3 flatVel)
        {
            if (!Data.CanMove || !(_moveDir.sqrMagnitude > 0.001f)) return;

            if (!InGround || isJumping)
            {
                DragControl();
                return;
            }

            flatVel = flatVel.sqrMagnitude > 0.001f ? flatVel.normalized : Vector3.zero;

            float alignment = Vector3.Dot(flatVel.normalized, _moveDir);

            float dynamicDrag = Data.GroundDrag;
            if (alignment < Data.SlideAlignmentThreshold2)
            {
                float factor = Mathf.InverseLerp(0.0f, 0.8f, alignment);

                dynamicDrag = Mathf.Lerp(Data.HighBrakeDrag, Data.GroundDrag, factor);
            }

            _rb.linearDamping = dynamicDrag;
        }

        private void StepClimbHandler()
        {
            LastStepTime -= Time.deltaTime;
            if (!InGround || LastStepTime > 0) return;

            #region Step Up
            RaycastHit stepRayHit;

            float stepForwadDistance = OnSlope() ? Data.StepDistance * Data.OnSlopeStepDistanceMultiplier : Data.StepDistance;

            bool stepUpHit = Physics.BoxCast(stepStart.position, new Vector3(slopeBoxSize, 0.1f, slopeBoxSize), _moveDir, out stepRayHit, Quaternion.identity, stepForwadDistance, Data.GroundLayer);
            if (stepUpHit)
            {
                // Get angle of the normal of possible step, and check if it's a valid step relative to the player direction and angle
                float angle = Vector3.Angle(Vector3.up, stepRayHit.normal);
                Vector3 toPlayer = (new Vector3(transform.position.x, stepRayHit.point.y, transform.position.z) - stepRayHit.point).normalized;
                float dot = Vector3.Dot(toPlayer, stepRayHit.normal);

                if (!CanStep(angle, dot)) return;

                bool stepDepth = Physics.Raycast(stepHeight.position, _moveDir, Data.StepDepth, Data.GroundLayer);

                if (!stepDepth)
                {
                    _rb.AddForce(Vector3.up * Data.StepUpForce, ForceMode.VelocityChange);
                    _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, Mathf.Clamp(_rb.linearVelocity.y, 0f, Data.StepClampVel), _rb.linearVelocity.z);
                    LastStepTime = Data.StepBufferTime;
                    ClimbingStair = true;
                }
                else if (ClimbingStair)
                {
                    ClimbingStair = false;
                    //if (!isJumping)
                    //  ResetFallSpeed();
                }

#if UNITY_EDITOR
                UnityEngine.Debug.DrawRay(stepRayHit.point, stepRayHit.normal, Color.gray, 2f);
                UnityEngine.Debug.DrawRay(stepRayHit.point, toPlayer, Color.green, 2f);
#endif
            }
            else if (ClimbingStair)
            {
                ClimbingStair = false;
                //if (!isJumping)
                //  ResetFallSpeed();
            }

#if UNITY_EDITOR
            UnityEngine.Debug.DrawRay(stepStart.position, _moveDir * stepForwadDistance, Color.gray);
            UnityEngine.Debug.DrawRay(stepHeight.position, _moveDir * Data.StepDepth, Color.yellow);
#endif

            #endregion

            if (stepUpHit || OnSlope() || LastGroundTime < -Data.StepDownLastGroundThreshold) return;

#if UNITY_EDITOR
            UnityEngine.Debug.DrawRay(stepStart.position, Vector3.down * Data.StepDownDistance, Color.gray);
#endif

            bool stepDownHit = Physics.BoxCast(stepStart.position - Vector3.up * Data.StepDownOffset, new Vector3(0.05f, 0.05f, 0.05f), -_moveDir, out stepRayHit, Quaternion.identity, Data.StepDownDistance, Data.GroundLayer);
            if (stepDownHit)
            {
                UnityEngine.Debug.Log("Down Hit");
                float angle = Vector3.Angle(Vector3.up, stepRayHit.normal);
                Vector3 toPlayer = (new Vector3(transform.position.x, stepRayHit.point.y, transform.position.z) - stepRayHit.point).normalized;
                float dot = Vector3.Dot(toPlayer, stepRayHit.normal);

                if (!CanStep(angle, dot)) return;
                UnityEngine.Debug.Log("Down Step2");

                if (Mathf.Abs(angle - 90f) <= 3f)
                {
                    UnityEngine.Debug.Log("Down Step");
                    _rb.AddForce(Vector3.up * -Data.StepDownForce, ForceMode.Force);
                }
            }
        }

        private void Movement()
        {
            Data.CalculateDynamicValues(_maxSpeed);

            #region Vectores

            _moveDir = Data.CanMove ? MoveDir : Vector2.zero;

            Vector3 flatVel = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);

            bool onSlope = OnSlope();

            if (onSlope && !exitingSlope)
                _moveDir = GetSlopeMoveDirection();

            _targetSpeed = _moveDir * _maxSpeed;
            _targetSpeed = Vector3.Lerp(flatVel, _targetSpeed, 1);

            #endregion

#if UNITY_EDITOR
            //Debug.DrawRay(transform.position + Vector3.up * 1.1f, flatVel.normalized, Color.cyan, 1f);
#endif


            bool inputIsMoving = _moveDir.magnitude > 0;

            #region WallCollision
            RaycastHit wallHitInfo = new RaycastHit();

            bool wallHit = false;
            if (inputIsMoving)
                wallHit = Physics.CapsuleCast(WallStartPointUpper, WallStartPointBottom, wallSphereRadius, _moveDir, out wallHitInfo, distanceFromWall, Data.WallLayer);

            if (wallHit)
            {
                if (_moveDir.magnitude > 0)
                {
                    Vector3 wallDir = wallHitInfo.normal;
                    wallDir.y = 0;

                    Vector3 invertedWall = Quaternion.AngleAxis(180f, Vector3.up) * wallDir.normalized;
                    Vector3 moveDirRotated = Quaternion.AngleAxis(90f, invertedWall) * _moveDir;
                    Vector3 cross = Vector3.Cross(wallDir.normalized, moveDirRotated.normalized);

                    float dirAngle = Vector3.Angle(invertedWall, moveDirRotated);

                    if (Data.WallMinAngleToMove < dirAngle)
                    {
                        _moveDir = cross.normalized;
                        _targetSpeed = (_moveDir * _maxSpeed) * Mathf.Clamp(dirAngle / 89f, 0.2f, 1f);
                    }
                    else
                    {
                        _moveDir = Vector2.zero;
                        _targetSpeed = _moveDir * _maxSpeed;
                    }

#if UNITY_EDITOR
                    UnityEngine.Debug.DrawRay(transform.position + Vector3.up * 1f, wallDir.normalized, Color.red);
                    UnityEngine.Debug.DrawRay(transform.position + Vector3.up * 1f, cross.normalized, Color.black);
                    UnityEngine.Debug.DrawRay(transform.position + Vector3.up * 1f, moveDirRotated, Color.white);
#endif
                }
            }
            #endregion

            #region Acceleration And Speed

            float accelRate;
            bool desaccel = Vector3.Dot(flatVel.normalized, _moveDir) < -0.75f || _targetSpeed.magnitude == 0f;

            accelRate = desaccel ? Data.decelerationAmount : Data.accelerationAmount;

            if (!InGround && Data.AirMovement)
            {
                accelRate = desaccel ? Data.decelerationAmount * Data.AirDecelMult : Data.accelerationAmount * Data.AirAccelMult;
                _targetSpeed = _targetSpeed * Data.AirMaxSpeedMult;
            }

            if (CanJumpHang() && Data.JumpHang)
            {
                _targetSpeed *= Data.JumpHandMaxSpeed;
                accelRate *= Data.JumpHangAccelMult;
            }

            if (Data.ConserveMomentumOnAir && ConserveMomentum())
            {
                accelRate = 0f;
            }

            OnMoveAfterAccel?.Invoke(ref accelRate);

            Vector3 speedDif = _targetSpeed - flatVel;
            Vector3 movementForce = speedDif * accelRate;

            #endregion

            #region SlopeGroundHandler

            if (onSlope)
            {
                movementForce *= Data.SlopeSpeedMultiplier;

                Vector3 downDir = groundHit.normal.normalized * -1f;
#if UNITY_EDITOR
                UnityEngine.Debug.DrawRay(transform.position + Vector3.up * 1f, downDir * 2f, Color.white);
#endif
                if (flatVel.sqrMagnitude > 0.01f)
                    _rb.AddForce(Vector3.down * Data.SlopeDownForce, ForceMode.Force);
            }

            #endregion


            if (Data.StepClimb)
                StepClimbHandler();


            Vector3 appliedForce = movementForce;

            float mf = movementForce.magnitude;

            OnBeforeMove?.Invoke(ref appliedForce, ref _moveDir);

            if (InGround)
            {
                if (!onSlope)
                    ResetFallSpeed();

                if (Data.AntiSlideMethod1)
                    AntiSlideHandler(flatVel);
                if (Data.AntiSlideMethod2)
                    AntiSlideHandler2(flatVel);
            }
            if (InGround || (!InGround && Data.AirMovement))
            {
                _rb.AddForce(appliedForce, ForceMode.Force);
            }


            base.UseGravity(!onSlope);

#if UNITY_EDITOR
            UnityEngine.Debug.DrawLine(transform.position, transform.position + appliedForce.normalized * 5f, Color.yellow);
            UnityEngine.Debug.DrawLine(transform.position, transform.position + MoveDir.normalized * 3f, Color.magenta);

            Debugcanvas.Instance.AddTextToDebugLog("targetSpeed: ", _targetSpeed.ToString("0.0"));
            Debugcanvas.Instance.AddTextToDebugLog("Conserve momentum: ", ConserveMomentum().ToString());
            Debugcanvas.Instance.AddTextToDebugLog("Movement: ", movementForce.ToString("0.0"));
            Debugcanvas.Instance.AddTextToDebugLog("speedDif: ", speedDif.ToString("0.0"));
            Debugcanvas.Instance.AddTextToDebugLog("AccelRate: ", accelRate.ToString("0.0"));
#endif
        }
        #endregion

        #region Slope
        private bool OnSlope()
        {
            if (!Data.SlopeDetection) return false;

            float angle = Vector3.Angle(Vector3.up, groundHit.normal);
            return angle < Data.MaxSlopeAngle && angle >= Data.MinSlopeAngle;
        }

        private Vector3 GetSlopeMoveDirection()
        {
            if (!Data.SlopeDetection) return _moveDir;

            Vector3 normalSlope = groundHit.normal;
            Vector3 cross = Vector3.Cross(Quaternion.AngleAxis(90f, Vector3.up) * _moveDir, normalSlope);

#if UNITY_EDITOR
            UnityEngine.Debug.DrawRay(transform.position + Vector3.up * 1f, cross, Color.black);
            UnityEngine.Debug.DrawRay(transform.position + Vector3.up * 1f, Quaternion.AngleAxis(90f, Vector3.up) * _moveDir, Color.green);
            UnityEngine.Debug.DrawRay(transform.position + Vector3.up * 1f, normalSlope, Color.red);
#endif

            return cross.normalized;
        }
        #endregion


#if UNITY_EDITOR
        #region Gizmos

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(crouchPivotPos, Vector3.one * 0.1f);
            if (!GIZMOS) return;
            if (_groundT != null)
            {
                if (InGround)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireCube(groundHit.point, Vector3.one * 0.1f);
                }
                Gizmos.color = InGround ? Color.blue : Color.red;
                Gizmos.DrawWireSphere(_groundT.position + Vector3.down * GroundSize, GroundSize);
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(WallStartPointUpper + _moveDir * distanceFromWall, wallSphereRadius);
            Gizmos.DrawWireSphere(WallStartPointBottom + _moveDir * distanceFromWall, wallSphereRadius);
        }
        #endregion
#endif
    }
}


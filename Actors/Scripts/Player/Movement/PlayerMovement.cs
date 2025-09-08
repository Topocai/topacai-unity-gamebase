
using UnityEngine;
using EditorAttributes;
using System.Reflection;
using Topacai.Utils;
using Topacai.CustomPhysics;
using Topacai.Inputs;
using Topacai.TDebug;
using System;

namespace Topacai.Player.Movement
{
    public class PlayerMovement : CustomRigidbody
    {
        public delegate void BeforeWallDetect(ref Vector3 moveDir, ref Vector3 flatVel, ref RaycastHit wallHitInfo);
        public delegate void AfterDefineAccel(ref float accelRate);
        public delegate void BeforeMove(ref Vector3 finalForce, ref Vector3 moveDir);

        public event BeforeWallDetect OnMoveBeforeWall;
        public event AfterDefineAccel OnMoveAfterAccel;
        public event BeforeMove OnBeforeMove;

        [Header("Data")]
        [Tooltip("Sets here the data asset that the movement will use, this will be copied during runtime in order to keep runtime changes during gameplay, also used this to revert any runtime change in data")]
        [SerializeField] protected MovementSO _defaultData;

        [Tooltip("The data asset that will be used in runtime, when the monobehaviour starts the default data will be copied here")]
        [field: SerializeField] public MovementSO Data { protected set; get; }

        [Header("Ground Check")]
        [Tooltip("The transform that will be used to check if the player is on ground")]
        [SerializeField] protected Transform _groundT;
        [Tooltip("Size of the sphere ground check")]
        [SerializeField] protected float _groundSize = 0.18f;

        [Header("Collision Checks")]
        [Tooltip("Wall detection is performed using a capsule cast, this transform is used as a center in Y axys to start the cast")]
        [Clamp(-10f, 0.05f), SerializeField] private Transform wallCheckMidPoint;
        [Tooltip("How tall the capsule will be. Put negative values")]
        [SerializeField] protected float heightToCheckWall = -0.42f;
        [Tooltip("How far away from player the collision will be detected")]
        [SerializeField] protected float distanceFromWall = 0.14f;
        [Tooltip("Radious size of capsule")]
        [SerializeField] protected float wallSphereRadius = 0.3f;

        [Header("Slope")]
        [Tooltip("Short amount of time that is used as threshold to determine if player is trying to exit from slope (i.e jump)")]
        [Range(0.001f, 0.1f), SerializeField] private float exitingSlopeTime = 0.07f;

        [Header("StepClimb")]
        [Tooltip("The transform used to check Y height of the bottom step")]
        [SerializeField] protected Transform stepStart;
        [Tooltip("The transform used to check Y height of the top step (how high the player can step)")]
        [SerializeField] protected Transform stepHeight;
        [Tooltip("The check for step is used as a rectangle-form, this is the size of the rectangle")]
        [Range(0.1f, 0.5f), SerializeField] private float stepBoxSize = 0.2f;

        [Header("Debug")]
        [SerializeField] protected bool GIZMOS = false;
        [SerializeField] protected bool ShowDebug = false;
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public bool isJumping { get; protected set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public bool isJumpApex { get; protected set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public bool isFalling { get; protected set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public bool isCrouched { get; protected set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public bool jumpCut { get; protected set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public bool exitingSlope { get; protected set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public bool WasJumpPressed { get; protected set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public bool IsJumpPressed { get; protected set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public bool GroundIsTerrain { get; protected set; }
        /*[field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))]*/
        public float LastGroundTime { get; protected set; }
        /*[field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))]*/
        public float LastPressedJump { get; protected set; }
        /*[field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))]*/
        public float LastStepTime { get; protected set; }
        /*[field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))]*/
        public float LastJumpApex { get; protected set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public float _initialPlayerHeight { get; protected set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public float _maxSpeed { get; protected set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public Vector3 _targetSpeed { get; protected set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(ShowDebug))] public bool ClimbingStair { get; protected set; }

        public Rigidbody Rigidbody => _rb;
        public Vector3 FlatVel { get { return new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z); } }
        public Vector3 MoveDir { get { return _moveDir; } protected set { _moveDir = value; } }
        public Vector3 MoveDirNative { get { return GetMoveDirByCameraAndInput(); } }
        public MovementSO DataAsset { get { return Data; } protected set { Data = value; } }
        public MovementSO DefaultData { get { return _defaultData; } }

        protected Vector3 crouchPivotPos;
        protected Collider _lastGroundHit;
        protected RaycastHit groundHit;
        protected Vector3 _moveDir;
        protected Collider _lastStep = default;

        protected float _accelerationAmount;
        protected float _decelerationAmount;
        protected float _gravityScale = 1f;
        protected float _jumpForce;

        protected SimpleActionHandler _jumpInput;
        protected SimpleActionHandler _crouchInput;
        protected SimpleActionHandler _sprintInput;

        protected float GroundSize => _groundSize * transform.localScale.magnitude;
        protected float PlayerHeight => _initialPlayerHeight * transform.localScale.y;
        protected bool InGround => LastGroundTime >= 0;
        protected Vector3 InputDir => InputHandler.MoveDir;
        protected Vector3 WallStartPointUpper => wallCheckMidPoint.position + Vector3.down * (PlayerHeight * 0.5f + heightToCheckWall * transform.localScale.y);
        protected Vector3 WallStartPointBottom => wallCheckMidPoint.position + Vector3.down * (PlayerHeight * 0.5f + -heightToCheckWall * transform.localScale.y);

        protected virtual void Awake()
        {
            Data = Instantiate(_defaultData);
        }

        protected virtual void Start()
        {
            PlayerBrain.Instance.PlayerReferences.Rigidbody = _rb;
            _maxSpeed = Data.WalkSpeed;

            float[] jumpData = _defaultData.CalculateJumpForce(_defaultData.JumpHeight, _defaultData.JumpTimeToApex, customGravity.y);
            _jumpForce = jumpData[0];
            _gravityScale = jumpData[1];

            _defaultData.OnValuesChanged.AddListener(SyncValuesWithBaseDataMovement);

            _jumpInput = InputHandler.GetActionHandler(ActionName.Jump);
            _crouchInput = InputHandler.GetActionHandler(ActionName.Crouch);
            _sprintInput = InputHandler.GetActionHandler(ActionName.Run);
        }

        /// <summary>
        /// If the default data asset is changed during runtime this keeps the in-runtime data copy syncronized with it
        /// </summary>
        protected void SyncValuesWithBaseDataMovement()
        {
            if (Data == null) return;

            FieldInfo[] fields = typeof(MovementSO).GetFields(BindingFlags.Instance | BindingFlags.Public);

            foreach (FieldInfo field in fields)
            {
                object baseValue = field.GetValue(_defaultData);
                field.SetValue(Data, baseValue);
            }

            Debug.Log("Data changed");
        }
        protected virtual void FixedUpdate()
        {
            LastGroundTime -= Time.deltaTime;
            LastPressedJump -= Time.deltaTime;
            LastJumpApex -= Time.deltaTime;

            base.Gravity();

            CheckGround();
            DragControl();
            if (!Data.FreezeMove)
                Movement();
            CrouchInputs();
            ControlGravityScale();
        }
        protected virtual void Update()
        {
            _rb.mass = Data.RBMass;
            AirCheckers();
            InputHandlers();


            MoveDir = GetMoveDirByCameraAndInput();
        }

        #region Booleans

        protected virtual bool CanJump()
        {
            return LastGroundTime > 0 && !isJumping;
        }

        protected virtual bool CanJumpCut()
        {
            return isJumping && Math.Abs(_rb.linearVelocity.y) > 0;
        }

        protected virtual bool CanJumpHang()
        {
            return LastJumpApex > 0;
        }

        protected virtual bool CanStep(float angle, float dot)
        {
            return angle >= Data.StepMinAngle && dot >= Data.StepDotThreshold;
        }

        protected virtual bool ConserveMomentum()
        {
            return Mathf.Abs(_rb.linearVelocity.magnitude) > _targetSpeed.magnitude && (Vector3.Dot(_rb.linearVelocity.normalized, _moveDir) > 0.7f || Vector3.Equals(_moveDir, Vector3.zero)) && !InGround;
        }

        #endregion

        #region Checkers and Gravity

        protected void AirCheckers()
        {
            if (isJumping && Mathf.Abs(_rb.linearVelocity.y) < Data.WhenCancelJumping)
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

        protected void CheckGround()
        {
            if (Physics.SphereCast(_groundT.position, GroundSize, Vector3.down, out groundHit, GroundSize, Data.GroundLayer))
            {
                LastGroundTime = isJumping ? 0.01f : Data.CoyoteTime;
                if (_lastGroundHit != groundHit.collider)
                {
                    _lastGroundHit = groundHit.collider;

                    var terrain = groundHit.collider.GetComponent<TerrainCollider>();
                    GroundIsTerrain = terrain != null;
                }
            }
        }

        protected void DragControl()
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

        protected void ControlGravityScale()
        {
            if (InGround && !isJumping)
            {
                SetGravityScale(Data.GroundGravityScale);
            }
            else if (!InGround && jumpCut && Data.LargeJump)
            {
                SetGravityScale(_gravityScale * Data.JumpCutGravityMult);
            }
            else if (isJumping)
            {
                SetGravityScale(_gravityScale * Data.JumpingGravityMult);
            }
            else if (!InGround && isFalling)
            {
                SetGravityScale(_gravityScale * Data.FallingGravityMult);
            }

            if (Data.LimitFallSpeed)
                _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, Mathf.Clamp(_rb.linearVelocity.y, -Data.MaxFallSpeed, Data.MaxFallSpeed), _rb.linearVelocity.z);
        }
        #endregion

        #region Inputs
        protected virtual Vector3 GetMoveDirByCameraAndInput()
        {
            /*
            Vector3 cameraDir = FirstpersonCamera.CameraDirFlat;

            Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraDir).normalized;
            Vector3 cameraForward = Vector3.Cross(cameraRight, Vector3.up).normalized;

            Vector3 moveDir = cameraForward * InputDir.y + cameraRight * InputDir.x;

            return moveDir.normalized;*/

            Transform cameraTransform = Camera.main.transform;
            Vector3 dir = cameraTransform.forward * InputDir.y + cameraTransform.right * InputDir.x;
            dir.y = 0;

            return dir.normalized;
        }

        protected virtual void JumpInput()
        {
            _jumpInput.Update(Time.deltaTime);
            if (!WasJumpPressed && (_jumpInput.InstantPress || _jumpInput.IsPressed))
            {
                WasJumpPressed = true;
                LastPressedJump = Data.JumpBufferInput;
            }
            else
            {
                WasJumpPressed = false;
            }

            IsJumpPressed = _jumpInput.All;
        }

        protected virtual void RunInput()
        {
            if (Data.CanChangeSpeed)
                SwitchRun(_sprintInput.IsPressing);
        }

        protected virtual void CrouchInputs()
        {
            if (_crouchInput.IsPressing && !isCrouched)
            {
                Crouch();
            }
            else if (!_crouchInput.IsPressing && isCrouched)
            {
                Crouch();
            }
        }

        protected virtual void InputHandlers()
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

        protected void ResetExitingSlope() => exitingSlope = false;
#endregion

        #region Actions
        public virtual void Jump(bool forceJump, float height = -999f, float timeToApex = -999f)
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

        protected virtual void Jump(float height = -999f, float timeToApex = -999f)
        {
            if (height < 0)
            {
                height = Data.JumpHeight;
                timeToApex = Data.JumpTimeToApex;
            }
            LastPressedJump = 0;
            LastGroundTime = 0;

            float[] jumpData = Data.CalculateJumpForce(height, timeToApex, customGravity.y);

            _jumpForce = jumpData[0];
            _gravityScale = jumpData[1];
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

            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }

        protected virtual void SwitchRun(bool run)
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

        protected virtual void Crouch()
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

        protected void StepClimbHandler()
        {
            LastStepTime -= Time.deltaTime;
            if (isJumping || LastStepTime > 0 || GroundIsTerrain) return;

            #region Step Up
            // Step distance is increased if the player is on slope
            float stepDistance = OnSlope() ? Data.StepDistance * Data.OnSlopeStepDistanceMultiplier : Data.StepDistance;

            // Detect if player has a step in direction of movement and then calculate if is a valid step.
            // Because is only a simple raycast keep in mind that all steps colliders has to be at the same height of the check pos
            // a small offset is added backwards to detect steps that are too close to player and got stucked in border steps.
            // also, step detection is affected by slope so, is perfecly possible to step up being in a slope
            // MAKE SURE to put a different layer player and ground.
            RaycastHit stepRayHit;
            Vector3 stepDir = _moveDir;

            bool stepUpHit = Physics.Raycast(stepStart.position + _moveDir.normalized * -0.075f, stepDir, out stepRayHit, stepDistance, Data.GroundLayer);

            if (_moveDir.magnitude > 0 && stepUpHit)
            {
                // Get angle of the normal of possible step, and check if it's a valid step relative to the player direction and angle
                float stepAngle = Vector3.Angle(Vector3.up, stepRayHit.normal);
                Vector3 toPlayer = (new Vector3(transform.position.x, stepRayHit.point.y, transform.position.z) - stepRayHit.point).normalized;
                float dot = Vector3.Dot(toPlayer, stepRayHit.normal);

#if UNITY_EDITOR
                Debug.DrawRay(stepRayHit.point, stepRayHit.normal, Color.gray, 0.1f);
                Debug.DrawRay(stepRayHit.point, toPlayer, Color.green, 0.1f);
#endif

                if (!CanStep(stepAngle, dot) && _lastStep != stepRayHit.collider)
                {
                    return;
                }

                bool stepDepth = Physics.Raycast(stepHeight.position, _moveDir, Data.StepDepth, Data.GroundLayer);

                if (!stepDepth)
                {
                    _rb.AddForce(Vector3.up * Data.StepUpForce, ForceMode.VelocityChange);
                    _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, Mathf.Clamp(_rb.linearVelocity.y, 0f, Data.StepClampVel), _rb.linearVelocity.z);
                    LastStepTime = Data.StepBufferTime;
                    ClimbingStair = true;
                    _lastStep = stepRayHit.collider;
                }
                else if (ClimbingStair)
                {
                    ClimbingStair = false;
                    _lastStep = null;
                    if (!isJumping)
                        ResetFallSpeed();
                }
            }
            else if (ClimbingStair)
            {
                ClimbingStair = false;
                _lastStep = null;
                if (!isJumping)
                    ResetFallSpeed();
            }

#if UNITY_EDITOR
            Debug.DrawRay(stepStart.position, _moveDir * stepDistance, Color.gray);
            Debug.DrawRay(stepHeight.position, _moveDir * Data.StepDepth, Color.yellow);
#endif

            #endregion

            #region Step down

            if (stepUpHit || OnSlope() || LastGroundTime < -Data.StepDownGroundBuffer || isJumping || isJumpApex) return;

            // Step down system - Keep player on ground if is walking down in steps.
            // Check if player has ground under it with a minimum distance to detect the step, then, apply a down force.
            // use castall to make sure to not detect collider that is above the ground player is walking on.

            // This works a little weird, maybe better to implement it in a different way.

            RaycastHit[] stepDownHits = new RaycastHit[1];
            Vector3 downHalfExtents = new Vector3(stepBoxSize * 0.2f, 0.1f, stepBoxSize * 0.2f);
            Vector3 stepDownStart = stepStart.position - Vector3.up * Data.StepDownOffset;

            stepDownHits = Physics.BoxCastAll(stepDownStart, downHalfExtents, Vector3.down, Quaternion.LookRotation(Vector3.down), (stepHeight.position.y - stepStart.position.y) + Data.StepDownDistance, Data.GroundLayer);

            bool isStepDownHit = stepDownHits.Length > 0;
            if (isStepDownHit)
            {
                //Check if the normal of ground is aproximate pointing to and upwards.

                // If the detected step is not the same as the ground player is standing on, return because the collider detected is above the ground.
                RaycastHit stepDownHit = stepDownHits[0];
                if (InGround && stepDownHit.collider != groundHit.collider) return;

                float distanceFromOrigin = Vector3.Distance(stepDownStart, stepDownHit.point);
                if (distanceFromOrigin < Data.StepDownMinDistance) return;

                Vector3 toPlayer = (transform.position - stepDownHit.point).normalized;
                float dot = Vector3.Dot(toPlayer, stepDownHit.normal);

#if UNITY_EDITOR
                Debug.DrawRay(stepDownHit.point, stepDownHit.normal, Color.gray, 0.1f);
                Debug.DrawRay(stepDownHit.point, toPlayer, Color.green, 0.1f);
#endif
                if (dot < 0.50f) return;

                float angle = Vector3.Angle(Vector3.forward, stepDownHit.normal);

                if (Mathf.Abs(angle - 90f) <= 35f) // is looking upwards
                {
                    _rb.AddForce(Vector3.up * -Data.StepDownForce, ForceMode.Force);
                }
            }

            #endregion
        }

        protected void Movement()
        {
            var dynamicValues = Data.CalculateDynamicValues(_maxSpeed);
            _accelerationAmount = dynamicValues[0];
            _decelerationAmount = dynamicValues[1];

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

            #region Wall detection and redirection

            // Check if the player is colliding with a wall in the direction of movement with a capsule cast
            RaycastHit wallHitInfo = new RaycastHit();
            bool wallHit = false;
            if (inputIsMoving)
            {
                // Limit the height where the wall is checked if is on ground at step height.
                // On air keeps the correct height to avoid get stuck moving to wall
                // On ground allow player to climb higher step heights.
                Vector3 bottomCheck = WallStartPointBottom;
                if (InGround) bottomCheck.y = Mathf.Clamp(bottomCheck.y, stepHeight.position.y + wallSphereRadius, 10);

                wallHit = Physics.CapsuleCast(WallStartPointUpper, bottomCheck, wallSphereRadius, _moveDir, out wallHitInfo, distanceFromWall, Data.WallLayer);
            }

            if (wallHit)
            {
                // By getting the wall normal direction, transforms it to get te opposite direction
                // then invert the movement direction vertically in direction of the wall
                // and finally get the cross product between the wall normal and the inverted movement direction in order to get the new movement direction
                //
                // Before using the new movement direction, we check if the angle between the wall normal and the inverted movement direction is greater than the minimum angle to move (that means the player is moving diagonally to the wall)
                // and apply that direction with a multiplier between 0.2f and 1f that depends on the previous calculated angle. (the more perpendicular is player moving to wall, more slower the movement is)
                Vector3 wallDir = wallHitInfo.normal;
                wallDir.y = 0;

                Vector3 invertedWall = Quaternion.AngleAxis(180f, Vector3.up) * wallDir.normalized; // vector from the player looking to wall
                Vector3 moveDirRotated = Quaternion.AngleAxis(90f, invertedWall) * _moveDir; // vector of player movement direction but rotated in order to get horizontally direction as vertically (right is up and left is down)
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
                Debug.DrawRay(transform.position + Vector3.up * 1f, wallDir.normalized, Color.red);
                Debug.DrawRay(transform.position + Vector3.up * 1f, cross.normalized, Color.black);
                Debug.DrawRay(transform.position + Vector3.up * 1f, moveDirRotated, Color.white);
#endif
            }
            #endregion

            #region Acceleration And Speed

            // Gets the acceleration based in if the player is moving or changing direction (acceleration and desacceleration)
            // and if the player is on the ground or not,
            // also conserves momentum by not applying desacceleration if the player is at higher speeds and moving in the same direction or not moving (only on air)
            //
            // A higher force will be calculated if the desired/target speed is distant from the current speed
            float accelRate;
            bool desaccel = Vector3.Dot(flatVel.normalized, _moveDir) < -0.75f || _targetSpeed.magnitude == 0f;

            accelRate = desaccel ? _decelerationAmount : _accelerationAmount;

            if (!InGround && Data.AirMovement)
            {
                accelRate = desaccel ? _decelerationAmount * Data.AirDecelMult : _accelerationAmount * Data.AirAccelMult;
                _targetSpeed = _targetSpeed * Data.AirMaxSpeedMult;
            }

            if (CanJumpHang() && Data.JumpHang)
            {
                Debugcanvas.Instance.AddTextToDebugLog("jumping apex", " ", 0.1f);
                _targetSpeed *= Data.JumpHandMaxSpeed;
                accelRate *= Data.JumpHangAccelMult;
            }

            if (Data.ConserveMomentumOnAir && ConserveMomentum())
            {
                accelRate = 0f;
            }

            // Event for movement components
            OnMoveAfterAccel?.Invoke(ref accelRate);

            // The speed difference between the desired speed and the current speed is calculated without the Y component to avoid affect the vertical/fall speed
            Vector3 speedDif = _targetSpeed - flatVel;
            Vector3 movementForce = speedDif * accelRate;

            #endregion

            #region SlopeGroundHandler

            if (onSlope)
            {
                movementForce *= Data.SlopeSpeedMultiplier;

                Vector3 downDir = groundHit.normal.normalized * -1f;
#if UNITY_EDITOR
                Debug.DrawRay(transform.position + Vector3.up * 1f, downDir * 2f, Color.white);
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
            }
            if (InGround || (!InGround && Data.AirMovement))
            {
                _rb.AddForce(appliedForce, ForceMode.Force);
            }


            base.UseGravity(!onSlope);

#if UNITY_EDITOR
            Debug.DrawLine(transform.position, transform.position + appliedForce.normalized * 5f, Color.yellow);
            Debug.DrawLine(transform.position, transform.position + MoveDir.normalized * 3f, Color.magenta);

            Debugcanvas.Instance.AddTextToDebugLog("targetSpeed: ", _targetSpeed.ToString("0.0"));
            Debugcanvas.Instance.AddTextToDebugLog("movedir2: ", _moveDir.ToString("0.0"));
            Debugcanvas.Instance.AddTextToDebugLog("Conserve momentum: ", ConserveMomentum().ToString());
            Debugcanvas.Instance.AddTextToDebugLog("Movement: ", movementForce.ToString("0.0"));
            Debugcanvas.Instance.AddTextToDebugLog("speedDif: ", speedDif.ToString("0.0"));
            Debugcanvas.Instance.AddTextToDebugLog("AccelRate: ", accelRate.ToString("0.0"));
#endif
        }
        #endregion

        #region Slope
        protected bool OnSlope()
        {
            if (!Data.SlopeDetection) return false;

            float angle = Vector3.Angle(Vector3.up, groundHit.normal);
            return angle < Data.MaxSlopeAngle && angle >= Data.MinSlopeAngle;
        }

        protected Vector3 GetSlopeMoveDirection()
        {
            if (!Data.SlopeDetection) return _moveDir;

            Vector3 normalSlope = groundHit.normal;
            Vector3 cross = Vector3.Cross(Quaternion.AngleAxis(90f, Vector3.up) * _moveDir, normalSlope);

#if UNITY_EDITOR
            Debug.DrawRay(transform.position + Vector3.up * 1f, cross, Color.black);
            Debug.DrawRay(transform.position + Vector3.up * 1f, Quaternion.AngleAxis(90f, Vector3.up) * _moveDir, Color.green);
            Debug.DrawRay(transform.position + Vector3.up * 1f, normalSlope, Color.red);
#endif

            return cross.normalized;
        }
        #endregion

#if UNITY_EDITOR
        #region Gizmos

        protected virtual void OnDrawGizmos()
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

            Vector3 bottomCheck = WallStartPointBottom + _moveDir * distanceFromWall;
            if (InGround) bottomCheck.y = Mathf.Clamp(bottomCheck.y, stepHeight.position.y + wallSphereRadius, 10);
            Gizmos.DrawWireSphere(bottomCheck + _moveDir * distanceFromWall, wallSphereRadius);
        }
        #endregion
#endif
    }
}


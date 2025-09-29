using EditorAttributes;
using Topacai.Inputs;
using Topacai.TDebug;
using UnityEngine;

namespace Topacai.Player.Movement.Components
{
    public class GrippingComponent : MovementComponent
    {
        [Header("Movement Settings")]
        [SerializeField] private AnimationCurve gripVelCurve;
        [SerializeField] private float timeToMaxSpeed;
        [SerializeField] private float speed;
        [SerializeField] private float minSpeed;
        [Space(5)]
        [SerializeField] private float moveModifier = 0.2f;
        [SerializeField] private float gripDrag;
        [Space(10)]
        [SerializeField] private ForceMode forceMode = ForceMode.VelocityChange;
        [SerializeField] private bool stopWhenCloseToGrip;
        [SerializeField] private bool stopOnObstacles;
        [SerializeField, EnableField(nameof(stopOnObstacles))] private bool useComponentPositionForObstacles;

        [Header("Collision Settings")]
        [SerializeField] private LayerMask _layerMask;

        [Header("Player Settings")]
        [SerializeField, EnableField(nameof(stopWhenCloseToGrip))] private float distanteToStop;
        [SerializeField] private float gripDistance;
        [SerializeField] private float maxVerticalSpeed;
        [SerializeField] private float timeToRecoverMovement;

        [Header("Visuals Settings")]
        [SerializeField] private LineRenderer _lineRenderer;

        [Header("Debug")]
#if UNITY_EDITOR
        [SerializeField] private bool _showGizmos;
        [SerializeField] private bool _displayDebugOnCanvas;
#endif
        [SerializeField] private bool _showDebug;

        [field: SerializeField, ReadOnly, ShowField(nameof(_showDebug))] public float LastGripUsage { get; private set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(_showDebug))] public bool IsGripping { get; private set; }
        [field: SerializeField, ReadOnly, ShowField(nameof(_showDebug))] public bool Gripped { get; private set; }
        [field: SerializeField] public float Cooldown { get; set; }
        [field: SerializeField] public bool OwnInputs { get; set; }
        
        [ReadOnly, ShowField(nameof(_showDebug))] public float _lastGrippingTime;

        private float _currentSpeed;
        private float _originalMaxFallSpeed;
        private Vector3 _gripHitPos;
        private Vector3 _gripPos;
        private bool _originalCanMoveAir;
        private RaycastHit _hit;

        RaycastHit obstacleHit;

        #region Unity Callbacks
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_showGizmos) return;
            Gizmos.color = IsGripping ? Color.red : Color.green;
            Gizmos.DrawCube(_gripHitPos, Vector3.one * 0.5f);

            if (IsGripping)
            {
                Gizmos.DrawWireSphere(_gripPos, 0.15f);
            }

            Gizmos.DrawWireCube(obstacleHit.point, Vector3.one * 0.5f);
        }
#endif

        private void Update()
        {
            if (OwnInputs)
                ReceibeInput();

            if (IsGripping && _lineRenderer != null)
            {
                _lineRenderer.SetPosition(1, _movement.transform.position);
            }
        }

        private void FixedUpdate()
        {
            LastGripUsage -= Time.deltaTime;
            _lastGrippingTime -= Time.deltaTime;

            if (IsGripping)
            {
                _lastGrippingTime = 0f;

                _currentSpeed += Time.fixedDeltaTime / timeToMaxSpeed;
                _currentSpeed = Mathf.Clamp01(_currentSpeed);

                float fixedCurve = gripVelCurve.Evaluate(_currentSpeed);
                float currentSpeed = Mathf.Lerp(minSpeed, speed, fixedCurve);

                // Adds an offset from hit point of grip relative to player input direction
                _gripPos = _gripHitPos + new Vector3(_movement.MoveDir.x, 0, _movement.MoveDir.y).normalized * moveModifier;
                Vector3 dir = _gripPos - _movement.transform.position;

                _movement.Rigidbody.AddForce(dir.normalized * currentSpeed, forceMode);

                float distance = Vector3.Distance(_gripPos, _movement.transform.position);

                /// Checks for flags to stop grip

                // Check for distance between player and grip point and stops if is close
                if (stopWhenCloseToGrip && distance < distanteToStop)
                {
                    StopGrip();
                }

                // Check for obstacles between player and grip point by using raycast
                Vector3 _obstaclePos = useComponentPositionForObstacles ? transform.position : Movement.transform.position;
                Vector3 _obstacleDir = _obstaclePos - _gripHitPos;


                if (stopOnObstacles && Physics.Raycast(_gripHitPos, _obstacleDir.normalized, out obstacleHit, _obstacleDir.magnitude, Movement.Data.WallLayer))
                {
#if UNITY_EDITOR
                    if (_showGizmos)
                        Debug.DrawRay(_gripHitPos, _obstacleDir.normalized * _obstacleDir.magnitude, Color.red, 10f);
#endif
                    StopGrip();
                }

                
#if UNITY_EDITOR
                if (_displayDebugOnCanvas)
                {
                    Debugcanvas.Instance.AddTextToDebugLog("gripSpeed", currentSpeed.ToString("0.00"));
                    Debugcanvas.Instance.AddTextToDebugLog("gripCurve", fixedCurve.ToString("0.00"));
                    Debugcanvas.Instance.AddTextToDebugLog("gripCSpeed", _currentSpeed.ToString("0.00"));
                }
#endif
            }

            /// When player is grapping it air movement is disabled and only applies an offset from hit point
            /// Also checks if player is on ground to recover movement
            if (Gripped)
            {
                if (_movement.LastGroundTime > 0)
                {
                    Gripped = false;
                    _movement.Data.AirMovement = _originalCanMoveAir;
                    return;
                }

                if (!IsGripping && _lastGrippingTime < -timeToRecoverMovement && _movement.Data.AirMovement != _originalCanMoveAir)
                {
                    _movement.Data.AirMovement = _originalCanMoveAir;
                }
            }
            else
            {
                if (IsGripping && _movement.LastGroundTime < 0)
                {
                    Gripped = true;
                    _originalCanMoveAir = _movement.DefaultData.AirMovement;
                    _movement.Data.AirMovement = false;
                }
            }
        }
        #endregion

        public override bool IsUsing() => IsGripping;

        private void ReceibeInput()
        {
            if (!IsGripping)
            {
                if (InputHandler.GetActionHandler(ActionName.Interact).IsPressing)
                {

                    RayGrip();
                    StartGrip();
                }
            }
            else
            {
                if (!InputHandler.GetActionHandler(ActionName.Interact).IsPressing)
                {
                    StopGrip();
                }
            }
        }

        #region Gripping Methods

        public void RayGrip() => RayGrip(gripDistance);
        public void RayGrip(float distance)
        {
            Transform cameraTransform = Camera.main.transform;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward.normalized, out _hit, distance, _layerMask))
            {
                _gripHitPos = _hit.point;
            }
            else _gripHitPos = default;
        }

        public void SetGripPos(Vector3 pos) => _gripHitPos = pos;

        public void StartGrip()
        {
            if (_gripHitPos == default || LastGripUsage > 0) return;
            _currentSpeed = 0f;
            IsGripping = true;

            _originalMaxFallSpeed = _movement.DefaultData.MaxFallSpeed;
            _movement.Data.MaxFallSpeed = maxVerticalSpeed;
            //_originalDrag = _movement.DefaultData.AirDrag;
            //_movement.DefaultData.AirDrag = gripDrag;
            if (_lineRenderer != null)
            {
                _lineRenderer.SetPosition(0, _gripHitPos);
                _lineRenderer.enabled = true;
            }
            
        }

        public void StopGrip()
        {
            _movement.Data.MaxFallSpeed = _originalMaxFallSpeed;
            // _movement.DefaultData.AirDrag = _originalDrag;

            _gripHitPos = default;
            IsGripping = false;
            _currentSpeed = 0f;
            LastGripUsage = Cooldown;
            if (_lineRenderer != null)
                _lineRenderer.enabled = false;
        }
        #endregion

        
    }
}


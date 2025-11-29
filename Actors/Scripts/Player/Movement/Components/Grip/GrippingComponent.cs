using EditorAttributes;
using Topacai.Inputs;
using Topacai.Player.Movement.Components.Grip;
using Topacai.TDebug;
using Topacai.Utils.GameObjects.AttachableSO;
using UnityEngine;

namespace Topacai.Player.Movement.Components
{
    public class GrippingComponent : MovementComponent
    {
        [SerializeField] private GrippingValuesSO _defaultGripAsset;

        private GrippingValuesSO _grippeableAsset;

        public GrippingValuesSO CurrentGripAsset
        {
            get
            {
                if (_grippeableAsset != null)
                    return _grippeableAsset;
                else return _defaultGripAsset;
            }
            set
            {
                _grippeableAsset = value;
            }
        }

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

                _currentSpeed += Time.fixedDeltaTime / CurrentGripAsset.TimeToMaxSpeed;
                _currentSpeed = Mathf.Clamp01(_currentSpeed);

                float fixedCurve = CurrentGripAsset.GripVelCurve.Evaluate(_currentSpeed);
                float currentSpeed = Mathf.Lerp(CurrentGripAsset.MinSpeed, CurrentGripAsset.Speed, fixedCurve);

                // Adds an offset from hit point of grip relative to player input direction
                _gripPos = _gripHitPos + new Vector3(_movement.MoveDir.x, 0, _movement.MoveDir.z).normalized * CurrentGripAsset.MoveModifier;
                Vector3 dir = _gripPos - _movement.transform.position;

                _movement.Rigidbody.AddForce(dir.normalized * currentSpeed, CurrentGripAsset.ForceMode);

                float distance = Vector3.Distance(_gripHitPos, _movement.transform.position);

                /// Checks for flags to stop grip

                // Check for distance between player and grip point and stops if is close
                if (CurrentGripAsset.StopWhenCloseToGrip && distance < CurrentGripAsset.DistanteToStop)
                {
                    StopGrip();
                }

                // Check for obstacles between player and grip point by using raycast
                Vector3 _obstaclePos = CurrentGripAsset.UseComponentPositionForObstacles ? transform.position : Movement.transform.position;
                Vector3 _obstacleDir = _obstaclePos - _gripHitPos;


                if (CurrentGripAsset.StopOnObstacles && Physics.Raycast(_gripHitPos, _obstacleDir.normalized, out obstacleHit, _obstacleDir.magnitude, Movement.Data.WallLayer))
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

                if (!IsGripping && _lastGrippingTime < -CurrentGripAsset.TimeToRecoverMovement && _movement.Data.AirMovement != _originalCanMoveAir)
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
                if (_movement.PlayerBrain.InputHandler.GetActionHandler(ActionName.Interact).IsPressing)
                {

                    RayGrip();
                    StartGrip(CurrentGripAsset);
                }
            }
            else
            {
                if (!_movement.PlayerBrain.InputHandler.GetActionHandler(ActionName.Interact).IsPressing)
                {
                    StopGrip();
                }
            }
        }

        #region Gripping Methods

        public void RayGrip() => RayGrip(CurrentGripAsset.GripDistance);
        public void RayGrip(float distance)
        {
            Transform cameraTransform = Camera.main.transform;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward.normalized, out _hit, distance, CurrentGripAsset.LayerMask))
            {
                _gripHitPos = _hit.collider.transform.position;

                if (_hit.collider.transform.TryGetScriptableObject<GrippingValuesSO>(out var asset)) 
                {
                    CurrentGripAsset = asset;
                }

            }
            else _gripHitPos = default;
        }

        public void SetGripPos(Vector3 pos) => _gripHitPos = pos;

        public void StartGrip(GrippingValuesSO gripAsset = null)
        {
            if (_gripHitPos == default || LastGripUsage > 0) return;

            if (gripAsset != null)
            {
                _grippeableAsset = gripAsset;
            }

            _currentSpeed = 0f;
            IsGripping = true;

            _originalMaxFallSpeed = _movement.DefaultData.MaxFallSpeed;
            _movement.Data.MaxFallSpeed = CurrentGripAsset.MaxVerticalSpeed;
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

            _grippeableAsset = null;

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


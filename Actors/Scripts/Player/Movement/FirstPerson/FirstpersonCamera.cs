using System.Collections;
using System.Collections.Generic;
using Topacai.Inputs;
using UnityEngine;

namespace Topacai.Player.Movement.Firstperson
{
    public class FirstpersonCamera : MonoBehaviour
    {
        [SerializeField] protected PlayerBrain _playerBrain;

        [HideInInspector] public Vector3 CameraDir;
        [HideInInspector] public Vector3 CameraDirFlat;

        [SerializeField] protected FirstPersonReferences _references;

        public Transform CameraTransform => _references.FP_Camera;
        private Transform PlayerOrientation => _references.PlayerOrientation;
        private Transform CameraHolder => _references.FP_CameraHolder;
        private (float, float) Sensivity => _references.Config.GetSensivity(_playerBrain.InputHandler.CurrentDevice);
        private Vector2 _input => _playerBrain.InputHandler.CameraDir;

        private float _cameraX = 0;
        private float _cameraY = 0;
        private float _lastFrame;

        private float _sensMultiplier = 1f;

#if UNITY_EDITOR
        [Header("DEBUG")]
        [SerializeField] private bool SHOW_CAMERA_DIR = false;
        [SerializeField] private bool SHOW_CAMERA_DIR_FLAT = false;

        void OnDrawGizmos()
        {
            if (SHOW_CAMERA_DIR)
            {
                if (Application.isPlaying)
                    Debug.DrawRay(PlayerOrientation.position, CameraDir * 2, Color.red);
            }

            if (SHOW_CAMERA_DIR_FLAT)
            {
                if (Application.isPlaying)
                    Debug.DrawRay(PlayerOrientation.position, CameraDirFlat * 2, Color.gray);
            }
        }
#endif

        private void Awake()
        {
            _playerBrain = _playerBrain ?? GetComponent<PlayerBrain>();

            if (_playerBrain == null)
            {
                Debug.LogWarning("[FPCamera] FP Movement should belongs to a PlayerBrain");
                this.enabled = false;
                return;
            }

            _references.CameraController = this;
            _playerBrain.PlayerReferences.RegisterModule(_references);

            if (CameraTransform == null || PlayerOrientation == null || CameraHolder == null)
            {
                Debug.LogWarning("[FPCamera] Missing references");
                this.enabled = false;
                return;
            }
        }
        void Start()
        {
            CameraDir = Vector3.zero;
            CameraDirFlat = Vector3.zero;

            SetSensMultiplier(_playerBrain.InputHandler.CurrentDevice);

            _playerBrain.InputHandler.OnSchemeChanged.AddListener(OnSchemeChanged);
        }

        void Update()
        {
            if (_playerBrain == null)
            {
                Debug.LogWarning("[FPCamera] PlayerBrain is null");
                return;
            }
            CameraMovement();
            CameraDir = CameraTransform.forward;

            CameraDirFlat = PlayerOrientation.transform.forward;
        }

        private void OnSchemeChanged(OnSchemeChangedArgs args)
        {
            SetSensMultiplier(args.DeviceType);
        }

        private void SetSensMultiplier(Inputs.DeviceType deviceType)
        {
            switch (deviceType)
            {
                case Inputs.DeviceType.Keyboard:
                    _sensMultiplier = 0.033f;
                    break;
                case Inputs.DeviceType.Controller:
                    _sensMultiplier = 2f;
                    break;
            }
        }

        private void CameraMovement()
        {
            // Avoid movement variation between frames, framerate and time scale
            float deltaTime = Time.unscaledDeltaTime;
            float timeRatio = _lastFrame / deltaTime;

#if UNITY_EDITOR
            // When we use the frame debugger for some reason this values turns into
            // NaN values
            if (float.IsNaN(_cameraX) || float.IsNaN(_cameraY))
            {
                _cameraX = 0;
                _cameraY = 0;
            }
#endif

            _cameraX += _input.x * Sensivity.Item1 * timeRatio * _sensMultiplier;
            _cameraY -= _input.y * Sensivity.Item2 * timeRatio * _sensMultiplier;

            _cameraY = Mathf.Clamp(_cameraY, -89, 89);
            PlayerOrientation.localRotation = Quaternion.Euler(0, _cameraX, 0);
            CameraHolder.localRotation = Quaternion.Euler(_cameraY, 0, 0);

            _lastFrame = deltaTime;
        }

        public void SetPlayerBrain(PlayerBrain playerBrain) => _playerBrain = playerBrain;
    }
}

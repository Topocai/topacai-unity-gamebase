using System.Collections;
using System.Collections.Generic;
using Topacai.Inputs;
using UnityEngine;

namespace Topacai.Player.Firstperson.Camera
{
    public class FirstpersonCamera : MonoBehaviour
    {
        public static Vector3 CameraDir;
        public static Vector3 CameraDirFlat;
        public static Transform CameraTransform => PlayerBrain.Instance.PlayerReferences.FP_Camera;

        private Transform PlayerOrientation => PlayerBrain.Instance.PlayerReferences.PlayerOrientation;
        private Transform CameraHolder => PlayerBrain.Instance.PlayerReferences.CameraHolder;
        private (float, float) Sensivity => PlayerBrain.Instance.PlayerConfig.GetSensivity(InputHandler.CurrentDevice);
        private Vector2 _input => InputHandler.CameraDir;

        private float _cameraX;
        private float _cameraY;
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
        void Start()
        {
            CameraDir = Vector3.zero;
            CameraDirFlat = Vector3.zero;

            SetSensMultiplier(InputHandler.CurrentDevice);

            InputHandler.OnSchemeChanged.AddListener(OnSchemeChanged);
        }

        void Update()
        {
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

            _cameraX += _input.x * Sensivity.Item1 * timeRatio * _sensMultiplier;
            _cameraY -= _input.y * Sensivity.Item2 * timeRatio * _sensMultiplier;

            _cameraY = Mathf.Clamp(_cameraY, -89, 89);
            PlayerOrientation.localRotation = Quaternion.Euler(0, _cameraX, 0);
            CameraHolder.localRotation = Quaternion.Euler(_cameraY, 0, 0);

            _lastFrame = deltaTime;
        }
    }
}

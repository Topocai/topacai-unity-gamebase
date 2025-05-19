using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Topacai.Player
{
    [System.Serializable]
    public class PlayerReferences
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Transform playerOrientation;
        [SerializeField] private Transform cameraHolder;
        [SerializeField] private Transform FP_camera;

        public Rigidbody Rigidbody { get => rb; set => rb = value; }
        public Transform PlayerOrientation { get => playerOrientation; set => playerOrientation = value; }
        public Transform CameraHolder { get => cameraHolder; set => cameraHolder = value; }
        public Transform FP_Camera { get => FP_camera; set => FP_camera = value; }
    }

    [System.Serializable]
    public class PlayerConfig
    {
        #region Sensivity
        [SerializeField] private float sensivity_horizontal = 5f;
        [SerializeField] private float sensivity_vertical = 5f;

        [SerializeField] private float j_sensivity_horizontal = 50f;
        [SerializeField] private float j_sensivity_vertical = 50f;

        public void SetSensivity(Inputs.DeviceType device, float horizontal, float vertical)
        {
            switch (device)
            {
                case Inputs.DeviceType.Keyboard:
                    sensivity_horizontal = horizontal;
                    sensivity_vertical = vertical;
                    break;
                case Inputs.DeviceType.Controller:
                    j_sensivity_horizontal = horizontal;
                    j_sensivity_vertical = vertical;
                    break;
                default:
                    break;
            }
        }

        public (float, float) GetSensivity(Inputs.DeviceType device)
        {
            switch (device)
            {
                case Inputs.DeviceType.Keyboard:
                    return (sensivity_horizontal, sensivity_vertical);
                case Inputs.DeviceType.Controller:
                    return (j_sensivity_horizontal, j_sensivity_vertical);
                default:
                    return (sensivity_horizontal, sensivity_vertical);
            }
        }
        #endregion
    }

    public class PlayerBrain : MonoBehaviour
    {
        public static PlayerBrain Instance { get; private set; }

        [field: SerializeField] public PlayerReferences PlayerReferences { get; private set; }
        [field: SerializeField] public PlayerConfig PlayerConfig { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            PlayerReferences.Rigidbody = GetComponent<Rigidbody>();
        }

        #region Public Utility Methods

        public void TeleportPlayerTo(Transform pos) => TeleportPlayerTo(pos.position);

        public void TeleportPlayerTo(Vector3 pos)
        {
            PlayerReferences.Rigidbody.position = pos;
            transform.position = pos;
        }

        public void TeleportPlayerRelativeTo(Transform pos, Vector3? origin) => TeleportPlayerRelativeTo(pos.position, origin);

        public void TeleportPlayerRelativeTo(Transform pos) => TeleportPlayerRelativeTo(pos.position, transform.position);

        public void TeleportPlayerRelativeTo(Transform pos, Transform origin) => TeleportPlayerRelativeTo(pos.position, origin.position);

        public void TeleportPlayerRelativeTo(Vector3 pos, Vector3? origin)
        {
            if (origin == null)
                origin = transform.position;

            PlayerReferences.Rigidbody.position = (Vector3)origin + pos;
            transform.position = (Vector3)origin + pos;
        }

        #endregion

    }
}


using System.Collections;
using System.Collections.Generic;
using Topacai.Inputs;
using Topacai.Utils.GameObjects;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Topacai.Player
{
    [System.Serializable]
    public class PlayerReferences
    {
        [System.Serializable]
        public struct FirstPersonReferences
        {
            [SerializeField] private Transform cameraHolder;
            [SerializeField] private Transform FP_camera;

            public Transform FP_CameraHolder { get => cameraHolder; set => cameraHolder = value; }
            public Transform FP_Camera { get => FP_camera; set => FP_camera = value; }
        }

        [SerializeField] private Rigidbody rb;
        [SerializeField] private Transform playerOrientation;
        [SerializeField] private FirstPersonReferences firstPersonReferences;

        public Rigidbody Rigidbody { get => rb; set => rb = value; }
        public Transform PlayerOrientation { get => playerOrientation; set => playerOrientation = value; }
        
        public FirstPersonReferences FirstPersonConfig { get => firstPersonReferences; set => firstPersonReferences = value; }
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
        public const bool SINGLEPLAYER_MODE = true;
        public static List<PlayerBrain> Players { get; private set; }

        [field: SerializeField] public PlayerReferences PlayerReferences { get; private set; }
        [field: SerializeField] public PlayerConfig PlayerConfig { get; private set; }

        [SerializeField] private InputActionAsset _inputAsset;
        [SerializeField] private InputHandler _playerInputs;

        public InputHandler InputHandler => _playerInputs;

        private void CreateInputs()
        {
            if (_playerInputs != null || _inputAsset == null) return;

            var playerInput = gameObject.AddComponent<PlayerInput>();

            playerInput.actions = _inputAsset;

            gameObject.AddComponent<InputHandler>();
            
        }

        protected void Initialize()
        {
            CreateInputs();
        }

        private void Start()
        {
            CreateInputs();
        }

        #region Factory Pattern

        public static GameObject CreatePlayerPrefab(GameObject playerPrefab)
        {
            var instance = Instantiate(playerPrefab);

            if (instance.TryGetComponent(out PlayerBrain playerBrain))
            {
                Players.Add(playerBrain);

                playerBrain.Initialize();

                if (playerBrain.InputHandler == null)
                {
                    playerBrain.CreateInputs();
                }
            } else
            {
                throw new System.Exception("Player prefab must have a PlayerBrain component");
            }

            return instance;
        }

        public static GameObject CreatePlayerPrefab(GameObject playerPrefab, Vector3 pos)
        {
            var instance = CreatePlayerPrefab(playerPrefab);
            instance.transform.position = pos;
            return instance;
        }

        public static GameObject CreatePlayerPrefab(GameObject playerPrefab,Transform pos) => CreatePlayerPrefab(playerPrefab, pos.position);

        #endregion

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


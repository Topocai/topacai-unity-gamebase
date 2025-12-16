using System;
using UnityEngine;

namespace Topacai.Player.Movement.Firstperson
{
    [System.Serializable]
    public class FirstPersonReferences : IPlayerModule
    {
        [SerializeField] private Transform cameraHolder;
        [SerializeField] private Transform FP_camera;
        [SerializeField] private Transform playerOrientation;
        [SerializeField] private PlayerConfig _config;

        public Transform PlayerOrientation { get => playerOrientation; set => playerOrientation = value; }
        public Transform FP_CameraHolder { get => cameraHolder; set => cameraHolder = value; }
        public Transform FP_Camera { get => FP_camera; set => FP_camera = value; }

        public PlayerConfig Config { get => _config; set => _config = value; }

        public FirstpersonCamera CameraController { get; set; }

        public object Controller { get => CameraController; set => CameraController = value as FirstpersonCamera; }
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

    
}

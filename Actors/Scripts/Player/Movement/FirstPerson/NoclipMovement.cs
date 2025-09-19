using System;
using Topacai.Inputs;
using Topacai.Player.Movement;
using Topacai.Player.Movement.Firstperson.Camera;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Topacai.TDebug.Movement
{
    public class NoclipMovement : MonoBehaviour
    {
#if UNITY_EDITOR
        [Header("References")]
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private FirstpersonCamera FirstpersonCamera;

        [Header("Noclip Settings")]
        [SerializeField] private InputAction _switchNoclipHotkey;
        [SerializeField] private float _horizontalSpeed = 5f;
        [SerializeField] private float _runSpeed = 10f;
        [SerializeField] private float _upSpeed = 5f;
        [SerializeField] private float _downSpeed = 5f;

        private bool _goingUp = false;
        private bool _goingDown = false;
        private bool _isRunning = false;
        private bool _isNoclip;

        private Vector3 _inputDir;

        private ShortcutExecuter<Action> _switchNoclipExecuter;

        private void Start()
        {
            _switchNoclipExecuter = new ShortcutExecuter<Action>(_switchNoclipHotkey, SwitchNoclip);
            SetEnableNoclip(false);
        }

        private void OnDisable()
        {
            SetEnableNoclip(false);
        }

        private void Update()
        {
            _switchNoclipExecuter.Update();

            if (!_isNoclip) return;

            ReadInputs();

            Vector3 cameraDir = FirstpersonCamera.CameraDir.normalized;

            Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraDir).normalized;
            Vector3 cameraForward = Vector3.Cross(cameraRight, Vector3.up).normalized;

            Vector3 moveDir = cameraForward * _inputDir.y + cameraRight * _inputDir.x;

            float currentVel = _isRunning ? _runSpeed : _horizontalSpeed;

            _playerMovement.Rigidbody.position += moveDir.normalized * currentVel * Time.deltaTime;

            _playerMovement.Rigidbody.position += Vector3.up * (1 * (_goingUp ? _upSpeed : 0) + (_goingDown ? -_downSpeed : 0)) * Time.deltaTime;
        }

        private void ReadInputs()
        {
            _goingUp = InputHandler.GetActionHandler(ActionName.Jump).All;
            _goingDown = InputHandler.GetActionHandler(ActionName.Crouch).All;
            _isRunning = InputHandler.GetActionHandler(ActionName.Run).All;

            _inputDir = InputHandler.MoveDir;
        }

        public void SetEnableNoclip(bool value)
        {
            _isNoclip = value;

            if (_playerMovement == null || FirstpersonCamera == null) return;
            _playerMovement.Data.CanMove = !value;
            _playerMovement.Data.FreezeMove = value;

            _playerMovement.enabled = !value;

            if (_playerMovement.Rigidbody == null) return;

            _playerMovement.Rigidbody.useGravity = !value;
            _playerMovement.Rigidbody.isKinematic = value;
            _playerMovement.Rigidbody.linearVelocity = Vector3.zero; 
        }

        public void SwitchNoclip() => SetEnableNoclip(!_isNoclip);
#endif
    }
}
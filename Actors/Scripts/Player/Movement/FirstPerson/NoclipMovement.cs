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
        [SerializeField] private float _boostSpeed = 10f;
        [SerializeField] private float _upSpeed = 5f;
        [SerializeField] private float _downSpeed = 5f;

        [Header("Input settings")]
        [SerializeField] private InputAction _upInput;
        [SerializeField] private InputAction _downInput;
        [SerializeField] private InputAction _teleportInput;

        private bool _goingUp = false;
        private bool _goingDown = false;
        private bool _isBoosted = false;
        private bool _isNoclip;

        private Vector3 _inputDir;

        private ShortcutExecuter<Action> _switchNoclipExecuter;

        private void Start()
        {
            _switchNoclipExecuter = new ShortcutExecuter<Action>(_switchNoclipHotkey, SwitchNoclip);
            SetEnableNoclip(false);

            _upInput.Enable();
            _downInput.Enable();
            _teleportInput.Enable();
        }

        private void OnDisable()
        {
            SetEnableNoclip(false);
        }

        private void Teleport()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, FirstpersonCamera.CameraDir.normalized, out hit))
            {
                _playerMovement.Rigidbody.position = hit.point;
            }
        }

        private void Update()
        {
            _switchNoclipExecuter.Update();

            if (!_isNoclip) return;

            ReadInputs();

            if (_teleportInput.WasPerformedThisFrame())
            {
                Teleport();
            }

            Vector3 cameraDir = FirstpersonCamera.CameraDir.normalized;

            Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraDir).normalized;
            Vector3 cameraForward = Vector3.Cross(cameraRight, Vector3.up).normalized;

            Vector3 moveDir = cameraForward * _inputDir.y + cameraRight * _inputDir.x;

            float currentVel = _isBoosted ? _horizontalSpeed*_boostSpeed : _horizontalSpeed;

            _playerMovement.Rigidbody.position += moveDir.normalized * currentVel * Time.deltaTime;

            _playerMovement.Rigidbody.position += Vector3.up * (1 * (_goingUp ? (_isBoosted ? _upSpeed*_boostSpeed : _upSpeed) : 0) + (_goingDown ? -(_isBoosted ? _downSpeed*_boostSpeed : _downSpeed) : 0)) * Time.deltaTime;
        }

        private void ReadInputs()
        {
            _goingUp = _upInput.IsPressed();
            _goingDown = _downInput.IsPressed();
            _isBoosted = InputHandler.GetActionHandler(ActionName.Run).All;

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
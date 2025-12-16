using System.Collections;
using System.Collections.Generic;
using Topacai.Actors.Interactuables;
using Topacai.Inputs;

using UnityEngine;

namespace Topacai.Player.Movement.Firstperson.Interact
{
    public class FPInteractHandler : MonoBehaviour
    {
        [SerializeField] private InteractingSystem playerInteractingSystem;

        [SerializeField] private PlayerBrain _playerBrain;

        [SerializeField] private float _distance = 3f;
        [SerializeField] private LayerMask _interactuableLayerMask;

        private FirstPersonReferences _playerReferences;
        private FirstPersonReferences PlayerReferences
        {
            get 
            {
                if (_playerReferences == null)
                {
                    _playerReferences = _playerBrain.PlayerReferences.GetModule<FirstPersonReferences>();
                }
                return _playerReferences;
            }
            set => _playerReferences = value;
        }

        private Transform _cameraTransform => PlayerReferences.FP_Camera;

        RaycastHit hit;

        public PlayerBrain PlayerBrain => _playerBrain;

        private void Awake()
        {
            _playerBrain = _playerBrain ?? GetComponent<PlayerBrain>();

            if (_playerBrain == null)
            {
                Debug.LogError("PlayerBrain is null");
                enabled = false;
                return;
            }
        }

        void Start()
        {
            if (playerInteractingSystem == null)
            {
                playerInteractingSystem = GetComponent<InteractingSystem>();
                if (playerInteractingSystem == null)
                {
                    playerInteractingSystem = gameObject.AddComponent<InteractingSystem>();
                }
            }
        }

        public void SetPlayerBrain(PlayerBrain playerBrain) => _playerBrain = playerBrain;

        private void FixedUpdate()
        {
            if (_cameraTransform == null || _cameraTransform == null)
            {
                Debug.LogError("Camera references is null");
                enabled = false;
                return;
            }

            if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out hit, _distance, _interactuableLayerMask))
            {
                if (hit.collider.TryGetComponent(out IInteractuable interactuable))
                {
                    playerInteractingSystem.SetInteractuable(interactuable, this);
                    
                } else playerInteractingSystem.ResetInteractuable();
            } else playerInteractingSystem.ResetInteractuable();
        }

        private void Update()
        {
            if (_playerBrain.InputHandler.GetActionHandler(ActionName.Interact).IsPressed)
            {
                playerInteractingSystem.Interact(this);
            }

            if (_playerBrain.InputHandler.GetActionHandler(ActionName.Interact).IsPressing)
            {
                playerInteractingSystem.HoldInteract(this);
            }
        }
    }
}

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

        private Transform _cameraTransform => _playerBrain.PlayerReferences.FirstPersonConfig.FP_Camera;

        RaycastHit hit;

        public PlayerBrain PlayerBrain => _playerBrain;

        void Start()
        {
            if (_playerBrain == null)
            {
                _playerBrain = GetComponent<PlayerBrain>();
            }

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

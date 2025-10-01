using System.Collections;
using System.Collections.Generic;
using Topacai.Actors.Interactuables;
using Topacai.Inputs;
using Topacai.Player.Movement.Firstperson.Camera;
using UnityEngine;

namespace Topacai.Player.Movement.Firstperson.Interact
{
    public class FPInteractHandler : MonoBehaviour
    {
        [SerializeField] private PlayerBrain _playerBrain;

        [SerializeField] private float _distance = 3f;
        [SerializeField] private LayerMask _interactuableLayerMask;

        private Transform _cameraTransform => _playerBrain.PlayerReferences.FirstPersonConfig.FP_Camera;

        RaycastHit hit;

        void Start()
        {
            if (_playerBrain == null)
            {
                _playerBrain = GetComponent<PlayerBrain>();
            }
        }

        public void SetPlayerBrain(PlayerBrain playerBrain) => _playerBrain = playerBrain;

        private void FixedUpdate()
        {
            if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out hit, _distance, _interactuableLayerMask))
            {
                if (hit.collider.TryGetComponent(out IInteractuable interactuable))
                {
                    Interactuable.SetInteractuable(interactuable);
                    
                } else Interactuable.ResetInteractuable();
            } else Interactuable.ResetInteractuable();
        }

        private void Update()
        {
            if (_playerBrain.InputHandler.GetActionHandler(ActionName.Interact).IsPressed)
            {
                Interactuable.Interact();
            }

            if (_playerBrain.InputHandler.GetActionHandler(ActionName.Interact).IsPressing)
            {
                Interactuable.HoldInteract();
            }
        }
    }
}

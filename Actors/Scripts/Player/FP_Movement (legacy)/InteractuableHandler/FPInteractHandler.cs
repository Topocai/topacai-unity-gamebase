using System;
using System.Collections;
using System.Collections.Generic;
using Topacai.Actors.Interactuables;
using Topacai.Inputs;
using UnityEngine;

namespace Topacai.Player.Firstperson.Interact
{
    [Obsolete("This system is obselete, use Topacai.Player.Movement.Firstperson.Interact system instead")]
    public class FPInteractHandler : MonoBehaviour
    {
        [SerializeField] private float _distance = 3f;

        [SerializeField] private LayerMask _interactuableLayerMask;

        private Transform _cameraTransform => PlayerBrain.Instance.PlayerReferences.FP_Camera;

        RaycastHit hit;

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
            if (InputHandler.InteractPressed)
            {
                Interactuable.Interact();
            }

            if (InputHandler.IsInteracting)
            {
                Interactuable.HoldInteract();
            }
        }
    }
}

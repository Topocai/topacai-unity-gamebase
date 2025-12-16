using System;
using UnityEngine;

namespace Topacai.Player.Movement
{
    [System.Serializable]
    public partial class PlayerMovementReferences : IPlayerModule
    {
        [SerializeField] private Rigidbody rb;
        public Rigidbody Rigidbody { get => rb; set => rb = value; }

        public PlayerMovement MovementController { get; set; }
        public object Controller { get => MovementController; set => MovementController = value as PlayerMovement; }
    }

}

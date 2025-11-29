using EditorAttributes;
using UnityEngine;

namespace Topacai.Player.Movement.Components.Grip
{
    [CreateAssetMenu(fileName = "GrippingValuesSO", menuName = "ScriptableObjects/PlayerMovement/Components/GrippingValuesSO")]
    public class GrippingValuesSO : ScriptableObject
    {
        [Header("Movement Settings")]
        public AnimationCurve GripVelCurve;
        public float TimeToMaxSpeed;
        public float Speed;
        public float MinSpeed;
        [Space(5)]
        public float MoveModifier = 0.2f;
        public float GripDrag;
        [Space(10)]
        public ForceMode ForceMode = ForceMode.VelocityChange;
        public bool StopWhenCloseToGrip;
        public bool StopOnObstacles;
        [EnableField(nameof(StopOnObstacles))] public bool UseComponentPositionForObstacles;

        [Header("Collision Settings")]
        public LayerMask LayerMask;

        [Header("Player Settings")]
        [EnableField(nameof(StopWhenCloseToGrip))] public float DistanteToStop;
        public float GripDistance;
        public float MaxVerticalSpeed;
        public float TimeToRecoverMovement;
    }
}

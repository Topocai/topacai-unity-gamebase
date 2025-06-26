using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;
using UnityEngine.Events;


namespace Topacai.Player.Firstperson.Movement
{
    [CreateAssetMenu(fileName = "MovementAsset", menuName = "ScriptableObjects/FPMovement/MovementAsset")]
    public class FP_MovementAsset : ScriptableObject
    {
        [HideInInspector] public UnityEvent OnValuesChanged = new UnityEvent();

        [Header("Gravity")]
        public float RBMass = 1;
        [Space(10)]
        public LayerMask GroundLayer;
        public LayerMask WallLayer;
        [ReadOnly] public float GravityStrength;
        [ReadOnly] public float GravityScale;
        [Range(0.001f, 0.4f)] public float fallThreshold = 0.2f;
        [EnableField(nameof(LimitFallSpeed))] public float MaxFallSpeed = 120f;
        [Space(15)]
        public float GroundDrag = 0.2f;
        [Tooltip("Drag applied when player is in air but not falling")]
        public float AirDrag;
        public float FallDrag;
        [Space(15)]
        public float GroundGravityScale = 0.3f;
        public float FallingGravityMult = 7f;
        public float JumpingGravityMult = 8f;
        [EnableField(nameof(LargeJump))] public float JumpCutGravityMult = 10f;
        [Space(15)]

        [Header("Wall Collision")]
        public float WallMinAngleToMove = 25f;

        [Header("Jump")]
        [EnableField(nameof(CanJump))] public float JumpHeight = 10;
        [EnableField(nameof(CanJump))] public float JumpTimeToApex = 2.6f;
        [ReadOnly] public float JumpForce;
        [Range(0.001f, 1f), EnableField(nameof(CanJump))] public float WhenCancelJumping = 0.45f;

        [Space(5)]
        [EnableField(nameof(JumpHang))] public float JumpHangAccelMult;
        [EnableField(nameof(JumpHang))] public float JumpHandMaxSpeed;

        [Header("Movement")]
        public float Acceleration = 6.6f;
        public float Deceleration = 8.9f;
        [HideInInspector] public float accelerationAmount;
        [HideInInspector] public float decelerationAmount;

        public float WalkSpeed = 4;
        public float RunSpeed = 8;
        [Space(15)]
        [EnableField(nameof(AirMovement))] public float AirAccelMult = 1;
        [EnableField(nameof(AirMovement))] public float AirDecelMult = 0.33f;
        [EnableField(nameof(AirMovement))] public float AirMaxSpeedMult = 1;

        [Header("Slope")]
        [EnableField(nameof(SlopeDetection))] public float MinSlopeAngle = 12;
        [EnableField(nameof(SlopeDetection))] public float MaxSlopeAngle = 46f;
        [EnableField(nameof(SlopeDetection))] public float SlopeSpeedMultiplier = 1.7f;
        [EnableField(nameof(SlopeDetection))] public float SlopeDownForce = 18f;

        [Header("StairClimb")]
        [Tooltip("Max vertical velocity achieved when climbing a step")]
        [EnableField(nameof(StepClimb))] public float StepClampVel = 3.77f;
        [EnableField(nameof(StepClimb))] public float StepUpForce = 3;
        [Space(5)]
        [EnableField(nameof(StepClimb))] public float StepDistance = 0.33f;
        [EnableField(nameof(StepClimb))] public float StepDepth = 0.4f;
        [Tooltip("Add a higher distance to step up when the player is on a slope")]
        [EnableField(nameof(StepClimb))] public float OnSlopeStepDistanceMultiplier = 1.2f;
        [Space(5)]
        [EnableField(nameof(StepClimb))] public float StepDownForce = 32;
        [EnableField(nameof(StepClimb))] public float StepDownDistance = 0.47f;
        [EnableField(nameof(StepClimb))] public float StepDownOffset = 0.2f;
        [EnableField(nameof(StepClimb))] public float StepDownLastGroundThreshold = 0.005f;
        [Space(5)]
        [Tooltip("EXPERIMENTAL - adds a buffer time between steps")]
        [EnableField(nameof(StepClimb))] public float StepBufferTime;
        [Tooltip("Recommended: 80 or greater then Max Slope Angle if Slope Detection is enabled")]
        [EnableField(nameof(StepClimb))] public float StepMinAngle = 80; //80, greater then slopeAngle
        [Tooltip("Threshold to consider a normal face a step surface. Recommended: >= 0.75")]
        [EnableField(nameof(StepClimb))] public float StepDotThreshold = 0.75f;

        [Header("Util")]
        [Range(0.01f, 0.5f)] public float CoyoteTime = .15f;
        [Range(0.01f, 0.5f)] public float JumpBufferInput = .12f;
        [Range(0.01f, 0.5f), EnableField(nameof(CanJump))] public float JumpApexBuffer = .13f;

        [Header("Flags")]
        [Tooltip("Movement logic stills working but input is disabled")]
        public bool CanMove = true;
        [Tooltip("Movement logic disabled and reset to 0")]
        public bool FreezeMove;
        public bool CanJump = true;
        [Tooltip("Can change speed based on input (run or walk)")]
        public bool CanChangeSpeed = true;
        [Space(15)]
        public bool LimitFallSpeed = true;
        [Tooltip("Keep momentum when high speed and player is on air.")]
        public bool ConserveMomentumOnAir;
        [Space(15)]
        public bool AirMovement = true;
        [Tooltip("Jumping is higher if player still holding jump button")]
        public bool LargeJump;
        [Tooltip("Extra speed on jump apex (better feeling sometimes)")]
        public bool JumpHang;
        [Space(15)]
        public bool SlopeDetection = true;
        public bool StepClimb = true;
        [Space(15)]
        [Header("- EXPERIMENTAL or DEPRECATED -")]
        public bool AntiSlideMethod1;
        public bool AntiSlideMethod2;
        [Space(5)]
        public bool AntiSlideMethod1Air;
        public bool AntiSlideMethod2Air;

        [Header("AntiSlide - deprecated")]

        [FoldoutGroup("Slide Method 1", nameof(BrakeForceMultiplier), nameof(SlideAlignmentThreshold1))]
        [SerializeField] private Void slidem1Group;
        [HideProperty, EnableField(nameof(AntiSlideMethod1))] public float BrakeForceMultiplier;
        [HideProperty, EnableField(nameof(AntiSlideMethod1))] public float SlideAlignmentThreshold1 = 0.8f;

        [FoldoutGroup("Slide Method 2", nameof(HighBrakeDrag), nameof(SlideAlignmentThreshold2))]
        [SerializeField] private Void slidem2Group;
        [HideProperty, EnableField(nameof(AntiSlideMethod2))] public float HighBrakeDrag;
        [HideProperty, EnableField(nameof(AntiSlideMethod2))] public float SlideAlignmentThreshold2 = 0.8f;

        [Space(20)]

        [FoldoutGroup("Slide Method 1 Air", nameof(AirBrakeForceMultiplier), nameof(AirSlideAlignmentThreshold1))]
        [SerializeField] private Void airslidem1Group;
        [HideProperty, EnableField(nameof(AntiSlideMethod1Air))] public float AirBrakeForceMultiplier;
        [HideProperty, EnableField(nameof(AntiSlideMethod1Air))] public float AirSlideAlignmentThreshold1 = 0.8f;

        [FoldoutGroup("Slide Method 2 Air", nameof(AirHighBrakeDrag), nameof(AirSlideAlignmentThreshold2))]
        [SerializeField] private Void airslidem2Group;
        [HideProperty, EnableField(nameof(AntiSlideMethod2Air))] public float AirHighBrakeDrag;
        [HideProperty, EnableField(nameof(AntiSlideMethod2Air))] public float AirSlideAlignmentThreshold2 = 0.8f;

        public void CalculateDynamicValues(float maxSpeed)
        {
            accelerationAmount = (50 * Acceleration) / maxSpeed;
            decelerationAmount = (50 * Deceleration) / maxSpeed;

            accelerationAmount = Mathf.Clamp(Acceleration, 0.01f, 999f);
            decelerationAmount = Mathf.Clamp(Deceleration, 0.01f, 999f);
        }

        public void CalculateJumpForce(float height, float timeToApex, float gravityY)
        {
            //Calculate gravity strength using the formula (gravity = 2 * jumpHeight / timeToJumpApex^2) 
            GravityStrength = -(2 * height) / (timeToApex * timeToApex);
            //Calculate jumpForce using the formula (initialJumpVelocity = gravity * timeToJumpApex)
            JumpForce = Mathf.Abs(GravityStrength) * timeToApex;

            GravityScale = GravityStrength / gravityY;
        }

        private void OnValidate()
        {
            OnValuesChanged?.Invoke();
        }
    }

}


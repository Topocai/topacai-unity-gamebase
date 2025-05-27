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
        public float RBMass;
        [Space(10)]
        public LayerMask GroundLayer;
        public LayerMask WallLayer;
        [ReadOnly] public float GravityStrength;
        [ReadOnly] public float GravityScale;
        [Range(0.001f, 0.4f)] public float fallThreshold;
        [EnableField(nameof(LimitFallSpeed))] public float MaxFallSpeed;
        [Space(15)]
        public float GroundDrag;
        [Tooltip("Drag applied when player is in air but not falling")]
        public float AirDrag;
        public float FallDrag;
        [Space(15)]
        public float GroundGravityScale;
        public float FallingGravityMult;
        public float JumpingGravityMult;
        [EnableField(nameof(LargeJump))] public float JumpCutGravityMult;
        [Space(15)]

        [Header("Wall Collision")]
        public float WallMinAngleToMove = 20f;

        [Header("Jump")]
        [EnableField(nameof(CanJump))] public float JumpHeight;
        [EnableField(nameof(CanJump))] public float JumpTimeToApex;
        [ReadOnly] public float JumpForce;
        [Range(0.001f, 1f), EnableField(nameof(CanJump))] public float WhenCancelJumping;

        [Space(5)]
        [EnableField(nameof(JumpHang))] public float JumpHangAccelMult;
        [EnableField(nameof(JumpHang))] public float JumpHandMaxSpeed;

        [Header("Movement")]
        public float Acceleration;
        public float Deceleration;
        [HideInInspector] public float accelerationAmount;
        [HideInInspector] public float decelerationAmount;

        public float WalkSpeed;
        public float RunSpeed;
        [Space(15)]
        [EnableField(nameof(AirMovement))] public float AirAccelMult;
        [EnableField(nameof(AirMovement))] public float AirDecelMult;
        [EnableField(nameof(AirMovement))] public float AirMaxSpeedMult;

        [Header("Slope")]
        [EnableField(nameof(SlopeDetection))] public float MinSlopeAngle;
        [EnableField(nameof(SlopeDetection))] public float MaxSlopeAngle;
        [EnableField(nameof(SlopeDetection))] public float SlopeSpeedMultiplier;
        [EnableField(nameof(SlopeDetection))] public float SlopeDownForce;

        [Header("StairClimb")]
        [Tooltip("Max vertical velocity achieved when climbing a step")]
        [EnableField(nameof(StepClimb))] public float StepClampVel;
        [EnableField(nameof(StepClimb))] public float StepUpForce;
        [Space(5)]
        [EnableField(nameof(StepClimb))] public float StepDistance;
        [EnableField(nameof(StepClimb))] public float StepDepth;
        [Tooltip("Add a higher distance to step up when the player is on a slope")]
        [EnableField(nameof(StepClimb))] public float OnSlopeStepDistanceMultiplier;
        [Space(5)]
        [EnableField(nameof(StepClimb))] public float StepDownForce;
        [EnableField(nameof(StepClimb))] public float StepDownDistance;
        [EnableField(nameof(StepClimb))] public float StepDownOffset;
        [EnableField(nameof(StepClimb))] public float StepDownLastGroundThreshold;
        [Space(5)]
        [Tooltip("EXPERIMENTAL - adds a buffer time between steps")]
        [EnableField(nameof(StepClimb))] public float StepBufferTime;
        [Tooltip("Recommended: 80 or greater then Max Slope Angle if Slope Detection is enabled")]
        [EnableField(nameof(StepClimb))] public float StepMinAngle; //80, greater then slopeAngle
        [Tooltip("Threshold to consider a normal face a step surface. Recommended: >= 0.75")]
        [EnableField(nameof(StepClimb))] public float StepDotThreshold; // 0.75f

        [Header("Util")]
        [Range(0.01f, 0.5f)] public float CoyoteTime = .5f;
        [Range(0.01f, 0.5f)] public float JumpBufferInput = .5f;
        [Range(0.01f, 0.5f), EnableField(nameof(CanJump))] public float JumpApexBuffer;

        [Header("Flags")]
        [Tooltip("Movement logic stills working but input is disabled")]
        public bool CanMove;
        [Tooltip("Movement logic disabled and reset to 0")]
        public bool FreezeMove;
        public bool CanJump;
        [Tooltip("Can change speed based on input (run or walk)")]
        public bool CanChangeSpeed;
        [Space(15)]
        public bool LimitFallSpeed;
        [Tooltip("Keep momentum when high speed and player is on air.")]
        public bool ConserveMomentumOnAir;
        [Space(15)]
        public bool AirMovement;
        [Tooltip("Jumping is higher if player still holding jump button")]
        public bool LargeJump;
        [Tooltip("Extra speed on jump apex (better feeling sometimes)")]
        public bool JumpHang;
        [Space(15)]
        public bool SlopeDetection;
        public bool StepClimb;
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


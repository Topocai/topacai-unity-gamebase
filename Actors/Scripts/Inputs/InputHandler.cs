using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Topacai.Inputs
{
    public enum DeviceType
    {
        Keyboard,
        Controller
    }

    public class OnSchemeChangedArgs
    {
        public string NewScheme { get; set; }
        public DeviceType DeviceType { get; set; }

        public OnSchemeChangedArgs(string newScheme, DeviceType deviceType)
        {
            NewScheme = newScheme;
            DeviceType = deviceType;
        }
    }

    public class OnSchemeChangedEvent : UnityEvent<OnSchemeChangedArgs> { }

    public class InputHandler : MonoBehaviour
    {
        public static InputHandler Instance { get; private set; }

        public static OnSchemeChangedEvent OnSchemeChanged = new OnSchemeChangedEvent();

        public static PlayerInput PlayerInput;

        public static string CurrentScheme;
        public static DeviceType CurrentDevice;

        public static Vector2 MoveDir;
        public static Vector2 CameraDir;

        public static bool IsRunning;
        public static bool RunPressed;
        public static bool InstantRun;

        public static bool IsInteracting;
        public static bool InteractPressed;
        public static bool InstantInteract;

        public static bool IsJumping;
        public static bool JumpPressed;
        public static bool InstantJump;

        public static bool IsCrouching;
        public static bool CrouchPressed;
        public static bool InstantCrouch;

        public static bool PausePressed;

        private float interactHoldTime;
        private float jumpHoldTime;
        private float runHoldTime;
        private float crouchHoldTime;

        private InputAction _Move;
        private InputAction _Camera;
        private InputAction _Run;
        private InputAction _Interact;
        private InputAction _Jump;
        private InputAction _Crouch;
        private InputAction _Pause;

        [SerializeField] private float pressingThreshold = 0.099f;

        private void Start()
        {
           PlayerInput = GetComponent<PlayerInput>();

           _Move = PlayerInput.actions["Move"];
           _Camera = PlayerInput.actions["Camera"];
           _Run = PlayerInput.actions["Run"];
           _Interact = PlayerInput.actions["Interact"];
           _Jump = PlayerInput.actions["Jump"];
           _Pause = PlayerInput.actions["Pause"];
           _Crouch = PlayerInput.actions["Crouch"];

            OnSchemeChangedHandler(PlayerInput.currentControlScheme);
        }

        private void OnSchemeChangedHandler(string scheme)
        {
            CurrentScheme = scheme;

            switch (scheme)
            {
                case "GenericPCScheme":
                    CurrentDevice = DeviceType.Keyboard;
                    break;
                case "GenericJoystickScheme":
                    CurrentDevice = DeviceType.Controller;
                    break;
            }

            OnSchemeChanged.Invoke(new OnSchemeChangedArgs(scheme, CurrentDevice));
        }

        private void OnDisable()
        {
            IsRunning = false;
            IsInteracting = false;
            IsJumping = false;
            RunPressed = false;
            InteractPressed = false;
            JumpPressed = false;
            PausePressed = false;
        }

        private void Update()
        {
            string currentScheme = PlayerInput.currentControlScheme;
            if (currentScheme != CurrentScheme)
            {
                OnSchemeChangedHandler(currentScheme);
            }

            MoveDir = _Move.ReadValue<Vector2>();
            CameraDir = _Camera.ReadValue<Vector2>();

            PausePressed = _Pause.WasPressedThisFrame();

            UpdateInputState(_Run, ref RunPressed, ref IsRunning, ref runHoldTime);
            UpdateInputState(_Interact, ref InteractPressed, ref IsInteracting, ref interactHoldTime);
            UpdateInputState(_Jump, ref JumpPressed, ref IsJumping, ref jumpHoldTime);
            UpdateInputState(_Crouch, ref CrouchPressed, ref IsCrouching, ref crouchHoldTime);

            InstantJump = _Jump.WasPerformedThisFrame();
            InstantInteract = _Interact.WasPerformedThisFrame();
            InstantRun = _Run.WasPerformedThisFrame();
            InstantCrouch = _Crouch.WasPerformedThisFrame();
        }

        public static void UpdateInputState(InputAction action, ref bool isPressed, ref bool isPressing, ref float holdTime, float pressingThreshold = 0.15f)
        {
            if (action.IsPressed())
            {
                holdTime += Time.deltaTime;

                if (holdTime >= pressingThreshold)
                {
                    isPressing = true;
                }

                isPressed = false;
            }
            else
            {
                if (holdTime > 0 && holdTime <= pressingThreshold)
                {
                    isPressed = true;
                }
                else
                {
                    isPressed = false;
                }
                holdTime = 0;
                isPressing = false;
            }
        }
    }
}

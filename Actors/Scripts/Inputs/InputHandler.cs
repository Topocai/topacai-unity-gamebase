using System;
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

    [System.Serializable]
    public class SimpleActionHandler : IComparable
    {
        private InputAction _action;

        public string Name => _action.name;

        public bool IsPressing { get; private set; }
        public bool IsPressed { get; private set; }
        public bool InstantPress { get; private set; }

        public bool All => IsPressing || IsPressed || InstantPress;

        public float PressThreshold { get; private set; }

        private float _holdTime = 0f;
        private bool _isActive = true;

        public SimpleActionHandler(InputAction action, float pressThreshold = 0.075f)
        {
            _action = action;
            PressThreshold = pressThreshold;
        }

        public void SetThreshold(float threshold) => PressThreshold = threshold;

        public void Update(float deltaTime)
        {
            if(!_isActive) return;

            InstantPress = _action.WasPressedThisFrame();

            if (_action.IsPressed())
            {
                _holdTime += deltaTime;

                if (_holdTime >= PressThreshold)
                {
                    IsPressing = true;
                }

                IsPressed = false;
            }
            else
            {
                if (_holdTime > 0 && _holdTime <= PressThreshold)
                {
                    IsPressed = true;
                }
                else
                {
                    IsPressed = false;
                }
                _holdTime = 0;
                IsPressing = false;
            }
        }

        public void Disable() 
        {
            _action.Disable();
            IsPressing = false;
            IsPressed = false;
            InstantPress = false;
            _isActive = false;
        }

        public void Enable()
        {
            _action.Enable();
            IsPressing = true;
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            SimpleActionHandler other = obj as SimpleActionHandler;
            if (other == null)
            {
                string x = obj as string;
                if (x != null)
                {
                    return _action.name.CompareTo(x);
                }
                return 1;
            }
            return _action.name.CompareTo(other._action.name);
        }
    }

    public enum ActionName
    {
        Interact,
        Jump,
        Run,
        Crouch

    }

    public class InputHandler : MonoBehaviour
    {
        public static InputHandler Instance { get; private set; }

        public static OnSchemeChangedEvent OnSchemeChanged = new OnSchemeChangedEvent();

        public static PlayerInput PlayerInput;

        public static string CurrentScheme;
        public static DeviceType CurrentDevice;

        public static Vector2 MoveDir;
        public static Vector2 CameraDir;

        [Obsolete("This property is obsolote, call GetActionHandler with name and check for its values")] public static bool IsRunning;
        [Obsolete("This property is obsolote, call GetActionHandler with name and check for its values")] public static bool RunPressed;
        [Obsolete("This property is obsolote, call GetActionHandler with name and check for its values")] public static bool InstantRun;

        [Obsolete("This property is obsolote, call GetActionHandler with name and check for its values")] public static bool IsInteracting;
        [Obsolete("This property is obsolote, call GetActionHandler with name and check for its values")] public static bool InteractPressed;
        [Obsolete("This property is obsolote, call GetActionHandler with name and check for its values")] public static bool InstantInteract;

        [Obsolete("This property is obsolote, call GetActionHandler with name and check for its values")] public static bool IsJumping;
        [Obsolete("This property is obsolote, call GetActionHandler with name and check for its values")] public static bool JumpPressed;
        [Obsolete("This property is obsolote, call GetActionHandler with name and check for its values")] public static bool InstantJump;

        [Obsolete("This property is obsolote, call GetActionHandler with name and check for its values")] public static bool IsCrouching;
        [Obsolete("This property is obsolote, call GetActionHandler with name and check for its values")] public static bool CrouchPressed;
        [Obsolete("This property is obsolote, call GetActionHandler with name and check for its values")] public static bool InstantCrouch;

        public static bool PausePressed;

        private static HashSet<SimpleActionHandler> _actionHandlers = new();

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

            var move = new SimpleActionHandler(PlayerInput.actions["Move"], pressingThreshold);
            var run = new SimpleActionHandler(PlayerInput.actions["Run"], pressingThreshold);
            var interact = new SimpleActionHandler(PlayerInput.actions["Interact"], pressingThreshold);
            var jump = new SimpleActionHandler(PlayerInput.actions["Jump"], pressingThreshold);
            var crouch = new SimpleActionHandler(PlayerInput.actions["Crouch"], pressingThreshold);

            _actionHandlers.Add(move);
            _actionHandlers.Add(run);
            _actionHandlers.Add(interact);
            _actionHandlers.Add(jump);
            _actionHandlers.Add(crouch);

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

            foreach (var actionHandler in _actionHandlers)
            {
                actionHandler.Update(Time.deltaTime);
            }
        }

        public static void RegisterActionHandler(SimpleActionHandler actionHandler) => _actionHandlers.Add(actionHandler);
        public static SimpleActionHandler GetActionHandler(string name)
        {
            foreach (var handler in _actionHandlers)
            {
                if (handler.Name == name)
                {
                    return handler;
                }
            }

            return null;
        }

        public static SimpleActionHandler GetActionHandler(InputAction action) => GetActionHandler(action.name);

        public static SimpleActionHandler GetActionHandler(ActionName action) => GetActionHandler(action.ToString());

        public static void UnregisterActionHandler(SimpleActionHandler actionHandler) => _actionHandlers.Remove(actionHandler);

        public static void UnregisterActionHandler(string name) => _actionHandlers.Remove(GetActionHandler(name));

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

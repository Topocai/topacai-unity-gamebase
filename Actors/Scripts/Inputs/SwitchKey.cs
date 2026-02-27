using UnityEngine.InputSystem;

using System;

namespace Topacai.Inputs
{
    /// <summary>
    /// SwitchKey holds an InputAction to manage a key that could be switcheable or holdeable (for example, aim input)
    /// </summary> <summary>
    /// 
    /// </summary>
    public class SwitchKey
    {
        /// <summary>
        /// emitted when key is upped or second press on switch mode
        /// </summary>
        public event Action<InputAction.CallbackContext> OnStop;
        /// <summary>
        /// emitted when key is started pressing or when the key is pressed once on switch mode
        /// </summary>
        public event Action<InputAction.CallbackContext> OnStart;

        public bool IsSwitch => !_holdMode;
        public bool IsHold => _holdMode;

        /// <summary>
        /// Returns true if player is holding or didn't press again after first press on switch mode
        /// </summary>
        public bool IsPressing
        {
            get
            {
                if (_holdMode)
                    return _inputAction.IsPressed();

                return _beingPress;
            }
        }

        private bool _holdMode = true;
        private bool _beingPress = false;

        private InputAction _inputAction;
        public InputAction InputAction => _inputAction;

        #region public methods

        public SwitchKey(InputAction input, bool switchMode = false)
        {
            _inputAction = input;
            _holdMode = !switchMode;

            this.Enable();
        }

        public void SetSwitch(bool toSwitch)
        {
            _holdMode = !toSwitch;

            _beingPress = false;

            OnStop?.Invoke(new());
        }

        public void SetInputAction(InputAction input)
        {
            this.Disable();

            _inputAction = input;

            this.Enable();
        }

        #endregion

        #region enable / disable

        public void Enable()
        {
            _inputAction?.Enable();

            _beingPress = false;

            if (_inputAction != null)
            {
                _inputAction.canceled += OnActionStopped;
                _inputAction.started += OnActionStarted;
            }
        }

        public void Disable()
        {
            _inputAction?.Disable();

            _beingPress = false;
            OnStop?.Invoke(new());

            if (_inputAction != null)
            {
                _inputAction.canceled -= OnActionStopped;
                _inputAction.started -= OnActionStarted;
            }
        }

        #endregion

        private void OnActionStarted(InputAction.CallbackContext ctx)
        {
            if (!_holdMode)
            {
                _beingPress = !_beingPress;

                if (_beingPress) OnStart?.Invoke(ctx);
                else OnStop?.Invoke(ctx);

                return;
            }

            OnStart?.Invoke(ctx);
        }

        private void OnActionStopped(InputAction.CallbackContext ctx)
        {
            if (_holdMode) OnStop?.Invoke(ctx);
        }

    }
}
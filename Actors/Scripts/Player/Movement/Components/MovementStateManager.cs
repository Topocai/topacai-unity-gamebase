using System.Collections.Generic;

namespace Topacai.Player.Movement.Components
{
    /// <summary>
    /// Manages state of movement components
    /// in order to check if a component is being used or not
    /// without having to know the class of the component just by name state
    /// </summary>
    public class MovementStateManager
    {
        private Dictionary<string, MovementComponent> _registeredStates = new();

        /// <summary>
        /// Checks for the state of a component only when it is called.
        /// If the component is not registered, it will return false
        /// </summary>
        public bool GetState(string stateName)
        {
            if (_registeredStates.TryGetValue(stateName.ToLower(), out MovementComponent state))
                return state.IsUsing();
            else
                return false;
        }

        public void RegisterState(string stateName, MovementComponent state)
        {
            _registeredStates.TryAdd(stateName.ToLower(), state);
        }
    }
}
using System.Collections.Generic;

namespace Topacai.Player.Movement.Components
{
    /// <summary>
    /// Used by MovementRegistry to hold priority order data
    /// </summary>
    public class MovementData
    {
        public MovementStateManager StateManager = new();

        public readonly List<MovementComponent> Components = new();
        public readonly List<MovementComponent> FirstCall = new();
        public readonly List<MovementComponent> BeforeMove = new();
        public readonly List<MovementComponent> AfterAcceleration = new();
        public readonly List<MovementComponent> BeforeAcceleration = new();
        public readonly List<MovementComponent> GroundChanged = new();
    }

    /// <summary>
    /// Table of priority of a MovementComponent on each callback
    /// </summary>
    [System.Serializable]
    public struct MovementPriority
    {
        public int FirstCall;
        public int BeforeMove;
        public int AfterAcceleration;
        public int BeforeAcceleration;
        public int GroundChanged;
    }
}
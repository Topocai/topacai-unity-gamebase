using System.Collections.Generic;

namespace Topacai.Player.Movement.Components
{
    public struct MovementData
    {
        public MovementStateManager StateManager;
        public HashSet<MovementComponent> Components;
    }
}
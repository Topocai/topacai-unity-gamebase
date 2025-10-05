using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Topacai.Actors.Interactuables
{
    public class InteractuableSelectedEventArgs
    {
        public InteractuableSelectedEventArgs(IInteractuable interactuable, InteractingSystem system, object interacter = null)
        {
            CurrentInteractuable = interactuable;
            LastInteractuable = system.Client;
            Interacter = interacter;
        }
        public IInteractuable LastInteractuable { get; }
        public IInteractuable CurrentInteractuable { get; }
        public object Interacter { get; }
    }

    public class InteractuableInteractEventArgs
    {
        public InteractuableInteractEventArgs(IInteractuable interactuable, object interacter = null)
        {
            Interactuable = interactuable;
            CanInteract = interactuable?.CanInteract ?? null;
            InCooldown = interactuable?.InCooldown ?? null;
            CooldownLeft = interactuable?.CooldownLeft() ?? null;
            Interacter = interacter;
        }
        public IInteractuable Interactuable { get; }
        public bool? CanInteract { get; }
        public bool? InCooldown { get; }
        public float? CooldownLeft { get; }
        public object Interacter { get; }
    }

    public class InteractuableHoldInteractEventArgs : InteractuableInteractEventArgs
    {
        public InteractuableHoldInteractEventArgs(IInteractuable interactuable, object interacter = null) : base(interactuable, interacter)
        {
            CanHoldInteract = interactuable?.CanHoldInteract ?? null;
            HoldInCooldown = interactuable?.HoldInCooldown ?? null;
            HoldCooldownLeft = interactuable?.HoldCooldownLeft() ?? null;
        }
        public bool? CanHoldInteract { get; }
        public bool? HoldInCooldown { get; }
        public float? HoldCooldownLeft { get; }
    }

    public class InteractuableChangedCooldownEventArgs
    {
        public InteractuableChangedCooldownEventArgs(IInteractuable interactuable, bool inCooldown, bool holdInCooldown)
        {
            Interactuable = interactuable;
            InCooldown = inCooldown;
            HoldInCooldown = holdInCooldown;
        }
        public IInteractuable Interactuable { get; }
        public bool InCooldown { get; }
        public bool HoldInCooldown { get; }
    }

    public class InteractuableSelectedEvent : UnityEvent<InteractuableSelectedEventArgs> { }

    public class InteractuableInteractEvent : UnityEvent<InteractuableInteractEventArgs> { }

    public class InteractuableHoldInteractEvent : UnityEvent<InteractuableHoldInteractEventArgs> { }

    public class InteractuableChangedCooldownEvent : UnityEvent<InteractuableChangedCooldownEventArgs> { }

    public class InteractingSystem : MonoBehaviour
    {
        public IInteractuable Client { get; private set; }
        public InteractuableSelectedEvent OnInteractuableSelected { get; private set; } = new InteractuableSelectedEvent();

        public InteractuableInteractEvent OnInteract { get; private set; } = new InteractuableInteractEvent();
        public InteractuableHoldInteractEvent OnHoldInteract { get; private set; } = new InteractuableHoldInteractEvent();
        public InteractuableChangedCooldownEvent OnInteractuableChangedCooldown { get; private set; } = new InteractuableChangedCooldownEvent();

        public void ResetInteractuable()
        {
            if (Client == null) return;
            OnInteractuableSelected.Invoke(new InteractuableSelectedEventArgs(null, this));
            Client.Deselect();
            Client = null;
        }

        public void Interact(object sender = null)
        {
            OnInteract.Invoke(new InteractuableInteractEventArgs(Client, sender));

            if (Client == null) return;

            if (!Client.CanInteract || Client.InCooldown) return;
            
            Client.Interact(sender);
        }

        public void HoldInteract(object sender = null)
        {
            OnHoldInteract.Invoke(new InteractuableHoldInteractEventArgs(Client, sender));
            if (Client == null) return;

            if (!Client.CanHoldInteract || Client.InCooldown) return;

            Client.HoldInteract(sender);
        }

        public void SetInteractuable(IInteractuable interactuable, object sender = null)
        {
            if (Client == interactuable) return;

            OnInteractuableSelected.Invoke(new InteractuableSelectedEventArgs(interactuable, this, sender));

            if (Client != null)
                Client.Deselect(sender);

            Client = interactuable;
            interactuable.Select(sender);
        }

    }
    public interface IInteractuable
    {
        bool CanInteract { get; }
        bool CanHoldInteract { get; }
        bool InCooldown { get; }
        bool HoldInCooldown { get; }

        float CooldownLeft();
        float HoldCooldownLeft();
        void Interact(object sender = null);
        void HoldInteract(object sender = null);
        void Select(object sender = null);
        void Deselect(object sender = null);
    }
}

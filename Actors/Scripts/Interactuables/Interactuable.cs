using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Topacai.Actors.Interactuables
{
    public class InteractuableSelectedEventArgs
    {
        public InteractuableSelectedEventArgs(IInteractuable interactuable)
        {
            CurrentInteractuable = interactuable;
            LastInteractuable = Interactuable.Client;
        }
        public IInteractuable LastInteractuable { get; }
        public IInteractuable CurrentInteractuable { get; }
    }

    public class InteractuableInteractEventArgs
    {
        public InteractuableInteractEventArgs(IInteractuable interactuable)
        {
            Interactuable = interactuable;
            CanInteract = interactuable?.CanInteract ?? null;
            InCooldown = interactuable?.InCooldown ?? null;
            CooldownLeft = interactuable?.CooldownLeft() ?? null;
        }
        public IInteractuable Interactuable { get; }
        public bool? CanInteract { get; }
        public bool? InCooldown { get; }
        public float? CooldownLeft { get; }
    }

    public class InteractuableHoldInteractEventArgs : InteractuableInteractEventArgs
    {
        public InteractuableHoldInteractEventArgs(IInteractuable interactuable) : base(interactuable)
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
        public InteractuableChangedCooldownEventArgs(bool inCooldown, bool holdInCooldown)
        {
            InCooldown = inCooldown;
            HoldInCooldown = holdInCooldown;
        }
        public bool InCooldown { get; }
        public bool HoldInCooldown { get; }
    }

    public class InteractuableSelectedEvent : UnityEvent<InteractuableSelectedEventArgs> { }

    public class InteractuableInteractEvent : UnityEvent<InteractuableInteractEventArgs> { }

    public class InteractuableHoldInteractEvent : UnityEvent<InteractuableHoldInteractEventArgs> { }

    public class InteractuableChangedCooldownEvent : UnityEvent<InteractuableChangedCooldownEventArgs> { }

    public class Interactuable : MonoBehaviour
    {
        public static IInteractuable Client { get; private set; }
        public static InteractuableSelectedEvent OnInteractuableSelected { get; private set; } = new InteractuableSelectedEvent();

        public static InteractuableInteractEvent OnInteract { get; private set; } = new InteractuableInteractEvent();
        public static InteractuableHoldInteractEvent OnHoldInteract { get; private set; } = new InteractuableHoldInteractEvent();
        public static InteractuableChangedCooldownEvent OnInteractuableChangedCooldown { get; private set; } = new InteractuableChangedCooldownEvent();

        private static bool _lastInteractuableCooldown = false;
        private static bool _lastInteractuableHoldCooldown = false;

        public static void ResetInteractuable()
        {
            if (Client == null) return;
            OnInteractuableSelected.Invoke(new InteractuableSelectedEventArgs(null));
            Client.Deselect();
            Client = null;
            _lastInteractuableCooldown = false;
            _lastInteractuableHoldCooldown = false;
        }

        public static void Interact()
        {
            OnInteract.Invoke(new InteractuableInteractEventArgs(Client));

            if (Client == null) return;

            if (!Client.CanInteract || Client.InCooldown) return;
            
            Client.Interact();
        }

        public static void HoldInteract()
        {
            OnHoldInteract.Invoke(new InteractuableHoldInteractEventArgs(Client));
            if (Client == null) return;

            if (!Client.CanHoldInteract || Client.InCooldown) return;

            Client.HoldInteract();
        }

        public static void SetInteractuable(IInteractuable interactuable)
        {
            if (Client == interactuable) return;
            UnityEngine.Debug.Log("Holding Int");
            OnInteractuableSelected.Invoke(new InteractuableSelectedEventArgs(interactuable));

            if (Client != null)
                Client.Deselect();

            Client = interactuable;
            interactuable.Select();

            _lastInteractuableCooldown = Client.InCooldown;
            _lastInteractuableHoldCooldown = Client.HoldInCooldown;
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
        void Interact();
        void HoldInteract();
        void Select();
        void Deselect();
    }
}

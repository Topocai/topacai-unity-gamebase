using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;
using UnityEngine.Events;

namespace Topacai.Actors.Interactuables.SimpleInteractuable
{
    public class SimpleInteractuable : MonoBehaviour, IInteractuable
    {

        [SerializeField] protected UnityEvent _onSelect;
        [SerializeField] protected UnityEvent _onDeselect;
        [SerializeField] protected UnityEvent _onInteract;
        [SerializeField] protected UnityEvent _onHoldInteract;

        [Space(15)]

        [Header("Interactuable")]
        [SerializeField] protected float _cooldownTime = 0f;
        [SerializeField] protected float _holdCooldownTime = 0f;

        [SerializeField] protected bool _canInteract = true;
        [SerializeField] protected bool _canHoldInteract = false;

        public bool CanInteract => _canInteract;
        public bool CanHoldInteract => _canHoldInteract;

        [field: SerializeField, ReadOnly] public bool HoldInCooldown { get; private set; } = false;
        [field: SerializeField, ReadOnly] public bool InCooldown { get; private set; } = true;

        protected float _lastTimeInteracted = 0f;
        protected float _lastTimeHoldInteracted = 0f;

        public float CooldownLeft()
        {
            return Mathf.Clamp(_lastTimeInteracted, 0f, _cooldownTime);
        }

        public virtual void Deselect(object interacter = null)
        {
            Debug.Log("Deselect: " + gameObject.name);
            _onDeselect.Invoke();
        }

        public virtual void Interact(object interacter = null)
        {
            Debug.Log("Interact: " + gameObject.name);
            _lastTimeInteracted = _cooldownTime;
            _onInteract.Invoke();
        }

        public void HoldInteract(object interacter = null)
        {
            Debug.Log("Interacting: " + gameObject.name);
            _onHoldInteract.Invoke();
        }

        public virtual void Select(object interacter = null)
        {
            Debug.Log("Select: " + gameObject.name);
            _onSelect.Invoke();
        }

        protected virtual void FixedUpdate()
        {
            _lastTimeInteracted -= Time.deltaTime;
            //_lastTimeHoldInteracted -= Time.deltaTime;

            InCooldown = _lastTimeInteracted > 0f;
            //HoldInCooldown = _lastTimeHoldInteracted > 0f;
        }

        public float HoldCooldownLeft()
        {
            return Mathf.Clamp(_lastTimeHoldInteracted, 0f, _holdCooldownTime);
        }    
    }
}

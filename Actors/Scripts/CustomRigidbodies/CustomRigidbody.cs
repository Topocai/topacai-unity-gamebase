using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Topacai.CustomPhysics
{
    [RequireComponent(typeof(Rigidbody))]
    public class CustomRigidbody : MonoBehaviour
    {
        public delegate void OnApplyGravity(ref Vector3 gravity);

        [Header("Gravity Settings")]
        [SerializeField] protected float _gravityScale = 1f;
        [Space(10)]
        [SerializeField] private bool _gravityOn = true;
        [SerializeField] protected bool _useCustomGravity = false;
        [SerializeField, ShowField(nameof(_useCustomGravity))] protected Vector3 _customGravity = new Vector3(0, -9.81f, 0);

        public static Vector3 Gravity = new Vector3(0, -9.81f, 0);

        public Vector3 CustomGravity => _customGravity;

        protected Vector3 _inUseGravity = Vector3.zero;
        protected event OnApplyGravity _OnBeforeApplyGravity;

        protected Rigidbody _rb;
        protected virtual void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
        }

        public virtual void SetGravityScale(float v)
        {
            _gravityScale = v;
        }

        public virtual void UseGravity(bool v) => _gravityOn = v;

        public virtual void UseCustomGravity(bool v) => _useCustomGravity = v;

        public virtual void SetCustomGravity(Vector3 v) => _customGravity = v;

        protected virtual void UpdateGravity()
        {
            if (_rb == null)
            {
                _rb = GetComponent<Rigidbody>();
            } 
            _inUseGravity = _useCustomGravity ? _customGravity : Gravity;

            Vector3 finalGravity = _inUseGravity * _gravityScale;

            _OnBeforeApplyGravity?.Invoke(ref _inUseGravity);

            if (_gravityOn)
                _rb.AddForce(finalGravity, ForceMode.Acceleration);
        }

        private void FixedUpdate()
        {
            UpdateGravity();
        }

        public virtual void ResetFallSpeed() => _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
    }
}

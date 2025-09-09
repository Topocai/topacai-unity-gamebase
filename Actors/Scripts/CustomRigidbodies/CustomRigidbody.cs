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
        [SerializeField] protected float gravityScale = 1f;
        [Space(10)]
        [SerializeField] private bool gravityOn = true;
        [SerializeField] protected bool useCustomGravity = false;
        [SerializeField, ShowField(nameof(useCustomGravity))] protected Vector3 customGravity = new Vector3(0, -9.81f, 0);

        public static Vector3 gravity = new Vector3(0, -9.81f, 0);

        protected Vector3 inUseGravity = Vector3.zero;
        protected event OnApplyGravity OnBeforeApplyGravity;

        protected Rigidbody _rb;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
        }

        protected virtual void SetGravityScale(float v)
        {
            gravityScale = v;
        }

        protected virtual void UseGravity(bool v) => gravityOn = v;

        protected virtual void UseCustomGravity(bool v) => useCustomGravity = v;

        protected virtual void SetCustomGravity(Vector3 v) => customGravity = v;

        protected virtual void Gravity()
        {
            if (_rb == null)
            {
                _rb = GetComponent<Rigidbody>();
            } 
            inUseGravity = useCustomGravity ? customGravity : gravity;

            Vector3 finalGravity = inUseGravity * gravityScale;

            OnBeforeApplyGravity?.Invoke(ref inUseGravity);

            if (gravityOn)
                _rb.AddForce(finalGravity, ForceMode.Acceleration);
        }

        private void FixedUpdate()
        {
            Gravity();
        }

        public virtual void ResetFallSpeed() => _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
    }
}

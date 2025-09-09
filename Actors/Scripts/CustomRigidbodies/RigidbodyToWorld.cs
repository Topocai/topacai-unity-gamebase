using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Topacai.CustomPhysics
{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyToWorld : CustomRigidbody
    {
        [SerializeField] private Transform world;

        Vector3 customWorldPosition;
        Vector3 directionToWorld;

        private void Start()
        {
            OnBeforeApplyGravity += CalculateNewGravity;
        }

        private void FixedUpdate()
        {
            base.Gravity();
            RotateRelative();
        }

        private void RotateRelative()
        {
            transform.up = directionToWorld.normalized * -1;
        }

        private void CalculateNewGravity(ref Vector3 gravity)
        {
            customWorldPosition = world.transform.position;
            directionToWorld = (customWorldPosition - transform.position).normalized;

            gravity = directionToWorld * customGravity.magnitude;
#if UNITY_EDITOR
            Debug.DrawRay(transform.position, directionToWorld * 2, Color.red);
            Debug.DrawRay(transform.position, gravity.normalized * 3, Color.blue);
#endif
        }
    }
}


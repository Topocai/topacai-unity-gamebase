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
            BeforeGravity += CalculateNewGravity;
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

        private void CalculateNewGravity()
        {
            customWorldPosition = world.transform.position;
            directionToWorld = (customWorldPosition - transform.position).normalized;

            finalGravity = directionToWorld * customGravity.magnitude;
#if UNITY_EDITOR
            UnityEngine.Debug.DrawRay(transform.position, directionToWorld * 2, Color.red);
            UnityEngine.Debug.DrawRay(transform.position, finalGravity.normalized * 3, Color.blue);
#endif
        }
    }
}


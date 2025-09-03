using EditorAttributes;
using System;
using Topacai.Inputs;
using UnityEngine;

namespace Topacai.Player.Firstperson.Movement.Components
{
    [Obsolete("This system is obselete, use Topacai.Player.Movement.Components system instead")]
    public class Planning : MovementComponent
    {
        [SerializeField] private float _planningVerticalSpeed = 1f;
        [SerializeField] private float _delayToReactive = 0.66f;

        [SerializeField] private bool SHOW_DEBUG;

        [SerializeField, ShowField(nameof(SHOW_DEBUG))] private bool Show_States;
        [SerializeField, ShowField(nameof(Show_States)), ReadOnly] private float _delayTimer = 0f;

        [field: SerializeField, ReadOnly] public static bool _isPlanning { get; private set; }
        [field: SerializeField, ReadOnly] public static bool _canPlan { get; private set; } = true;

        private void Update()
        {
            _canPlan = !(CheckState("isDashing") || CheckState("isGripping")) && _delayTimer < 0f;

            bool newPlanning = InputHandler.IsJumping && _canPlan;

            if (_isPlanning && !newPlanning) _delayTimer = _delayToReactive;
            _isPlanning = newPlanning;
        }

        private void FixedUpdate()
        {
            _delayTimer -= Time.deltaTime;
            if (!_isPlanning) return;

            Vector3 vel = _movement.Rigidbody.linearVelocity;
            Vector3 newVel = new Vector3(vel.x, Mathf.Clamp(vel.y, -_planningVerticalSpeed, _movement.Data.MaxFallSpeed), vel.z);

            _movement.Rigidbody.linearVelocity = Vector3.Lerp(vel, newVel, Time.fixedDeltaTime * 8f);
        }
    }
}

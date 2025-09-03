using System;
using Topacai.Player.Firstperson.Movement.Components;
using UnityEngine;

namespace Topacai.Player.Firstperson.Movement.Components.CameraEffects
{
    [Obsolete("This system is obselete, use Topacai.Player.Movement.Firstperson.Components.CameraEffects system instead")]
    public class ViewVobbing : MovementComponent
    {
        [Space(15)]
        [SerializeField] private ViewVobbingAsset Data;
        [SerializeField] private Transform cameraHolder;
        [SerializeField] private Transform cameraTransform;

        [field: SerializeField] public bool Shake { get; private set; } = true;
        [field: SerializeField] public bool ResetPos { get; private set; } = true;
        [field: SerializeField] public bool TargetFocus { get; private set; } = true;

        private float period;
        private float fixedMovement;
        private Vector3 initialPos;
        private float time;

        private void Start()
        {
            if (cameraHolder == null || cameraTransform == null || Data == null || _movement == null)
            {
                Debug.LogError("CameraStepShake: Missing references");
                this.enabled = false;
            }

            initialPos = cameraHolder.localPosition;
        }

        private void Update()
        {
            fixedMovement = _movement.FlatVel.magnitude;
            if (_movement.LastGroundTime < 0)
            {
                cameraHolder.localPosition = initialPos;
                return;
            }
            if (fixedMovement > Data.startShakeThreshold)
            {
                Vector3 shakeWave = GetShakeVector();
                shakeWave.y = shakeWave.y + Data.verticalOffset;
                cameraHolder.localPosition = Vector3.MoveTowards(cameraHolder.localPosition, cameraHolder.localPosition + shakeWave, (Data.speed * fixedMovement / Data.movementSpeedSmooth) * Time.deltaTime);
            }

            if (ResetPos)
                ResetPosition();
            if (TargetFocus)
                cameraTransform.LookAt(FocusTarget());
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            cameraHolder.localPosition = initialPos;
        }

        public void SetShake(bool value) => Shake = value;
        public void SetResetPos(bool value) => ResetPos = value;
        public void SetTargetFocus(bool value) => TargetFocus = value;

        private void ResetPosition()
        {
            if (cameraHolder.localPosition == initialPos) return;

            cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, initialPos, Data.resetPosSpeed * Time.deltaTime);
        }

        private Vector3 GetShakeVector()
        {
            period = 2 * Mathf.PI / (Data.factor);

            time += Time.deltaTime / Data.smooth;

            float movementFactor = Mathf.Clamp(fixedMovement / 4f, Data.movementFactorMin, Data.movementFactorMax);

            Vector3 result = Vector3.zero;
            result.y += Mathf.Sin((period * time - Data.verticalShift) * Data.frequency) * Data.verticalAmplitude * movementFactor;
            result.x += Mathf.Cos((period * time - Data.horizontalShift) * Data.frequency / 2) * Data.horizontalAmplitude * movementFactor;
            return result;
        }
        private Vector3 FocusTarget()
        {
            Vector3 pos = new Vector3(transform.position.x, transform.position.y + cameraHolder.localPosition.y, transform.position.z);
            pos += cameraHolder.forward * Data.stableStrenght;
            return pos;
        }
    }
}


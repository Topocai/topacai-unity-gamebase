using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Topacai.Player.Movement.Firstperson.Components.CameraEffects
{
    [CreateAssetMenu(fileName = "ViewVobbing Asset", menuName = "ScriptableObjects/FPMovement/CameraEffects/ViewVobbingAsset")]
    public class ViewVobbingAsset : ScriptableObject
    {
        public float startShakeThreshold = 0.34f;
        public float resetPosSpeed = 1;
        public float smooth = 1.2f;
        public float speed = 1f;
        public float stableStrenght = 15f;
        [Space(15)]
        public float factor = 0.4f;
        public float horizontalAmplitude = 0.4f;
        public float verticalAmplitude = 0.4f;
        public float frequency = 1f;
        public float verticalOffset = 0.4f;
        public float verticalShift = -1f;
        public float horizontalShift;
        [Space(15)]
        public float movementFactorMin = 0.2f;
        public float movementFactorMax = 1.2f;
        public float movementSpeedSmooth = 1.2f;
    }

}


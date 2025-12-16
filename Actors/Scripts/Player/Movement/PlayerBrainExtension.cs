using System;
using UnityEngine;

namespace Topacai.Player.Movement
{
    public static class PlayerBrainMovementExtension
    {
        #region Public Utility Methods

        public static void TeleportPlayerTo(this PlayerBrain brain, Transform pos) => TeleportPlayerTo(brain, pos.position);

        public static void TeleportPlayerTo(this PlayerBrain brain, Vector3 pos)
        {
            brain.PlayerReferences.GetModule<PlayerMovementReferences>().Rigidbody.position = pos;
            brain.transform.position = pos;
        }

        public static void TeleportPlayerToUsingPivot(this PlayerBrain brain, Vector3 pos, Vector3 pivot)
        {
            Vector3 offset = brain.transform.TransformPoint(pivot) - brain.transform.position;
            brain.TeleportPlayerTo(pos - offset);
        }

        public static void TeleportPlayerRelativeTo(this PlayerBrain brain, Transform pos, Vector3? origin) => TeleportPlayerRelativeTo(brain, pos.position, origin);

        public static void TeleportPlayerRelativeTo(this PlayerBrain brain, Transform pos) => TeleportPlayerRelativeTo(brain, pos.position, brain.transform.position);

        public static void TeleportPlayerRelativeTo(this PlayerBrain brain, Transform pos, Transform origin) => TeleportPlayerRelativeTo(brain, pos.position, origin.position);

        public static void TeleportPlayerRelativeTo(this PlayerBrain brain, Vector3 pos, Vector3? origin)
        {
            if (origin == null)
                origin = brain.transform.position;

            brain.PlayerReferences.GetModule<PlayerMovementReferences>().Rigidbody.position = (Vector3)origin + pos;
            brain.transform.position = (Vector3)origin + pos;
        }

        #endregion
    }

}

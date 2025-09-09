using Topacai.Player.Movement.Firstperson.Camera;
using Topacai.TDebug;
using UnityEngine;

namespace Topacai.Player.Movement.Firstperson
{
    public class FirstpersonMovement : PlayerMovement
    {
        protected override Vector3 GetMoveDirByCameraAndInput()
        {
            Vector3 cameraDir = FirstpersonCamera.CameraDirFlat;

            Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraDir).normalized;
            Vector3 cameraForward = Vector3.Cross(cameraRight, Vector3.up).normalized;

            Vector3 moveDir = cameraForward * _InputDir.y + cameraRight * _InputDir.x;

            return moveDir.normalized;
        }
    }
}


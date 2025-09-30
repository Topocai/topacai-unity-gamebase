using Topacai.Player.Movement.Firstperson.Camera;
using Topacai.TDebug;
using UnityEngine;

namespace Topacai.Player.Movement.Firstperson
{
    public class FirstpersonMovement : PlayerMovement
    {
        [SerializeField] protected FirstpersonCamera _cameraController;

        protected override void Start()
        {
            base.Start();

            if (_cameraController == null) 
            {
                _cameraController = GetComponent<FirstpersonCamera>();
            }
        }

        protected override Vector3 GetMoveDirByCameraAndInput()
        {
            if (_cameraController == null)
            {
                Debug.LogError("[FirstpersonMovement] CameraController is null");
                return Vector3.zero;
            }
            Vector3 cameraDir = _cameraController.CameraDirFlat;

            Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraDir).normalized;
            Vector3 cameraForward = Vector3.Cross(cameraRight, Vector3.up).normalized;

            Vector3 moveDir = cameraForward * _InputDir.y + cameraRight * _InputDir.x;

            return moveDir.normalized;
        }

        public void SetFPCameraController(FirstpersonCamera cameraController) => _cameraController = cameraController;
    }
}


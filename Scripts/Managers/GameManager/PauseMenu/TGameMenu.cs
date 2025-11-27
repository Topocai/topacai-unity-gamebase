using Topacai.Utils.GameMenu;

using UnityEngine;
using UnityEngine.InputSystem;

namespace Topacai.Managers.GM.PauseMenu
{
    public class TGameMenu : MonoBehaviour
    {
        [SerializeField] private UIMenu _menu = new();
        [SerializeField] private InputAction _backInput;

        [SerializeField] private Transform _gameMenuTransform;

        public void Init()
        {
            if (_gameMenuTransform == null)
            {
                Debug.LogWarning("Game menu transform is null");
                return;
            }

            _menu.SetNode(UIMenu.GetTreeFromTransform(_gameMenuTransform));

            _backInput.Enable();

            _backInput.performed += _ => _menu.BackExitAction();
        }

        private void OnEnable()
        {
            _backInput.Enable();

            _gameMenuTransform.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            _backInput.Disable();

            _gameMenuTransform.gameObject.SetActive(false);
        }
    }
}

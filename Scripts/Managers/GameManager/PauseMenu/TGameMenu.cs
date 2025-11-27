using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Topacai.Utils.GameMenu;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Topacai.Managers.GM.PauseMenu
{
    public class TGameMenu : MonoBehaviour
    {
        public UnityEvent<ClickEvent> OnAnyButtonClicked = new();

        [SerializeField] private UIMenu _menu = new();
        [SerializeField] private InputAction _backInput;

        [SerializeField] private Transform _gameMenuTransform;

        private List<Button> _buttons = new();

        public UIMenu Menu => _menu;

        public void Init()
        {
            if (_gameMenuTransform == null)
            {
                Debug.LogWarning("Game menu transform is null");
                return;
            }

            _menu.SetNode(UIMenu.GetTreeFromTransform(_gameMenuTransform));

            _backInput.Enable();
            _backInput.performed += _ => BackListener();

            CheckForButtons();

            _menu.OnMenuChanged.AddListener(CheckForButtons);
        }

        public void BackListener()
        {
            foreach (var button in _buttons)
            {
                button.UnregisterCallback<ClickEvent>(OnButtonClicked);
            }
            _menu.BackExitAction();
        }

        public void CheckForButtons()
        {
            var b = _menu?.CurrentNode?.View?.Document?.rootVisualElement?.Query<Button>().ToList();

            if (b != null)
            {
                foreach (var button in b)
                    button.RegisterCallback<ClickEvent>(OnButtonClicked);

                if (_buttons?.Count > 0)
                    foreach (var button in _buttons)
                        button.UnregisterCallback<ClickEvent>(OnButtonClicked);
                _buttons = b;
            }
        }

        private void OnEnable()
        {
            _backInput.Enable();
            _gameMenuTransform.gameObject.SetActive(true);

            CheckForButtons();
        }

        private void OnDisable()
        {
            _backInput.Disable();

            _gameMenuTransform.gameObject.SetActive(false);
        }

        private void OnButtonClicked(ClickEvent args)
        {
            OnAnyButtonClicked?.Invoke(args);
        }
    }
}

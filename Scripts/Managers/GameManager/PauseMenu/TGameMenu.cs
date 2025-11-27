using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Topacai.Utils.MenuSystem;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Topacai.Managers.GM.PauseMenu
{
    public class TGameMenu : UIMenuHandler
    {
        public UnityEvent<ClickEvent> OnAnyViewButton = new();
        public UnityEvent<ClickEvent> OnAnyPersistentButton = new();

        [SerializeField] protected UIMenu _overlay = new();
        [SerializeField] private InputAction _backInput;

        [SerializeField] private Transform _gameMenuTransform;
        [SerializeField] private Transform _overlayTransform;

        protected List<Button> _actualViewButtons = new();
        protected List<Button> _persistentButtons = new();

        protected const string MENU_CONTAINER = "menu-container";

        public override UIMenu MainMenu => _menu;

        public UIMenu Overlay => _overlay;

        public void Init()
        {
            if (_gameMenuTransform == null)
            {
                Debug.LogWarning("Game menu transform is null");
                return;
            }

            if (_overlayTransform == null)
            {
                Debug.LogWarning("Overlay transform is null");
                return;
            }

            _menu = _gameMenuTransform.GetComponent<UIMenuHandler>().MainMenu;

            _menu.SetNode(UIMenu.GetTreeFromTransform(_gameMenuTransform.GetChild(0)));
            _overlay.SetNode(UIMenu.GetTreeFromTransform(_overlayTransform));

            _menu.SetRoot(_overlay.CurrentNode.View.Document, MENU_CONTAINER);

            _backInput.Enable();
            _backInput.performed += _ => BackListener();

            CheckForViewButtons();
            CheckForPersistentButtons();

            _menu.OnMenuChanged.AddListener(CheckForViewButtons);

            _overlay.OnMenuChanged.AddListener(CheckForPersistentButtons);
        }

        public void BackListener()
        {
            foreach (var button in _actualViewButtons)
            {
                button.UnregisterCallback<ClickEvent>(OnViewButtonClicked);
            }
            _menu.BackExitAction();
        }

        public void CheckForPersistentButtons()
        {
            var b = _overlay?.CurrentNode?.View?.Document?.rootVisualElement?.Query<Button>().ToList();

            if (b != null)
            {
                foreach (var button in b)
                    button.RegisterCallback<ClickEvent>(OnViewButtonClicked);

                if (_persistentButtons?.Count > 0)
                    foreach (var button in _persistentButtons)
                        button.UnregisterCallback<ClickEvent>(OnViewButtonClicked);
                _persistentButtons = b;
            }
        }

        public void CheckForViewButtons()
        {
            var b = _menu?.CurrentNode?.View?.Document?.rootVisualElement?.Query<Button>().ToList();

            if (b != null)
            {
                foreach (var button in b)
                    button.RegisterCallback<ClickEvent>(OnPersistentButtonClicked);

                if (_actualViewButtons?.Count > 0)
                    foreach (var button in _actualViewButtons)
                        button.UnregisterCallback<ClickEvent>(OnPersistentButtonClicked);
                _actualViewButtons = b;
            }
        }

        private void OnEnable()
        {
            _backInput.Enable();
            _overlayTransform.gameObject.SetActive(true);

            CheckForViewButtons();
            CheckForPersistentButtons();

            _menu.Refresh();
        }

        private void OnDisable()
        {
            _backInput.Disable();

            _overlayTransform.gameObject.SetActive(false);
        }

        private void OnPersistentButtonClicked(ClickEvent args)
        {
            OnAnyPersistentButton?.Invoke(args);
        }

        private void OnViewButtonClicked(ClickEvent args)
        {
            OnAnyViewButton?.Invoke(args);
        }
    }
}

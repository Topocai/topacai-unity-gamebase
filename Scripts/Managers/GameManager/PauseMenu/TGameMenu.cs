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
        [Header("T Game Menu")]
        public UnityEvent<ClickEvent> OnAnyViewButton = new();
        public UnityEvent<ClickEvent> OnAnyPersistentButton = new();

        [SerializeField] protected UIMenu _overlay = new();
        [Space(10)]
        [SerializeField] private InputAction _backInput;
        [Space(15)]
        [SerializeField] private Transform _gameMenuTransform;
        [SerializeField] private Transform _overlayTransform;

        protected List<Button> _actualViewButtons = new();
        protected List<Button> _overlayButtons = new();

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

            #region Menu view init

            _menu = _gameMenuTransform.GetComponent<UIMenuHandler>().MainMenu;

            _menu.SetNode(UIMenu.GetTreeFromTransform(_gameMenuTransform.GetChild(0)));
            _overlay.SetMainView(_overlayTransform.GetComponent<IPage>());

            _menu.SetDocumentAsChildOf(_overlay.MenuDocument, MENU_CONTAINER);

            _backInput.Enable();
            _backInput.performed += _ => BackListener();

            #endregion

            #region GameMenu init

            CheckForMenuViewButtons();
            CheckForOverlayButtons();

            _menu.OnMenuChanged.AddListener(CheckForMenuViewButtons);
            _menu.OnMenuExit.AddListener(() => GameManager.Instance.PauseGame(this, false));

            _overlay.OnMenuChanged.AddListener(CheckForOverlayButtons);

            #endregion
        }

        public void BackListener()
        {
            foreach (var button in _actualViewButtons)
            {
                button.UnregisterCallback<ClickEvent>(OnMenuViewButtonClicked);
            }
            _menu.BackExitAction();
        }

        public void CheckForOverlayButtons()
        {
            var b = _overlay?.MenuDocument?.rootVisualElement?.Query<Button>().ToList();

            if (b != null)
            {
                foreach (var button in b)
                    button.RegisterCallback<ClickEvent>(OnMenuViewButtonClicked);

                if (_overlayButtons?.Count > 0)
                    foreach (var button in _overlayButtons)
                        button.UnregisterCallback<ClickEvent>(OnMenuViewButtonClicked);
                _overlayButtons = b;
            }
        }

        public void CheckForMenuViewButtons()
        {
            var b = _menu?.MenuDocument?.rootVisualElement?.Query<Button>().ToList();

            if (b != null)
            {
                foreach (var button in b)
                    button.RegisterCallback<ClickEvent>(OnOverlayButtonClicked);

                if (_actualViewButtons?.Count > 0)
                    foreach (var button in _actualViewButtons)
                        button.UnregisterCallback<ClickEvent>(OnOverlayButtonClicked);
                _actualViewButtons = b;
            }
        }

        private void OnEnable()
        {
            _backInput.Enable();
            _overlayTransform.gameObject.SetActive(true);

            CheckForMenuViewButtons();
            CheckForOverlayButtons();

            _menu.Refresh();
        }

        private void OnDisable()
        {
            _backInput.Disable();

            _overlayTransform.gameObject.SetActive(false);
        }

        private void OnOverlayButtonClicked(ClickEvent args)
        {
            OnAnyPersistentButton?.Invoke(args);
        }

        private void OnMenuViewButtonClicked(ClickEvent args)
        {
            OnAnyViewButton?.Invoke(args);
        }
    }
}

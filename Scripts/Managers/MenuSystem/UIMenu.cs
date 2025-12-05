using EditorAttributes;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using Topacai.Utils.Editor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Topacai.Utils.MenuSystem
{
    [System.Serializable]
    public class UIMenu
    {
        #region Types declaration

        public enum MenuType
        {
            OneView,
            TreeView
        }

        public class MenuNode
        {
            public IPage Page { get; set; }
            public MenuNode Parent { get; set; }
            public MenuNode[] Children { get; set; }
        }

        #endregion

        #region Fields

        [Header("Events")]
        public UnityEvent OnMenuChanged = new();
        public UnityEvent OnMenuExit = new();

        [Space(10)]
        [Header("UIMenu Settings")]
        [SerializeField] protected MenuType _menuType;
        [SerializeField] protected InterfaceReference<IPage> _mainPageReference;
        protected IPage _mainPage
        {
            get
            {
                return _mainPageReference?.Value ?? null;
            }

            set
            {
                _mainPageReference.Value = value;
            }
        }
        protected MenuNode _mainNode;

        [SerializeField] protected UIDocument _menuDocument;

        [Header("Assets references")]
        [SerializeField] protected VisualTreeAsset _loadingPage;
        [SerializeField] protected VisualTreeAsset _noAvaiblePage;

        public UIDocument MenuDocument => _menuDocument;

        protected UIDocument _rootDocument;
        protected string _rootElementId = "";

        public MenuNode CurrentNode { get; private set; }
        public Stack<MenuNode> ViewStack { get; private set; } = new();

        #endregion

        public UIMenu()
        {
            if (_menuType == MenuType.OneView && _mainPage != null)
            {
                _mainNode = new MenuNode() { Page = _mainPage };
                SetupMainView();
            }
        }

        public UIMenu(UIDocument menuD)
        {
            if (_menuType == MenuType.OneView)
            {
                SetupMainView();
            }
            _menuDocument = menuD;
        }

        protected virtual void SetDocumentOnRoot()
        {
            if (_rootDocument != null)
            {
                var h = _rootElementId != String.Empty ? _rootDocument.rootVisualElement.Q<VisualElement>(_rootElementId) : _rootDocument.rootVisualElement;

                if (h != null)
                    h.Add(MenuDocument.rootVisualElement);
            }
        }

        #region Public Methods

        public virtual void Refresh()
        {
            CurrentNode?.Page.OnEnterCall(null);
            ShowPage(CurrentNode.Page);

            SetDocumentOnRoot();
        }

        public virtual void SetDocumentAsChildOf(UIDocument document, string id = "")
        {
            _rootDocument = document;
            _rootElementId = id;
        }

        public virtual void SetMainView(IPage page)
        {
            if (_menuType == MenuType.OneView && page != null)
            {
                _mainPage = page;
                _mainNode = new MenuNode() { Page = page };
                SetupMainView();
            }
        }

        public virtual void SetNode(MenuNode node)
        {
            if (_menuType == MenuType.OneView)
            {
                _mainNode = node;
            }
            CurrentNode = node;
            CurrentNode?.Page.OnEnterCall(null);
            ShowPage(CurrentNode.Page);
            ViewStack.Clear();
        }

        public virtual MenuNode GetRootNode()
        {
            if (_menuType == MenuType.OneView)
            {
                return CurrentNode;
            }

            var n = CurrentNode;
            while (n.Parent != null)
            {
                n = n.Parent;
            }

            return n;
        }
             
        public virtual void BackExitAction()
        {
            // BackExitAction listen to back petition, checks if the current view 
            // should be closed or executed as back action
            // and pass the logic as a callback to the view
            // in order to avoid execute the back or exit if the view doesn't allow it

            PageCallbackArgs args = new PageCallbackArgs()
            {
                Menu = this
            };

            if (_menuType == MenuType.OneView)
            {
                args.Callback = ExitMenu;
                CurrentNode.Page.OnExitCall(args);
                return;
            }

            var back = ViewStack.Count > 0 ? ViewStack.Peek() : null;

            if (back == null)
            {
                args.Callback = ExitMenu;
            }
            else
            {
                args.Callback = Back;
            }

            CurrentNode.Page.OnExitCall(args);
        }

        public virtual void GoChildren(string id)
        {
            var node = CurrentNode.Children.FirstOrDefault(x => x.Page.Id == id);
            if (node != null)
            {
                NavigateTo(node);
            }
            else
            {
                Debug.LogWarning($"Can't find node with id: {id}");
            }
        }

        protected virtual void WaitForPageLoad(object sender, EventArgs a)
        {
            if (sender is IPage p)
            {
                ShowPage(p);
                p.OnPageUpdated -= WaitForPageLoad;
            }
        }

        public virtual void ShowPage(IPage p)
        {
            if (!p.IsAvaible)
            {
                if (_noAvaiblePage != null)
                    MenuDocument.visualTreeAsset = _noAvaiblePage;
            }
            else if(p.IsLoading)
            {
                p.OnPageUpdated += WaitForPageLoad;
            }

            MenuDocument.visualTreeAsset = p.PageDocument;

            SetDocumentOnRoot();
        }

        public virtual void NavigateTo(MenuNode node, bool back = false, PageCallbackArgs args = null)
        {
            if (node == null) return;

            if (_menuType == MenuType.TreeView)
            {
                if (!back && (ViewStack.Count == 0 || ViewStack.Peek() != CurrentNode))
                {
                    ViewStack.Push(CurrentNode);
                }
            }

            CurrentNode = node;

            var a = args ?? new PageCallbackArgs();
            a.Callback = (_) => ShowPage(node.Page);
            node.Page.OnEnterCall(a);

            OnMenuChanged?.Invoke();
        }

        #endregion

        #region Private/Protected Methods

        protected virtual void SetupMainView()
        {
            CurrentNode = _mainNode;
            CurrentNode?.Page?.OnEnterCall(null);
            ShowPage(CurrentNode.Page);
            OnMenuChanged?.Invoke();
        }

        protected virtual void ExitMenu(PageCallbackArgs args = null)
        {
            if (_menuType == MenuType.TreeView)
            {
                ViewStack.Clear();

                var n = CurrentNode;
                while (n.Parent != null)
                {
                    n = n.Parent;
                }

                NavigateTo(n);
            }
            else
            {
                NavigateTo(_mainNode);
            }

            OnMenuExit?.Invoke();
        }

        protected virtual void Back(PageCallbackArgs args = null)
        {
            var b = ViewStack.Pop();

            if (b == null)
            {
                b = CurrentNode.Parent;
                if (b == null)
                {
                    Debug.LogWarning("Can't find back node");
                    return;
                }
            }
            NavigateTo(b, true);
        }

        #endregion

        /// <summary>
        /// Iterates through the transform and builds a tree of IPageViewers
        /// that are founded starting from the transform to all their children
        /// using the transform hierarchy as a tree
        /// MENUS HANDLERS ARE IGNORED
        /// </summary>
        /// <param name="t"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static MenuNode GetTreeFromTransform(Transform t, UIMenu.MenuNode parent = null)
        {
            if (t.TryGetComponent(out IPage viewer))
            {
                if (t.TryGetComponent(out UIMenuHandler uiMenuHandler))
                {
                    return null;
                }

                var node = new UIMenu.MenuNode()
                {
                    Page = viewer,
                    Parent = parent
                };

                if (t.childCount > 0)
                {
                    UIMenu.MenuNode[] childs = new UIMenu.MenuNode[t.childCount];

                    for (int i = 0; i < t.childCount; i++)
                    {
                        childs[i] = GetTreeFromTransform(t.GetChild(i), node);
                    }

                    node.Children = childs;
                }

                return node;
            }

            return null;
        }
    }
}



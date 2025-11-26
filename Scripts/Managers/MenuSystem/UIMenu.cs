using EditorAttributes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Topacai.Utils.GameMenu
{
    public interface IPageViewer
    {
        public IPage Page { get; }
        public void Back(Action callback);
        public void Back(Action callback, IPage page);
    }

    [System.Serializable]
    public class UIMenu
    {
        public enum MenuType
        {
            OneView,
            TreeView
        }

        public class MenuNode
        {
            public IPageViewer View { get; set; }
            public MenuNode Parent { get; set; }
            public MenuNode[] Children { get; set; }
        }

        [SerializeField] private MenuType _menuType;
        [SerializeField] private MenuNode _mainView;

        public MenuNode CurrentNode { get; private set; }
        public Stack<MenuNode> ViewStack { get; private set; } = new();

        public void SetNode(MenuNode node)
        {
            CurrentNode = node;
            ViewStack.Clear();
        }

        public UIMenu()
        {
            if (_menuType == MenuType.TreeView)
            {
                
            }
            else
            {
                CurrentNode = _mainView;
            }
        }

        public void BackExitAction()
        {
            if (_menuType == MenuType.OneView)
            {
                CurrentNode.View.Page.OnExitCall(ExitView);
                return;
            }

            var back = ViewStack.Count > 0 ? ViewStack.Peek() : null;

            if (back == null)
            {
                CurrentNode.View.Page.OnExitCall(ExitView);
                return;
            }

            CurrentNode.View.Back(Back, back.View.Page);
        }

        public void NavigateTo(MenuNode node, bool back = false)
        {
            if (node == null) return;

            if (!back && (ViewStack.Count == 0 || ViewStack.Peek() != CurrentNode))
            {
                ViewStack.Push(CurrentNode);
            }

            CurrentNode = node;

            node.View.Page.OnEnterCall(null);
        }

        private void ExitView()
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
                NavigateTo(_mainView);
            }
        }

        private void Back()
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
    }
}



using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Topacai.Utils.MenuSystem
{
    public class SimpleViewer : MonoBehaviour, IPageController, IPage
    {
        #region Fields

        [Header("View settings")]
        [Tooltip("Allow back action from this view")]
        [SerializeField] protected bool _isBackeable;
        [Tooltip("Allow 'exit' from view")]
        [SerializeField] protected bool _isExitable;
        [Tooltip("Page document to show on view")]
        [SerializeField] protected bool _keepParent = true;
        [Header("Page settings")]
        [SerializeField] protected VisualTreeAsset _pageDocument;
        [SerializeField] protected string _pageName;

        protected VisualElement _rootElement = null;

        protected UIDocument _documentComponent;
        protected bool _isLoading = false;

        #region Interface fields/properties implementation

        public virtual string Id => _pageName;
        public virtual IPage Page => this;
        public virtual VisualTreeAsset PageDocument => _pageDocument;
        public virtual bool IsLoading => _isLoading;

        #endregion

        #endregion

        protected virtual void Start()
        {
            _documentComponent = _documentComponent ?? GetComponent<UIDocument>();
        }

        private void CallbackArgs(PageCallbackArgs c)
        {
            c ??= new PageCallbackArgs();

            c.Page = this;
            c.PageController = this;
            c?.Callback?.Invoke(c);
        }

        #region Interface methods implementation

        public virtual void Back(PageCallbackArgs callback)
        {
            if (_isBackeable)
            {
                CallbackArgs(callback);
            }
        }

        public virtual void Back(PageCallbackArgs callback, IPage page)
        {
            if (_isBackeable) 
            {
                CallbackArgs(callback);
            } 
        }

        public virtual void OnEnterCall(PageCallbackArgs callback)
        {
            CallbackArgs(callback);
        }

        public virtual void OnExitCall(PageCallbackArgs callback)
        {
            CallbackArgs(callback);
        }

        #endregion
    }
}



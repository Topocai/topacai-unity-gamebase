using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Topacai.Utils.MenuSystem
{
    public class SimplePage : MonoBehaviour, IPage
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

        public virtual event EventHandler OnPageUpdated;

        public virtual string Id => _pageName;
        public virtual IPage Page => this;
        public virtual VisualTreeAsset PageDocument => _pageDocument;
        public virtual bool IsLoading => _isLoading;
        public virtual bool IsAvaible { get; protected set; }

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
            IsAvaible = true;
            CallbackArgs(callback);
        }

        public virtual void OnExitCall(PageCallbackArgs callback)
        {
            IsAvaible = false;
            CallbackArgs(callback);
        }

        #endregion
    }
}



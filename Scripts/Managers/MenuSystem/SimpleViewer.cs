using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Topacai.Utils.MenuSystem
{
    [RequireComponent(typeof(UIDocument))]
    public class SimpleViewer : MonoBehaviour, IPageViewer, IPage
    {
        #region Fields

        [Header("View settings")]
        [Tooltip("Allow back action from this view")]
        [SerializeField] protected bool _isBackeable;
        [Tooltip("Allow 'exit' from view")]
        [SerializeField] protected bool _isExitable;
        [Tooltip("Page document to show on view")]
        [SerializeField] protected VisualTreeAsset _pageDocument;

        protected UIDocument _documentComponent;
        protected bool _isLoading = false;

        #region Interface fields/properties implementation

        public virtual IPage Page => this;
        public virtual UIDocument Document
        {
            get
            {
                if (_documentComponent == null)
                {
                    _documentComponent = GetComponent<UIDocument>();
                }

                return _documentComponent;
            }
        }
        public virtual VisualTreeAsset PageDocument => _pageDocument;
        public virtual bool IsLoading => _isLoading;

        #endregion

        #endregion

        protected virtual void Start()
        {
            _documentComponent = _documentComponent ?? GetComponent<UIDocument>();
        }

        #region Interface methods implementation

        public virtual void Back(Action callback)
        {
            if (_isBackeable)
            {
                _documentComponent.enabled = false;
                callback?.Invoke();
            }
        }

        public virtual void Back(Action callback, IPage page)
        {
            if (_isBackeable) 
            {
                _documentComponent.enabled = false;
                callback?.Invoke();
            } 
        }

        public virtual void OnEnterCall(Action callback)
        {
            _documentComponent.visualTreeAsset = PageDocument;

            _documentComponent.enabled = true;
            callback?.Invoke();
        }

        public virtual void OnExitCall(Action callback)
        {
            _documentComponent.enabled = false;
            callback?.Invoke();
        }

        #endregion
    }
}



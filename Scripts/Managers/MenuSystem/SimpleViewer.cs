using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Topacai.Utils.GameMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class SimpleViewer : MonoBehaviour, IPageViewer, IPage
    {
        [SerializeField] protected bool _isBackeable;
        [SerializeField] protected bool _isExitable;

        protected UIDocument _documentComponent;

        [SerializeField] protected bool _isLoading;
        [SerializeField] protected VisualTreeAsset _pageDocument;

        public IPage Page => this;
        public UIDocument Document
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
        public VisualTreeAsset PageDocument => _pageDocument;
        public bool IsLoading => _isLoading;

        protected virtual void Start()
        {
            _documentComponent = GetComponent<UIDocument>();
        }

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
    }
}



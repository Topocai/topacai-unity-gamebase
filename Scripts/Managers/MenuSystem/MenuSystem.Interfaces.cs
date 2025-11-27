using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Topacai.Utils.MenuSystem
{
    public partial interface IPage
    {
        public bool IsLoading { get; }
        public string Id { get; }
        public VisualTreeAsset PageDocument { get; }
        public void OnExitCall(PageCallbackArgs args);
        public void OnEnterCall(PageCallbackArgs args);
    }

    public interface IPageController
    {
        public IPage Page { get; }
        public void Back(PageCallbackArgs args);
        public void Back(PageCallbackArgs args, IPage page);
    }

    public class PageCallbackArgs 
    {
        public object Sender { get; set; }

        public Action<PageCallbackArgs> Callback { get; set; }
        public UIMenu Menu { get; set; }
        public IPageController PageController { get; set; }
        public IPage Page { get; set; }
    }
}



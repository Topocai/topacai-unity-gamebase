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
        public void OnExitCall(Action callback);
        public void OnEnterCall(Action callback);
    }

    public interface IPageController
    {
        public IPage Page { get; }
        public void Back(Action callback);
        public void Back(Action callback, IPage page);
    }
}



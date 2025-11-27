using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Topacai.Utils.MenuSystem
{
    public partial interface IPage
    {
        public bool IsLoading { get; }
        public VisualTreeAsset PageDocument { get; }
        public void OnExitCall(Action callback);
        public void OnEnterCall(Action callback);
    }

    public interface IPageViewer
    {
        public IPage Page { get; }
        public UIDocument Document { get; }
        public void Back(Action callback);
        public void Back(Action callback, IPage page);
    }
}



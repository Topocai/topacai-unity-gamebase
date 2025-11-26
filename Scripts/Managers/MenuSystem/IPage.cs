using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Topacai.Utils.GameMenu
{
    public partial interface IPage
    {
        public bool IsLoading { get; }
        public VisualTreeAsset PageDocument { get; }
        public void OnExitCall(Action callback);
        public void OnEnterCall(Action callback);
    }
}



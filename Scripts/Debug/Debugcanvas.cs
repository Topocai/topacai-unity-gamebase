using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Topacai.TDebug
{
    public class Debugcanvas : MonoBehaviour
    {
        public static Debugcanvas Instance;

        private void Awake()
        {
            Instance = this;
        }
#if UNITY_EDITOR
        [SerializeField] private Transform debugLogs;

        public void AddTextToDebugLog(string suffix, string text)
        {
            if (debugLogs == null) return;
            foreach (Transform child in debugLogs)
            {
                if (child.name == suffix)
                {
                    child.GetComponent<TextMeshProUGUI>().text = $"{suffix}: {text}";
                    return;
                }
            }

            GameObject newChild = new GameObject(suffix, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            newChild.transform.SetParent(debugLogs, false);

            newChild.GetComponent<TextMeshProUGUI>().text = $"{suffix}: {text}";
        }
#endif
    }
}

#if UNITY_EDITOR

using UnityEngine;

namespace Topacai.Utils.GameObjects.AttachableSO.Editor
{
    using UnityEditor;

    [InitializeOnLoad]
    public static class AttachableSODropInjector
    {
        static AttachableSODropInjector()
        {
            Editor.finishedDefaultHeaderGUI += OnFinishedDefaultHeaderGUI;
        }

        private static void OnFinishedDefaultHeaderGUI(Editor editor)
        {
            if (editor.target is GameObject go)
            {
                DrawSOArea(go);
            }
        }

        private static void DrawSOArea(GameObject go)
        {
            GUILayout.Space(10);
            GUILayout.Label("Scriptable Attachment", EditorStyles.boldLabel);

            Rect dropArea = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag ScriptableObject here");

            /// Check if user is dragging something into the area
            /// and when its droped, check if it is a ScriptableObject
            /// and attach it adding an AttachableScriptableObject for each one
            Event guiEvent = Event.current;
            if (!dropArea.Contains(guiEvent.mousePosition)) return;

            if (guiEvent.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                guiEvent.Use();
            }
            else if (guiEvent.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                guiEvent.Use();

                foreach (Object obj in DragAndDrop.objectReferences)
                {
                    if (obj is ScriptableObject so)
                    {
                        AttachableScriptableObject attach = go.AddComponent<AttachableScriptableObject>();

                        attach.Assign(so);
                    }
                }
            }
        }
    }
}

#endif

#if UNITY_EDITOR

using UnityEngine;

namespace Topacai.Utils.GameObjects.AttachableSO.Editor
{

    using UnityEditor;

    [CustomEditor(typeof(AttachableScriptableObject))]
    /// <summary>
    /// Shows a stylized editor for the AttachableScriptableObject component.
    /// just to drag and drop an scriptable object or to remove the component
    /// from the gameobject
    /// </summary>
    public class AttachableSOEditor : Editor
    {

        // This script was stylized by IA, u can see the original code on previous
        // commit that contains only the logic to serealize the attached SO

        private SerializedProperty scriptableProp;

        private GUIStyle _titleStyle;
        private GUIStyle _subtitleStyle;
        private GUIStyle _boxStyle;

        private void OnEnable()
        {
            scriptableProp = serializedObject.FindProperty("_attachable");

            _titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft
            };

            _subtitleStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 8,
                fontStyle = FontStyle.Italic,
                wordWrap = true,
                normal = { textColor = new Color(0.75f, 0.75f, 0.75f) }
            };

            _boxStyle = new GUIStyle("HelpBox")
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.Space(5);

            EditorGUILayout.BeginVertical(_boxStyle);

            EditorGUILayout.LabelField("Scriptable Object Attachment", _titleStyle);
            GUILayout.Space(2);
            EditorGUILayout.LabelField(
                "Use transform extensions TryGetScriptableObject, GetScriptableObject and GetScriptableObjects to search for attached ScriptableObjects.",
                _subtitleStyle
            );

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(scriptableProp, new GUIContent("Attached Asset"), true);

            GUILayout.Space(10);

            GUI.backgroundColor = new Color(0.85f, 0.45f, 0.45f);
            if (GUILayout.Button("Detach Component", GUILayout.Height(18)))
            {
                RemoveThisComponent();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private void RemoveThisComponent()
        {
            AttachableScriptableObject targetComp = (AttachableScriptableObject)target;

            if (EditorUtility.DisplayDialog(
                "Detach Component",
                "Are you sure you want to remove this component from the GameObject?",
                "Remove",
                "Cancel"))
            {
                Undo.DestroyObjectImmediate(targetComp);
            }
        }
    }

}

#endif
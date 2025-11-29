#if UNITY_EDITOR

using UnityEngine;

namespace Topacai.Utils.GameObjects.AttachableSO.Editor
{

    using UnityEditor;

    [CustomEditor(typeof(AttachableScriptableObject))]
    public class AttachableSOEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SerializedProperty scriptableProp = serializedObject.FindProperty("_attachable");

            GUILayout.Space(10);

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Attach a ScriptableObject", EditorStyles.boldLabel);

            GUIStyle bigField = new GUIStyle(EditorStyles.objectField)
            {
                fontSize = 12,
                fixedHeight = 40
            };

            EditorGUILayout.PropertyField(scriptableProp, GUIContent.none, true, GUILayout.Height(40));
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }

}

#endif
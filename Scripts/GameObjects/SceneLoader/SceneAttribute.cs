using UnityEngine;

namespace Topacai.Static.GameObjects.Scenes
{
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Attribute that converts a string field into an enum that shows all scenes in build scenes list
    /// as options
    /// </summary>
    public class SceneNameAttribute : PropertyAttribute { }

    [CustomPropertyDrawer(typeof(SceneNameAttribute))]
    public class SceneNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var scenes = EditorBuildSettings.scenes;
            string[] sceneNames = new string[scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
            {
                sceneNames[i] = System.IO.Path.GetFileNameWithoutExtension(scenes[i].path);
            }

            int index = Mathf.Max(0, System.Array.IndexOf(sceneNames, property.stringValue));
            index = EditorGUI.Popup(position, label.text, index, sceneNames);
            property.stringValue = sceneNames[index];
        }
    }
#endif

}

using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Topacai.Utils.Editor
{
    [System.Serializable]
    /// <summary>
    /// Implements an interface referencer drawer in inspector to drag and drop a
    /// specific interface type (implemented on a class that can be referenced by Unity.Object type)
    /// </summary>
    public class InterfaceReference<T> where T : class
    {
        [SerializeField] private Object _component;

        private T customVal = null;
        public T Value
        {
            get
            {
                return customVal ?? _component as T;
            }

            set
            {
                customVal = value;
            }
        }
        public Object Component => _component;
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InterfaceReference<>))]
    public class InterfaceReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty componentProp = property.FindPropertyRelative("_component");

            EditorGUI.BeginProperty(position, label, property);

            Object obj = EditorGUI.ObjectField(position, label, componentProp.objectReferenceValue, typeof(Component), true);

            if (obj != null)
            {
                var interfaceType = fieldInfo.FieldType.GetGenericArguments()[0];

                if (interfaceType.IsAssignableFrom(obj.GetType()))
                {
                    componentProp.objectReferenceValue = obj;
                }
                else
                {
                    Debug.LogError($"{obj.name} does not implement {interfaceType.Name}");
                    componentProp.objectReferenceValue = null;
                }
            }
            else
            {
                componentProp.objectReferenceValue = null;
            }

            EditorGUI.EndProperty();
        }
#endif
    }
}

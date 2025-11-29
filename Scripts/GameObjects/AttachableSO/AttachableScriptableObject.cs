using UnityEngine;

namespace Topacai.Utils.GameObjects.AttachableSO
{    
    public class AttachableScriptableObject : MonoBehaviour
    {
        [SerializeField] private ScriptableObject _attachable;

        public bool IsType<T>()
        {
            return _attachable is T;
        }

        public T GetAs<T>() where T : ScriptableObject
        {
            return _attachable as T;
        }

        public void Assign(ScriptableObject so) => _attachable = so;

        public ScriptableObject ScriptableAttached => _attachable;
    }
}

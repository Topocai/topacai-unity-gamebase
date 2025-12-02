using UnityEngine;

using Topacai.Utils.GameObjects;
using System.Linq;

namespace Topacai.Utils.GameObjects.AttachableSO
{
    public static partial class AttachableSOTransformExtensions
    {
        public static bool TryGetScriptableObject<T>(this Transform t, out T result) where T : ScriptableObject
        {
            if (t.TryGetComponent<AttachableScriptableObject>(out var attachable))
            {
                if (attachable.IsType<T>())
                {
                    result = attachable.GetAs<T>();
                    return true;
                }
                else
                {
                   var a = t.GetComponents<AttachableScriptableObject>().First(x => x.IsType<T>());
                    if (a != null)
                    {
                        result = a.GetAs<T>();
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }

        public static T GetScriptableObject<T>(this Transform t) where T : ScriptableObject
        {
            T result = null;

            t.TryGetScriptableObject(out result);
            return result;
        }

        public static T[] GetScriptableObjects<T>(this Transform t) where T : ScriptableObject
        {
            if (!t.TryGetScriptableObject<T>(out var result))
            {
                return null;
            }
            else
            {
                return t.GetComponents<AttachableScriptableObject>().Where(x => x.IsType<T>()).Select(x => x.GetAs<T>()).ToArray();
            }
        }
    }
}

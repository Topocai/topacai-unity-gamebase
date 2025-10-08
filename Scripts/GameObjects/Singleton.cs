using UnityEngine;

namespace Topacai.Utils.GameObjects
{
    /// <summary>
    /// Generic Singleton, use as a base class to made a singleton monobehaviour
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        /// <summary>
        /// Current singleton instance
        /// </summary>
        private static T instance;
        /// <summary>
        /// Gets the current singleton instance, and if it doesn't exist, creates one
        /// </summary>
        public static T Instance 
        {
            get 
            { 
                /// Checks for the existence of an actual instance
                /// if is not setted, it searches for one, if doesn't exist, it creates one before returning
                if(instance == null)
                {
                    instance = (T)GameObject.FindFirstObjectByType(typeof(T));
                    if (instance == null)
                        CreateSingleton();
                }

                return instance;
            }
            private set => Instance = value;
        }

        private static void CreateSingleton()
        {
            // Searchs for an existing instance
            instance = (T)GameObject.FindFirstObjectByType(typeof(T));
            var instanceGo = instance?.gameObject;

            if (instance == null)
            {
                /// Create a gameobject with the component singleton
                /// and sets as instance
                instanceGo = new GameObject(typeof(T).Name);
                instance = instanceGo.AddComponent<T>(); 
            }

            if (Application.isPlaying)
                DontDestroyOnLoad(instanceGo);
        }

        protected virtual void Awake()
        {
            /// If the instance is not setted when the object is created
            /// it sets as instance
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
    }
}

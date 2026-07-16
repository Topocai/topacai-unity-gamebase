using UnityEngine;

namespace Topacai.Managers.PersistentResources
{
    /// <summary>
    /// Class that manages objects on Assets/Resources/PERSIST_OBJECTS_PATH/
    /// any gameobject there will be instantiated before any scene is loaded as "DontDestroyOnLoad"
    /// </summary>
    public static class PersistentResourcesManager
    {
        private const string PERSIST_OBJECTS_PATH = "PersistResources";

        // Call it before a scene load
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Execute()
        {
            Object[] persistObjects = Resources.LoadAll(PERSIST_OBJECTS_PATH, typeof(GameObject));
            foreach (var item in persistObjects)
            {
                Object.DontDestroyOnLoad(Object.Instantiate(item));
                Debug.Log("[PersistentResources] Object: " + item.ToString() + " loaded");
            }
        }
    }
}


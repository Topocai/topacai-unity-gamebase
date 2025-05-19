using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Global.Managers
{
    public static class PersistObjects
    {
        private const string PERSIST_OBJECTS_PATH = "PersistObjects";
        // Call it before a scene load
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Execute()
        {
            Object[] persistObjects = Resources.LoadAll(PERSIST_OBJECTS_PATH, typeof(GameObject));
            foreach (var item in persistObjects)
            {
                Object.DontDestroyOnLoad(Object.Instantiate(item));
                Debug.Log("Persist Object: " +item.ToString() + " loaded");
            }
        }
    }
}


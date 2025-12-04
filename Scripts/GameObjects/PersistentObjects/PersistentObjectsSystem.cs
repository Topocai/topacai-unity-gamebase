using System.Collections.Generic;
using System.Linq;
using Topacai.Managers.GM;
using Topacai.Utils.SaveSystem;

using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Topacai.Utils.GameObjects.Persistent
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class PersistentObjectsSystem
    {
        private const string DATA_KEY = "PersistentObjectsData";

        /// <summary>
        /// Event that is called when data is recovered by every category
        /// </summary>
        public static UnityEvent<string> OnDataRecoveredEvent = new UnityEvent<string>();

        // category -> uniqueID -> data
        /// <summary>
        /// Contains all the data recovered, indexed by category and then by uniqueID with the data recovered
        /// Data[category][uniqueID] => data as IPersistentDataObject
        /// </summary>
        private static Dictionary<string, Dictionary<string, IPersistentDataObject>> PersistentDataByCategory = new();

        public static bool CategoryExists(string category) => PersistentDataByCategory.ContainsKey(category);

        public static void AddDataToCategory(string category, Dictionary<string, IPersistentDataObject> data) => PersistentDataByCategory.Add(category, data);

        public static void SetDataInCategory(string category, string id, IPersistentDataObject data) => PersistentDataByCategory[category][id] = data;

        #region Event Subscriptions

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void SuscribeToProfileEvent() => SaveSystemClass.OnProfileChanged.AddListener(OnProfileChanged);

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void SuscribeToSceneLoaded() => GameManager.OnSceneLoaded.AddListener(OnNewSceneLoaded);

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void SuscribeToSceneUnload() => GameManager.OnUnloadingScene.AddListener(OnSceneUnloading);

        #endregion

        private static void OnProfileChanged(UserProfile profile)
        {
            Debug.Log("[PersistentObjects] Profile changed detected, recovering all objects");

            PersistentDataByCategory.Clear();
            RecoverAllObjects();
        }

        /// <summary>
        /// Same as RecoverAllObjects but filter the objects checked by the new scene loaded
        /// </summary>
        /// <param name="args"></param>
        private static void OnNewSceneLoaded(GameManager.OnSceneArgs args)
        {
            PersistentObjectMonobehaviour[] objects = GameObject.FindObjectsByType<PersistentObjectMonobehaviour>(FindObjectsSortMode.None);

            objects = objects.Where(x => x.gameObject.scene.name == args.TargetScene.name).ToArray();

            Debug.Log("Recovering new scene objects");

            foreach (var persistentObject in objects)
            {
                if (!PersistentDataByCategory.ContainsKey(persistentObject.Category))
                    PersistentDataByCategory.Add(persistentObject.Category, new());
            }

            var categoryKeys = PersistentDataByCategory.Keys.ToList();

            foreach (var category in categoryKeys)
            {
                RecoverCategory(category);
            }
        }

        private static void OnSceneUnloading(object sender, GameManager.OnSceneArgs args)
        {
            PersistentObjectMonobehaviour[] objects = GameObject.FindObjectsByType<PersistentObjectMonobehaviour>(FindObjectsSortMode.None);

            objects = objects.Where(x => x.gameObject.scene.name == args.TargetScene.name).ToArray();

            Debug.Log("Saving scene objects");

            foreach (var persistentObject in objects)
            {
                if (!PersistentDataByCategory.ContainsKey(persistentObject.Category))
                    PersistentDataByCategory.Add(persistentObject.Category, new());
            }

            var categoryKeys = PersistentDataByCategory.Keys.ToList();

            foreach (var category in categoryKeys)
            {
                SaveCategoryObjects(category);
            }
        }

        /// <summary>
        /// Helper to call when game manager save game event is called
        /// </summary>
        private static void OnSaveGame(object sender, System.EventArgs e)
        {
            Debug.Log("[PersistentObjects] Saving all objects");
            SaveAllObjects();
        }

        public static void SaveAllObjects()
        {
            PersistentObjectMonobehaviour[] objects = GameObject.FindObjectsByType<PersistentObjectMonobehaviour>(FindObjectsSortMode.None);

            foreach (var persistentObject in objects)
            {
                if (!PersistentDataByCategory.ContainsKey(persistentObject.Category))
                    PersistentDataByCategory.Add(persistentObject.Category, new());
            }

            var categoryKeys = PersistentDataByCategory.Keys.ToList();

            foreach (var category in categoryKeys)
            {
                SaveCategoryObjects(category);
            }
        }
        /// <summary>
        /// Saves all persistent objects in a specific category.
        /// </summary>
        /// <param name="category">Category to save.</param>
        public static void SaveCategoryObjects(string category)
        {
            PersistentObjectMonobehaviour[] objects = GameObject.FindObjectsByType<PersistentObjectMonobehaviour>(FindObjectsSortMode.None);

            if (objects.Length == 0)
            {
                Debug.LogWarning($"[PersistentObjects] No persistent objects found for category '{category}'");
                return;
            }

            var categoryData = new PersistentObjectsCategoryData(category, new());

            foreach (var objectInstance in objects)
            {
                objectInstance.UpdateData();
                categoryData.ObjectList.Add(objectInstance.GetUniqueID(), objectInstance.PersistentData);
            }

            SaveSystemClass.SaveLevelDataToProfile(categoryData, category, DATA_KEY);
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        /// <summary>
        /// Recover all persistent objects
        /// </summary>
        public static void RecoverAllObjects()
        {
            PersistentObjectMonobehaviour[] objects = GameObject.FindObjectsByType<PersistentObjectMonobehaviour>(FindObjectsSortMode.None);

            foreach (var persistentObject in objects)
            {
                if (!PersistentDataByCategory.ContainsKey(persistentObject.Category))
                    PersistentDataByCategory.Add(persistentObject.Category, new());
            }

            var categoryKeys = PersistentDataByCategory.Keys.ToList();

            foreach (var category in categoryKeys)
            {
                RecoverCategory(category);
            }
        }
        /// <summary>
        /// Recovers all persistent objects in a specific category.
        /// </summary>
        /// <param name="category">Category to recover.</param>
        public static void RecoverCategory(string category)
        {
            Debug.Log($"[PersistentObjects] Recovering category '{category}'");
            // Load category's data from save system
            PersistentObjectsCategoryData data = default(PersistentObjectsCategoryData);
            bool dataExists = SaveSystemClass.GetLevelData<PersistentObjectsCategoryData>(category, out data, DATA_KEY);

            if (!dataExists)
            {
                Debug.LogWarning($"[PersistentObjects] No data found for category '{category}'");
            }
            else
            {
                PersistentDataByCategory[data.Category] = data.ObjectList;
            }

            OnDataRecoveredEvent?.Invoke(category);
        }

        /// <summary>
        /// Retrieves the persistent data object for a given unique ID and category.
        /// </summary>
        /// <param name="uniqueID">Unique ID of the object.</param>
        /// <param name="category">Category of the object.</param>
        /// <returns>Persistent data object, or null if not found.</returns>
        public static IPersistentDataObject GetObjectData(string uniqueID, string category)
        {
            if (PersistentDataByCategory.TryGetValue(category, out var cat))
            {
                if (cat.TryGetValue(uniqueID, out var obj))
                {
                    return obj;
                }
            }

            Debug.LogWarning($"[PersistentObjects] Object data not found for ID: {uniqueID} in category: {category}");
            return null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void SuscribeToSaveGameEvent()
        {
            SaveSystem.SaveSystemClass.OnSaveGameEvent += OnSaveGame;
        }
    }
}

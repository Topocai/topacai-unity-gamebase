using System.Collections;
using System.Collections.Generic;
using Topacai.Utils.Editor;
using Topacai.Utils.GameObjects.Unique;
using Topacai.Utils.SaveSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Topacai.Utils.GameObjects.Persistent
{
    
#if UNITY_EDITOR

    [InitializeOnLoad]
    /// <summary>
    /// When exiting play mode and entering edit mode, recover all persistent objects
    /// and sets it transform to show the current state in inspector
    /// </summary>
    public static class PersistentObjectStateWatcher
    {
        [InitializeOnLoadMethod]
        private static void AddPersistentObjectListener() => ModeStateWatcher.OnPlayModeStateChangedEvent += PersistentObjectWatcher;

        private static void PersistentObjectWatcher(object sender, PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                PersistentObjectsSystem.RecoverAllObjects();

                var persistentObjects = GameObject.FindObjectsByType<PersistentObjectMonobehaviour>(FindObjectsSortMode.None);
                foreach (var persistentObject in persistentObjects)
                {
                    persistentObject.RecoveryAndApplyData();
                }
            }
        }
    }

#endif

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
            foreach (var category in PersistentDataByCategory.Keys)
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
            foreach (var category in PersistentDataByCategory.Keys)
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
            // Load category's data from save system
            PersistentObjectsCategoryData data = default(PersistentObjectsCategoryData);
            bool dataExists = SaveSystemClass.GetLevelData<PersistentObjectsCategoryData>(category, out data, DATA_KEY);

            if (!dataExists)
            {
                Debug.LogWarning($"[PersistentObjects] No data found for category '{category}'");
                return;
            }

            PersistentDataByCategory[data.Category] = data.ObjectList;
            OnDataRecoveredEvent?.Invoke(data.Category);
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

    /// <summary>
    /// Monobehaviour script that provides persistent functionality on gameobjects between game sessions
    /// It automatically saves and recovers gameobject transform data and you can add your own data
    /// by implementing IPersistentDataObject (which force you to save transform data, this will be changed later)
    /// </summary>
    public class PersistentObjectMonobehaviour : UniqueIDAssigner
    {
        [Tooltip("Category of the persistent object")]
        [SerializeField] protected string _category = null;

        /// <summary>
        /// The data that will be saved and recovered for the object. You could use PersistentObjectData struct to save all
        /// transform data, or implement your own IPersistentDataObject
        /// </summary>
        public IPersistentDataObject PersistentData { get; protected set; }

        #region Instance Methods

        private void SetLevelNameAsCategory()
        {
            _category = SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// Awake method makes sure to add the category to the PersistentDataByCategory dictionary and then
        /// the object itself on it. If the data already exists, the object will be updated
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (string.IsNullOrEmpty(_category)) SetLevelNameAsCategory();

            PersistentObjectsSystem.OnDataRecoveredEvent.AddListener(OnDataRecovered);

            if (!PersistentObjectsSystem.CategoryExists(_category))
            {
                PersistentObjectsSystem.AddDataToCategory(_category, new());
                PersistentObjectsSystem.RecoverCategory(_category);
            }

            string id = GetUniqueID();
            var objectData = PersistentObjectsSystem.GetObjectData(id, _category);

            if (objectData != null)
            {
                PersistentData = objectData;
            }
            else
            {
                UpdateData();
                PersistentObjectsSystem.SetDataInCategory(_category, id, PersistentData);
            }

            ApplyData();
            
        }

        public void RecoveryAndApplyData()
        {
            string id = GetUniqueID();
            var objectData = PersistentObjectsSystem.GetObjectData(id, _category);

            if (objectData != null)
            {
                PersistentData = objectData;
            }

            ApplyData();
        }

        /// <summary>
        /// Updates the persistent data of the object setting transform values and also call the OnUpdateData auxiliary method
        /// to implement any custom data to be saved
        /// </summary>
        public void UpdateData()
        {
            if(PersistentData == null)
            {
                PersistentData = new PersistentObjectData();
                Debug.LogWarning("PersistentData is null");
            }
            OnUpdateData();
            PersistentData.UniqueID = GetUniqueID();
            PersistentData.Position = (SerializeableVector3)transform.position;
            PersistentData.Rotation = (SerializeableVector3)transform.eulerAngles;
            PersistentData.Scale = (SerializeableVector3)transform.localScale;
        }

        /// <summary>
        /// Saves the persistent data of the object (all the category will be saved)
        /// </summary>
        public void SaveObjectData()
        {
            PersistentObjectsSystem.SaveCategoryObjects(_category);
        }

        /// <summary>
        /// Applies the persistent data of the object setting transform values
        /// and also call the OnApplyData auxiliary method
        /// </summary>
        public void ApplyData()
        {
            if(PersistentData == null) return;
            transform.position = (Vector3)PersistentData.Position;
            transform.eulerAngles = (Vector3)PersistentData.Rotation;
            transform.localScale = (Vector3)PersistentData.Scale;
            OnApplyData();
        }

        protected virtual void OnApplyData() { }

        public virtual void OnUpdateData() { }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Application.isPlaying)
            {
                PersistentObjectsSystem.OnDataRecoveredEvent.AddListener(OnDataRecovered);
            }
        }

        protected virtual void OnDisable()
        {
            if (Application.isPlaying)
            {
                PersistentObjectsSystem.OnDataRecoveredEvent.RemoveListener(OnDataRecovered);
            }
        }

        protected virtual void OnDataRecovered(string category)
        {
            if(category == _category)
            {
                Debug.Log($"({GetUniqueID()}) Data recovered for category: {category}");
                PersistentData = PersistentObjectsSystem.GetObjectData(GetUniqueID(), _category);
                ApplyData();
            }
        }

        #endregion
    }
}

using System.Collections.Generic;
using Topacai.Utils.GameObjects.Unique;
using Topacai.Utils.SaveSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Topacai.Utils.GameObjects.Persistent
{
    /// <summary>
    /// Monobehaviour script that provides persistent functionality on gameobjects between game sessions
    /// It automatically saves and recovers gameobject transform data and you can add your own data
    /// by implementing IPersistentDataObject (which force you to save transform data, this will be changed later)
    /// </summary>
    public class PersistentObjectMonobehaviour : UniqueIDAssigner
    {
        private const string DATA_KEY = "PersistentObjectsData";

        /// <summary>
        /// Event that is called when data is recovered by every category
        /// </summary>
        public static UnityEvent<string> OnDataRecoveredEvent = new UnityEvent<string>();

        // category -> object list
        /// <summary>
        /// Keep track of persistent objects instances in each category
        /// </summary>
        private static Dictionary<string, HashSet<PersistentObjectMonobehaviour>> PersistentInstancesByCategory = new();
        // category -> uniqueID -> data
        /// <summary>
        /// Contains all the data recovered, indexed by category and then by uniqueID with the data recovered
        /// Data[category][uniqueID] => data as IPersistentDataObject
        /// </summary>
        private static Dictionary<string, Dictionary<string, IPersistentDataObject>> PersistentDataByCategory = new();

        [Tooltip("Category of the persistent object")]
        [SerializeField] protected string _category = "other";

        public IPersistentDataObject PersistentData { get; protected set; }

        /// <summary>
        /// Helper to call when game manager save game event is called
        /// </summary>
        private static void OnSaveGame()
        {
            SaveAllObjects();
        }

        public static void SaveAllObjects()
        {
            foreach (var item in PersistentInstancesByCategory)
            {
                SaveCategoryObjects(item.Key);
            }
        }
        /// <summary>
        /// Saves all persistent objects in a specific category.
        /// </summary>
        /// <param name="category">Category to save.</param>
        public static void SaveCategoryObjects(string category)
        {
            if (!PersistentInstancesByCategory.TryGetValue(category, out var objects))
            {
                Debug.LogWarning($"[PersistentObjects] No persistent objects found for category '{category}'");
                return;
            }

            var dataObject = new PersistentObjectsCategoryData(category, new());

            foreach (var item in PersistentInstancesByCategory[category])
            {
                item.UpdateData();
                dataObject.ObjectList.Add(item.GetUniqueID(), item.PersistentData);
            }

            SaveSystem.SaveSystem.Instance.SaveLevelDataToProfile(dataObject, category, DATA_KEY);
        }
        /// <summary>
        /// Recover all persistent objects
        /// </summary>
        public static void RecoverAllObjects()
        {
            foreach (var item in PersistentInstancesByCategory.Keys)
            {
                RecoverCategory(item);
            }
        }
        /// <summary>
        /// Recovers all persistent objects in a specific category.
        /// </summary>
        /// <param name="category">Category to recover.</param>
        public static void RecoverCategory(string category)
        {
            if (SaveSystem.SaveSystem.Instance == null) return;

            // Load category's data from save system
            PersistentObjectsCategoryData data = default(PersistentObjectsCategoryData);
            bool dataExists = SaveSystem.SaveSystem.Instance.GetLevelData<PersistentObjectsCategoryData>(category, out data, DATA_KEY);

            if(!dataExists)
            {
                Debug.LogWarning($"[PersistentObjects] No data found for category '{category}'");
                return;
            }

            PersistentDataByCategory[data.Category] = data.ObjectList;
            OnDataRecoveredEvent.Invoke(data.Category);
        }
        /// <summary>
        /// Retrieves the persistent data object for a given unique ID and category.
        /// </summary>
        /// <param name="uniqueID">Unique ID of the object.</param>
        /// <param name="category">Category of the object.</param>
        /// <returns>Persistent data object, or null if not found.</returns>
        public static IPersistentDataObject GetObjectData(string uniqueID, string category)
        {
            if(PersistentDataByCategory.TryGetValue(category, out var cat))
            {
                if (cat.TryGetValue(uniqueID, out var obj))
                {
                    return obj;
                }
            }

            Debug.LogWarning($"[PersistentObjects] Object data not found for ID: {uniqueID} in category: {category}");
            return null;
        }
        /// <summary>
        /// Awake method makes sure to add the category to the PersistentInstancesByCategory dictionary and then
        /// the object itself on it.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            if(!PersistentInstancesByCategory.ContainsKey(_category))
            {
                PersistentInstancesByCategory.Add(_category, new());
                RecoverCategory(_category);
            }

            PersistentInstancesByCategory[_category].Add(this);

            SaveSystem.SaveSystem.OnSaveGameEvent.AddListener(OnSaveGame);
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

        protected virtual void Start()
        {
            if (PersistentData == null)
            {
                PersistentData = new PersistentObjectData();
            }
            PersistentData = GetObjectData(GetUniqueID(), _category);
            ApplyData();
            OnDataRecoveredEvent.AddListener(OnDataRecovered);
            
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Application.isPlaying)
            {
                OnDataRecoveredEvent.AddListener(OnDataRecovered);
            }
        }

        protected virtual void OnDisable()
        {
            if (Application.isPlaying)
            {
                OnDataRecoveredEvent.RemoveListener(OnDataRecovered);
            }
        }

        protected virtual void OnDataRecovered(string category)
        {
            if(category == _category)
            {
                PersistentData = GetObjectData(GetUniqueID(), _category);
                ApplyData();
            }
        }
    }

    
}

using System.Collections.Generic;
using Topacai.Utils.GameObjects.Unique;
using Topacai.Utils.SaveSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Topacai.GameObjects.Persistent
{
    public class PersistentObjectBase : UniqueIDAssigner
    {
        public static UnityEvent<string> OnDataRecoveredEvent = new UnityEvent<string>();
        private const string DATA_KEY = "PersistentObjectsData";

        // category -> object list
        private static Dictionary<string, HashSet<PersistentObjectBase>> PersistentInstancesByCategory = new();
        // category -> uniqueID -> data
        private static Dictionary<string, Dictionary<string, IPersistentDataObject>> PersistentDataByCategory = new();

        [SerializeField] protected string _category = "other";

        public IPersistentDataObject PersistentData { get; private set; }

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

        public static void SaveCategoryObjects(string category)
        {
            if (!PersistentInstancesByCategory.TryGetValue(category, out var objects))
            {
                Debug.LogWarning($"No persistent objects found for category '{category}'");
                return;
            }

            var dataObject = new PersistentObjectsCategoryData();
            dataObject.ObjectList = new();
            dataObject.Category = category;
            foreach (var item in PersistentInstancesByCategory[category])
            {
                item.UpdateData();
                dataObject.ObjectList.Add(item.GetUniqueID(), item.PersistentData);
            }

            SaveController.Instance.SaveLevelDataToProfile(dataObject, category, DATA_KEY);
        }

        public static void RecoverAllObjects()
        {
            foreach (var item in PersistentInstancesByCategory.Keys)
            {
                RecoverCategory(item);
            }
        }

        public static void RecoverCategory(string category)
        {
            if (SaveController.Instance == null) return;
            PersistentObjectsCategoryData data = default(PersistentObjectsCategoryData);
            bool dataExists = SaveController.Instance.GetLevelData<PersistentObjectsCategoryData>(category, out data, DATA_KEY);

            if(!dataExists) return;

            PersistentDataByCategory[data.Category] = data.ObjectList;
            OnDataRecoveredEvent.Invoke(data.Category);
        }

        public static IPersistentDataObject GetObjectData(string uniqueID, string category)
        {
            if(PersistentDataByCategory.TryGetValue(category, out var cat))
            {
                if (cat.TryGetValue(uniqueID, out var obj))
                {
                    return obj;
                }
            }

            Debug.LogWarning($"Object data not found for ID: {uniqueID} in category: {category}");
            return null;
        }

        protected override void Awake()
        {
            base.Awake();
            if(!PersistentInstancesByCategory.ContainsKey(_category))
            {
                PersistentInstancesByCategory.Add(_category, new());
                RecoverCategory(_category);
            }

            PersistentInstancesByCategory[_category].Add(this);

            SaveController.OnSaveGameEvent.AddListener(OnSaveGame);
        }

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

    public struct PersistentObjectsCategoryData
    {
        public string Category { get; set; }
        public Dictionary<string, IPersistentDataObject> ObjectList { get; set; }
    }
    public interface IPersistentDataObject
    {
        public string UniqueID { get; set; }
        public SerializeableVector3 Position { get; set; }
        public SerializeableVector3 Rotation { get; set; }
        public SerializeableVector3 Scale { get; set; }
    }

    public struct PersistentObjectData : IPersistentDataObject
    {
        public string UniqueID { get; set; }
        public SerializeableVector3 Position { get; set; }
        public SerializeableVector3 Rotation { get; set; }
        public SerializeableVector3 Scale { get; set; }

        public override int GetHashCode() => UniqueID?.GetHashCode() ?? 0;

        public override bool Equals(object obj)
        {
            if (!(obj is PersistentObjectData other))
            {
                if (obj is string)
                {
                    return UniqueID == (string)obj;
                }
                return false;
            }
            return UniqueID == other.UniqueID;
        }
    }
}

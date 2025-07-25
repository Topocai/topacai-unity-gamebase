using System.Collections.Generic;
using Topacai.Utils.GameObjects.Unique;
using Topacai.Utils.SaveSystem;
using UnityEngine;

namespace Topacai.GameObjects.Persistent
{
    public class PersistentObjectBase : UniqueIDAssigner
    {
        private const string DATA_KEY = "PersistentObjectsData";

        [SerializeField] private string _category = "other";

        protected virtual void SaveData<T>(T dataObject) where T : IPersistentDataObject
        {
            dataObject.UniqueID = GetUniqueID();
            dataObject.Position = (SerializeableVector3)transform.position;
            dataObject.Rotation = (SerializeableVector3)transform.eulerAngles;
            dataObject.Scale = (SerializeableVector3)transform.localScale;

            SaveController.Instance.SaveLevelDataToProfile(dataObject, GetUniqueID(), DATA_KEY + "/" + _category);
        }

        protected virtual void RecoverData<T>() where T : IPersistentDataObject
        {
            if (SaveController.Instance == null) return;
            T data = default(T);
            bool dataExists = SaveController.Instance.GetLevelData<T>(GetUniqueID(), out data, DATA_KEY + "/" + _category);

            if (!dataExists) return;
            transform.position = (Vector3)data.Position;
            transform.eulerAngles = (Vector3)data.Rotation;
            transform.localScale = (Vector3)data.Scale;
        }

        protected virtual void Start()
        {
            RecoverData<PersistentObjectData>();
            SaveController.OnSaveGameEvent.AddListener(OnSaveGame);
        }

        protected virtual void OnEnable()
        {
            if (Application.isPlaying)
            {
                //SaveController.OnSaveGameEvent.AddListener(OnSaveGame);
            }
        }

        protected virtual void OnDisable()
        {
            if (Application.isPlaying)
            {
                SaveController.OnSaveGameEvent.RemoveListener(OnSaveGame);
            }
        }


        protected virtual void OnSaveGame()
        {
            SaveData(new PersistentObjectData());
        }
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
    }
}

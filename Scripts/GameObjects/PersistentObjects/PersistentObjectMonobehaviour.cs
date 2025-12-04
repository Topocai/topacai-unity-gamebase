using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Topacai.Utils.GameObjects.Unique;
using Topacai.Utils.SaveSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Topacai.Utils.GameObjects.Persistent
{
    /// <summary>
    /// Monobehaviour script that provides persistent functionality on gameobjects between game sessions
    /// It automatically saves and recovers gameobject transform data and you can add your own data
    /// by implementing IPersistentDataObject (which force you to save transform data, this will be changed later)
    /// </summary>
    public class PersistentObjectMonobehaviour : UniqueIDAssigner
    {
        [Tooltip("Category of the persistent object")]
        [SerializeField] protected string _category = null;

        public string Category => _category;

        [field: SerializeField, HideInInspector] public Vector3 originalPosition { get; protected set; }
        [field: SerializeField, HideInInspector] public Vector3 originalRotation { get; protected set; }
        [field: SerializeField, HideInInspector] public Vector3 originalScale { get; protected set; }

        [field: SerializeField, HideInInspector] protected bool isInitialized = false;

        private bool _isSuscribedToDataRecoveredEvent = false;

#if UNITY_EDITOR

        [HideInInspector] public bool DisplayOriginalTransform = false;

        private Color originalTransformColor = Color.blue;

        private Coroutine highlighting;

        private void OnDrawGizmos()
        {
            if (!DisplayOriginalTransform) return;

            Gizmos.color = originalTransformColor;
            Gizmos.DrawWireCube(originalPosition, originalScale);
            Gizmos.color = default;

            Quaternion rot = Quaternion.Euler(originalRotation);
            Vector3 direction = rot * Vector3.forward;
            Debug.DrawRay(originalPosition, direction * 1.25f * originalScale.magnitude, Color.red);
        }

        public void HightlightOriginalTransform()
        {
            if (highlighting != null) StopCoroutine(highlighting);
            highlighting = StartCoroutine(Highlight());
        }

        private IEnumerator Highlight()
        {
            originalTransformColor = Color.red;
            yield return new WaitForSeconds(0.5f);
            originalTransformColor = Color.blue;

            SceneView.RepaintAll();
        }

#endif
        /// <summary>
        /// The data that will be saved and recovered for the object. You could use PersistentObjectData struct to save all
        /// transform data, or implement your own IPersistentDataObject
        /// </summary>
        public IPersistentDataObject PersistentData { get; protected set; }

        #region Instance Methods

        private void SetLevelNameAsCategory()
        {
            _category = gameObject.scene.name;
        }

        private void SetSuscribeToData(bool value)
        {
            if (value && !_isSuscribedToDataRecoveredEvent) PersistentObjectsSystem.OnDataRecoveredEvent.AddListener(OnDataRecovered);
            else if (!value && _isSuscribedToDataRecoveredEvent) PersistentObjectsSystem.OnDataRecoveredEvent.RemoveListener(OnDataRecovered);

            _isSuscribedToDataRecoveredEvent = value;
        }

        /// <summary>
        /// Awake method makes sure to add the category to the PersistentDataByCategory dictionary and then
        /// the object itself on it. If the data already exists, the object will be updated
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (string.IsNullOrEmpty(_category)) SetLevelNameAsCategory();

            if (PersistentData == null) PersistentData = new PersistentObjectData();

            SetSuscribeToData(true);

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

            if (!isInitialized)
            {
                originalPosition = transform.position;
                originalRotation = transform.eulerAngles;
                originalScale = transform.localScale;
                isInitialized = true;
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
                Debug.LogWarning("PersistentData is null", this);
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

        public void SetOriginalPosition(Vector3 position) => originalPosition = position;

        public void SetOriginalRotation(Vector3 rotation) => originalRotation = rotation;

        public void SetOriginalScale(Vector3 scale) => originalScale = scale;

        public void ResetTransform()
        {
            transform.position = originalPosition;
            transform.eulerAngles = originalRotation;
            transform.localScale = originalScale;

            if (PersistentData != null)
            {
                PersistentData.Position = (SerializeableVector3)transform.position;
                PersistentData.Rotation = (SerializeableVector3)transform.eulerAngles;
                PersistentData.Scale = (SerializeableVector3)transform.localScale;
            }

            SaveObjectData();
        }

        protected virtual void OnApplyData() { }

        public virtual void OnUpdateData() { }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetSuscribeToData(true);
        }

        protected virtual void OnDisable()
        {
            SetSuscribeToData(false);
        }

        protected virtual void OnDataRecovered(string category)
        {
            if (category == _category)
            {
                var savedData = PersistentObjectsSystem.GetObjectData(GetUniqueID(), _category);
                if (savedData == null)
                {
                    Debug.Log("[PersistentObjects] No data found for object, using original values", this);
                    ResetTransform();
                    UpdateData();
                }
                else
                {
                    Debug.Log("[PersistentObjects] Data found for object, using saved values", this);
                    PersistentData = savedData;
                }

                ApplyData();
            }
        }

        #endregion
    }
}

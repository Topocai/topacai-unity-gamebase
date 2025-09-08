using Codice.CM.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Topacai.TDebug
{
    public class Debugcanvas : MonoBehaviour
    {
        /// Make sure to include this assets in your resources project folder.
        
        /// <summary>
        /// The uxml document to show as debug ui
        /// </summary>
        private const string DEBUG_DOCUMENT_PATH = "Assets/Debug/debug-ui-document";
        /// <summary>
        /// Panel settings asset to put in the uxml debug document
        /// </summary>
        private const string PANEL_SETTINGS_PATH = "Assets/Debug/debug-ui-panel";
        /// <summary>
        /// The name of the container where to instantiate the logs labels
        /// </summary>
        private const string LOGS_CONTAINER_NAME = "debugLogsContainer";
        /// <summary>
        /// The class name to style logs labels
        /// </summary>
        private const string LOG_LABEL_CLASS = "debugLogsContainer__logLabel";

        public static Debugcanvas Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

#if UNITY_EDITOR
        private VisualTreeAsset _debugDocumentAsset;
        private PanelSettings _panelSettingsAsset;

        private UIDocument _document;

        private VisualElement _logsContainer;

        struct LogLine
        {
            public string Name { get; set; }
            public float WhenRemove { get; set; }
        }

        private List<LogLine> _registerLogs = new();

        private void Update()
        {
            /// Loop over all registered logs and check if is time to remove it
            /// using Time.time value that shows the time passed in application in seconds
            /// if the time is already to remove, the struct and the visual element are deleted.
            int registeredLogs = _registerLogs.Count;

            if (registeredLogs <= 0) return;

            float currentTime = Time.time;
            for (var i = 0; i < registeredLogs; i++) 
            {
                if (currentTime >= _registerLogs[i].WhenRemove) 
                {
                    var existingLabel = _document.rootVisualElement.Q<Label>(_registerLogs[i].Name);
                    _logsContainer.Remove(existingLabel);
                    _registerLogs.RemoveAt(i);
                }
            }
        }

        private void Start()
        {
            /// Searches for the uxml and panel assets to create a new 'UIDocument' component
            /// into the gameobject and searches for the container using the LOGS_CONTAINER_NAME id.
            _debugDocumentAsset = Resources.Load<VisualTreeAsset>(DEBUG_DOCUMENT_PATH);
            _panelSettingsAsset = Resources.Load<PanelSettings>(PANEL_SETTINGS_PATH);

            if (_debugDocumentAsset == null || _panelSettingsAsset == null)
            {
                Debug.LogError($"Debug UI document or panel settings not found in path {DEBUG_DOCUMENT_PATH} or {PANEL_SETTINGS_PATH}");
                return;
            }

            _document = gameObject.AddComponent<UIDocument>();
            _document.panelSettings = _panelSettingsAsset;
            _document.visualTreeAsset = _debugDocumentAsset;

            _logsContainer = _document.rootVisualElement.Q<VisualElement>(LOGS_CONTAINER_NAME);
        }

        private int FindLog(string name)
        {
            return _registerLogs.FindIndex(x => x.Name == name);
        }

        /// <summary>
        /// Add or updates a text line in the debug ui
        /// make sure to pass the suffix as an unique identifier because is used to find the actual label
        /// </summary>
        /// <param name="suffix">Identifier of the log and also displayed first on line as '$suffix$: '</param>
        /// <param name="text">The text to show next to suffix</param>
        /// <param name="duration" optional>Duration to show the log, if is updated again the time is resetted, if the value is not passed the log will be persistent</param>
        public void AddTextToDebugLog(string suffix, string text, float duration = 0)
        {
            if (_document == null || _logsContainer == null) return;

            if (duration > 0)
            {
                int index = FindLog(suffix);
                if (index < 0)
                    _registerLogs.Add(new LogLine { Name = suffix, WhenRemove = Time.time + duration });
                else _registerLogs[index] = new LogLine { Name = suffix, WhenRemove = Time.time + duration };
            }
            
            // Searches for an existing label with the passed suffix, if it exists updates the text
            var existingLabel = _document.rootVisualElement.Q<Label>(suffix);
            if (existingLabel != null)
            {
                existingLabel.text = $"{suffix}: {text}";
                return;
            }
            // Creates a new label with the proper class and then added to the container
            var newLabel = new Label() { text = $"{suffix}: {text}", name = suffix, };
            newLabel.AddToClassList(LOG_LABEL_CLASS);
            _logsContainer.Add(newLabel);
        }
#endif
    }
}

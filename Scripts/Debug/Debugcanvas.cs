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

        /// <summary>
        /// Add or updates a text line in the debug ui
        /// make sure to pass the suffix as an unique identifier because is used to find the actual label
        /// </summary>
        /// <param name="suffix">Identifier of the log and also displayed first on line as '$suffix$: '</param>
        /// <param name="text">The text to show next to suffix</param>
        public void AddTextToDebugLog(string suffix, string text)
        {
            if (_document == null || _logsContainer == null) return;
            
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

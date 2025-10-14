#if UNITY_EDITOR

using Topacai.Utils.Editor;
using UnityEditor;
using UnityEngine;

using Topacai.Utils.GameObjects.Unique;

namespace Topacai.Utils.GameObjects.Persistent.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PersistentObjectMonobehaviour), true)]
    public class PersistentObjectEditor : UniqueIDAssignerEditor
    {        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            PersistentObjectMonobehaviour persistentObject = (PersistentObjectMonobehaviour)target;

            if (GUILayout.Button("Save Original Transform"))
            {
                persistentObject.SetOriginalPosition(persistentObject.transform.position);
                persistentObject.SetOriginalRotation(persistentObject.transform.rotation.eulerAngles);
                persistentObject.SetOriginalScale(persistentObject.transform.localScale);
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Recover Original Transform"))
            {
                persistentObject.ResetTransform();
            }

            if (GUILayout.Button(persistentObject.DisplayOriginalTransform ? "Hide Original Transform" : "Show Original Transform"))
            {
                persistentObject.DisplayOriginalTransform = !persistentObject.DisplayOriginalTransform;
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Highlight/Look At Original Transform"))
            {
                persistentObject.HightlightOriginalTransform();
                SceneView.RepaintAll();

                SceneView.lastActiveSceneView?.LookAt(persistentObject.originalPosition, SceneView.lastActiveSceneView.rotation, 2f);
            }
        }
    }

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
}

#endif

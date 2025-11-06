using UnityEngine;
using Topacai.Utils;
using System.Collections.Generic;

#if UNITY_EDITOR

using Topacai.TDebug;
using UnityEditor;

#endif

namespace Topacai.Static.Colliders
{
#if UNITY_EDITOR
    public class InvisibleCollisionsEditorWindow : EditorWindow
    {
        [MenuItem("TopacaiTools/Invisible Collisions")]
        public static void ShowWindow()
        {
            GetWindow<InvisibleCollisionsEditorWindow>("Invisible Collisions");
        }

        private bool inEditCollisionsMode = false;

        private void OnGUI()
        {
            /// When enter edit collision mode all objects in scene converts into non-clickeable and non-editable
            /// except for all invisible collisions (and all classes that inherit from invisible collisions).
            if (GUILayout.Button(inEditCollisionsMode ? "Exit Edit Collisions Mode" : "Enter Edit Collisions Mode"))
            {
                inEditCollisionsMode = !inEditCollisionsMode;

                foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                {
                    if (go.TryGetComponent<InvisibleCollisions>(out var c))
                    {
                        go.hideFlags = inEditCollisionsMode ? HideFlags.None : HideFlags.NotEditable;

                        if (!inEditCollisionsMode)
                            SceneVisibilityManager.instance.DisablePicking(go, false);
                        else
                            SceneVisibilityManager.instance.EnablePicking(go, false);

                        EditorUtility.SetDirty(go);

                        c.SetShow(inEditCollisionsMode);
                    }
                    else if (go.scene.IsValid() && go.hideFlags == (!inEditCollisionsMode ? HideFlags.NotEditable : HideFlags.None))
                    {
                        go.hideFlags = inEditCollisionsMode ? HideFlags.NotEditable : HideFlags.None;

                        if (inEditCollisionsMode)
                            SceneVisibilityManager.instance.DisablePicking(go, false);
                        else 
                            SceneVisibilityManager.instance.EnablePicking(go, false);

                        EditorUtility.SetDirty(go);
                    }
                }

                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Show All Collisions"))
            {
                foreach (InvisibleCollisions col in Object.FindObjectsByType<InvisibleCollisions>(FindObjectsSortMode.InstanceID))
                {
                    if (col.gameObject.scene.IsValid())
                        col.SetShow(true);
                }
            }

            if (GUILayout.Button("Hide All Collisions"))
            {
                foreach (InvisibleCollisions col in Object.FindObjectsByType<InvisibleCollisions>(FindObjectsSortMode.InstanceID))
                {
                    if (col.gameObject.scene.IsValid())
                        col.SetShow(false);
                }
            }

            GUILayout.Space(10);

            /// Used for refresh all invisible collisions on scene, util if groups are not showed or not correctly working
            if (GUILayout.Button("Refresh all collisions"))
            {
                foreach (InvisibleCollisions col in Object.FindObjectsByType<InvisibleCollisions>(FindObjectsSortMode.InstanceID))
                {
                    if (col.gameObject.scene.IsValid())
                    {
                        EditorUtility.SetDirty(col.gameObject);
                        col.RegisterGroup(true);
                        
                    }
                }
            }

            foreach (var group in InvisibleCollisions.GetGroups())
            {
                GUI.color = group.Key;

                GUILayout.Label(group.Key.ToString());


                if (GUILayout.Button("Focus this group"))
                {
                    foreach (InvisibleCollisions col in Object.FindObjectsByType<InvisibleCollisions>(FindObjectsSortMode.InstanceID))
                    {
                        if (col.gameObject.scene.IsValid())
                        {
                            if (group.Value.Contains(col))
                                col.SetShow(true);
                            else
                                col.SetShow(false);
                        }
                    }
                }

                if (GUILayout.Button("Hide this group"))
                {
                    foreach (var col in group.Value)
                    {
                        col.SetShow(false);
                    }
                }

                if (GUILayout.Button("Show this group"))
                {
                    foreach (var col in group.Value)
                    {
                        col.SetShow(true);
                    }
                }

                if (GUILayout.Button("Set wire this group"))
                {
                    foreach (var col in group.Value)
                    {
                        col.SetFill(false);
                    }
                }

                if (GUILayout.Button("Fill this group"))
                {
                    foreach (var col in group.Value)
                    {
                        col.SetFill(true);
                    }
                }

                GUI.color = Color.white;

            }
        }
    }
#endif

    [ExecuteAlways]
    public class InvisibleCollisions : MonoBehaviour
    {
#if UNITY_EDITOR
        private static Dictionary<Color, HashSet<InvisibleCollisions>> _groups = new();

        public static Dictionary<Color, HashSet<InvisibleCollisions>> GetGroups() => _groups;

        protected Color _currentGroup;

        [SerializeField] protected Color _groupColor = Color.green;
        [SerializeField] protected bool _show = false;
        [SerializeField] protected bool _fill = false;

        public void SetShow(bool show)
        {
            _show = show;
            EditorUtility.SetDirty(this);
        }
        public void SetFill(bool fill)
        {
            _fill = fill;
            EditorUtility.SetDirty(this);
        }

        public void RegisterGroup(bool force = false)
        {
            if (_currentGroup == _groupColor && !force) return;

            if (_groups.ContainsKey(_currentGroup))
            {
                _groups[_currentGroup].Remove(this);

                if (_groups[_currentGroup].Count == 0)
                {
                    _groups.Remove(_currentGroup);
                }
            }

            if (!_groups.ContainsKey(_groupColor))
                _groups.Add(_groupColor, new HashSet<InvisibleCollisions>());

            _groups[_groupColor].Add(this);

            _currentGroup = _groupColor;
        }

        protected virtual void OnValidate()
        {
            RegisterGroup();
        }

        protected virtual void OnEnable()
        {
            RegisterGroup();
        }

        protected virtual void OnDrawGizmos()
        {
            if (!_show) return;

            Collider col = GetComponent<Collider>();
            if (col == null || !col.enabled) return;

            Gizmos.color = _groupColor;

            if (col is BoxCollider box)
            {
                Gizmos.matrix = box.transform.localToWorldMatrix;
                if (!_fill)
                    Gizmos.DrawWireCube(box.center, box.size);
                else
                    Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.matrix = sphere.transform.localToWorldMatrix;
                if (!_fill)
                    Gizmos.DrawWireSphere(sphere.center, sphere.radius);
                else
                    Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
            else if (col is CapsuleCollider capsule)
            {
                Vector3 pointsCapsule = capsule.transform.up * capsule.height * 0.5f;
                GizmosUtils.DrawCapsule(capsule.center + pointsCapsule, capsule.center - pointsCapsule, capsule.radius, _groupColor);
            }
            else if (col is MeshCollider meshCol && meshCol.sharedMesh != null)
            {
                Gizmos.matrix = meshCol.transform.localToWorldMatrix;
                if (!_fill)
                    Gizmos.DrawWireMesh(meshCol.sharedMesh);
                else 
                    Gizmos.DrawMesh(meshCol.sharedMesh);
            }
        }
#endif
    }
}

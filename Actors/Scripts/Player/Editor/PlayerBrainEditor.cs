#if UNITY_EDITOR

using Topacai.Player;
using UnityEditor;
using UnityEngine;

namespace Topacai.Player.Editor
{
    [CustomEditor(typeof(PlayerBrain))]
    public class PlayerBrainEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var player = (PlayerBrain)target;

            if (GUILayout.Button("Teleport to Player Position"))
            {
                SceneView.lastActiveSceneView?.LookAt(player.transform.position, SceneView.lastActiveSceneView.rotation, 2f);
            }

            if (GUILayout.Button("Move Player here"))
            {
                player.transform.position = SceneView.lastActiveSceneView.camera.transform.position;
            }

            base.OnInspectorGUI();
        }
    }
}

#endif
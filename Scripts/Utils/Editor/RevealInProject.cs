#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Topacai.Utils.Editor
{
    /// <summary>
    /// Adds an item for contextual menu when right-clicking an asset
    /// to reveal it in the project window and go to the asset itself
    /// (Util when you are using search for assets and want to go directly to the asset)
    /// </summary>
    public static class RevealInProject
    {
        [MenuItem("Assets/Reveal in Project Window", false, 2000)]
        private static void RevealSelectedAsset()
        {
            Object selected = Selection.activeObject;
            if (selected != null)
            {
                EditorGUIUtility.PingObject(selected);
            }
        }

        [MenuItem("Assets/Reveal in Project Window", true)]
        private static bool ValidateRevealSelectedAsset()
        {
            return Selection.activeObject != null;
        }
    }

}

#endif
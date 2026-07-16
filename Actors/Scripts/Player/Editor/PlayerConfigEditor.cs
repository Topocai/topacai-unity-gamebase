#if UNITY_EDITOR

using Topacai.Player;
using Topacai.Utils.Editor;

using UnityEditor;
using UnityEngine;

namespace Topacai.Player.Editor
{
    public class PlayerConfigWindow : EditorWindow
    {
        #region Style

        #region Style fields
        private GUIStyle boxStyle;
        private GUIStyle labelStyle;
        #endregion

        #region Style configuration
        private void OnEnable()
        {
            boxStyle = new GUIStyle("box")
            {
                padding = new RectOffset(10, 10, 8, 8),
            };

            labelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11
            };
        }
        #endregion

        #endregion

        [MenuItem("TopacaiTools/Player config")]
        public static void ShowWindow()
        {
            GetWindow<PlayerConfigWindow>("Player Config Menu");
        }

        private bool _isSinglePlayer = PlayerBrain.SINGLEPLAYER_MODE;

        private void PlayerModeField()
        {
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = _isSinglePlayer ? new Color(0.3f, 0.5f, 0.9f, 0.3f) : new Color(0.6f, 0.2f, 0.9f, 0.2f);
            EditorGUILayout.BeginVertical(boxStyle);


            GUILayout.Label("PlayerBrain mode settings", labelStyle);
            _isSinglePlayer = EditorGUILayout.ToggleLeft("Singleplayer mode", _isSinglePlayer);
            GUILayout.Space(5);

            EditorGUILayout.EndVertical();

            GUI.backgroundColor = originalColor;

            if (_isSinglePlayer != PlayerBrain.SINGLEPLAYER_MODE) PlayerBrain.SINGLEPLAYER_MODE = _isSinglePlayer;
        }

        private void OnGUI()
        {
            PlayerModeField();
        }
    }

}

#endif
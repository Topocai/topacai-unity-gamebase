#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Topacai.Channels.Editor
{
    public class ChannelManagerWindow : EditorWindow
    {
        string channelName = "DontUseSpaces";

        List<ArgumentData> arguments = new();

        [MenuItem("TopacaiTools/Channel Manager")]
        public static void ShowWindow()
        {
            GetWindow<ChannelManagerWindow>("Channel Manager");
        }

        private void OnGUI()
        {
            ChannelManager.ReloadAssets = EditorGUILayout.ToggleLeft("Recompile after add", ChannelManager.ReloadAssets);
            GUILayout.Label("Channel Name", EditorStyles.boldLabel);
            channelName = EditorGUILayout.TextField("Channel Name: ", channelName);

            GUILayout.Space(10);

            GUILayout.Label("Channel arguments", EditorStyles.boldLabel);
            for (int i = 0; i < arguments.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");

                arguments[i] = DrawArgument(arguments[i]);

                if (GUILayout.Button("Remove"))
                {
                    arguments.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add argument"))
            {
                arguments.Add(new ArgumentData());
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Create Channel"))
            {
                ChannelManager.CreateChannel(channelName, arguments);
            }
        }

        private ArgumentData DrawArgument(ArgumentData argument)
        {
            argument.type = EditorGUILayout.TextField("Type", argument.type);
            argument.name = EditorGUILayout.TextField("Name", argument.name);
            return argument;
        }
    }
}

#endif
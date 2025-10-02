using UnityEngine;
using System.Collections.Generic;
using Topacai.Utils.Files;
using UnityEditor;
using System;

namespace Topacai.Channels
{

    public static partial class ChannelBroadcaster
    {

    }
    public enum TypeData
    {
        Float,
        Int,
        String,
        Bool,
        Component
    }

    [System.Serializable]
    public struct ArgumentData
    {
        public string type;
        public string name;
    }

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

    public static class ChannelManager
    {
        private const string CHANNEL_BROADCASTER_PATH = "Assets/TopacaiCore/GeneratedScripts/ChannelBroadcaster/";

        public static void CreateChannel(string name, List<ArgumentData> arguments)
        {
            string args = "";

            for (int i = 0; i < arguments.Count; i++)
            {
                args += $"public {arguments[i].type} {arguments[i].name};\n";
            }

            string code =
$@"
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Topacai.Channels 
{{
    public class Channel{name}Args
    {{
        {args}
    }}
    
    public static partial class ChannelBroadcaster
    {{
        public static class Channel{name}
        {{
            public static void Broadcast(Channel{name}Args args) =>
                ChannelBaseClass<Channel{name}Args>.Broadcast(args);

            public static void AddListener(ChannelDelegate<Channel{name}Args> listener) =>
                ChannelBaseClass<Channel{name}Args>.AddListener(listener);

            public static void RemoveListener(ChannelDelegate<Channel{name}Args> listener) =>
                ChannelBaseClass<Channel{name}Args>.RemoveListener(listener);
        }}   
    }}
}}
";
            FileManager.WriteFile(CHANNEL_BROADCASTER_PATH, $"{name}.cs", code);

            AssetDatabase.Refresh();

            Debug.Log($"Channel {name} created");
        }
    }
    public delegate void ChannelDelegate<T>(T arg);
    public static class ChannelBaseClass<T>
    {
        public static event ChannelDelegate<T> OnChannelEvent;

        public static void Broadcast(T args) => OnChannelEvent?.Invoke(args);
        public static void AddListener(ChannelDelegate<T> listener) => OnChannelEvent += listener;
        public static void RemoveListener(ChannelDelegate<T> listener) => OnChannelEvent -= listener;
    }
}

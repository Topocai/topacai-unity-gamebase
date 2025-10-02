using UnityEngine;
using System.Collections.Generic;
using Topacai.Utils.Files;
using UnityEditor;
using System.IO;

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

    public static class ChannelManager
    {
        private const string CHANNEL_BROADCASTER_PATH = "Assets/TopacaiCore/GeneratedScripts/ChannelBroadcaster/";

        /// <summary>
        /// Creates a channel with custom arguments that can be invoked, add/remove listeners
        /// Automatic generates a c# script adding the desired channel
        /// </summary>
        /// <param name="name"></param>
        /// <param name="arguments"></param>
        public static void CreateChannel(string name, List<ArgumentData> arguments)
        {
            string args = "";

            for (int i = 0; i < arguments.Count; i++)
            {
                args += $"public {arguments[i].type} {arguments[i].name};\n\t\t";
            }

            string code =
$@"
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///                                                                                                                        ///
///                                                                                                                        ///
///     ▄▀█ █░█ ▀█▀ █▀█ █▀▄▀█ ▄▀█ ▀█▀ █ █▀▀   █▀▀ █▀▀ █▄░█ █▀▀ █▀█ ▄▀█ ▀█▀ █▀▀ █▀▄   █▀▀ █▀█ █▀▄ █▀▀       ///
///     █▀█ █▄█ ░█░ █▄█ █░▀░█ █▀█ ░█░ █ █▄▄   █▄█ ██▄ █░▀█ ██▄ █▀▄ █▀█ ░█░ ██▄ █▄▀   █▄▄ █▄█ █▄▀ ██▄       ///
///                                                                                                                        ///
///                                               By Topacai                                                               ///
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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

            if (!File.Exists($"{CHANNEL_BROADCASTER_PATH}/topacai-assembly-reference.asmref"))
            {
                FileManager.WriteFile(CHANNEL_BROADCASTER_PATH, "topacai-assembly-reference.asmref", "{ \"reference\": \"Topacai\" }");
            }

            Debug.Log($"Channel {name} created");
            AssetDatabase.Refresh();
        }
    }

    ///
    /// All channels will use this delegate and static class
    /// To keep logic on all channels
    ///
    public delegate void ChannelDelegate<T>(T arg);
    public static class ChannelBaseClass<T>
    {
        public static event ChannelDelegate<T> OnChannelEvent;

        public static void Broadcast(T args) => OnChannelEvent?.Invoke(args);
        public static void AddListener(ChannelDelegate<T> listener) => OnChannelEvent += listener;
        public static void RemoveListener(ChannelDelegate<T> listener) => OnChannelEvent -= listener;
    }
}

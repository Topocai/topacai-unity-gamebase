using System;
using System.Collections.Generic;
using System.IO;
using Topacai.Channels;
using Topacai.Utils.Files;
using UnityEditor;
using UnityEngine;

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

        private const string CHANNEL_SYSTEM_VERSION = "0.3.1";

        /// <summary>
        /// Creates a channel with custom arguments that can be invoked, add/remove listeners
        /// Automatic generates a c# script adding the desired channel
        /// </summary>
        /// <param name="name"></param>
        /// <param name="arguments"></param>
        public static void CreateChannel(string name, List<ArgumentData> arguments, string customNamespace = "")
        {
            string args = "";

            for (int i = 0; i < arguments.Count; i++)
            {
                args += $"public {arguments[i].type} {arguments[i].name};\n\t\t";
            }

            string argsClassName = $"Channel{name}Args";

            string code =
$@"
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//                                                                                                                          //
// █▀▀ █░░█ █▀▀█ █▀▀▄ █▀▀▄ █▀▀ █░░   █▀▀ █░░█ █▀▀ ▀▀█▀▀ █▀▀ █▀▄▀█                                                           //
// █░░ █▀▀█ █▄▄█ █░░█ █░░█ █▀▀ █░░   ▀▀█ █▄▄█ ▀▀█ ░░█░░ █▀▀ █░▀░█                                                           //
// ▀▀▀ ▀░░▀ ▀░░▀ ▀░░▀ ▀░░▀ ▀▀▀ ▀▀▀   ▀▀▀ ▄▄▄█ ▀▀▀ ░░▀░░ ▀▀▀ ▀░░░▀                                                           //
//                                                                                                                          //
// ▄▀█ █░█ ▀█▀ █▀█ █▀▄▀█ ▄▀█ ▀█▀ █ █▀▀     █▀▀ █▀▀ █▄░█ █▀▀ █▀█ ▄▀█ ▀█▀ █▀▀ █▀▄                                  //
// █▀█ █▄█ ░█░ █▄█ █░▀░█ █▀█ ░█░ █ █▄▄     █▄█ ██▄ █░▀█ ██▄ █▀▄ █▀█ ░█░ ██▄ █▄▀                                  //
//                                                                                                                          //
// ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░                                      //
//                                                                                                                          //
//   Auto-generated channel script by Topacai Channel System v{CHANNEL_SYSTEM_VERSION}                                      //
//   Namespace: Topacai.Channels{((customNamespace != "") ? $".{customNamespace}" : "")}                                    //
//                                                                                                                          //
//   This script defines the channel '{name}' with its argument class and broadcasting logic.                               //
//                                                                                                                          //
//   ASCII Cat:                                                                                                             //
//                                /\_/\                                                                                     //
//                               < >W< >                                                                                    //
//                                ^^ ^^                                                                                     //
//                                                                                                                          //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Topacai.Channels{((customNamespace != "") ? $".{customNamespace}" : "")}
{{
    public class {argsClassName}
    {{
        {args}
    }}
    
    public static partial class ChannelBroadcaster
    {{
        public static class Channel{name}
        {{
            public static void Broadcast({argsClassName} args) =>
                ChannelBaseClass<{argsClassName}>.Broadcast(args);

            public static void AddListener(ChannelDelegate<{argsClassName}> listener) =>
                ChannelBaseClass<{argsClassName}>.AddListener(listener);

            public static void RemoveListener(ChannelDelegate<{argsClassName}> listener) =>
                ChannelBaseClass<{argsClassName}>.RemoveListener(listener);
        }}   
    }}

    [CreateAssetMenu(menuName = ""ScriptableObjects/Channels/Channel{name}"")]
    /// <summary>
    /// Scriptable object that represents a channel ({name}), it can be used to define non static events from this channel
    /// or assign the channel through an asset reference
    /// </summary>
    public class Channel{name}Asset : ChannelAsset
    {{
        public event ChannelDelegate<{argsClassName}> NonStaticChannel;

        [Tooltip(""If this is unchecked, all events called trough this asset only will be affected to the ones that are suscribed directly from the same asset"")]
        public bool isStatic = true;

        public override bool StaticChannel => isStatic;

        public override System.Type ArgsType => typeof({argsClassName});

        public void Broadcast({argsClassName} args)
        {{   
            if (StaticChannel)
                ChannelBaseClass<{argsClassName}>.Broadcast(args);
            else
                NonStaticChannel?.Invoke(args);
        }}
            

        public void Suscribe(ChannelDelegate<{argsClassName}> listener)
        {{
            if (StaticChannel)
                ChannelBaseClass<{argsClassName}>.AddListener(listener);
            else
                NonStaticChannel += listener;
        }}
            

        public void Unsuscribe(ChannelDelegate<{argsClassName}> listener)
        {{
            if (StaticChannel)
                ChannelBaseClass<{argsClassName}>.RemoveListener(listener);
            else 
                NonStaticChannel -= listener;
        }}
            
    }}

    ///                              /\_/\
    ///                             <(o.o)>
    ///                              ^^ ^^
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
    public abstract class ChannelAsset : ScriptableObject
    {
        public abstract System.Type ArgsType { get; }

        public abstract bool StaticChannel { get; }
    }
   
}

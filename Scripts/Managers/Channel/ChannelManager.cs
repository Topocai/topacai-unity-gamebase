using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
#if UNITY_EDITOR
        public const string CHANNEL_BROADCASTER_PATH = "Assets/TopacaiCore/GeneratedScripts/ChannelBroadcaster/";
        public const string CHANNEL_SYSTEM_VERSION = "1.0.0-alpha";
        public const string MAIN_ASSEMBLYDEF_NAME = "TopacaiChannels";

        private static bool _ReloadAssets = false;

        public static bool ReloadAssets { get => _ReloadAssets; set => _ReloadAssets = value; }

        /// <summary>
        /// Creates a new assembly asset with the given name, root namespace and references.
        /// If the name is equal to <see cref="MAIN_ASSEMBLYDEF_NAME"/>, only Topacai is added to the references and not TopacaiChannels.
        /// </summary>
        /// <param name="name">The name of the assembly asset.</param>
        /// <param name="rootNamespace">The root namespace of the assembly asset.</param>
        /// <param name="references">The references of the assembly asset.</param>
        /// <param name="subPath">The subpath of the assembly asset.</param>
        private static void CreateAssemblyAsset(string name, string rootNamespace, string[] references = null, string subPath = null)
        {
            string referencesParsed = references != null && references.Length > 0 ? String.Join(",\n", references.Select(x => $"\"{x}\"")) : "";

            referencesParsed += name == MAIN_ASSEMBLYDEF_NAME ? ",\n\"Topacai\"" : $",\n\"Topacai\",\n\"{MAIN_ASSEMBLYDEF_NAME}\""; ;

            string txt =
$@"
{{
    ""name"": ""{name}"",
    ""rootNamespace"": ""{rootNamespace}"",
    ""references"": [
        {referencesParsed}
    ],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}}
";
            string path = subPath == null ? CHANNEL_BROADCASTER_PATH : Path.Combine(CHANNEL_BROADCASTER_PATH, subPath);
            FileManager.WriteFile(path, $"{name}.asmdef", txt);
        }

        private static void CreateChannelAssemblyAsset()
        {
            CreateAssemblyAsset(MAIN_ASSEMBLYDEF_NAME, "Topacai.Channels");
        }

        /// <summary>
        /// Creates a channel with custom arguments that can be invoked, add/remove listeners
        /// Automatic generates a c# script adding the desired channel
        /// </summary>
        /// <param name="name"></param>
        /// <param name="arguments"></param>
        public static void CreateChannel(
            string name, List<ArgumentData> arguments,
            string customNamespace = "",
            string[] requiredNamespaces = null,
            string[] requiredAssemblyReferences = null,
            string channelAssembly = null)
        {
            string args = "";

            for (int i = 0; i < arguments.Count; i++)
            {
                args += $"public {arguments[i].type} {arguments[i].name};\n\t\t";
            }

            string references = requiredNamespaces != null && requiredNamespaces.Length > 0 ? String.Join(";\nusing ", requiredNamespaces) : null;

            string argsClassName = $"Channel{name}Args";

            string code =
$@"
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

{(references != null ? $"using {references};" : "")}

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

namespace Topacai.Channels{((!String.IsNullOrEmpty(customNamespace)) ? $".{customNamespace}" : "")}
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

            if (!File.Exists($"{CHANNEL_BROADCASTER_PATH}/{MAIN_ASSEMBLYDEF_NAME}.asmdef"))
            {
                CreateChannelAssemblyAsset();
            }

            string channelPath = CHANNEL_BROADCASTER_PATH;

            if (requiredAssemblyReferences != null && requiredAssemblyReferences.Length > 0)
            {
                channelAssembly = $"{name}Channels";
            }

            if (!String.IsNullOrEmpty(channelAssembly))
                channelPath = Path.Combine(CHANNEL_BROADCASTER_PATH, channelAssembly);

            if (!Directory.Exists(channelPath))
                CreateAssemblyAsset(channelAssembly, customNamespace, requiredAssemblyReferences, channelAssembly);

            FileManager.WriteFile(channelPath, $"{name}.cs", code);

            Debug.Log($"Channel {name} created");

            if (ReloadAssets)
                AssetDatabase.Refresh();
        }
#endif
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

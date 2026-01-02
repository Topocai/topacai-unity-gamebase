#if UNITY_EDITOR
using UnityEngine;

using UnityEditor;

using System.IO;
using System.Linq;
using System.Collections.Generic;

using UnityEditorInternal;

namespace Topacai.Channels.Editor
{
    public class ChannelManagerWindow : EditorWindow
    {
        string channelName = "DontUseSpaces";

        List<ArgumentData> arguments = new();
        List<string> requiredNamespaces = new();

        List<AssemblyDefinitionAsset> requiredAssemblyReferences = new();

        int channelGroup = 0;

        bool useExistingGroup = false;

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

            // ========================== /* ARGUMENTS */ ========================== //

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

            GUILayout.Space(10);

            // ========================== /* REQUIRED NAMESPACES */ ========================== //

            GUILayout.Label("Required namespaces", EditorStyles.boldLabel);
            for (int i = 0; i < requiredNamespaces.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");

                requiredNamespaces[i] = EditorGUILayout.TextField("Namespace: ", requiredNamespaces[i]);

                if (GUILayout.Button("Remove"))
                {
                    requiredNamespaces.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add required namespace"))
            {
                requiredNamespaces.Add("");
            }

            GUILayout.Space(20);

            // custom namespace

            string customNamespace = EditorGUILayout.TextField("Custom Namespace", "");
            string assemblyGroup = "";

            // ========================== /* CHANNEL GROUP/ASSEMBLY */ ========================== //

            useExistingGroup = EditorGUILayout.Toggle("Use existing group", useExistingGroup);

            if (useExistingGroup)
            {
                var channelsGroups = Directory.EnumerateDirectories(ChannelManager.CHANNEL_BROADCASTER_PATH);

                if (channelsGroups.Count() == 0)
                {
                    EditorGUILayout.HelpBox("No channel groups found", MessageType.Error);
                }
                else
                {
                    var options = channelsGroups.Select(x => Path.GetFileName(x)).ToArray();

                    channelGroup = EditorGUILayout.Popup(channelGroup, options);

                    assemblyGroup = options[channelGroup];
                }
            }
            else
            {
                GUILayout.Label("Required assemblies", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("If one assembly is required, a new group with a new assembly definition would be created for this channel.", MessageType.Info);
                for (int i = 0; i < requiredAssemblyReferences.Count; i++)
                {
                    EditorGUILayout.BeginVertical("box");

                    requiredAssemblyReferences[i] = DrawAssemblyReference(requiredAssemblyReferences[i]);

                    if (GUILayout.Button("Remove"))
                    {
                        requiredAssemblyReferences.RemoveAt(i);
                        break;
                    }

                    EditorGUILayout.EndVertical();
                }

                if (GUILayout.Button("Add required assembly"))
                {
                    requiredAssemblyReferences.Add(null);
                }
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Create Channel"))
            {
                string[] requiredAssemblyes = requiredAssemblyReferences.Count() > 0 ?
                requiredAssemblyReferences.Select(x => x.name).ToArray()
                : null;

                ChannelManager.CreateChannel(channelName, arguments, customNamespace, requiredNamespaces.ToArray(), requiredAssemblyes, assemblyGroup);
            }
        }

        private AssemblyDefinitionAsset DrawAssemblyReference(AssemblyDefinitionAsset assemblyReference)
        {
            return (AssemblyDefinitionAsset)EditorGUILayout.ObjectField("Assembly Reference: ", assemblyReference, typeof(AssemblyDefinitionAsset), false);
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
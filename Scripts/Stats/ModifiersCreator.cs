using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Topacai.StatsSystem
{
    public struct ModifierConfig<T>
    {
        public ModifierOperation ModifierType;
        public T ModifierValue;
        public string Name;
        public float Duration;
    }

    [CustomEditor(typeof(MultipleModifiersCreatorSO))]
    public class MultipleModifiersCreatorEditor : Editor
    {
        int fieldIndex = 0;
        float durationSelected = 0f;
        ModifierOperation modifierTypeSelected;

        int intModifierValueSelected = 0;
        float floatModifierValueSelected = 0f;
        bool boolModifierValueSelected = false;

        Type[] supportedTypes = { typeof(int), typeof(float), typeof(bool) };

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var creator = target as MultipleModifiersCreatorSO;

            if (creator == null)
            {
                return;
            }

            var scriptObject = creator.ScriptObject;

            if (scriptObject == null)
            {
                return;
            }

            DrawStats(creator.Modifiers);

            var scriptAsset = creator.GetScriptAsset();

            if (scriptObject != null)
                DrawModifierAdder(scriptObject);
        }

        private void DrawStats(IEnumerable<StatModifier> modifiers)
        {
            var c = modifiers.Count();
            if (c <= 0) return;

            // Show a warning that modifiers will be lost the exactly info at recompile or close unity, but they will be exposed 
            // as simple modifiers in the inspector and could be removed manually
            foreach (var modifier in modifiers)
            {
                
                if (DrawConfigurableModifier(modifier as ConfigurableModifier<float>)) { }
                else if (DrawConfigurableModifier(modifier as ConfigurableModifier<int>)) { }
                else if (DrawConfigurableModifier(modifier as ConfigurableModifier<bool>)) { }
                else
                {
                    // After recompile configurable modifiers will be converted into the base StatModifier or BaseStatModifier class
                    // so they can't be expose all info about them but they should be showed to the user

                    if (DrawBaseModifier(modifier as BaseStatModifier<float>)) { }
                    else if (DrawBaseModifier(modifier as BaseStatModifier<int>)) { }
                    else if (DrawBaseModifier(modifier as BaseStatModifier<bool>)) { }
                    else
                    {
                        GUILayout.Label(modifier.ToString());
                    }
                }
                
                if (GUILayout.Button("Remove Modifier"))
                {
                    var creator = (MultipleModifiersCreatorSO)target;
                    creator.RemoveStatModifier(modifier);
                }
            }
        }

        private bool DrawBaseModifier<T>(BaseStatModifier<T> modifier)
        {
            if (modifier == null)
            {
                return false;
            }

            var statType = modifier.GetStatType();

            if (statType == null) return false;

            string durationString = modifier.Duration > 0 ? modifier.Duration.ToString() : "Permanent";

            GUILayout.Label($"{modifier.StatName} ( {durationString} | {statType.Name} )");
            // small gray label as hint
            GUILayout.Label(modifier.ToString());

            return true;
        }

        private bool DrawConfigurableModifier<T>(ConfigurableModifier<T> modifier)
        {
            if (modifier == null)
            {
                return false;
            }

            var statType = modifier.GetStatType();

            if (statType == null) return false;

            string durationString = modifier.Duration > 0 ? modifier.Duration.ToString() : "Permanent";

            GUILayout.Label($"{modifier.StatName} ( {durationString} | {statType.Name} )");

            string modifierAction = "";

            if (statType == typeof(bool))
            {
                modifierAction = modifier.operationType == ModifierOperation.Add ? "|" : modifier.operationType == ModifierOperation.Multiply ? "&" : "!";
            } 
            else 
                modifierAction = modifier.operationType == ModifierOperation.Add ? "+" : modifier.operationType == ModifierOperation.Multiply ? "*" : "-";

            EditorGUILayout.LabelField(modifierAction, modifier.ModifierValue.ToString());

            return true;
        }

        private void DrawModifierAdder(Type objectType)
        {
            // Only float, bool and int fields are supported
            var fields = objectType.GetFields();
            fields = fields.Where(x => supportedTypes.Contains(x.FieldType)).ToArray();

            string[] fieldNames = fields.Select(x => x.Name).ToArray();

            fieldIndex = EditorGUILayout.Popup("Fields: ", fieldIndex, fieldNames);
            durationSelected = EditorGUILayout.FloatField("Duration: ", durationSelected);
            modifierTypeSelected = (ModifierOperation)EditorGUILayout.EnumPopup("Modifier action", modifierTypeSelected);

            var fieldType = fields[fieldIndex].FieldType;

            if (fieldType == typeof(int))
            {
                intModifierValueSelected = EditorGUILayout.IntField("Modifier value: ", intModifierValueSelected);
            }
            else if (fieldType == typeof(float)) 
            {
                floatModifierValueSelected = EditorGUILayout.FloatField("Modifier value: ", floatModifierValueSelected);
            }
            else if (fieldType == typeof(bool))
            {
                boolModifierValueSelected = EditorGUILayout.Toggle("Modifier value: ", boolModifierValueSelected);
            }

            if (GUILayout.Button("Add modifier"))
            {
                AddModifier(fields[fieldIndex]);
            }
        }

        private void AddModifier(FieldInfo field)
        {
            var creator = target as MultipleModifiersCreatorSO;

            var fieldType = field.FieldType;

            if (fieldType == typeof(int))
            {
                var modifierConfig = new ModifierConfig<int>();
                modifierConfig.Duration = durationSelected;
                modifierConfig.ModifierType = modifierTypeSelected;
                modifierConfig.ModifierValue = intModifierValueSelected;
                modifierConfig.Name = field.Name;

                creator.CreateStatModifier(modifierConfig);
            }
            else if (fieldType == typeof(float))
            {
                var modifierConfig = new ModifierConfig<float>();
                modifierConfig.Duration = durationSelected;
                modifierConfig.ModifierType = modifierTypeSelected;
                modifierConfig.ModifierValue = floatModifierValueSelected;
                modifierConfig.Name = field.Name;

                creator.CreateStatModifier(modifierConfig);
            }
            else if (fieldType == typeof(bool))
            {
                var modifierConfig = new ModifierConfig<bool>();
                modifierConfig.Duration = durationSelected;
                modifierConfig.ModifierType = modifierTypeSelected;
                modifierConfig.ModifierValue = boolModifierValueSelected;
                modifierConfig.Name = field.Name;

                creator.CreateStatModifier(modifierConfig);
            }
        }
    }

    [System.Serializable]
    public class SerializedModifier
    {
        [SerializeReference]
        public StatModifier modifier;

        public SerializedModifier(StatModifier modifier)
        {
            this.modifier = modifier;
        }
    }

    [CreateAssetMenu(menuName = "Topacai/Stats/ModifierSO", fileName = "ModifierSO")]
    public class MultipleModifiersCreatorSO : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private List<SerializedModifier> modifiers = new();
        public IEnumerable<StatModifier> Modifiers => modifiers.Select(m => m.modifier);

        [SerializeField] private MonoScript ScriptAsset;

        private Type scriptObject;

        public Type ScriptObject => scriptObject;
        public MonoScript GetScriptAsset() => ScriptAsset;

        private void OnValidate()
        {
            if (ScriptAsset == null) return;

            scriptObject = ScriptAsset.GetClass();
        }

        public void CreateStatModifier(ModifierConfig<int> modifierConfig)
        {
            modifiers.Add(new (new IntModifier(modifierConfig.Duration, modifierConfig.Name, modifierConfig.ModifierType, modifierConfig.ModifierValue)));
            EditorUtility.SetDirty(this);
        }

        public void CreateStatModifier(ModifierConfig<float> modifierConfig)
        {
            modifiers.Add(new (new FloatModifier(modifierConfig.Duration, modifierConfig.Name, modifierConfig.ModifierType, modifierConfig.ModifierValue)));
            EditorUtility.SetDirty(this);
        }

        public void CreateStatModifier(ModifierConfig<bool> modifierConfig)
        {
            modifiers.Add(new (new BoolModifier(modifierConfig.Duration, modifierConfig.Name, modifierConfig.ModifierType, modifierConfig.ModifierValue)));
            EditorUtility.SetDirty(this);
        }

        public void CreateStatModifier<T>(float duration, Func<T, T> operation, string name)
        {
            modifiers.Add(new (new BaseStatModifier<T>(duration, operation, name)));
            EditorUtility.SetDirty(this);
        }

        public void RemoveStatModifier(StatModifier modifier)
        {
            for (int i = 0; i < modifiers.Count; i++)
            {
                if(modifiers[i].modifier == modifier)
                {
                    modifiers.RemoveAt(i);
                    break;
                }
            }
        }
    }
}

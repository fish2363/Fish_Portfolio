#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModuleSO))]
public class ModuleSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        ModuleSO module = (ModuleSO)target;
        GUILayout.Space(12);
        DrawAddLogicButton(module);
        GUILayout.Space(8);
        DrawTriggerEffectButtons(module);
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawAddLogicButton(ModuleSO module)
    {
        if (!GUILayout.Button("모듈 로직 추가", GUILayout.Height(30)))
            return;

        GenericMenu menu = new GenericMenu();

        var types = TypeCache.GetTypesDerivedFrom<IModuleLogicDef>()
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .OrderBy(GetDisplayName);

        foreach (Type type in types)
        {
            Type capturedType = type;
            string displayName = GetDisplayName(capturedType);

            menu.AddItem(new GUIContent(displayName), false, () =>
            {
                Undo.RecordObject(module, "Add Module Logic");

                module.logicDefinitions.Add(
                    (IModuleLogicDef)Activator.CreateInstance(capturedType)
                );

                EditorUtility.SetDirty(module);
                AssetDatabase.SaveAssets();
            });
        }

        menu.ShowAsContext();
    }

    private void DrawTriggerEffectButtons(ModuleSO module)
    {
        if (module.logicDefinitions == null)
            return;

        for (int i = 0; i < module.logicDefinitions.Count; i++)
        {
            IModuleLogicDef logicDef = module.logicDefinitions[i];
            if (logicDef == null)
                continue;

            if (logicDef is not ModuleTriggerDef triggerDef)
                continue;

            GUILayout.Space(6);

            string triggerName = GetDisplayName(logicDef.GetType());
            GUILayout.Label($"[{i}] {triggerName}", EditorStyles.boldLabel);

            if (GUILayout.Button("+ 이펙트 추가", GUILayout.Height(24)))
                ShowAddEffectMenu(module, triggerDef);
        }
    }

    private void ShowAddEffectMenu(ModuleSO module, ModuleTriggerDef triggerDef)
    {
        GenericMenu menu = new GenericMenu();

        var types = TypeCache.GetTypesDerivedFrom<IModuleEffectDef>()
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .OrderBy(GetDisplayName);

        foreach (Type type in types)
        {
            Type capturedType = type;
            string displayName = GetDisplayName(capturedType);

            menu.AddItem(new GUIContent(displayName), false, () =>
            {
                Undo.RecordObject(module, "Add Module Effect");

                triggerDef.effectDefs.Add(
                    (IModuleEffectDef)Activator.CreateInstance(capturedType)
                );

                EditorUtility.SetDirty(module);
                AssetDatabase.SaveAssets();
            });
        }

        menu.ShowAsContext();
    }

    private static string GetDisplayName(Type type)
    {
        var attribute = type
            .GetCustomAttributes(typeof(ModuleDisplayNameAttribute), false)
            .FirstOrDefault() as ModuleDisplayNameAttribute;

        return attribute != null ? attribute.Name : type.Name;
    }
}
#endif

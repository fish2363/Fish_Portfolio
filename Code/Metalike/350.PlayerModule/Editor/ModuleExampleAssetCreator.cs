#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ModuleExampleAssetCreator
{
    private const string TargetFolder = "Assets/01.Work/KYH/350.ModuleExamples";

    [MenuItem("Tools/Modules/Create Learning Example Module")]
    public static void CreateLearningExampleModule()
    {
        EnsureFolder(TargetFolder);

        ModuleSO module = ScriptableObject.CreateInstance<ModuleSO>();
        module.moduleName = "Learning Example - On Equip Log";
        module.description = "Sample module for learning production flow.";
        module.moduleCategory = Category.Support;
        module.moduleType = ModuleType.Core;
        module.tier = ModuleTier.Common;

        var trigger = new OnEquipModuleTriggerDef();
        trigger.effectDefs.Add(new ExampleLogModuleEffectDef
        {
            message = "Module equipped successfully.",
            includeOwnerName = true
        });

        module.logicDefinitions.Add(trigger);

        string assetPath = $"{TargetFolder}/LearningExample_OnEquipLog.asset";
        AssetDatabase.CreateAsset(module, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = module;

        Debug.Log($"[Module Example] Created: {assetPath}");
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
            return;

        string[] parts = folderPath.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
#endif

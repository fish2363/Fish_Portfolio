using GondrLib.ObjectPool.RunTime;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewModule", menuName = "Modules/BaseModule")]
public class ModuleSO : ScriptableObject
{
    public string moduleName;
    [TextArea] public string descript;

    [Header("Meta Data")]
    public Category moduleCategory;
    public ModuleType moduleType;
    public ModuleTier tier;
    public int maxLevel = 1;

    [Header("Module Behaviors")]
    [SerializeReference]
    public List<IModuleLogicDef> logicDefinitions = new List<IModuleLogicDef>();

    public List<IModuleLogic> CreateAllLogics()
    {
        List<IModuleLogic> logics = new List<IModuleLogic>();
        foreach (var def in logicDefinitions)
        {
            if (def != null)
                logics.Add(def.CreateLogic());
        }
        return logics;
    }
}
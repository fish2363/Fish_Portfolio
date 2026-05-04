using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewModule", menuName = "Modules/BaseModule")]
public class ModuleSO : ScriptableObject
{
    public string moduleName;

    [TextArea]
    public string description;

    public Sprite icon;

    [Header("Meta Data")]
    public Category moduleCategory;
    public ModuleType moduleType;
    public ModuleTier tier;

    [Header("Module Behaviors")]
    [SerializeReference]
    public List<IModuleLogicDef> logicDefinitions = new();

    public void FillLogics(List<IModuleLogic> results)
    {
        if (results == null)
            return;

        for (int i = 0; i < logicDefinitions.Count; i++)
        {
            IModuleLogicDef logicDef = logicDefinitions[i];
            if (logicDef == null)
                continue;

            IModuleLogic logic = logicDef.CreateLogic();
            if (logic != null)
                results.Add(logic);
        }
    }
}

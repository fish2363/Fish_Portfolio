using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

[CustomEditor(typeof(ModuleSO))]
public class ModuleSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ModuleSO so = (ModuleSO)target;
        GUILayout.Space(15);

        if (GUILayout.Button("이거 모듈 로직 추가 버튼", GUILayout.Height(30)))
        {
            GenericMenu menu = new GenericMenu();

            var types = TypeCache.GetTypesDerivedFrom<IModuleLogicDef>()
                .Where(t => !t.IsAbstract && !t.IsInterface);

            foreach (var type in types)
            {
                menu.AddItem(new GUIContent(type.Name), false, () =>
                {
                    Undo.RecordObject(so, "Add Module Logic");
                    so.logicDefinitions.Add((IModuleLogicDef)Activator.CreateInstance(type));
                    EditorUtility.SetDirty(so);
                });
            }
            menu.ShowAsContext();
        }

        GUILayout.Space(10);
    }
}
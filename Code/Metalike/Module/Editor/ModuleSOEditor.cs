using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

[CustomEditor(typeof(ModuleSO))]
public class ModuleSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 1. 기본 인스펙터 그리기 (ModuleSO의 기본 변수들 표시)
        DrawDefaultInspector();

        ModuleSO so = (ModuleSO)target;
        GUILayout.Space(15);

        // 2. 모듈 로직 자동 감지 및 추가 버튼
        if (GUILayout.Button("이거 모듈 로직 추가 버튼", GUILayout.Height(30)))
        {
            GenericMenu menu = new GenericMenu();

            // IModuleLogicDef를 상속받는 모든 구현된 클래스를 찾아냅니다.
            var types = TypeCache.GetTypesDerivedFrom<IModuleLogicDef>()
                .Where(t => !t.IsAbstract && !t.IsInterface);

            foreach (var type in types)
            {
                menu.AddItem(new GUIContent(type.Name), false, () =>
                {
                    Undo.RecordObject(so, "Add Module Logic"); // Ctrl+Z 지원
                    so.logicDefinitions.Add((IModuleLogicDef)Activator.CreateInstance(type));
                    EditorUtility.SetDirty(so);
                });
            }
            menu.ShowAsContext();
        }

        GUILayout.Space(10);
    }
}
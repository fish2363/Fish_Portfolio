using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

[CustomEditor(typeof(ItemSO))]
public class ItemSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ItemSO so = (ItemSO)target;
        GUILayout.Space(15);

        if (GUILayout.Button("이거 효과 추가 버튼", GUILayout.Height(30)))
        {
            GenericMenu menu = new GenericMenu();

            var types = TypeCache.GetTypesDerivedFrom<IEquipItemDef>()
                .Where(t => !t.IsAbstract && !t.IsInterface);

            foreach (var type in types)
            {
                menu.AddItem(new GUIContent(type.Name), false, () =>
                {
                    Undo.RecordObject(so, "Add Module Logic");
                    so.effectDefinitions.Add((IEquipItemDef)Activator.CreateInstance(type));
                    EditorUtility.SetDirty(so);
                });
            }
            menu.ShowAsContext();
        }
        GUILayout.Space(10);
    }
}
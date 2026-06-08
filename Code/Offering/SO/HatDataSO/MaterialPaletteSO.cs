using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "UI/Material Palette", fileName = "MaterialPalette")]
public class MaterialPaletteSO : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string id;          // 버튼/선택 ID (예: "RED")
        public Material material;  // 실제 적용할 머티리얼 에셋
        public Color previewColor; // UI 버튼에 보여줄 색(선택)
    }

    public List<Entry> entries = new();

    public Material GetMaterial(string id)
    {
        for (int i = 0; i < entries.Count; i++)
            if (entries[i].id == id) return entries[i].material;
        return null;
    }

    public Color GetPreviewColor(string id)
    {
        for (int i = 0; i < entries.Count; i++)
            if (entries[i].id == id) return entries[i].previewColor;
        return Color.white;
    }
}
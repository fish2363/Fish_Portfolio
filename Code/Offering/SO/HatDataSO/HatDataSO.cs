using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Data/Hat Database", fileName = "HatDatabase")]
public class HatDataSO : ScriptableObject
{
    [System.Serializable]
    public class HatEntry
    {
        public int id;
        public string displayName;
        public int price;
        public GameObject hatPrefab;      // 착용용 프리팹(Head/HatSocket에 붙일 것)
        public GameObject previewModel;   // 버튼에서 돌릴 3D 모델(선택, 없으면 hatPrefab 써도 됨)
    }

    public List<HatEntry> hats = new();

    public HatEntry Get(int id)
    {
        for (int i = 0; i < hats.Count; i++)
            if (hats[i].id == id) return hats[i];
        return null;
    }

    public GameObject GetHatPrefab(int id) => Get(id)?.hatPrefab;
}
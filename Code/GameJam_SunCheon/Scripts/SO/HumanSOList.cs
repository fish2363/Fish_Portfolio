using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HumanSOList", menuName = "SO/HumanSOList")]
public class HumanSOList : ScriptableObject
{
    public List<HumanSO> humanSOs = new();

    // 순차 반환용 리스트
    [Header("순서대로 반환할 리스트")]
    public List<HumanSO> orderedHumanSOs = new();

    private int _orderedIndex = 0;

    private void Awake()
    {
        ResetOrderedIndex();
    }
    private void OnDestroy()
    {
        ResetOrderedIndex();
    }
    /// <summary>
    /// 리스트에서 랜덤한 HumanSO 하나 반환
    /// </summary>
    public HumanSO GetRandomHumanSO(WaveData wave)
    {
        float rand = Random.value;  // 0 ~ 1 사이
        float cumulative = 0f;

        foreach (var data in wave.spawnList)
        {
            cumulative += data.probability;
            if (rand < cumulative)
                return data.humanSO;  // 선택된 SO 반환
        }

        // 안전용: 혹시 rand가 1에 가까워서 선택 안되었을 경우
        return wave.spawnList[0].humanSO;
    }


    /// <summary>
    /// 순서대로 반환 (끝에 도달하면 null 반환 or 다시 처음으로)
    /// </summary>
    public HumanSO GetNextOrderedHumanSO()
    {
        if (orderedHumanSOs == null || orderedHumanSOs.Count == 0)
            return null;

        if (_orderedIndex >= orderedHumanSOs.Count)
        {
            // 다 돌았으면 다시 처음으로 → 원하면 여기서 null 반환으로 바꿔도 됨
            _orderedIndex = 0;
        }

        var result = orderedHumanSOs[_orderedIndex];
        _orderedIndex++;
        return result;
    }

    /// <summary>
    /// 순서 인덱스 초기화
    /// </summary>
    public void ResetOrderedIndex()
    {
        _orderedIndex = 0;
    }
}

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class HumanSpawnData
{
    public HumanSO humanSO;   // 어떤 손님인지
    [Range(0f, 1f)] public float probability = 0.1f; // 스폰 확률
}

[Serializable]
public class WaveData
{
    public string WaveName;
    public List<HumanSpawnData> spawnList = new List<HumanSpawnData>();
    public int maxTroubleCustomer = 2; // 연속 진상 제한
    public float interval;
}

public class InGameManager : MonoBehaviour
{
    [SerializeField] private HumanSOList _humanSOList;
    [SerializeField] private Human prefab;
    [SerializeField] private RectTransform movePoint;   // 기준점
    [SerializeField] private RectTransform spawnParent; // UI 부모
    [SerializeField] private float spacing = 100f;      // UI 간격(px)
    [SerializeField] private float moveDuration = 0.8f; // 이동 시간
    [SerializeField] private Vector2 offscreenOffset = new Vector2(200f, 0); // 화면 밖 시작 위치
    [SerializeField] private int maxSpawnCount = 5;     // 최대 생성 개수
    [SerializeField] private WaveData[] waveDatas;

    [SerializeField] private GameObject _jangBanPrefabs;
    [SerializeField] private RectTransform _jangBanSpawnPoint;
    [SerializeField] private RectTransform _jangBanParent;
    public bool IsAlreadySpawn { get; set; }

    private WaveData _currentWaveData;
    private List<Human> _spawnedHumans = new();
    private int spawnCount = 0;

    
    public void StartGame()
    {
        Initialize();
    }

    public void TutorialStartGame()
    {
        StartCoroutine(OrderSpawnRoutine());
    }


    private void Initialize()
    {
        foreach (var wave in waveDatas)
        {
            NormalizeWaveProbabilities(wave);
        }

        _currentWaveData = waveDatas[0];
        StartCoroutine(SpawnRoutine());
    }
    public void CreateJangBan()
    {
        if (IsAlreadySpawn)
        {
            InGameUIManager.Instance.ShowFloatingText("이미 쟁반이 있습니다.",Color.red);
            return;
        }

            // 부모를 바로 지정해서 Instantiate
            GameObject rect = Instantiate(_jangBanPrefabs, _jangBanParent);

        // RectTransform 기준으로 위치를 스폰 포인트에 맞추기
        RectTransform rt = rect.GetComponent<RectTransform>();
        rt.anchoredPosition = _jangBanSpawnPoint.anchoredPosition;

        IsAlreadySpawn = true;
    }

    public void NormalizeWaveProbabilities(WaveData wave)
    {
        float total = 0f;
        foreach (var data in wave.spawnList)
            total += data.probability;

        if (total <= 0f) total = 1f;

        foreach (var data in wave.spawnList)
            data.probability /= total;
    }

    private IEnumerator OrderSpawnRoutine()
    {
        while (true)
        {
            // 최대 개수 제한
            if (_spawnedHumans.Count >= 1)
                yield return new WaitUntil(() => _spawnedHumans.Count < 1);

            // Human 생성
            Human obj = Instantiate(prefab, spawnParent);
            RectTransform rect = obj.GetComponent<RectTransform>();

            // 시작 위치: MovePoint 기준 + offscreen + 간격
            Vector2 startPos = movePoint.anchoredPosition + offscreenOffset + new Vector2(_spawnedHumans.Count * spacing, 0);
            rect.anchoredPosition = startPos;

            // HumanSO 순서대로 선택 및 초기화
            HumanSO humanType = _humanSOList.GetNextOrderedHumanSO();
            if (humanType == null)
            {
                Debug.LogWarning("더 이상 순서대로 스폰할 HumanSO가 없습니다!");
                yield break; // 루틴 종료
            }

            obj.Initialize(humanType, this);
            _spawnedHumans.Add(obj);

            // 목표 위치 계산 및 Tween 이동
            UpdateTargetPositions();

            spawnCount++;
            yield return new WaitForSeconds(1.5f);
            FindAnyObjectByType<TutorialManager>()?.UnlockStep();
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // 최대 개수 제한
            if (_spawnedHumans.Count >= maxSpawnCount)
                yield return new WaitUntil(() => _spawnedHumans.Count < maxSpawnCount);

            // Human 생성
            Human obj = Instantiate(prefab, spawnParent);
            RectTransform rect = obj.GetComponent<RectTransform>();

            // 시작 위치: MovePoint 기준 + offscreen + 간격
            Vector2 startPos = movePoint.anchoredPosition + offscreenOffset + new Vector2(_spawnedHumans.Count * spacing, 0);
            rect.anchoredPosition = startPos;

            // HumanSO 랜덤 선택 및 초기화
            HumanSO humanType = _humanSOList.GetRandomHumanSO(_currentWaveData);
            obj.Initialize(humanType, this);

            _spawnedHumans.Add(obj);

            // 목표 위치 계산 및 Tween 이동
            UpdateTargetPositions();

            spawnCount++;

            yield return new WaitForSeconds(_currentWaveData.interval);
        }
    }

    /// <summary>
    /// 리스트 순서대로 목표 위치 재계산 및 이동
    /// </summary>
    public void UpdateTargetPositions()
    {
        for (int i = 0; i < _spawnedHumans.Count; i++)
        {
            RectTransform rect = _spawnedHumans[i].GetComponent<RectTransform>();
            Vector2 targetPos = movePoint.anchoredPosition + new Vector2(i * spacing, 0);
            rect.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutCubic);
        }
    }

    /// <summary>
    /// Human 제거
    /// </summary>
    public void RemoveHuman(Human human)
    {
        if (_spawnedHumans.Contains(human))
        {
            _spawnedHumans.Remove(human);
            UpdateTargetPositions(); // 뒤에 있는 애들 앞으로 당기기
        }
    }
}
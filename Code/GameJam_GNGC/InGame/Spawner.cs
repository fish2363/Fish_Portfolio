using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Spawner : MonoBehaviour
{
    [Header("스폰 딜레이")]
    public MinMax spawnWait;
    private float waitSecond;

    public EatSOList EatSOList;
    private EatSO spawnSO;

    [Header("한 번에 스폰할 개수")]
    public int count = 10;

    private BoxCollider2D area;
    public List<GameObject> planetList = new List<GameObject>();

    [Header("레벨당 오르는 점수")]
    public int[] levelScore;

    [Header("레벨 범위별 확률 설정")]
    public LevelPercent[] levelPercents;

    [Header("최대 생성")]
    public int maxPlanet;

    private BlackHole player;

    private void Awake()
    {
        player = FindAnyObjectByType<BlackHole>();
    }

    void Start()
    {
        area = GetComponent<BoxCollider2D>();
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        waitSecond = UnityEngine.Random.Range(spawnWait.min, spawnWait.max - 1);
        yield return new WaitForSeconds(waitSecond);
        Debug.Log("생성");

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = GetRandomPosition();

            // 현재 플레이어 레벨 기준으로 확률 가져오기
            Percent currentPercent = GetPercentForLevel(player.Level);

            int randPercent = UnityEngine.Random.Range(0, 100);
            int randPlanet = UnityEngine.Random.Range(0, EatSOList.eatSOs.Count);

            // 누적 확률로 행성 등급 결정
            int level = 0;
            if (randPercent < currentPercent.firstPlanet)
            {
                level = 0;
            }
            else if (randPercent < currentPercent.firstPlanet + currentPercent.secondPlanet)
            {
                level = 1;
            }
            else
            {
                level = 2;
            }

            // 해당 등급에 맞는 SO 선택
            spawnSO = EatSOList.eatSOs[randPlanet];

            // 생성 및 설정
            GameObject instance = Instantiate(spawnSO.planet, spawnPos, Quaternion.identity);

            float size = spawnSO.sizes[level];
            instance.transform.localScale = new Vector3(size, size, size);

            Planet planet = instance.GetComponent<Planet>();
            planet.currentSO = spawnSO;
            planet.currentScale = size;
            planet.currentLevel = level + 1;
            planet.score = levelScore[level];

            planetList.Add(instance);
        }
        yield return new WaitWhile(() => maxPlanet <= planetList.Count);
        StartCoroutine(SpawnRoutine());
    }

    private Vector2 GetRandomPosition()
    {
        Vector2 basePosition = transform.position;
        Vector2 size = area.size;
        Vector2 worldSize = new Vector2(size.x * transform.lossyScale.x, size.y * transform.lossyScale.y);

        float posX = basePosition.x + UnityEngine.Random.Range(-worldSize.x / 2f, worldSize.x / 2f);
        float posY = basePosition.y + UnityEngine.Random.Range(-worldSize.y / 2f, worldSize.y / 2f);

        return new Vector2(posX, posY);
    }

    private Percent GetPercentForLevel(int level)
    {
        foreach (var lp in levelPercents)
        {
            if (level >= lp.minLevel && level <= lp.maxLevel)
            {
                return lp.percent;
            }
        }

        // 조건에 맞는 범위가 없을 경우 마지막 값을 반환
        return levelPercents.Length > 0 ? levelPercents[levelPercents.Length - 1].percent : new Percent();
    }

    private void OnDrawGizmos()
    {
        if (area == null)
            area = GetComponent<BoxCollider2D>();

        Vector2 size = area.size;
        Vector2 worldSize = new Vector2(size.x * transform.lossyScale.x, size.y * transform.lossyScale.y);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, worldSize);
    }
}

[Serializable]
public struct MinMax
{
    public float min;
    public float max;
}

[Serializable]
public struct Percent
{
    public float firstPlanet;
    public float secondPlanet;
    public float thirdPlanet;
}

[Serializable]
public struct LevelPercent
{
    public int minLevel;
    public int maxLevel;
    public Percent percent;
}
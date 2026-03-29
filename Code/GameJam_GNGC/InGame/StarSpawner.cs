using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StarSpawner : MonoBehaviour
{
    [Header("스폰 딜레이")]
    public MinMax spawnWait;
    private float waitSecond;

    public EatSOList EatSOList;
    private EatSO spawnSO;

    [Header("한 번에 스폰할 개수")]
    public int count = 10;
    [Header("별 점수")]
    public int starScore = 10;

    private BoxCollider2D area;
    private List<GameObject> planetList = new List<GameObject>();
    [Header("최대 생성")]
    public int maxPlanet;

    void Start()
    {
        area = GetComponent<BoxCollider2D>();
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitWhile(() => maxPlanet <= planetList.Count);
        waitSecond = UnityEngine.Random.Range(spawnWait.min, spawnWait.max - 1);
        yield return new WaitForSeconds(waitSecond);
        Debug.Log("생성");

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = GetRandomPosition();


            // 해당 등급에 맞는 SO 선택
            int randStarPrefabs = UnityEngine.Random.Range(0, EatSOList.eatSOs.Count);
            spawnSO = EatSOList.eatSOs[randStarPrefabs];

            // 생성 및 설정
            GameObject instance = Instantiate(spawnSO.planet, spawnPos, Quaternion.identity);

            float size = spawnSO.sizes[0];
            instance.transform.localScale = new Vector3(size, size, size);

            StarPlanet planet = instance.GetComponent<StarPlanet>();
            planet.currentSO = spawnSO;
            planet.currentScale = size;
            planet.score = starScore;

            planetList.Add(instance);
        }

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

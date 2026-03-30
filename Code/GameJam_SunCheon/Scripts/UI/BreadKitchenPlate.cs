using System.Collections.Generic;
using UnityEngine;

public class BreadKitchenPlate : MonoBehaviour
{
    [Header("내가 속한 쟁반 (Left/Right)")]
    [SerializeField] private TraySide traySide;

    [Header("스폰할 빵 프리팹 (Food 스크립트 포함)")]
    public Food breadPrefab;

    [Header("빵을 놓을 부모(없으면 현재 오브젝트에 부착)")]
    public Transform breadParent;

    [Header("랜덤 오프셋 범위")]
    public Vector2 offsetRange = new Vector2(0.2f, 0.2f);

    private TrayData tray;
    private readonly List<GameObject> spawnedBreads = new List<GameObject>();

    private void OnEnable()
    {
        if (breadParent == null) breadParent = transform;
        tray = TrayManager.Instance.GetTray(traySide);
        tray.FoodCount.OnValueChanged -= RefreshBread;
        tray.FoodCount.OnValueChanged += RefreshBread;

        // 시작 시 현재 상태 한 번 반영
        RefreshBread(tray.FoodCount.Value);
    }

    private void OnDisable()
    {
        if (tray != null)
            tray.FoodCount.OnValueChanged -= RefreshBread;
    }

    private bool _refreshing;

    private void RefreshBread(int _)
    {
        if (_refreshing) return;
        _refreshing = true;
        try
        {
            // 1) 전부 제거
            for (int i = spawnedBreads.Count - 1; i >= 0; i--)
                if (spawnedBreads[i]) Destroy(spawnedBreads[i]);
            spawnedBreads.Clear();

            // 2) 정확히 Foods.Count 만큼 재생성
            int target = tray.Foods.Count;
            for (int i = 0; i < target; i++)
                SpawnOneBreadAtIndex(i);
        }
        finally
        {
            _refreshing = false;
        }
    }

    private void SpawnOneBreadAtIndex(int index)
    {
        Vector3 basePos = breadParent.position;
        Vector3 randomOffset = new Vector3(
            UnityEngine.Random.Range(-offsetRange.x, offsetRange.x),
            0f,
            UnityEngine.Random.Range(-offsetRange.y, offsetRange.y)
        );

        Food bread = Instantiate(
            breadPrefab,
            basePos + randomOffset,
            breadParent.rotation,
            breadParent
        );
        bread.transform.SetParent(breadParent.parent, true);

        // 안전하게 enum 할당
        WantFoodEnum value = (index < tray.Foods.Count) ? tray.Foods[index] : default;
        bread.foodEnum = value;

        var meta = bread.GetComponent<BreadMeta>();
        if (meta != null)
        {
            meta.originTray = traySide;
            meta.foodEnum = value;
        }

        spawnedBreads.Add(bread.gameObject);
    }
    private void RemoveOneBread()
    {
        GameObject bread = spawnedBreads[^1];
        spawnedBreads.RemoveAt(spawnedBreads.Count - 1);
        Destroy(bread);
    }
    // ▲▲ 여기까지 복붙 ▲▲
}

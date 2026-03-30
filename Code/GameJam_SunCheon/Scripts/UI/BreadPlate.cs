using UnityEngine;
using UnityEngine.EventSystems;

public class BreadPlate : DragObj
{
    [SerializeField] private TraySide traySide;   // ← 왼쪽/오른쪽 선택
    [SerializeField] private RectTransform _grid;
    [SerializeField] private GameObject _breadPrefab;
    [SerializeField] private int _maxBreadCount = 3;

    private TrayData tray;   // 내 쟁반 데이터 캐시
    private bool _syncing;
    protected override void OnEnable()
    {
        isPutNotOhterObj = true;

        tray = TrayManager.Instance.GetTray(traySide);
        tray.FoodCount.OnValueChanged -= SyncWithManager;
        tray.FoodCount.OnValueChanged += SyncWithManager;
        SyncWithManager(tray.FoodCount.Value);
    }

    private void OnDisable()
    {
        tray.FoodCount.OnValueChanged -= SyncWithManager;
    }

    private void SyncWithManager(int _)
    {
        if (_syncing) return;
        _syncing = true;
        try
        {
            // 1) 현재 모든 자식 제거
            for (int i = _grid.childCount - 1; i >= 0; i--)
                Destroy(_grid.GetChild(i).gameObject);

            // 2) 데이터(트레이) 기준으로 정확히 다시 생성
            int target = Mathf.Min(tray.Foods.Count, _maxBreadCount);
            for (int i = 0; i < target; i++)
                Instantiate(_breadPrefab, _grid);
        }
        finally
        {
            _syncing = false;
        }
    }

    public override void Execute(DragObj obj)
    {
        base.Execute(obj);

        if (obj.TryGetComponent(out FinishBread finish))
        {
            Destroy(finish.gameObject);

            WantFoodEnum foodEnum =
                finish.currentPiping == Piping.Cream ?
                WantFoodEnum.Bread_Cream :
                WantFoodEnum.Bread_Pot;

            // → 자기 쟁반 데이터에 추가
            tray.Add(foodEnum);
        }
    }

    public void RemoveLastBread()
    {
        tray.RemoveLast();  // 자기 쟁반에서만 제거 → UI 자동 갱신
    }
}

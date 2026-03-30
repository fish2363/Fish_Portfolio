using System.Collections.Generic;
using UnityEngine;
public enum TraySide
{
    Left,
    Right
}
[System.Serializable]
public class TrayData
{
    private readonly List<WantFoodEnum> foods = new();
    public IReadOnlyList<WantFoodEnum> Foods => foods;
    public NotifyValue<int> FoodCount = new NotifyValue<int>(0);

    public void Add(WantFoodEnum food)
    {
        foods.Add(food);
        FoodCount.Value = foods.Count;
    }

    public void Remove(WantFoodEnum food)
    {
        if (foods.Remove(food))
            FoodCount.Value = foods.Count;
    }

    public void RemoveLast()
    {
        if (foods.Count > 0)
        {
            foods.RemoveAt(foods.Count - 1);
            FoodCount.Value = foods.Count;
        }
    }
}

[DefaultExecutionOrder(-100)]
public class TrayManager : MonoBehaviour
{
    public static TrayManager Instance;

    // 왼쪽/오른쪽 쟁반별 데이터
    private readonly Dictionary<TraySide, TrayData> trays =
        new Dictionary<TraySide, TrayData>
        {
            { TraySide.Left,  new TrayData() },
            { TraySide.Right, new TrayData() }
        };

    private void Awake() => Instance = this;

    public TrayData GetTray(TraySide side) => trays[side];
}

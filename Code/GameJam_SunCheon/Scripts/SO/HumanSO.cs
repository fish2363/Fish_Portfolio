using UnityEngine;
using System;

[Serializable]
public class WantMenu
{
    public string dialogue;
    public WantFoodEnum foodEnum;
}

[Flags]
public enum WantFoodEnum
{
    None = 0,
    Juice = 1 << 0,
    Stew = 1 << 1,
    Bread_Pot = 1 << 2,
    Bread_Cream = 1 << 3
}

[CreateAssetMenu(fileName = "HumanSO", menuName = "SO/HumanSO")]
public class HumanSO : ScriptableObject
{
    [Header("이 사람 대사")]
    public WantMenu[] dialogue;
    [Header("참는 시간")]
    public float waitTime;
    [Header("분노 단계별 외형")]
    public Sprite[] visual;

    /// <summary>
    /// 대사 배열에서 랜덤으로 하나를 리턴
    /// </summary>
    public WantMenu GetRandomDialogue()
    {
        if (dialogue == null || dialogue.Length == 0)
            return null; // 배열이 비었을 때 안전 처리

        int index = UnityEngine.Random.Range(0, dialogue.Length);
        return dialogue[index];
    }
}

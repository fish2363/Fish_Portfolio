using UnityEngine;

[CreateAssetMenu(fileName = "Pattern_", menuName = "Game/Pattern")]
public class PatternDataSO : ScriptableObject
{
    [Header("패턴 기본 정보")]
    public string patternName = "새로운 패턴";
    [TextArea(2, 4)]
    public string description = "패턴 설명";

    

    

    

    /// <summary>
    /// 패턴에서 사용되는 고유한 원소들 반환
    /// </summary>
    //public PatternElement[] GetUniqueElements()
    //{
    //    if (patternSequence == null || patternSequence.Length == 0)
    //        return new PatternElement[0];

    //    System.Collections.Generic.HashSet<PatternElement> uniqueElements =
    //        new System.Collections.Generic.HashSet<PatternElement>();

    //    foreach (PatternElement element in patternSequence)
    //    {
    //        uniqueElements.Add(element);
    //    }

    //    PatternElement[] result = new PatternElement[uniqueElements.Count];
    //    uniqueElements.CopyTo(result);
    //    return result;
    //}

    /// <summary>
    /// 패턴 시퀀스 검증
    /// </summary>
    //public bool ValidatePattern()
    //{
    //    if (patternSequence == null || patternSequence.Length != patternLength)
    //    {
    //        Debug.LogError($"PatternData {name}: 패턴 길이가 맞지 않습니다. 설정: {patternLength}, 실제: {patternSequence?.Length ?? 0}");
    //        return false;
    //    }

    //    if (patternLength < 3 || patternLength > 12)
    //    {
    //        Debug.LogError($"PatternData {name}: 패턴 길이가 범위를 벗어났습니다. ({patternLength})");
    //        return false;
    //    }

    //    return true;
    //}

    ///// <summary>
    ///// 패턴 자동 생성 (에디터용)
    ///// </summary>
    //[ContextMenu("랜덤 패턴 생성")]
    //public void GenerateRandomPattern()
    //{
    //    patternSequence = new PatternElement[patternLength];

    //    // 사용할 원소 개수 (2~6개 사이)
    //    int elementCount = Random.Range(2, 7);
    //    PatternElement[] availableElements = new PatternElement[elementCount];

    //    // 랜덤하게 원소 선택
    //    System.Array elementValues = System.Enum.GetValues(typeof(PatternElement));
    //    for (int i = 0; i < elementCount; i++)
    //    {
    //        availableElements[i] = (PatternElement)elementValues.GetValue(Random.Range(0, elementValues.Length));
    //    }

    //    // 패턴 시퀀스 생성
    //    for (int i = 0; i < patternLength; i++)
    //    {
    //        patternSequence[i] = availableElements[Random.Range(0, elementCount)];
    //    }

    //    Debug.Log($"패턴 생성 완료: {name} (길이: {patternLength}, 사용 원소: {elementCount}개)");
    //}

    
}

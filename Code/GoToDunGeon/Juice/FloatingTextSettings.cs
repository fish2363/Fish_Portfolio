using UnityEngine;

[CreateAssetMenu(fileName = "FloatingTextSettings", menuName = "Game/Floating Text Settings")]
public class FloatingTextSettings : ScriptableObject
{
    [Header("----색상 설정----")]
    [Tooltip("maxDamage 기준으로 색상이 변화")]
    public float maxDamage = 100;
    [Tooltip("데미지가 낮을 때 색상")]
    public Color lowDamageColor = Color.white;
    [Tooltip("데미지가 maxDamage에 가까워질 때 색상")]
    public Color highDamageColor = Color.red;

    [Header("----위치 이동 설정----")]
    [Tooltip("텍스트의 X축 랜덤 오프셋")]
    public float randomXOffset = 0.2f;
    [Tooltip("위로 이동하는 거리")]
    public float moveUpDistance = 2f;

    [Header("----시간 설정----")]
    [Tooltip("텍스트가 유지되는 전체 시간")]
    public float moveDuration = 0.8f;
    [Tooltip("커지는 데 걸리는 시간")]
    public float scaleUpTime = 0.2f;

    [Header("----크기 변화 설정----")]
    [Tooltip("시작 크기 배율")]
    public float startScale = 0.2f;
}
using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "StopInfoSO", menuName = "SO/StopInfoSO")]
public class StopInfoSO : ScriptableObject
{
    [Range(0, 1)] 
    public float StopPower;
    public float Duration;
    public bool IsInfinite;
    public StopChannel StopChannel;
    public Ease EaseType;

    public float GetEffectScore()
        => (1f - StopPower) * 10f + Duration + (IsInfinite ? 100f : 0f);
}

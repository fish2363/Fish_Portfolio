using UnityEngine;

[CreateAssetMenu(fileName = "Movement data", menuName = "SO/Combat/Movement", order = 0)]
public class MovementDataSO : ScriptableObject
{
    public float maxSpeed;
    public float duration;
    public AnimationCurve moveCurve;
}

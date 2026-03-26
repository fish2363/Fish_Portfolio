using UnityEngine;

public interface IKnockBackable
{
    public void BackStep(Vector3 direction, MovementDataSO knockBackMovement);
}

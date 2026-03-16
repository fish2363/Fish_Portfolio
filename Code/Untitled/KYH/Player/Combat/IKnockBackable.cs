using UnityEngine;

public interface IKnockBackable
{
    public void KnockBack(Vector3 direction, MovementDataSO knockBackMovement);
}

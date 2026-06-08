using UnityEngine;

public class ActionData : MonoBehaviour, IEntityComponent
{
    public Vector3 HitPoint { get; set; }
    public Vector3 HitNormal { get; set; }
    public bool HitByPowerAttack { get; set; }
    public DamageData LastDamageData { get; set; } //마지막으로 맞은 데미지에 대한 데이터

    private Entity _entity;
    public void Initialize(Entity entity)
    {
        _entity = entity;
    }
}
